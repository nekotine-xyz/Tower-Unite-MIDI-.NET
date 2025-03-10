﻿using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using NHotkey;
using NHotkey.WindowsForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WindowsInput.Native;

namespace TowerUniteMidiDotNet.Windows
{
    public partial class MainWindow : Form
    {
        public const string Version = "1.2.3";
        public static int KeyDelay = 20;

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
            { 56, VirtualKeyCode.VK_G }, // Cowbell -> Snare Rim
            { 82, VirtualKeyCode.VK_G }, // Shaker -> Snare Rim
			{ 38, VirtualKeyCode.VK_F }, // Acoustic Snare -> Snare
			{ 40, VirtualKeyCode.VK_F }, // Electric Snare -> Snare
            { 39, VirtualKeyCode.VK_B }, // Hand Clap -> Clap
			{ 46, VirtualKeyCode.VK_D }, // Open Hi-hat -> Closed Hit-Hat (OPEN HI HAT SOUNDS AWFUL!)
			{ 42, VirtualKeyCode.VK_D }, // Closed Hi-hat -> Closed Hi-Hat
			{ 44, VirtualKeyCode.VK_D }, // Pedal Hi-Hat -> Closed Hi-Hat
			{ 49, VirtualKeyCode.VK_R }, // Crash Cymbal 1 -> Crash Cymbal 1
			{ 52, VirtualKeyCode.VK_R }, // Chinese Cymbal -> Crash Cymbal 1
			{ 55, VirtualKeyCode.VK_U }, // Splash Cymbal -> Crash Cymbal 2
			{ 57, VirtualKeyCode.VK_U }, // Crash Cymbal 2 -> Crash Cymbal 2
			{ 50, VirtualKeyCode.VK_T }, // High Tom -> High Tom
            { 43, VirtualKeyCode.VK_T }, // High Floor Tom -> High Tom
			{ 48, VirtualKeyCode.VK_Y }, // Mid Tom -> Mid Tom
			{ 47, VirtualKeyCode.VK_Y }, // Low-Mid Tom -> Mid Tom
            { 45, VirtualKeyCode.VK_J }, // Low Tom -> Floor Tom
			{ 41, VirtualKeyCode.VK_J }, // Low Floor Tom -> Floor Tom
			// ... why isn't there a ride cymbal in Tower Unite???
			//{ 51, VirtualKeyCode.VK_U }, // Ride Cymbal -> Crash Cymbal 2
			{ 51, VirtualKeyCode.VK_D }, // Ride Cymbal -> Closed Hi-Hat
			{ 53, VirtualKeyCode.VK_D }, // Ride Bell -> Closed Hi-hat
            { 59, VirtualKeyCode.VK_U }, // Ride Cymbal 2 -> Crash Cymbal 2
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
                Interval = 100
            };
            playbackTimer.Tick += new EventHandler(PlaybackTimer_Tick);
            MIDIPlaybackSpeedSlider.Minimum = 4; // corresponds to 0.2x speed
            MIDIPlaybackSpeedSlider.Maximum = 40; // corresponds to 2x speed
            MIDIPlaybackSpeedSlider.SmallChange = 1; // increment by 0.05x
            MIDIPlaybackSpeedSlider.LargeChange = 2; // larger increment, e.g., when clicking the slider track
            MIDIPlaybackSpeedSlider.Value = 20;
            MIDIPlaybackSpeedSlider.MouseWheel += MIDIPlaybackSpeedSlider_MouseWheel;
            MIDIPlaybackTransposeSlider.Value = 0;
            MIDIPlaybackTransposeSlider.MouseWheel += MIDIPlaybackTransposeSlider_MouseWheel;
            OctaveTranspositionSlider.Value = 0;
            OctaveTranspositionSlider.MouseWheel += OctaveTranspositionSlider_MouseWheel;
            label7.Text = "100%";

            try
            {
                HotkeyManager.Current.AddOrReplace("Start", startKey, OnHotkeyPress);
                HotkeyManager.Current.AddOrReplace("Stop", stopKey, OnHotkeyPress);
            }
            catch (NHotkey.HotkeyAlreadyRegisteredException)
            {
                MessageBox.Show("A hotkey is already in use! Please close programs that might be using F1 or F2 as hotkeys, and restart TUMDN.");
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
            var devices = InputDevice.GetAll();
            if (!devices.Any())
            {
                Log("No MIDI devices found.");
                return;
            }
            foreach (InputDevice device in devices)
            {
                DeviceComboBox.Items.Add(device.Name); // only the device name is added
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
            if (EventListView.InvokeRequired)
            {
                EventListView.Invoke(new Action<string>(Log), logText);
            }
            else
            {
                EventListView.Items.Insert(0, logText);
                EventListView.EnsureVisible(0);
                if (EventListView.Items.Count > 100)
                {
                    EventListView.Items.RemoveAt(EventListView.Items.Count - 1);
                }
            }
        }

        #region MIDI Playback

        private void PlayMidi()
        {
            try
            {
                if (currentMidiFile == null)
                {
                    return; // no MIDI file is loaded
                }

                if (currentMidiFile.MidiPlayback.IsRunning)
                {
                    // pause playback
                    currentMidiFile.MidiPlayback.Stop();
                    isMidiPlaying = false;
                    MIDIPlayButton.Text = "Play";
                    Log("Paused MIDI playback.");
                }
                else
                {
                    if (isMidiPlaying)
                    {
                        // resume playback
                        currentMidiFile.MidiPlayback.Start();
                        MIDIPlayButton.Text = "Pause";
                        playbackTimer.Start(); // Ensure the timer is running
                        Log("Resumed MIDI playback.");
                    }
                    else
                    {
                        // start playback from the beginning
                        StartMidiPlayback();
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Error during MIDI playback: {ex.Message}");
            }
        }

        private void StartMidiPlayback()
        {
            // detach any existing event handlers to avoid multiple subscriptions
            currentMidiFile.MidiPlayback.Finished -= OnMidiPlaybackComplete;
            currentMidiFile.MidiPlayback.NotesPlaybackStarted -= OnMidiPlaybackNoteEventReceived;

            // set up MIDI playback
            var duration = currentMidiFile.MidiFile.GetDuration<MetricTimeSpan>();
            progressBar1.Maximum = (int)duration.TotalMicroseconds;
            progressBar1.Value = 0;

            MIDIPlaybackTransposeSlider.Enabled = false;
            // MIDIPlaybackSpeedSlider.Enabled = false;

            currentMidiFile.MidiPlayback.NotesPlaybackStarted += OnMidiPlaybackNoteEventReceived;
            currentMidiFile.MidiPlayback.Finished += OnMidiPlaybackComplete;
            currentMidiFile.MidiPlayback.Speed = midiPlaybackSpeed;
            currentMidiFile.MidiPlayback.Start();
            isMidiPlaying = true;
            playbackTimer.Start();
            MIDIPlayButton.Text = "Pause";

            Log($"Started playing {currentMidiFile.MidiName}.");
        }

        private void OnMidiPlaybackNoteEventReceived(object sender, NotesEventArgs e)
        {
            foreach (var midiNote in e.Notes)
            {
                // only process notes on the drum channel if Drum Mode is enabled
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
                if (currentMidiFile != null)
                {
                    try
                    {
                        // enable the sliders as playback is stopped
                        MIDIPlaybackTransposeSlider.Enabled = true;
                        MIDIPlaybackSpeedSlider.Enabled = true;

                        // stop the playback and reset to the start
                        currentMidiFile.MidiPlayback.Stop();
                        currentMidiFile.MidiPlayback.MoveToStart();

                        // reset the state and UI elements
                        isMidiPlaying = false;
                        playbackTimer.Stop();
                        progressBar1.Value = 0;
                        MIDIPlayButton.Text = "Play"; // update button label

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

        private void SelectDevice(int index)
        {
            try
            {
                InputDevice newDevice = InputDevice.GetByIndex(index);
                if (currentMidiDevice != null && newDevice.GetHashCode() == currentMidiDevice.GetHashCode())
                {
                    return; // device is already selected
                }
                if (currentMidiDevice != null)
                {
                    currentMidiDevice.EventReceived -= OnMidiEventReceived;
                    currentMidiDevice.Dispose();
                }
                currentMidiDevice = newDevice;
                currentMidiDevice.EventReceived += OnMidiEventReceived;
                Log($"Selected {currentMidiDevice.Name}.");
            }
            catch (Exception ex)
            {
                Log($"Error selecting device: {ex.Message}");
            }
        }

        private void AttemptReconnectToDevice()
        {
            if (currentMidiDevice != null)
            {
                string deviceName = currentMidiDevice.Name;
                currentMidiDevice.Dispose();

                try
                {
                    currentMidiDevice = InputDevice.GetByName(deviceName);
                    currentMidiDevice.EventReceived += OnMidiEventReceived;
                    currentMidiDevice.StartEventsListening();
                    Log($"Reconnected to {currentMidiDevice.Name}.");
                }
                catch (Exception ex)
                {
                    Log($"Error reconnecting to device: {ex.Message}");
                }
            }
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
            try
            {
                if (e.Event is NoteOnEvent noteEvent && noteEvent.Velocity > 0)
                {
                    // only process notes on the drum channel if Drum Mode is enabled
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
            catch (Exception ex)
            {
                Log($"Error processing MIDI event: {ex.Message}");
                AttemptReconnectToDevice();
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
                        if (!currentMidiFile?.MidiPlayback.IsRunning ?? false)
                        {
                            // if the MIDI playback is not running, either start or resume playback
                            PlayMidi();
                        }
                        else
                        {
                            // if the MIDI playback is running, pause it
                            currentMidiFile.MidiPlayback.Stop();
                            isMidiPlaying = false;
                            MIDIPlayButton.Text = "Play";
                            Log("Paused MIDI playback.");
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

        private void DeviceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = DeviceComboBox.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= InputDevice.GetDevicesCount())
            {
                return;
            }
            SelectDevice(selectedIndex);
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
                    currentMidiFile = new MidiContainer(openFileDialog.SafeFileName, MidiFile.Read(openFileDialog.FileName));
                    MIDIPlayButton.Enabled = true;
                    MIDIStopButton.Enabled = true;
                    Log($"Loaded {openFileDialog.SafeFileName}.");
                    LogInitialTempo(currentMidiFile.MidiFile);
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

        private void LogInitialTempo(MidiFile midiFile)
        {
            foreach (var trackChunk in midiFile.GetTrackChunks())
            {
                var setTempoEvent = trackChunk.Events.OfType<SetTempoEvent>().FirstOrDefault();
                if (setTempoEvent != null)
                {
                    double bpm = 60000000 / setTempoEvent.MicrosecondsPerQuarterNote;
                    Log($"Initial Tempo: {bpm:F2} BPM");
                    break;
                }
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
                    int newKeyPressDuration = FpsInputDialog.UpdateKeyPressDurationFromFPS(fpsDialog.Fps);
                    Log($"FPS adjusted to {fpsDialog.Fps} with key press duration set to {newKeyPressDuration}ms.");
                }
                else
                {
                    Core.Note.KeyPressDuration = MainWindow.KeyDelay;
                    Log($"FPS adjustment reset. Key press duration set to default {Core.Note.KeyPressDuration}ms.");
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
            midiPlaybackSpeed = MIDIPlaybackSpeedSlider.Value * 0.05;
            label7.Text = $"{(midiPlaybackSpeed * 100):F0}%"; // display speed as percentage

            if (currentMidiFile != null)
            {
                currentMidiFile.MidiPlayback.Speed = midiPlaybackSpeed;
            }
        }

        private void MIDIPlaybackSpeedSlider_MouseWheel(object sender, MouseEventArgs e)
        {
            midiPlaybackSpeed = Math.Max(0.05, MIDIPlaybackSpeedSlider.Value / 20.0); // setup for a replacement of the sliders to make room for other UI elements

            if (e.Delta > 0) // scrolling up
            {
                MIDIPlaybackSpeedSlider.Value = Math.Min(MIDIPlaybackSpeedSlider.Maximum, MIDIPlaybackSpeedSlider.Value + 1);
            }
            else if (e.Delta < 0) // scrolling down
            {
                MIDIPlaybackSpeedSlider.Value = Math.Max(MIDIPlaybackSpeedSlider.Minimum, MIDIPlaybackSpeedSlider.Value - 1);
            }

            // prevents focus shift to the slider on scroll
            ((HandledMouseEventArgs)e).Handled = true;
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

        private void MIDIPlaybackTransposeSlider_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0) // scrolling up
            {
                MIDIPlaybackTransposeSlider.Value = Math.Min(MIDIPlaybackTransposeSlider.Maximum, MIDIPlaybackTransposeSlider.Value + 1);
            }
            else if (e.Delta < 0) // scrolling down
            {
                MIDIPlaybackTransposeSlider.Value = Math.Max(MIDIPlaybackTransposeSlider.Minimum, MIDIPlaybackTransposeSlider.Value - 1);
            }

            // update the transposition based on the slider's value
            midiTransposition = MIDIPlaybackTransposeSlider.Value;

            // prevents focus shift to the slider on scroll
            ((HandledMouseEventArgs)e).Handled = true;
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

        private void OctaveTranspositionSlider_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0) // scrolling up
            {
                OctaveTranspositionSlider.Value = Math.Min(OctaveTranspositionSlider.Maximum, OctaveTranspositionSlider.Value + 1);
            }
            else if (e.Delta < 0) // scrolling down
            {
                OctaveTranspositionSlider.Value = Math.Max(OctaveTranspositionSlider.Minimum, OctaveTranspositionSlider.Value - 1);
            }

            // prevents focus shift to the slider on scroll
            ((HandledMouseEventArgs)e).Handled = true;
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