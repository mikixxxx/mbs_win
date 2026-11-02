AI generated.

# MBS — Terminal Music Player

MBS is a lightweight C# music player with a terminal-based interface, originally built for Linux and ported to **Windows 11**.

## Features

- Browse local folders and add individual tracks or entire folders to a playlist.
- Play **MP3**, **OGG**, and **WAV** files.
- Shuffle and repeat the playlist.
- Three-band equalizer (bass, mid, treble), volume control, and seeking.
- Keyboard-driven interface powered by Terminal.Gui.

## Requirements

- Windows 11 (x64) and a terminal such as Windows Terminal.
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) to build from source.

## Build and run

From the project root:

```powershell
dotnet restore .\mbs.csproj
dotnet run --project .\mbs.csproj
```

To publish a self-contained Windows x64 executable, run `build-win11.cmd`. The output will be in `dist\win-x64\` (including `mbs.exe`). An internet connection is needed to restore NuGet packages on the first build.

## Keyboard shortcuts

| Key | Action |
| --- | --- |
| Enter | Open a folder or add the selected audio file |
| Space | Add a file/folder in the browser; play a track in the playlist |
| D | Remove the selected playlist track |
| N / P / S | Next track / pause-resume / stop |
| C / V | Seek backward / forward by 10 seconds |
| U / J | Volume up / down |
| R / F | Bass up / down |
| T / G | Mid up / down |
| Z / H | Treble up / down |
| Esc | Quit |

Use the **Shuffle** and **Repeat** checkboxes to control playlist playback. Repeat loops the entire playlist, not a single track.

## Tech stack

.NET 10, Terminal.Gui, NAudio, NAudio.Vorbis, and ATL (`z440.atl.core`). This Windows port replaces the original FMOD audio backend with NAudio.
