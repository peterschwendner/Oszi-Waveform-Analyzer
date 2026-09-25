# Oszi-Waveform-Analyzer
Displays and analyzes analog and digital signals from an oscilloscope.
Executes SCPI commands on an oscillosope. Transfers waveforms over USB, VXI-11 and TCP to the computer.
Has decoders for UART, SPI, I2C, USB bus, CAN bus. Has special features that you find nowhere else.

![OsziWaveformAnalyzer](https://github.com/user-attachments/assets/c058ae70-8507-4213-9f49-14a93fb323d4)


Please read the detailed project description and find the release download here:

https://netcult.ch/elmue/Oszi-Waveform-Analyzer/

## Additions in this fork

This fork of [Elmue/Oszi-Waveform-Analyzer](https://github.com/Elmue/Oszi-Waveform-Analyzer) adds:

### Hameg oscilloscopes over RS232 (COM port)

| Serie | Default port settings | Protocol | Tested |
|---|---|---|---|
| HMO1522, HMO1002, HMO1202, HMO2022 (HMO Compact) | 115200 8N1 RTS | SCPI `:CHANnel:DATA` (floats in Volt), logic pod | Real HMO1522 |
| HM2008, HM1508, HM1008 (CombiScope) | 19200 8N2 RTS | SCPI `:TRACe` (8 bit values) | Simulation only |
| HM507 | 19200 8N2 RTS | Proprietary binary protocol | Simulation only |

- New connection mode **COM** in the Transfer window for RS232 ports, USB to RS232 adapters and the USB virtual COM port of the Hameg HO720 interface.
- Port settings like `19200 8N2 RTS` (baudrate, databits, parity, stopbits, handshake), stored separately for each oscilloscope serie.
- The HM2008 and HM507 support was written from the Hameg programming documentation and tested against a protocol simulation, but not yet with real hardware. Feedback is welcome.

### Analysis windows

Right-click on the analog signal of a channel:

- **FFT Spectrum**: Hann, Hamming, Blackman-Harris, Flat Top and Rectangular windows, dBV or Volt, logarithmic frequency axis, zoom, peak detection with interpolated frequencies, CSV export.
- **X/Y Plot**: two channels as Lissajous figure with phosphor-like intensity display and phase measurement. This replaces the XY mode of the oscilloscope with captures recorded in Yt mode.

### Documentation

The manual [Compiled/Manual.htm](Compiled/Manual.htm) has new chapters *FFT Spectrum*, *X/Y Plot*, *Hameg Oscilloscopes* and *Option 4: COM Port (RS232)*.
The *Show Help* links in the new windows open these chapters.

### Build

```
C:/Windows/Microsoft.NET/Framework64/v4.0.30319/MSBuild.exe SourceCode/OsziWaveformAnalyzer.csproj -p:Configuration=Release
```
(see `build.bat`). The program is written to `Compiled/OsziWaveformAnalyzer.exe`.
