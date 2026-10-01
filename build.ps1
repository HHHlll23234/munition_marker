param(
    # Optional overrides; normally auto-detection finds the Steam library.
    [string]$SeaPowerDir,
    [string]$AnchorChainDir,
    [switch]$Deploy
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$project = Join-Path $root "MunitionMarkers.csproj"
$dist = Join-Path $root "dist"

$buildArgs = @("build", $project, "-c", "Release", "-v", "minimal")
if ($SeaPowerDir) { $buildArgs += "-p:SeaPowerDir=$SeaPowerDir" }
if ($AnchorChainDir) { $buildArgs += "-p:AnchorChainDir=$AnchorChainDir" }

dotnet @buildArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE"
}

New-Item -ItemType Directory -Force -Path $dist | Out-Null
Copy-Item -LiteralPath (Join-Path $root "bin\Release\MunitionMarkers.dll") -Destination $dist -Force
Copy-Item -LiteralPath (Join-Path $root "_info.ini") -Destination $dist -Force
Copy-Item -LiteralPath (Join-Path $root "workshop\preview.png") -Destination $dist -Force

Write-Host "Built: $dist\MunitionMarkers.dll (workshop-ready: dll + _info.ini + preview.png)"

if ($Deploy) {
    if (-not $SeaPowerDir) {
        # Same auto-detection order as the .csproj: common Steam library locations.
        foreach ($cand in @(
            "C:\Program Files (x86)\Steam\steamapps\common\Sea Power",
            "C:\SteamLibrary\steamapps\common\Sea Power",
            "D:\SteamLibrary\steamapps\common\Sea Power",
            "E:\SteamLibrary\steamapps\common\Sea Power",
            "F:\SteamLibrary\steamapps\common\Sea Power",
            "G:\SteamLibrary\steamapps\common\Sea Power")) {
            if (Test-Path (Join-Path $cand "Sea Power_Data")) { $SeaPowerDir = $cand; break }
        }
    }
    if (-not $SeaPowerDir) {
        throw "Sea Power install directory not found. Re-run with -SeaPowerDir `"C:\path\to\Sea Power`"."
    }
    $modDir = Join-Path $SeaPowerDir "Sea Power_Data\StreamingAssets\MunitionMarkers"
    New-Item -ItemType Directory -Force -Path $modDir | Out-Null
    Copy-Item -LiteralPath (Join-Path $dist "MunitionMarkers.dll") -Destination $modDir -Force
    Copy-Item -LiteralPath (Join-Path $dist "_info.ini") -Destination $modDir -Force
    Copy-Item -LiteralPath (Join-Path $dist "preview.png") -Destination $modDir -Force
    Write-Host "Deployed to: $modDir"
}
