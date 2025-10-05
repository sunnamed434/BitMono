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
Copy-Item "..\package.json" $TestProject -Force
Copy-Item "..\README.md" $TestProject -Force

# Copy asset files if they exist
if (Test-Path "..\BitMonoConfig.asset") { Copy-Item "..\BitMonoConfig.asset" $TestProject -Force }
if (Test-Path "..\BitMonoConfig.asset.meta") { Copy-Item "..\BitMonoConfig.asset.meta" $TestProject -Force }

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
$CliDest = Join-Path $TestProject "BitMono.CLI"
if (!(Test-Path $CliDest)) { New-Item -ItemType Directory -Path $CliDest -Force | Out-Null }
if (Test-Path $CliSourceRoot) {
    Copy-Item (Join-Path $CliSourceRoot "*") $CliDest -Recurse -Force
    Write-Host "BitMono.CLI copied under Assets (import will be disabled by editor script)" -ForegroundColor Green
    # Generate .meta files to disable plugin import for all CLI DLLs
    $dlls = Get-ChildItem -Path $CliDest -Filter *.dll -Recurse -ErrorAction SilentlyContinue
    foreach ($dll in $dlls) {
        $metaPath = "$($dll.FullName).meta"
        $guidBase = [System.IO.Path]::GetFileNameWithoutExtension($dll.Name)
        $content = @"
fileFormatVersion: 2
guid: ${guidBase}000000000000000000000000000000
PluginImporter:
  serializedVersion: 2
  isPreloaded: 0
  isOverridable: 0
  platformData:
  - first:
      Any:
    second:
      enabled: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"@
        $content | Out-File -FilePath $metaPath -Encoding ascii -Force
    }
    Write-Host ".meta generation complete for CLI DLLs" -ForegroundColor Green
} else {
    Write-Host "ERROR: BitMono.CLI build output not found at $CliSourceRoot" -ForegroundColor Red
}

Write-Host "✅ Done! Open Unity and refresh (Ctrl+R)" -ForegroundColor Green
Read-Host "Press Enter to continue"
