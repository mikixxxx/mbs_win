@echo off
setlocal
cd /d "%~dp0"
where dotnet >nul 2>nul
if errorlevel 1 (
  echo ERROR: .NET 10 SDK is not installed or not on PATH.
  echo https://dotnet.microsoft.com/download/dotnet/10.0
  exit /b 1
)

dotnet publish mbs.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -o dist\win-x64
if errorlevel 1 (
  echo BUILD FAILED. See error messages above.
  exit /b 1
)
echo.
echo OK. Run dist\win-x64\mbs.exe inside Windows Terminal.
endlocal
