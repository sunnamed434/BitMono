# BitMono Unity - Setup Test Project
Write-Host "BitMono Unity - Setup Test Project" -ForegroundColor Green

$TestProject = Join-Path (Split-Path $PSScriptRoot -Parent) "..\..\..\test\BitMono.Unity.TestProject\Assets\BitMono.Unity"

# Clean and create directories
if (Test-Path $TestProject) { Remove-Item -Recurse -Force $TestProject }
New-Item -ItemType Directory -Path $TestProject -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $TestProject "Editor") -Force | Out-Null

# Copy files from main source to test project
Copy-Item "..\Editor\*.cs" (Join-Path $TestProject "Editor") -Force
Copy-Item "..\Editor\*.asmdef" (Join-Path $TestProject "Editor") -Force
if (Get-ChildItem "..\Editor\*.cs.meta" -ErrorAction SilentlyContinue) { Copy-Item "..\Editor\*.cs.meta" (Join-Path $TestProject "Editor") -Force }
if (Get-ChildItem "..\Editor\*.asmdef.meta" -ErrorAction SilentlyContinue) { Copy-Item "..\Editor\*.asmdef.meta" (Join-Path $TestProject "Editor") -Force }
Copy-Item "..\package.json" $TestProject -Force
Copy-Item "..\README.md" $TestProject -Force

# Native IL2CPP metadata decryptor source plugin. Single source of truth is src/BitMono.IL2CPP/native; refresh the
# package copy from it, then mirror into the test project so Unity compiles it into GameAssembly.dll.
$NativeSource = "..\..\BitMono.IL2CPP\native\global_metadata_decrypt.cpp"
$PackagePlugins = "..\Plugins\BitMono"
if (Test-Path $NativeSource) {
    New-Item -ItemType Directory -Path $PackagePlugins -Force | Out-Null
    Copy-Item $NativeSource (Join-Path $PackagePlugins "global_metadata_decrypt.cpp") -Force
}
$TestPlugins = Join-Path $TestProject "Plugins\BitMono"
New-Item -ItemType Directory -Path $TestPlugins -Force | Out-Null
Copy-Item "$PackagePlugins\*" $TestPlugins -Recurse -Force

# Copy BitMonoConfig.asset (and .meta, if present) to preserve GUID/script binding
$configAsset = Join-Path (Join-Path (Split-Path $PSScriptRoot -Parent) "..") "BitMonoConfig.asset"
if (Test-Path $configAsset) {
    Copy-Item $configAsset $TestProject -Force
}
$configMeta = "$configAsset.meta"
if (Test-Path $configMeta) {
    Copy-Item $configMeta $TestProject -Force
}

# Build BitMono.CLI if needed
$CliSourcePath = "..\..\..\src\BitMono.CLI\bin\Release\net462\BitMono.CLI.exe"
if (!(Test-Path $CliSourcePath)) {
    Write-Host "Building BitMono.CLI..." -ForegroundColor Yellow
    dotnet build "..\..\..\src\BitMono.CLI\BitMono.CLI.csproj" --configuration Release
}

# Copy BitMono.CLI to project root (outside Assets)
$CliBase = "..\..\..\src\BitMono.CLI\bin\Release\net462"
$CliSourceRoot = Join-Path $CliBase "win-x64"
if (!(Test-Path (Join-Path $CliSourceRoot "BitMono.CLI.exe"))) {
    $CliSourceRoot = $CliBase
}
# BitMono.CLI~ : the ~ suffix makes Unity ignore the folder entirely, so the ~160 build-time DLLs
# (AsmResolver/MonoMod/...) are never imported and never ship into the IL2CPP player, where they'd
# otherwise wedge the build at "Extracting script serialization layouts". The .exe still runs from disk.
$CliDest = Join-Path $TestProject "BitMono.CLI~"
if (!(Test-Path $CliDest)) { New-Item -ItemType Directory -Path $CliDest -Force | Out-Null }
if (Test-Path $CliSourceRoot) {
    Copy-Item (Join-Path $CliSourceRoot "*") $CliDest -Recurse -Force
    Write-Host "BitMono.CLI copied into BitMono.CLI~ (Unity-ignored, never ships to the player)" -ForegroundColor Green
} else {
    Write-Host "ERROR: BitMono.CLI build output not found at $CliSourceRoot" -ForegroundColor Red
}

Write-Host "✅ Done! Open Unity and refresh (Ctrl+R)" -ForegroundColor Green
Read-Host "Press Enter to continue"
