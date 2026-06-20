@echo off
setlocal enabledelayedexpansion
echo BitMono Unity - Setup Test Project
echo Current directory: %CD%

set "TEST_PROJECT=..\..\..\test\BitMono.Unity.TestProject\Assets\BitMono.Unity"

REM Clean and create directories
if exist "%TEST_PROJECT%" rmdir /s /q "%TEST_PROJECT%"
mkdir "%TEST_PROJECT%"
mkdir "%TEST_PROJECT%\Editor"

REM Copy files from main source to test project
copy "..\Editor\*.cs" "%TEST_PROJECT%\Editor\" /Y
copy "..\Editor\*.asmdef" "%TEST_PROJECT%\Editor\" /Y
REM Copy .meta files if they exist (to preserve GUIDs)
if exist "..\Editor\*.cs.meta" copy "..\Editor\*.cs.meta" "%TEST_PROJECT%\Editor\" /Y
if exist "..\Editor\*.asmdef.meta" copy "..\Editor\*.asmdef.meta" "%TEST_PROJECT%\Editor\" /Y
copy "..\package.json" "%TEST_PROJECT%\" /Y
copy "..\README.md" "%TEST_PROJECT%\" /Y

REM Copy BitMonoConfig.asset (and .meta if exists) to preserve GUID/script binding
if exist "..\BitMonoConfig.asset" copy "..\BitMonoConfig.asset" "%TEST_PROJECT%\" /Y
if exist "..\BitMonoConfig.asset.meta" copy "..\BitMonoConfig.asset.meta" "%TEST_PROJECT%\" /Y

REM Static .meta is committed; no generation needed

REM Build BitMono.CLI if needed
if not exist "..\..\..\src\BitMono.CLI\bin\Release\net462\BitMono.CLI.exe" (
    echo Building BitMono.CLI...
    dotnet build "..\..\..\src\BitMono.CLI\BitMono.CLI.csproj" --configuration Release
)

REM Copy BitMono.CLI into Assets (will be disabled via PluginImporter)
set "CLI_BASE=..\..\..\src\BitMono.CLI\bin\Release\net462"
set "CLI_SOURCE=!CLI_BASE!\win-x64"
if not exist "!CLI_SOURCE!\BitMono.CLI.exe" (
    set "CLI_SOURCE=!CLI_BASE!"
)
REM BitMono.CLI~ : the ~ suffix makes Unity ignore the folder, so the ~160 build-time DLLs are never
REM imported and never ship into the IL2CPP player (where they'd wedge the build). The .exe runs from disk.
set "CLI_DEST=%TEST_PROJECT%\BitMono.CLI~"

if not exist "!CLI_DEST!" mkdir "!CLI_DEST!"

if exist "!CLI_SOURCE!\BitMono.CLI.exe" (
    echo Copying BitMono.CLI from !CLI_SOURCE! to !CLI_DEST!
    xcopy "!CLI_SOURCE!\*" "!CLI_DEST!\" /E /I /Y
    if !ERRORLEVEL! EQU 0 (
        echo BitMono.CLI copied into BitMono.CLI~ ^(Unity-ignored, never ships to the player^)
    ) else (
        echo ERROR: Failed to copy BitMono.CLI
    )
) else (
    echo ERROR: BitMono.CLI not found at !CLI_SOURCE!
)

echo ✅ Done! Open Unity and refresh (Ctrl+R)
pause
