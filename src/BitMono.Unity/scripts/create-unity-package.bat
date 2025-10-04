@echo off
setlocal enabledelayedexpansion

REM BitMono Unity - Create Unity Package (Batch Version)
REM 
REM Usage Examples:
REM   create-unity-package.bat                    - Quick test project setup (no params needed!)
REM   create-unity-package.bat "C:\Output" "1.0" - Create standalone package
REM   create-unity-package.bat "" "" 1 1         - Explicit test project setup
REM 
REM Parameters:
REM   OutputPath - Target directory (optional for test project)
REM   Version - Package version (optional for test project, defaults to 0.1.0)
REM   IncludeCli - Include BitMono.CLI (1=yes, 0=no, default=yes)
REM   TestProject - Setup for test project (1=yes, 0=no, default=auto-detect)

set "OUTPUT_PATH=%~1"
set "VERSION=%~2"
set "INCLUDE_CLI=%~3"
set "TEST_PROJECT=%~4"

REM Set defaults
if "%INCLUDE_CLI%"=="" set "INCLUDE_CLI=1"
if "%TEST_PROJECT%"=="" set "TEST_PROJECT=0"

echo.
echo BitMono Unity - Create Unity Package
echo ====================================
echo Version: %VERSION%
echo Output Path: %OUTPUT_PATH%
echo Include CLI: %INCLUDE_CLI%
echo Test Project: %TEST_PROJECT%
echo.

REM Set defaults for manual usage
if "%OUTPUT_PATH%"=="" (
    if "%TEST_PROJECT%"=="1" (
        set "OUTPUT_PATH=%~dp0..\..\..\test\BitMono.Unity.TestProject\Assets\BitMono.Unity"
        echo Using test project location: !OUTPUT_PATH!
    ) else (
        REM If no parameters provided, assume local testing and use test project
        echo No parameters provided - assuming local test project setup
        set "TEST_PROJECT=1"
        set "OUTPUT_PATH=%~dp0..\..\..\test\BitMono.Unity.TestProject\Assets\BitMono.Unity"
        echo Using test project location: !OUTPUT_PATH!
    )
)

if "%VERSION%"=="" (
    if "%TEST_PROJECT%"=="1" (
        set "VERSION=0.1.0"
        echo Using default version for test project: !VERSION!
    ) else (
        REM If no version provided and not test project, assume local testing
        echo No version provided - assuming local test project setup
        set "TEST_PROJECT=1"
        set "VERSION=0.1.0"
        echo Using default version for test project: !VERSION!
    )
)

REM Clean and create output directory
if exist "%OUTPUT_PATH%" (
    echo Removing existing output directory: %OUTPUT_PATH%
    rmdir /s /q "%OUTPUT_PATH%"
)
mkdir "%OUTPUT_PATH%" 2>nul
mkdir "%OUTPUT_PATH%\Editor" 2>nul

echo Created output directory: %OUTPUT_PATH%

REM Copy Unity package files
echo Copying Unity package files...

REM Copy Editor scripts
set "EDITOR_SOURCE=%~dp0..\Editor"
if exist "%EDITOR_SOURCE%" (
    copy "%EDITOR_SOURCE%\*.cs" "%OUTPUT_PATH%\Editor\" >nul 2>&1
    copy "%EDITOR_SOURCE%\*.asmdef" "%OUTPUT_PATH%\Editor\" >nul 2>&1
    echo Copied Editor scripts
) else (
    echo ERROR: Editor directory not found: %EDITOR_SOURCE%
    exit /b 1
)

REM Copy package.json and update version
set "PACKAGE_JSON_SOURCE=%~dp0..\package.json"
if exist "%PACKAGE_JSON_SOURCE%" (
    REM Use PowerShell to update package.json version
    powershell -Command "& {$json = Get-Content '%PACKAGE_JSON_SOURCE%' -Raw | ConvertFrom-Json; $json.version = '%VERSION%'; $json | ConvertTo-Json -Depth 10 | Set-Content '%OUTPUT_PATH%\package.json'}"
    echo Copied and updated package.json with version: %VERSION%
) else (
    echo ERROR: package.json not found: %PACKAGE_JSON_SOURCE%
    exit /b 1
)

REM Copy asset files if they exist
set "ASSET_SOURCE=%~dp0..\BitMonoConfig.asset"
if exist "%ASSET_SOURCE%" (
    copy "%ASSET_SOURCE%" "%OUTPUT_PATH%\" >nul
    echo Copied BitMonoConfig.asset
)

set "ASSET_META_SOURCE=%~dp0..\BitMonoConfig.asset.meta"
if exist "%ASSET_META_SOURCE%" (
    copy "%ASSET_META_SOURCE%" "%OUTPUT_PATH%\" >nul
    echo Copied BitMonoConfig.asset.meta
)

REM Copy BitMono.CLI if requested (default: yes)
if "%INCLUDE_CLI%"=="1" (
    echo Including BitMono.CLI...
    
    REM For test project, copy CLI to project root
    if "%TEST_PROJECT%"=="1" (
        for %%i in ("%OUTPUT_PATH%") do set "PROJECT_ROOT=%%~dpi"
        for %%i in ("!PROJECT_ROOT!") do set "CLI_DEST_PATH=%%~dpi..\BitMono.CLI"
    ) else (
        set "CLI_DEST_PATH=%OUTPUT_PATH%\BitMono.CLI"
    )
    
    REM Look for BitMono.CLI in various locations
    set "CLI_FOUND=0"
    set "SCRIPT_DIR=%~dp0"
    
    REM Check net462\win-x64
    set "CLI_PATH=!SCRIPT_DIR!..\..\..\src\BitMono.CLI\bin\Release\net462\win-x64"
    if exist "!CLI_PATH!" (
        if not exist "!CLI_DEST_PATH!" mkdir "!CLI_DEST_PATH!" 2>nul
        xcopy "!CLI_PATH!\*" "!CLI_DEST_PATH!\" /E /I /Y >nul
        echo Copied BitMono.CLI from: !CLI_PATH! to: !CLI_DEST_PATH!
        set "CLI_FOUND=1"
    ) else (
        REM Check net462
        set "CLI_PATH=!SCRIPT_DIR!..\..\..\src\BitMono.CLI\bin\Release\net462"
        if exist "!CLI_PATH!" (
            if not exist "!CLI_DEST_PATH!" mkdir "!CLI_DEST_PATH!" 2>nul
            xcopy "!CLI_PATH!\*" "!CLI_DEST_PATH!\" /E /I /Y >nul
            echo Copied BitMono.CLI from: !CLI_PATH! to: !CLI_DEST_PATH!
            set "CLI_FOUND=1"
        ) else (
            REM Check project root
            set "CLI_PATH=!SCRIPT_DIR!..\..\..\BitMono.CLI"
            if exist "!CLI_PATH!" (
                if not exist "!CLI_DEST_PATH!" mkdir "!CLI_DEST_PATH!" 2>nul
                xcopy "!CLI_PATH!\*" "!CLI_DEST_PATH!\" /E /I /Y >nul
                echo Copied BitMono.CLI from: !CLI_PATH! to: !CLI_DEST_PATH!
                set "CLI_FOUND=1"
            ) else (
                REM Check linux-x64
                set "CLI_PATH=!SCRIPT_DIR!..\..\..\src\BitMono.CLI\bin\Release\net462\linux-x64"
                if exist "!CLI_PATH!" (
                    if not exist "!CLI_DEST_PATH!" mkdir "!CLI_DEST_PATH!" 2>nul
                    xcopy "!CLI_PATH!\*" "!CLI_DEST_PATH!\" /E /I /Y >nul
                    echo Copied BitMono.CLI from: !CLI_PATH! to: !CLI_DEST_PATH!
                    set "CLI_FOUND=1"
                )
            )
        )
    )
    
    if "%CLI_FOUND%"=="0" (
        echo WARNING: BitMono.CLI not found in any expected location. Creating package without CLI.
    )
)

echo.
echo ✅ Unity package created successfully!
echo Location: %OUTPUT_PATH%
echo Version: %VERSION%

if "%TEST_PROJECT%"=="1" (
    echo Type: Test Project Setup (includes BitMono.CLI)
    echo Next: Open Unity and refresh (Ctrl+R)
) else (
    echo Type: Standalone Unity Package
    echo Next: Import this package into your Unity project
)

REM List contents for verification
echo.
echo Package contents:
for /r "%OUTPUT_PATH%" %%i in (*) do (
    set "FILE_PATH=%%i"
    set "RELATIVE_PATH=!FILE_PATH:%OUTPUT_PATH%\=!"
    echo   !RELATIVE_PATH!
)

REM Pause for manual usage
if "%TEST_PROJECT%"=="1" (
    echo.
    pause
)

endlocal
