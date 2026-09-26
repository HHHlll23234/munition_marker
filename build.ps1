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

Write-Host "Built: $dist\MunitionMarkers.dll"

if ($Deploy) {
    if (-not $SeaPowerDir) {
        $SeaPowerDir = "F:\SteamLibrary\steamapps\common\Sea Power"
    }
    $modDir = Join-Path $SeaPowerDir "Sea Power_Data\StreamingAssets\MunitionMarkers"
    New-Item -ItemType Directory -Force -Path $modDir | Out-Null
    Copy-Item -LiteralPath (Join-Path $dist "MunitionMarkers.dll") -Destination $modDir -Force
    Copy-Item -LiteralPath (Join-Path $dist "_info.ini") -Destination $modDir -Force
    Write-Host "Deployed to: $modDir"
}
