@echo off
rem Builds Compiled\OsziWaveformAnalyzer.exe with the MSBuild of the .NET Framework 4.x
rem which is part of every Windows installation (no Visual Studio required).
rem The 32 bit MSBuild is used because it exists on 32 and 64 bit Windows.

set MSBUILD=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
if not exist "%MSBUILD%" (
    echo ERROR: %MSBUILD% not found. Install the .NET Framework 4.x.
    goto End
)

"%MSBUILD%" "%~dp0SourceCode\OsziWaveformAnalyzer.csproj" /p:Configuration=Release /v:minimal /nologo
if errorlevel 1 (
    echo.
    echo BUILD FAILED
) else (
    echo.
    echo Build succeeded: %~dp0Compiled\OsziWaveformAnalyzer.exe
)

:End
rem Keep the window open when started with a double click in Explorer
echo %CMDCMDLINE% | %WINDIR%\System32\find.exe /i "%~0" >nul && pause
