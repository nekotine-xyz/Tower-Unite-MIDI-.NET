using System;
using System.Windows.Forms;
using System.Collections.Generic;
using NHotkey;
using NHotkey.WindowsForms;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Devices;
using TowerUniteMidiDotNet.Core;
using TowerUniteMidiDotNet.Util;
using System.IO;

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
		private Dictionary<int, Note> noteLookup;
		private Keys startKey = Keys.F1;
		private Keys stopKey = Keys.F2;

		//It's called this because I plan on adding AZERTY support. Eventually...
		private List<char> qwertyLookup = new List<char>()
		{
			'1','!','2','@','3','4','$','5','%','6','^','7',
			'8','*','9','(','0','q','Q','w','W','e','E','r',
			't','T','y','Y','u','i','I','o','O','p','P','a',
			's','S','d','D','f','g','G','h','H','j','J','k',
			'l','L','z','Z','x','c','C','v','V','b','B','n',
			'm'
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
			InitializeComponent();

			ScanDevices();
			BuildNoteDictionary();

			MIDIPlaybackSpeedSlider.Value = 10;
			MIDIPlaybackTransposeSlider.Value = 0;
			OctaveTranspositionSlider.Value = 0;

			try
			{
				HotkeyManager.Current.AddOrReplace("Start", startKey, OnHotkeyPress);
				HotkeyManager.Current.AddOrReplace("Stop", stopKey, OnHotkeyPress);
			}
			catch (NHotkey.HotkeyAlreadyRegisteredException ex)
			{
				// strange error I got once and I can't tell if it will happen again -x
				MessageBox.Show("A hotkey is already in use! Please report this to @xelapilled on Twitter.");
			}

			Text += " " + Version;
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
			noteLookup = new Dictionary<int, Note>();

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
						noteLookup.Add((row + noteLookupOctaveTransposition) * 12 + column, new Note(qwertyLookup[(row * 12 + column) - 1], true));
					}
					else
					{
						noteLookup.Add((row + noteLookupOctaveTransposition) * 12 + column, new Note(qwertyLookup[row * 12 + column]));
					}
				}
			}

			Log($"Note dictionary built. Middle C is C{noteLookupOctaveTransposition + 1}.");
		}

		/// <summary>
		/// Pushes <paramref name="logText"/> to the EventListView log. If there are more than 100 items in the log, the log will start being culled.
		/// </summary>
		/// <param name="logText">The text to push to the log.</param>
		/*
		private void Log(string logText)
		{
			// if there are more than 99 items in the EventListView, remove the oldest one
			// this is done to limit the number of logs displayed in the EventListView
			if (EventListView.Items.Count > 99)
			{
				EventListView.Items.RemoveAt(0);
			}

		    // directly adding log text to the EventListView control
		    // this can cause a crash if this method is called from a thread other than the one 
		    // which created the EventListView, leading to a cross-thread operation error.
				EventListView.Items.Add(logText);

			// ensuring the added log item is visible in the EventListView control
			// again, this direct manipulation can cause a cross-thread operation error if
			// the method is called from a thread other than the main UI thread.
		   EventListView.Items[EventListView.Items.Count - 1].EnsureVisible();
		}
		*/
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
				// if the current method call is already on the UI thread, directly add the logText to the EventListView control
				EventListView.Items.Add(logText);
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
				currentMidiFile.MidiPlayback.NotesPlaybackStarted -= OnMidiPlaybackNoteEventReceived;
			}

			MIDIPlaybackTransposeSlider.Enabled = false;
			MIDIPlaybackSpeedSlider.Enabled = false;

			currentMidiFile.MidiPlayback.NotesPlaybackStarted += OnMidiPlaybackNoteEventReceived;
			currentMidiFile.MidiPlayback.Finished += OnMidiPlaybackComplete;
			currentMidiFile.MidiPlayback.Speed = midiPlaybackSpeed;
			currentMidiFile.MidiPlayback.Start();
			Log($"Started playing {currentMidiFile.MidiName}.");
		}

		private void OnMidiPlaybackNoteEventReceived(object sender, NotesEventArgs e)
		{
			foreach (Melanchall.DryWetMidi.Smf.Interaction.Note midiNote in e.Notes)
			{
				if (noteLookup.TryGetValue(midiNote.NoteNumber + midiTransposition, out Note note))
				{
					note.Play();
					if (detailedLogging)
					{
						Invoke((MethodInvoker)(() =>
						{
							Log($"Recieved MIDI number {midiNote.NoteNumber}, the note is {(note.IsShiftedKey ? "^" : string.Empty)}{note.NoteCharacter}.");
						}));
					}
				}
			}
		}

		private void StopMidi()
		{
			if (currentMidiFile?.MidiPlayback.IsRunning == true)
			{
				MIDIPlaybackTransposeSlider.Enabled = true;
				MIDIPlaybackSpeedSlider.Enabled = true;
				currentMidiFile.MidiPlayback.Stop();
				currentMidiFile.MidiPlayback.MoveToStart();
				Log($"Stopped playing {currentMidiFile.MidiName}.");
			}
		}

		/*
		private void OnMidiPlaybackComplete(object sender, EventArgs e)
		{
		    // this line disposes of the MIDI output device when the playback completes.
		    // while this releases any resources associated with the output device, it does
		    // not actually stop the MIDI playback or reset its state. thus, playback continues
		    // until manually stopped or until the program crashes due to trying to access a 
		    // disposed object in subsequent operations.
		    currentMidiFile.MidiPlayback.OutputDevice.Dispose();
		}
		*/

		private void OnMidiPlaybackComplete(object sender, EventArgs e)
		{
			try
			{
				// instead of disposing immediately, just log for now.
				Log($"MIDI playback for {currentMidiFile.MidiName} completed.");

				// stop the MIDI playback using the existing method
				StopMidi();
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
			if (e.Event is NoteOnEvent)
			{
				NoteOnEvent evt = e.Event as NoteOnEvent;

				if (evt.Velocity == 0)
				{
					return;
				}

				if (noteLookup.TryGetValue(evt.NoteNumber, out Note note))
				{
					note.Play();
					if (detailedLogging)
					{
						Invoke((MethodInvoker)(() =>
						{
							Log($"Recieved MIDI number {evt.NoteNumber}, the note is {(note.IsShiftedKey ? "^" : string.Empty)}{note.NoteCharacter}.");
						}));
					}
				}
			}
		}

		#endregion

		#region Event Handlers

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
							PlayMidi();
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

		/*
		private void MIDIBrowseButton_Click(object sender, EventArgs e)
		{
		    OpenFileDialog openFileDialog = new OpenFileDialog()
		    {
		        FileName = "Select your MIDI file.",
        
		        Filter = "MIDI Files (*.mid;*.midi)|*.mid;*.midi",
        
		        Title = "Open MIDI File",
        
		        // every time the OpenFileDialog is opened, it starts in the C:\ directory.
		        // this does not remember the user's last location, which is inconvenient
		        // if the user often navigates to another directory to select MIDI files.
		        InitialDirectory = @"C:\"
		    };

		    {
		        currentMidiFile = new MidiContainer(openFileDialog.SafeFileName, Melanchall.DryWetMidi.Smf.MidiFile.Read(openFileDialog.FileName));
        
		        MIDIPlayButton.Enabled = true;
		        MIDIStopButton.Enabled = true;
        
		        Log($"Loaded {openFileDialog.SafeFileName}.");
		    }
		}
		*/

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
				// save the directory of the selected MIDI file
				Properties.Settings.Default.LastUsedDirectory = Path.GetDirectoryName(openFileDialog.FileName);
				Properties.Settings.Default.Save();

				currentMidiFile = new MidiContainer(openFileDialog.SafeFileName, Melanchall.DryWetMidi.Smf.MidiFile.Read(openFileDialog.FileName));
				MIDIPlayButton.Enabled = true;
				MIDIStopButton.Enabled = true;
				Log($"Loaded {openFileDialog.SafeFileName}.");
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

		private void InputPingToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (Prompt.ShowIntDialog("By inputting your ping, the program can work out an appropriate amount of delay to reduce the chance of a missed black key. Note that this will add latency to your keys.", "Input your ping", out int result))
			{
				if (result == 0)
				{
					return;
				}

				KeyDelay = result / 6;
				KeyDelay = (KeyDelay < 8) ? 8 : KeyDelay;
				Log($"Your ping is {result}, your calculated delay is {KeyDelay}ms.");
			}
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
				ToolTipController.SetToolTip((TrackBar)sender, $"+{MIDIPlaybackTransposeSlider.Value.ToString()} semitones");
			}
			else
			{
				ToolTipController.SetToolTip((TrackBar)sender, $"{MIDIPlaybackTransposeSlider.Value.ToString()} semitones");
			}

			midiTransposition = MIDIPlaybackTransposeSlider.Value;
		}

		private void OctaveTranspositionSlider_ValueChanged(object sender, EventArgs e)
		{
			if (OctaveTranspositionSlider.Value > 0)
			{
				ToolTipController.SetToolTip((TrackBar)sender, $"+{OctaveTranspositionSlider.Value.ToString()} octaves");
			}
			else
			{
				ToolTipController.SetToolTip((TrackBar)sender, $"{OctaveTranspositionSlider.Value.ToString()} octaves");
			}

			noteLookupOctaveTransposition = 3 + OctaveTranspositionSlider.Value;
			BuildNoteDictionary();
		}
		private void creditsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var creditsForm = new CreditsForm())
			{
				creditsForm.StartPosition = FormStartPosition.CenterParent;
				creditsForm.ShowDialog(this);
			}
		}

        #endregion
    }
}