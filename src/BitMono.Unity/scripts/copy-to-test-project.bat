@echo off
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
copy "..\package.json" "%TEST_PROJECT%\" /Y
copy "..\README.md" "%TEST_PROJECT%\" /Y

REM Copy asset files if they exist
if exist "..\BitMonoConfig.asset" copy "..\BitMonoConfig.asset" "%TEST_PROJECT%\" /Y
if exist "..\BitMonoConfig.asset.meta" copy "..\BitMonoConfig.asset.meta" "%TEST_PROJECT%\" /Y

REM Build BitMono.CLI if needed
if not exist "..\..\..\src\BitMono.CLI\bin\Release\net462\BitMono.CLI.exe" (
    echo Building BitMono.CLI...
    dotnet build "..\..\..\src\BitMono.CLI\BitMono.CLI.csproj" --configuration Release
)

REM Copy BitMono.CLI to Unity test project root (outside Assets)
set "CLI_SOURCE=..\..\..\src\BitMono.CLI\bin\Release\net462"
set "CLI_DEST=..\..\..\test\BitMono.Unity.TestProject\BitMono.CLI"

if not exist "%CLI_DEST%" mkdir "%CLI_DEST%"

if exist "%CLI_SOURCE%\BitMono.CLI.exe" (
    echo Copying BitMono.CLI from %CLI_SOURCE% to %CLI_DEST%
    xcopy "%CLI_SOURCE%\*" "%CLI_DEST%\" /E /I /Y
    if %ERRORLEVEL% EQU 0 (
        echo BitMono.CLI copied successfully
    ) else (
        echo ERROR: Failed to copy BitMono.CLI
    )
) else (
    echo ERROR: BitMono.CLI not found at %CLI_SOURCE%
)

REM Config files are now auto-detected from BitMono.CLI location

echo ✅ Done! Open Unity and refresh (Ctrl+R)
pause
