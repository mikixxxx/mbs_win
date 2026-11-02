
# MBS — portace na Windows 11

Přepracovaná verze původního terminálového hudebního přehrávače.
Zachovává **Terminal.Gui 1.7.2**, **ATL 4.9.0**, **MP3/OGG/WAV**, souborový prohlížeč,
playlist, shuffle/repeat a třípásmový ekvalizér.

## Co se změnilo

- Odstraněna vazba na FMOD a chybějící FMOD C# / nativní knihovny.
- Zvuk přehrává **NAudio 2.2.1**, OGG dekóduje **NAudio.Vorbis 1.5.0**.
- Cíl sestavení **.NET 10 pro Windows x64** (`net10.0-windows`).
- EQ používá tři biquad filtry (bass shelf, mid peak, treble shelf); 50 % je 0 dB.
- Prohlížeč souborů umožňuje přechod na seznam disků z kořene jednotky.
- Přidání větší složky aktualizuje playlist najednou, ne po každém souboru.

## Jak sestavit (Windows 11)

1. Nainstaluj **.NET 10 SDK (x64)**: https://dotnet.microsoft.com/download/dotnet/10.0
2. Archiv rozbal do samostatné složky a spusť **Windows Terminal / PowerShell** v této složce.
3. Spusť `build-win11.cmd`, případně ručně:

   ```powershell
   dotnet restore .\mbs.csproj
   dotnet run --project .\mbs.csproj
   ```

4. Pro samostatné EXE včetně .NET runtime:

   ```powershell
   dotnet publish .\mbs.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -o .\dist\win-x64
   ```

5. Výsledek je `dist\win-x64\mbs.exe`. Pokud vzniknou i pomocné soubory, ponech je vedle EXE.

Při prvním sestavení je třeba internet pro obnovení balíčků z NuGetu. Pro spuštění
použij **Windows Terminal**, ideálně ve větším okně (cca 120×32 znaků).

## Klávesy

| Klávesa | Funkce |
| --- | --- |
| Enter | Otevřít složku / přidat vybraný audio soubor |
| Space v prohlížeči | Přidat soubor / všechny podporované skladby ze složky |
| Space v playlistu | Přehrát vybranou skladbu |
| D v playlistu | Odebrat skladbu |
| N | Další skladba |
| P | Pauza / pokračovat |
| S | Stop |
| C / V | Posun -10 / +10 sekund |
| U / J | Hlasitost + / - |
| R / F | Basy + / - |
| T / G | Středy + / - |
| Z / H | Výšky + / - |

Zaškrtávací políčka **Shuffle / Repeat** upravují způsob pokračování playlistu.


## Původní zdroje

Původní MBS pro linux.


AI generated...