# Tower Unite MIDI .NET 1.2

## Introduction 🎹

Tower Unite MIDI .NET (TUMDN), originally developed by Bailey Eaton (Yoshify), is a program that acts as a gateway for MIDI devices to passthrough to Tower Unite'sinstruments. It allows players to play in-game instruments using real-life MIDI devices, or to play MIDI files. My updates have introduced new features, optimizations, and multiple improvements since Yoshify's release in 2019, and will continue to be in active development for the forseeable future.

Some parts of TUMDN have been completely rewritten, entirely new features have been written from scratch, and numerous bugs/crashes have been fixed. All in an effort to provide a fuller experience based on the foundation of Yoshify's original work. Currently, this is the best program for playing MIDI-enabled devices 

## Installation Guide

1. Obtain the latest release from the "Releases" page.
2. Extract the contents of TUMDN.7z to its own folder, wherever you like.
3. Run TowerUniteMidiDotNet.exe

The included .dll files must be present in the same folder as the .exe, or else TUMDN will refuse to start.

## Overview

You'll notice 2 tabs in the program, **MIDI Device Setup** and **MIDI Playback**.

**MIDI Device Setup** is your go-to tab for setting up and using your MIDI devices, where as **MIDI Playback** is the tab you'll need for playing back MIDI files.

1. **MIDI Device Setup**

   1.1. To setup your MIDI device, press the 'Scan for devices' button, then select your device from the 'Input Devices' dropdown list. If it isn't showing up, please report this to me.
   
   1.2. Once you have selected your MIDI device, press 'Start Listening' (or **F1**) and TUMDN will start receiving inputs from your device.
   
   1.3. To stop receiving input from your MIDI device, press 'Stop Listening' (or **F2**).
   
   By default, notes are transposed so that middle C (C4) on your device aligns with Tower Unite. If you're unhappy with this transposition, and your device doesn't natively support transposing, you can use the 'Octave Transposition' slider to customise it to your liking.

2. **MIDI Playback**

   2.1. Click 'Browse' then navigate to the file you wish to play.
   
   2.2. Open your Tower Unite window and press the **F1** key to begin playing the MIDI file.
   
   2.3. To stop MIDI playback early, press the **F2** key.

   TUMDN will remember the last directory you selected, so it would be beneficial to have a folder of all the MIDIs you wish to use. If you wish to transpose or modify the playback speed of your file, use the respective sliders for each. You cannot do this while a file is playing.

3. **Drum Mode**

   TUMDN starts in 'Piano Mode' by default, if you wish to play in 'Drum Mode' then click the checkbox to the bottom right of the program.

   Drum Mode checks for events on MIDI channel 10, which is the standard channel for percussion in General MIDI specification. While in Piano Mode, notes on the percussion channel will be ignored. Conversely, while in Drum Mode, any channel that ISN'T the percussion channel will be ignored.

   If you are using a MIDI device while in Drum Mode, make sure you configure your device to output to this channel or TUMDN will ignore your inputs.

   If you are using MIDI playback and nothing is happening, it's likely your MIDI file does not have percussion located in MIDI channel 10. You'll have to manually fix this, or use a different MIDI file.

   If you are still having trouble, please contact me.

4. **Options**

   4.1. **FPS Input Tool**
      
      FPS Adjust Tool is a work-in-progress feature meant for use if Tower Unite is running below 60 FPS and is therefore having its notes frequently missed.

      TUMDN works best when Tower Unite is above 60 FPS. This is because it, like all games, reads user inputs in real-time. The more below 60 FPS it runs, progressively the game will start to not read inputs, as TUMDN presses and releases keys too fast.

      Introducing the FPS Adjust Tool, simply input your FPS and TUMDN will calculate (approximately) the right amount of time how long keys should be held. This feature is still VERY unfinished and not perfect, as songs begin to sound more distorted the lower you set your FPS.
      
   4.2. **Auto Transpose**
   
      If enabled, notes that are played outside Tower Unite's range will automatically be transposed to the nearest octave within playable range. Disabled by default.

   4.3. **Detailed Logging**

      Enables the logging of events such as MIDI key presses, gives you generally more information of what is going on under the hood. Mainly used for debugging.

## Known Issues

1. Currently only supports a QWERTY keyboard layout.
2. Sometimes users will have to spam the F2 hotkey for the program to stop MIDI playback. I can't reliably reproduce this issue but I have experienced it, still working on exactly how to fix it.

## Contact Me

Please either raise an issue here on Github, message me on Twitter (@xelapilled) or Discord (xelapilled), or [email me.](xela@xela.contact)

## Upcoming

- Visual indicator of MIDI playback length
- Sustain pedal functionality
- Drag and Drop MIDI files
- Pause and play
- Adjustable speed during MIDI playback
- Forward/backward functionality
- Queue multiple MIDI files to play them in succession
- A way to select specific instruments/channels from a MIDI file to play independently

## Support Me

If you're willing and able, and would like to support TUMDN's development, consider supporting me.

https://ko-fi.com/xelapilled

## Credits

Originally developed by Bailey Eaton, A.K.A Yoshify

Updated and maintained by Nadya, A.K.A Xela

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
