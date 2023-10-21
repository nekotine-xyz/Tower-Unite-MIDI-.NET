using WindowsInput;
using WindowsInput.Native;
using System.Windows.Input;
using TowerUniteMidiDotNet.Windows;

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

        // Shared InputSimulator instance across all Note instances
        private static readonly InputSimulator inputSim = new InputSimulator();

        // Shared KeyConverter instance for efficient key conversion
        private static readonly KeyConverter converter = new KeyConverter();

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

        public void Play()
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

    }
}
