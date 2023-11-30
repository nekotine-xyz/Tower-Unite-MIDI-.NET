using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Smf;
using NHotkey;
using NHotkey.WindowsForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using TowerUniteMidiDotNet.Core;
using WindowsInput.Native;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace TowerUniteMidiDotNet.Windows
{
    public partial class MainWindow : Form
    {
        public const string Version = "1.2";
        public static int KeyDelay = 15;

        private InputDevice currentMidiDevice;
        private MidiContainer currentMidiFile;
        private bool detailedLogging = false;
        private int noteLookupOctaveTransposition = 3;
        private int midiTransposition = 0;
        private double midiPlaybackSpeed = 1.0;
        private Dictionary<int, Core.Note> noteLookup;
        private readonly Keys startKey = Keys.F1;
        private readonly Keys stopKey = Keys.F2;
        private bool isMidiPlaying = false;
        private readonly object playbackLock = new object();
        private bool isDrumModeEnabled = false;
        private bool isAutoTranspositionEnabled = false;
        private readonly System.Windows.Forms.Timer playbackTimer;

        // called qwertyLookup for future integration of qwertz, azerty, etc.
        private readonly List<char> qwertyLookup = new List<char>()
        {
            '1','!','2','@','3','4','$','5','%','6','^','7',
            '8','*','9','(','0','q','Q','w','W','e','E','r',
            't','T','y','Y','u','i','I','o','O','p','P','a',
            's','S','d','D','f','g','G','h','H','j','J','k',
            'l','L','z','Z','x','c','C','v','V','b','B','n',
            'm'
        };

        // the decisions I made to work with the limited drumkit of Tower Unite is rather arbitrary, might introduce some system to allow players to assign as they wish
        private readonly Dictionary<int, VirtualKeyCode> drumMapping = new Dictionary<int, VirtualKeyCode>()
        {
            { 35, VirtualKeyCode.SPACE }, // Acoustic Bass Drum -> Kick
			{ 36, VirtualKeyCode.SPACE }, // Bass Drum 1 -> Kick
            { 37, VirtualKeyCode.VK_G }, // Side Stick -> Snare Rim
            { 54, VirtualKeyCode.VK_G }, // Tambourine -> Snare Rim
            { 82, VirtualKeyCode.VK_G }, // Shaker -> Snare Rim
			{ 38, VirtualKeyCode.VK_F },  // Acoustic Snare -> Snare
			{ 39, VirtualKeyCode.VK_B }, // Hand Clap -> Clap
			{ 40, VirtualKeyCode.VK_F },  // Electric Snare -> Snare
			{ 46, VirtualKeyCode.VK_D },  // Open Hi-hat -> Closed Hit-Hat (OPEN HI HAT SOUNDS AWFUL!)
			{ 42, VirtualKeyCode.VK_D },  // Closed Hi-hat -> Closed Hi-Hat
			{ 44, VirtualKeyCode.VK_D },  // Pedal Hi-Hat -> Closed Hi-Hat
			{ 49, VirtualKeyCode.VK_R },  // Crash Cymbal 1 -> Crash Cymbal 1
			{ 52, VirtualKeyCode.VK_R },  // Chinese Cymbal -> Crash Cymbal 1
			{ 55, VirtualKeyCode.VK_U },  // Splash Cymbal -> Crash Cymbal 2
			{ 57, VirtualKeyCode.VK_U },  // Crash Cymbal 2 -> Crash Cymbal 2
			{ 50, VirtualKeyCode.VK_T },  // High Tom -> High Tom
			{ 48, VirtualKeyCode.VK_Y },  // Mid Tom -> Mid Tom
			{ 43, VirtualKeyCode.VK_Y },  // High Floor Tom -> Mid Tom
			{ 45, VirtualKeyCode.VK_J },  // Low Tom -> Floor Tom
			{ 47, VirtualKeyCode.VK_Y },  // Low-Mid Tom -> Mid Tom
			{ 41, VirtualKeyCode.VK_J },  // Low Floor Tom -> Floor Tom
			// ... why isn't there a ride cymbal in Tower Unite???
			//{ 51, VirtualKeyCode.VK_U },  // Ride Cymbal -> Crash Cymbal 2
			{ 51, VirtualKeyCode.VK_D },  // Ride Cymbal -> Closed Hi-Hat
			{ 59, VirtualKeyCode.VK_U },  // Ride Cymbal 2 -> Crash Cymbal 2
			{ 53, VirtualKeyCode.VK_D },  // Ride Bell -> Closed Hi-hat
		};

        /// <summary>
        /// A container class for holding a MIDI file, its Playback object and its filename.
        /// </summary>
        private class MidiContainer
        {
            public MidiFile MidiFile;
            public Playback MidiPlayback;
            public string MidiName;

            public MidiContainer(string name, MidiFile file)
            {
                MidiFile = file;
                MidiName = name;
                MidiPlayback = file.GetPlayback();
            }
        }

        public MainWindow()
        {
            // check for multiple instances of TUMDN
            if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1)
            {
                MessageBox.Show("Tower Unite MIDI .NET is already running!", "Instance Check", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Environment.Exit(0); // terminate this instance
            }

            InitializeComponent();

            ScanDevices();
            BuildNoteDictionary();

            playbackTimer = new Timer
            {
                Interval = 1000 // Update every second
            };
            playbackTimer.Tick += new EventHandler(PlaybackTimer_Tick);
            MIDIPlaybackSpeedSlider.Value = 10;
            MIDIPlaybackTransposeSlider.Value = 0;
            OctaveTranspositionSlider.Value = 0;

            try
            {
                HotkeyManager.Current.AddOrReplace("Start", startKey, OnHotkeyPress);
                HotkeyManager.Current.AddOrReplace("Stop", stopKey, OnHotkeyPress);
            }
            catch (NHotkey.HotkeyAlreadyRegisteredException)
            {
                // this should not occur with my check for multiple instances, but if it does I need to know why
                MessageBox.Show("A hotkey is already in use! Please report this on the Github, or xelapilled on Twitter or Discord.");
            }

            Text += " " + Version;
        }

        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            if (currentMidiFile != null && currentMidiFile.MidiPlayback.IsRunning)
            {
                var currentTime = currentMidiFile.MidiPlayback.GetCurrentTime<MetricTimeSpan>();
                var duration = currentMidiFile.MidiFile.GetDuration<MetricTimeSpan>();

                progressBar1.Value = (int)((currentTime.TotalMicroseconds * progressBar1.Maximum) / duration.TotalMicroseconds);
            }
        }

        /// <summary>
        /// Will look for any MIDI input devices connected to your computer.
        /// </summary>
        private void ScanDevices()
        {
            DeviceComboBox.Items.Clear();

            foreach (InputDevice device in InputDevice.GetAll())
            {
                DeviceComboBox.Items.Add($"{device.Name} ID:{device.Id}");
            }
        }

        /// <summary>
        /// Builds the note lookup dictionary for the program to reference. The key in the Dictionary is the notes MIDI number, whereas
        /// the value is the corresponding Note object itself.
        /// </summary>
        private void BuildNoteDictionary()
        {
            noteLookup = new Dictionary<int, Core.Note>();

            for (int row = 0; row < 6; row++)
            {
                for (int column = 0; column < 12; column++)
                {
                    if (row == 5 && column == 1)
                    {
                        break;
                    }

                    if (!char.IsLetterOrDigit(qwertyLookup[row * 12 + column]) || char.IsUpper(qwertyLookup[row * 12 + column]))
                    {
                        noteLookup.Add((row + noteLookupOctaveTransposition) * 12 + column, new Core.Note(qwertyLookup[(row * 12 + column) - 1], true));
                    }
                    else
                    {
                        noteLookup.Add((row + noteLookupOctaveTransposition) * 12 + column, new Core.Note(qwertyLookup[row * 12 + column]));
                    }
                }
            }

            Log($"Note dictionary built. Middle C is C{noteLookupOctaveTransposition + 1}.");
        }

        private void Log(string logText)
        {
            // check if the current method call is on a different thread than the one that created the EventListView control
            if (EventListView.InvokeRequired)
            {
                // if it is on a different thread, invoke the Log method on the UI thread
                // this ensures that the Log method is executed on the correct thread
                EventListView.Invoke(new Action<string>(Log), logText);
            }
            else
            {
                // if the current method call is already on the UI thread, insert the logText at the top of the EventListView control
                EventListView.Items.Insert(0, logText);

                // ensure the newest log item is visible in the EventListView control
                EventListView.EnsureVisible(0);

                // if there are more than 100 items in the EventListView, remove the oldest one (which is the last one in the list)
                if (EventListView.Items.Count > 100)
                {
                    EventListView.Items.RemoveAt(EventListView.Items.Count - 1);
                }
            }
        }

        #region MIDI Playback

        private void PlayMidi()
        {
            if (currentMidiFile == null || currentMidiFile.MidiPlayback.IsRunning)
            {
                return;
            }
            else if (currentMidiFile != null)
            {
                currentMidiFile.MidiPlayback.Finished -= OnMidiPlaybackComplete;

                currentMidiFile.MidiPlayback.NotesPlaybackStarted -= OnMidiPlaybackNoteEventReceived;
            }

            var duration = currentMidiFile.MidiFile.GetDuration<MetricTimeSpan>();
            progressBar1.Maximum = (int)duration.TotalMicroseconds;
            progressBar1.Value = 0;

            MIDIPlaybackTransposeSlider.Enabled = false;
            MIDIPlaybackSpeedSlider.Enabled = false;

            currentMidiFile.MidiPlayback.NotesPlaybackStarted += OnMidiPlaybackNoteEventReceived;
            currentMidiFile.MidiPlayback.Finished += OnMidiPlaybackComplete;
            currentMidiFile.MidiPlayback.Speed = midiPlaybackSpeed;
            currentMidiFile.MidiPlayback.Start();
            isMidiPlaying = true;
            playbackTimer.Start();
            Log($"Started playing {currentMidiFile.MidiName}.");
        }

        private void OnMidiPlaybackNoteEventReceived(object sender, NotesEventArgs e)
        {
            foreach (var midiNote in e.Notes)
            {
                // Only process notes on the drum channel if Drum Mode is enabled
                if (isDrumModeEnabled && midiNote.Channel == (FourBitNumber)9)
                {
                    if (drumMapping.TryGetValue(midiNote.NoteNumber, out VirtualKeyCode keyCode))
                    {
                        Core.Note.PlayDrum(keyCode);
                        if (detailedLogging)
                        {
                            Log($"Drum hit: MIDI number {midiNote.NoteNumber}, key code {keyCode}.");
                        }
                    }
                    else
                    {
                        Log($"Drum note out of range or not mapped: MIDI number {midiNote.NoteNumber}.");
                    }
                }
                else if (!isDrumModeEnabled) // Piano Mode
                {
                    if (midiNote.Channel != (FourBitNumber)9)
                    {
                        int originalNoteNumber = midiNote.NoteNumber + midiTransposition;
                        int transposedNoteNumber = originalNoteNumber;

                        // transpose the note if it's out of range and auto transpose is enabled
                        if (isAutoTranspositionEnabled)
                        {
                            transposedNoteNumber = Core.Note.TransposeToPlayableRange(originalNoteNumber);
                        }

                        if (noteLookup.TryGetValue(transposedNoteNumber, out Core.Note note))
                        {
                            note.Play();
                            if (detailedLogging)
                            {
                                string noteRepresentation = note.IsShiftedKey ? $"^{note.NoteCharacter}" : $"{note.NoteCharacter}";
                                if (isAutoTranspositionEnabled && originalNoteNumber != transposedNoteNumber)
                                {
                                    Log($"[AutoTranspose] MIDI number {originalNoteNumber} to {transposedNoteNumber}, character {noteRepresentation}.");
                                }
                                else
                                {
                                    Log($"Played piano note: MIDI number {transposedNoteNumber}, character {noteRepresentation}");
                                }
                            }
                        }
                        else if (!isAutoTranspositionEnabled)
                        {
                            Log($"Piano note out of range: MIDI number {originalNoteNumber} cannot be played.");
                        }
                    }
                }
            }
        }

        private void StopMidi()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(StopMidi));
                return;
            }
            lock (playbackLock)
            {
                if (isMidiPlaying && currentMidiFile?.MidiPlayback.IsRunning == true)
                {
                    try
                    {
                        MIDIPlaybackTransposeSlider.Enabled = true;
                        MIDIPlaybackSpeedSlider.Enabled = true;
                        currentMidiFile.MidiPlayback.Stop();
                        currentMidiFile.MidiPlayback.MoveToStart();
                        isMidiPlaying = false;
                        playbackTimer.Stop();
                        progressBar1.Value = 0;
                        Log($"Stopped playing {currentMidiFile.MidiName}.");
                    }
                    catch (Exception ex)
                    {
                        Log($"Error while stopping MIDI playback: {ex.Message}");
                    }
                }
            }
        }

        private void OnMidiPlaybackComplete(object sender, EventArgs e)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        StopMidi();
                        playbackTimer.Stop();
                        progressBar1.Value = 0;
                        // unlock the sliders here
                        MIDIPlaybackTransposeSlider.Enabled = true;
                        MIDIPlaybackSpeedSlider.Enabled = true;

                        Log($"MIDI playback for {currentMidiFile.MidiName} completed.");
                    }));
                }
                else
                {
                    // if already on the UI thread, directly execute the code
                    StopMidi();
                    playbackTimer.Stop();
                    progressBar1.Value = 0;
                    // unlock the sliders here
                    MIDIPlaybackTransposeSlider.Enabled = true;
                    MIDIPlaybackSpeedSlider.Enabled = true;

                    Log($"MIDI playback for {currentMidiFile.MidiName} completed.");
                }
            }
            catch (Exception ex)
            {
                Log($"Error on MIDI playback completion: {ex.Message}");
            }
        }

        #endregion

        #region MIDI In

        private void SelectDevice(int id)
        {
            InputDevice newDevice = InputDevice.GetById(id);

            if (currentMidiDevice?.Id == newDevice.Id)
            {
                return;
            }
            else
            {
                if (currentMidiDevice != null)
                {
                    currentMidiDevice.EventReceived -= OnMidiEventReceived;
                    currentMidiDevice.Dispose();
                }
            }

            currentMidiDevice = newDevice;
            currentMidiDevice.EventReceived += OnMidiEventReceived;
            Log($"Selected {currentMidiDevice.Name}.");
        }

        private void StartListening()
        {
            currentMidiDevice.StartEventsListening();
            Log($"Started listening to '{currentMidiDevice.Name}'.");
        }

        private void StopListening()
        {
            currentMidiDevice.StopEventsListening();
            Log($"Stopped listening to '{currentMidiDevice.Name}'.");
        }

        private void OnMidiEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            if (e.Event is NoteOnEvent noteEvent && noteEvent.Velocity > 0)
            {
                // Only process notes on the drum channel if Drum Mode is enabled
                if (isDrumModeEnabled && noteEvent.Channel == (FourBitNumber)9)
                {
                    OnDrumEventReceived(noteEvent.NoteNumber);
                }
                else if (!isDrumModeEnabled)
                {
                    int originalNoteNumber = noteEvent.NoteNumber + midiTransposition;
                    int transposedNoteNumber = originalNoteNumber;
                    // transpose the note if it's out of range and auto transpose is enabled
                    if (isAutoTranspositionEnabled)
                    {
                        transposedNoteNumber = Core.Note.TransposeToPlayableRange(originalNoteNumber);
                    }
                    if (noteLookup.TryGetValue(transposedNoteNumber, out Core.Note note))
                    {
                        note.Play();
                        if (detailedLogging)
                        {
                            string noteRepresentation = note.IsShiftedKey ? $"^{note.NoteCharacter}" : $"{note.NoteCharacter}";
                            if (isAutoTranspositionEnabled && originalNoteNumber != transposedNoteNumber)
                            {
                                Invoke((MethodInvoker)(() =>
                                {
                                    Log($"[AutoTranspose] MIDI number {originalNoteNumber} to {transposedNoteNumber}, character {noteRepresentation}.");
                                }));
                            }
                            else
                            {
                                Invoke((MethodInvoker)(() =>
                                {
                                    Log($"Received MIDI number {transposedNoteNumber}, the note is {noteRepresentation}.");
                                }));
                            }
                        }
                    }
                    else if (!isAutoTranspositionEnabled)
                    {
                        Invoke((MethodInvoker)(() =>
                        {
                            Log($"Piano note out of range: MIDI number {originalNoteNumber} cannot be played in Tower Unite.");
                        }));
                    }
                    // handle other MIDI events as needed
                }
            }
        }


        #endregion

        #region Event Handlers
        private void CheckboxDrums_CheckedChanged(object sender, EventArgs e)
        {
            // if checkbox is checked, enable Drum Mode
            isDrumModeEnabled = checkboxDrums.Checked;

            // disable the transpose slider if Drum Mode is enabled
            MIDIPlaybackTransposeSlider.Enabled = !isDrumModeEnabled;
            OctaveTranspositionSlider.Enabled = !isDrumModeEnabled;

            Log($"Drum Mode toggled. Current state: {isDrumModeEnabled}");
        }

        private void OnHotkeyPress(object sender, HotkeyEventArgs e)
        {
            switch (e.Name)
            {
                case "Start":
                    if (TabControl.SelectedIndex == 0)
                    {
                        if (StartListeningButton.Enabled)
                        {
                            StartListening();
                        }
                    }
                    else
                    {
                        if (MIDIPlayButton.Enabled)
                        {
                            // Restart MIDI playback from the beginning
                            RestartMidi();
                        }
                    }
                    break;
                case "Stop":
                    if (TabControl.SelectedIndex == 0)
                    {
                        if (StopListeningButton.Enabled)
                        {
                            StopListening();
                        }
                    }
                    else
                    {
                        if (MIDIStopButton.Enabled)
                        {
                            StopMidi();
                        }
                    }
                    break;
            }
        }

        private void RestartMidi()
        {
            StopMidi(); // Stop MIDI playback if it's currently running

            if (currentMidiFile != null)
            {
                // Reset the MIDI playback position to the start
                currentMidiFile.MidiPlayback.MoveToStart();

                // Start MIDI playback
                PlayMidi();
            }
        }

        private void DeviceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (InputDevice.GetById(DeviceComboBox.SelectedIndex) == currentMidiDevice)
            {
                return;
            }

            SelectDevice(DeviceComboBox.SelectedIndex);
            StartListeningButton.Enabled = true;
            StopListeningButton.Enabled = true;
        }

        private void StartListeningButton_Click(object sender, EventArgs e)
        {
            StartListening();
        }

        private void StopListeningButton_Click(object sender, EventArgs e)
        {
            StopListening();
        }

        private void MIDIBrowseButton_Click(object sender, EventArgs e)
        {
            string lastUsedDirectory = Properties.Settings.Default.LastUsedDirectory;
            if (string.IsNullOrEmpty(lastUsedDirectory))
            {
                lastUsedDirectory = @"C:\";
            }

            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                FileName = "Select your MIDI file.",
                Filter = "MIDI Files (*.mid;*.midi)|*.mid;*.midi",
                Title = "Open MIDI File",
                InitialDirectory = lastUsedDirectory
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // attempt to load the MIDI file
                    currentMidiFile = new MidiContainer(openFileDialog.SafeFileName, Melanchall.DryWetMidi.Smf.MidiFile.Read(openFileDialog.FileName));
                    MIDIPlayButton.Enabled = true;
                    MIDIStopButton.Enabled = true;
                    Log($"Loaded {openFileDialog.SafeFileName}.");
                }
                catch (Exception ex)
                {
                    // log the error and notify the user
                    Log($"Failed to load {openFileDialog.SafeFileName}: {ex.Message}");
                    MessageBox.Show($"An error occurred while opening the MIDI file: {ex.Message}", "Error Opening MIDI File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // save the directory of the selected MIDI file
                Properties.Settings.Default.LastUsedDirectory = Path.GetDirectoryName(openFileDialog.FileName);
                Properties.Settings.Default.Save();
            }
        }

        private void MIDIPlayButton_Click(object sender, EventArgs e)
        {
            PlayMidi();
        }

        private void MIDIStopButton_Click(object sender, EventArgs e)
        {
            StopMidi();
        }

        private void InputDeviceScanButton_Click(object sender, EventArgs e)
        {
            ScanDevices();
        }

        private void FPSAdjustToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var fpsDialog = new FpsInputDialog())
            {
                var result = fpsDialog.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    // call the method to update key press duration based on confirmed FPS
                    FpsInputDialog.UpdateKeyPressDurationFromFPS(fpsDialog.Fps);
                    Log($"FPS adjusted to {fpsDialog.Fps} with key press duration set to {Core.Note.KeyPressDuration}ms.");
                }
                else
                {
                    // reset the key press duration to default if Cancel was clicked or the dialog was closed
                    Core.Note.KeyPressDuration = MainWindow.KeyDelay;
                    Log("FPS adjustment cancelled or reset. Key press duration set to default.");
                }
            }
        }
        private void AutoTransposeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Core.Note.AutoTransposeEnabled = isAutoTranspositionEnabled;
            isAutoTranspositionEnabled = !isAutoTranspositionEnabled;
            autoTransposeToolStripMenuItem.Checked = isAutoTranspositionEnabled;
            Log($"Auto Transpose toggled. Current state: {isAutoTranspositionEnabled}");
        }

        private void DetailedLoggingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            detailedLogging = menuItem.Checked = !menuItem.Checked;
        }

        private void MIDIPlaybackSpeedSlider_ValueChanged(object sender, EventArgs e)
        {
            midiPlaybackSpeed = MIDIPlaybackSpeedSlider.Value / 10.0;
            ToolTipController.SetToolTip((TrackBar)sender, midiPlaybackSpeed.ToString() + "x");

            if (currentMidiFile != null)
            {
                currentMidiFile.MidiPlayback.Speed = midiPlaybackSpeed;
            }
        }

        private void MIDIPlaybackTransposeSlider_ValueChanged(object sender, EventArgs e)
        {
            if (MIDIPlaybackTransposeSlider.Value > 0)
            {
                ToolTipController.SetToolTip((TrackBar)sender, $"+{MIDIPlaybackTransposeSlider.Value} semitones");
            }
            else
            {
                ToolTipController.SetToolTip((TrackBar)sender, $"{MIDIPlaybackTransposeSlider.Value} semitones");
            }

            midiTransposition = MIDIPlaybackTransposeSlider.Value;
        }

        private void OctaveTranspositionSlider_ValueChanged(object sender, EventArgs e)
        {
            if (OctaveTranspositionSlider.Value > 0)
            {
                ToolTipController.SetToolTip((TrackBar)sender, $"+{OctaveTranspositionSlider.Value} octaves");
            }
            else
            {
                ToolTipController.SetToolTip((TrackBar)sender, $"{OctaveTranspositionSlider.Value} octaves");
            }

            noteLookupOctaveTransposition = 3 + OctaveTranspositionSlider.Value;
            BuildNoteDictionary();
        }
        private void CreditsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var creditsForm = new CreditsForm())
            {
                creditsForm.StartPosition = FormStartPosition.CenterParent;
                creditsForm.ShowDialog(this);
            }
        }

        private void OnDrumEventReceived(int noteNumber)
        {
            if (drumMapping.TryGetValue(noteNumber, out VirtualKeyCode keyCode))
            {
                Core.Note.PlayDrum(keyCode);
                if (detailedLogging)
                {
                    Log($"Drum hit: MIDI number {noteNumber}, key code {keyCode}.");
                }
            }
            else
            {
                Log($"Drum note out of range: MIDI number {noteNumber} is not mapped to a drum key.");
            }
        }
        #endregion
    }
}