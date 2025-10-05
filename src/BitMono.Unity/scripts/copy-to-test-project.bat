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
set "CLI_SOURCE=%CLI_BASE%\win-x64"
if not exist "%CLI_SOURCE%\BitMono.CLI.exe" (
    set "CLI_SOURCE=%CLI_BASE%"
)
set "CLI_DEST=%TEST_PROJECT%\BitMono.CLI"

if not exist "%CLI_DEST%" mkdir "%CLI_DEST%"

if exist "%CLI_SOURCE%\BitMono.CLI.exe" (
    echo Copying BitMono.CLI from %CLI_SOURCE% to %CLI_DEST%
    xcopy "%CLI_SOURCE%\*" "%CLI_DEST%\" /E /I /Y
    if %ERRORLEVEL% EQU 0 (
        echo BitMono.CLI copied successfully into Assets (import disabled by editor script)
        echo Generating .meta files to disable plugin import for CLI DLLs...
        for /R "%CLI_DEST%" %%F in (*.dll) do (
            >"%%~fF.meta" echo fileFormatVersion: 2
            >>"%%~fF.meta" echo guid: %%~nF000000000000000000000000000000
            >>"%%~fF.meta" echo PluginImporter:
            >>"%%~fF.meta" echo ^  serializedVersion: 2
            >>"%%~fF.meta" echo ^  isPreloaded: 0
            >>"%%~fF.meta" echo ^  isOverridable: 0
            >>"%%~fF.meta" echo ^  platformData:
            >>"%%~fF.meta" echo ^  - first:
            >>"%%~fF.meta" echo ^      Any:
            >>"%%~fF.meta" echo ^    second:
            >>"%%~fF.meta" echo ^      enabled: 0
            >>"%%~fF.meta" echo ^  userData:
            >>"%%~fF.meta" echo ^  assetBundleName:
            >>"%%~fF.meta" echo ^  assetBundleVariant:
        )
        echo .meta generation complete.
    ) else (
        echo ERROR: Failed to copy BitMono.CLI
    )
) else (
    echo ERROR: BitMono.CLI not found at %CLI_SOURCE%
)

echo ✅ Done! Open Unity and refresh (Ctrl+R)
pause
