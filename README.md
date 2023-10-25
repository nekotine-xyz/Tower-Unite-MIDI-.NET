# Tower Unite MIDI .NET 1.2

## What is Tower Unite MIDI .NET?

Tower Unite MIDI .NET, originally developed by Bailey Eaton (Yoshify), serves as a gateway for MIDI-enabled devices to passthrough to the game Tower Unite, allowing players to play the in-game piano using real-life MIDI devices. My update has introduced key optimizations and improvements to augment its performance and stability:

1. Improved Stability: Addressed and fixed prevalent crash issues. By wrapping certain functionalities in try/catch blocks, introducing strategic logging, and enhancing playback completion handling, the application's overall resilience has been significantly enhanced. Fixed a significant crash upon finishing playback of a MIDI file.
2. Latency Optimization: Recognizing the challenges posed by latency, particularly in connections prone to lag, I've augmented key delay functionality. This results in a reduced number of missed notes due to latency.
3. Code Refinements: Modifications include cleaner code practices and enhanced comments, aiming for a more readable and maintainable codebase.
4. Quality of Life: Added a feature for the program to remember the last directory from which the user has played a MIDI file.

With these improvements, Tower Unite MIDI .NET 1.2 offers players a smoother, stable, more reliable experience.

## How do I use it? 

1. Obtain the latest release from the "Releases" page.
2. Extract the archive into its own folder.
3. Run the program.
4. You'll notice 2 tabs in the program, **MIDI Device Setup** and **MIDI Playback**. **MIDI Device Setup** is your go-to tab for setting up and using your MIDI devices, where as **MIDI Playback** is the tab you'll need for playing back MIDI files.
5. **MIDI Device Setup**

   5A. To setup your MIDI device, please select it from the 'Input Devices' dropdown list. If it isn't showing up, please press the 'Scan for devices' button.
   
   5B. Once you have your MIDI device selected, press 'Start Listening' to start recieving input from your device. Alternatively, you can 
open your Tower Unite window and hit the **F1** key.
   
   5C. To stop recieving input from your MIDI device, press the 'Stop Listening' button, or alternatively press the **F2** key.
   
   5D. By default, the note's are transposed so that middle C (C4) on your device aligns with Tower Unite. If you're unhappy with this transposition, and your device doesn't natively support transposing, you can use the 'Octave Transposition' slider to customise it to your liking.

6. **MIDI Playback**

   6A. First, browse for your file using the 'Browse' button.
   
   6B. If you wish to transpose or modify the playback speed of your file, you can do so using the provided sliders. Note that you cannot modify this while the file is playing.
   
   6C. Open your Tower Unite window and press the **F1** key to begin file playback.
   
   6D. If you wish to stop playing the file early, hit the **F2** key.
   
7. **Options**

   7A. Ping Input
      
      On laggier connections, you'll notice some dud black keys. This is because Tower Unite doesn't have enough time to register the fact that the program has hit the shift key. To combat this, the program has a miniscule delay (15ms) built in to key presses, but sometimes that's not enough. If you're having problems, use this to input your Tower Unite ping and the program will do its best to assign a more appropriate key delay. Please note that this will add latency between your key presses vs. when you hear them.
      
   7B. Detailed Logging
   
      Enables the logging of events such as MIDI key presses.

## Known Issues
1. Sometimes, you may be required to spam the "stop" key or button when playing a MIDI, though I've rarely encountered this and cannot reliably replicate it.
2. Currently only supports a QWERTY keyboard layout.

## Reporting an issue
Please either raise an issue here on Github, message me on Twitter (@xelapilled) or Discord (xelapilled), or [email me.](xela@xela.contact)

## Upcoming
- Sustain pedal functionality
- Drag and Drop MIDI files
- Drum Mode
- Pause and play
- Improved black key recognition
- Adjustable speed during MIDI playback
- Forward/backward functionality

## License
MIT License

Copyright (c) 2019 Bailey Eaton

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
