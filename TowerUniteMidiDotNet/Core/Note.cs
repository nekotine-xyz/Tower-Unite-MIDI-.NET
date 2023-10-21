using WindowsInput;
using WindowsInput.Native;
using System.Windows.Input;
using TowerUniteMidiDotNet.Windows;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

namespace TowerUniteMidiDotNet.Core
{
    /// <summary>
    /// The class that contains a TowerUnite note's information and playback logic.
    /// </summary>
    public class Note
    {
        public readonly char NoteCharacter;
        public readonly VirtualKeyCode KeyCode;
        public readonly bool IsShiftedKey;

        private static readonly InputSimulator inputSim = new InputSimulator();
        private static readonly KeyConverter converter = new KeyConverter();

        private static ConcurrentQueue<Note> NoteQueue = new ConcurrentQueue<Note>();
        private static CancellationTokenSource cts = new CancellationTokenSource();
        private static Task ProcessingTask = Task.Run(() => ProcessQueue(), cts.Token);

        public static void AddToQueue(Note note)
        {
            NoteQueue.Enqueue(note);
        }

        private static async Task ProcessQueue()
        {
            while (!cts.Token.IsCancellationRequested)
            {
                if (NoteQueue.TryDequeue(out Note note))
                {
                    await note.PlayInternal();
                }
                else
                {
                    await Task.Delay(10); // small delay to prevent a busy loop
                }
            }
        }

        private async Task PlayInternal()
        {
            if (IsShiftedKey)
            {
                inputSim.Keyboard.KeyDown(VirtualKeyCode.LSHIFT);
                inputSim.Keyboard.Sleep(MainWindow.KeyDelay);
                inputSim.Keyboard.KeyDown(KeyCode);
                inputSim.Keyboard.Sleep(MainWindow.KeyDelay);
                inputSim.Keyboard.KeyUp(KeyCode);
                inputSim.Keyboard.KeyUp(VirtualKeyCode.LSHIFT);
            }
            else
            {
                inputSim.Keyboard.KeyDown(KeyCode);
                inputSim.Keyboard.Sleep(MainWindow.KeyDelay);
                inputSim.Keyboard.KeyUp(KeyCode);
            }
        }

        public void Play()
        {
            AddToQueue(this);
        }

        /// <summary>
        /// Creates a new Note object.
        /// </summary>
        /// <param name="noteCharacter">The corresponding Tower Unite note character.</param>
        /// <param name="isShifted">Whether the shift key is required for this note or not.</param>
        public Note(char noteCharacter, bool isShifted = false)
        {
            NoteCharacter = noteCharacter;
            IsShiftedKey = isShifted;

            try
            {
                // converting the character into a VirtualKeyCode
                Key key = (Key)converter.ConvertFromString(noteCharacter.ToString());
                KeyCode = (VirtualKeyCode)KeyInterop.VirtualKeyFromKey(key);
            }
            catch
            {
                // handle or log the conversion error as necessary
                // wip
            }
        }
    }
}
