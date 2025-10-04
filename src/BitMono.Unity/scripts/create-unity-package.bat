@echo off
echo BitMono Unity - Create Unity Package
echo Current directory: %CD%

REM Check if output path argument is provided
if "%1"=="" (
    echo No output path provided, using default test project location
    set "OUTPUT_PATH=..\..\..\test\BitMono.Unity.TestProject\Assets\BitMono.Unity"
    set "IS_TEST_PROJECT=1"
) else (
    echo Output path provided: %1
    set "OUTPUT_PATH=%1"
    set "IS_TEST_PROJECT=0"
)

REM Clean and create output directory
if exist "%OUTPUT_PATH%" rmdir /s /q "%OUTPUT_PATH%"
mkdir "%OUTPUT_PATH%"
mkdir "%OUTPUT_PATH%\Editor"

REM Copy Unity package files
copy "..\Editor\*.cs" "%OUTPUT_PATH%\Editor\" /Y
copy "..\Editor\*.asmdef" "%OUTPUT_PATH%\Editor\" /Y
copy "..\package.json" "%OUTPUT_PATH%\" /Y
copy "..\README.md" "%OUTPUT_PATH%\" /Y

REM Copy asset files if they exist
if exist "..\BitMonoConfig.asset" copy "..\BitMonoConfig.asset" "%OUTPUT_PATH%\" /Y
if exist "..\BitMonoConfig.asset.meta" copy "..\BitMonoConfig.asset.meta" "%OUTPUT_PATH%\" /Y

REM If this is for the test project, also copy BitMono.CLI
if "%IS_TEST_PROJECT%"=="1" (
    echo Setting up test project with BitMono.CLI...
    
    REM Build BitMono.CLI if needed
    if not exist "..\..\..\src\BitMono.CLI\bin\Release\net462\BitMono.CLI.exe" (
        echo Building BitMono.CLI...
        dotnet build "..\..\..\src\BitMono.CLI\BitMono.CLI.csproj" --configuration Release
    )
    
    REM Copy BitMono.CLI to test project root
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
) else (
    echo Creating standalone Unity package (no BitMono.CLI included)
)

echo.
echo ✅ Unity package created successfully!
echo Location: %OUTPUT_PATH%
if "%IS_TEST_PROJECT%"=="1" (
    echo Type: Test Project Setup (includes BitMono.CLI)
    echo Next: Open Unity and refresh (Ctrl+R)
) else (
    echo Type: Standalone Unity Package
    echo Next: Import this package into your Unity project
)
echo.
pause