# Oszi-Waveform-Analyzer
Displays and analyzes analog and digital signals from an oscilloscope.
Executes SCPI commands on an oscillosope. Transfers waveforms over USB, VXI-11 and TCP to the computer.
Has decoders for UART, SPI, I2C, USB bus, CAN bus. Has special features that you find nowhere else.

![OsziWaveformAnalyzer](https://github.com/user-attachments/assets/c058ae70-8507-4213-9f49-14a93fb323d4)

Please read the detailed project description and find the release download here:

https://netcult.ch/elmue/Oszi-Waveform-Analyzer/

## Copyright and licensing

This repository is a modified version of [Elmue/Oszi-Waveform-Analyzer](https://github.com/Elmue/Oszi-Waveform-Analyzer).

The original project is distributed under the GNU General Public License version 3. This fork contains modifications and new functionality by Peter Schwendner made in 2026; see the Git history for details.

The GPL applies to software and other material distributed under that license. Product names, trademarks, external standards, manufacturer documentation, drivers and other third-party material remain the property of their respective owners. Their mention is solely for identification and interoperability purposes and does not imply endorsement.

See [LICENSE](LICENSE), [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md), and [Documentation/Standards.md](Documentation/Standards.md).

## Additions in this fork

This fork of [Elmue/Oszi-Waveform-Analyzer](https://github.com/Elmue/Oszi-Waveform-Analyzer) adds:

### Hameg oscilloscopes over RS232 (COM port)

| Serie | Default port settings | Protocol | Tested |
|---|---|---|---|
| HMO1522, HMO1002, HMO1202, HMO2022 (HMO Compact) | 115200 8N1 RTS | SCPI `:CHANnel:DATA` (floats in Volt), logic pod | Real HMO1522 |
| HM2008, HM1508, HM1008 (CombiScope) | 115200 8N2 RTS | SCPI `:TRACe` (8 bit values) | Simulation only |
| HM507 | 115200 8N2 RTS | Proprietary binary protocol | Real HM507 |

- New connection mode **COM** in the Transfer window for RS232 ports, USB to RS232 adapters and the USB virtual COM port of the Hameg HO720 interface.
- Port settings like `19200 8N2 RTS` (baudrate, databits, parity, stopbits, handshake), stored separately for each oscilloscope serie.
- HMO1522 and HM507 are tested with real oscilloscopes (transfer over RS232, then X/Y and FFT).
- The HM2008 support was written from the Hameg SCPI programming manual and tested against a protocol simulation, but not yet with real hardware. Feedback is welcome.

### Analysis windows

Right-click on the analog signal of a channel:

- **FFT Spectrum**: Hann, Hamming, Blackman-Harris, Flat Top and Rectangular windows, dBV or Volt, logarithmic frequency axis, zoom, peak detection with interpolated frequencies, CSV export.
- **X/Y Plot**: two channels as Lissajous figure with phosphor-like intensity display and phase measurement. This replaces the XY mode of the oscilloscope with captures recorded in Yt mode.
- Demo file `Compiled/Samples/X-Y Plot Square 1 kHz.oszi`: draws a square in the X/Y Plot, and its trapezoid waves show odd harmonics with 1/n² in the FFT.

### WAV import

Uncompressed WAV files (PCM 8/16/24/32 bit, float 32/64 bit) can be imported with the new button **Open...** from any folder,
or from `Compiled/Samples` (WAV files there are ignored by git, because test files like oscilloscope music are copyrighted).
Each audio channel becomes an analog channel (full scale = 1.0). This is useful to test the FFT and to display
oscilloscope music (stereo: Left = X, Right = Y) with the X/Y Plot.

### Open... and Save as...

The buttons **Open...** and **Save as...** open and save files in any folder, so own captures can be kept outside of the program folder.
The folder then becomes the folder of the list *Input File* and of the button *Save* (its tooltip shows the folder).

### Documentation

The manual [Compiled/Manual.htm](Compiled/Manual.htm) has new chapters *FFT Spectrum*, *X/Y Plot*, *Hameg Oscilloscopes*, *Option 4: COM Port (RS232)* and *WAV Files*.
The *Show Help* links in the new windows open these chapters.

Copyrighted standards and vendor manuals are not mirrored in this repository. See [Documentation/Standards.md](Documentation/Standards.md) for references.

### Build

Run `build.bat` (double-click or from any folder). It uses the MSBuild of the .NET Framework 4.x
that is part of every Windows installation, so Visual Studio is not required.
The program is written to `Compiled/OsziWaveformAnalyzer.exe`.

The warnings MSB3644 and MSB3270 are harmless. MSB3644 disappears if the .NET Framework 4.x Developer Pack is installed.
