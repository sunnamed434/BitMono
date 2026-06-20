@echo off
REM Validates the #276 native decryptor (src/BitMono.IL2CPP/native/global_metadata_decrypt.cpp) in all three
REM build modes with MSVC. Compile-checks always run; the data-driven tests run only if a built test player's
REM global-metadata.dat(.enc/.plain) is present. Needs Visual Studio (Build Tools) with the C++ workload.
setlocal
set SRC=%~dp0..\src\BitMono.IL2CPP\native\global_metadata_decrypt.cpp

REM Find vcvars64.bat via vswhere (works for VS 2017+ / Build Tools / Community / Pro).
set VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe
set VCVARS=
if exist "%VSWHERE%" (
    for /f "usebackq tokens=*" %%i in (`"%VSWHERE%" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath`) do set VCVARS=%%i\VC\Auxiliary\Build\vcvars64.bat
)
if not defined VCVARS ( echo Could not locate Visual Studio C++ tools via vswhere. & exit /b 1 )
call "%VCVARS%" >nul 2>&1

set OUT=%TEMP%\bmnative
if not exist "%OUT%" mkdir "%OUT%"
cd /d "%OUT%"

echo ===== 1) STANDALONE validator =====
cl /nologo /EHsc /O2 /DBITMONO_STANDALONE_TEST /Fe:"%OUT%\bm_standalone.exe" "%SRC%" 1>cl1.txt 2>&1
if errorlevel 1 ( echo COMPILE_FAILED & type cl1.txt & exit /b 1 )
echo compiled OK

echo ===== 2) HOOK test (CreateFileW -^> mapping delivery) =====
cl /nologo /EHsc /O2 /DBITMONO_HOOK_TEST /Fe:"%OUT%\bm_hook.exe" "%SRC%" 1>cl2.txt 2>&1
if errorlevel 1 ( echo COMPILE_FAILED & type cl2.txt & exit /b 1 )
echo compiled OK

echo ===== 3) PLUGIN (default mode, no defines = Unity source-plugin) =====
cl /nologo /EHsc /O2 /LD /Fe:"%OUT%\bm_plugin.dll" "%SRC%" 1>cl3.txt 2>&1
if errorlevel 1 ( echo COMPILE_FAILED & type cl3.txt & exit /b 1 )
dumpbin /exports "%OUT%\bm_plugin.dll" | findstr /i "BitMono"

REM Optional data-driven run against a built test player (only exists after a Unity IL2CPP build).
set MD=%~dp0BitMono.Unity.TestProject\Build\Il2cpp\BitMonoTest_Data\il2cpp_data\Metadata
if not exist "%MD%\global-metadata.dat.enc" ( echo. & echo No built test metadata found; skipping data tests. & echo ALL COMPILE CHECKS PASSED & exit /b 0 )
set PLAIN=%MD%\global-metadata.dat.plain
if not exist "%PLAIN%" set PLAIN=%MD%\global-metadata.dat
echo. & echo ===== data: standalone decrypt round-trip =====
"%OUT%\bm_standalone.exe" "%MD%\global-metadata.dat.enc" "%PLAIN%"
echo. & echo ===== data: hook serves decrypted bytes =====
"%OUT%\bm_hook.exe" "%MD%\global-metadata.dat.enc" "%PLAIN%"
echo. & echo ALL CHECKS PASSED
