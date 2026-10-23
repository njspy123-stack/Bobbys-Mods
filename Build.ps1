param([string]$GamePath = 'C:\Program Files (x86)\Steam\steamapps\common\Gorilla Tag')
$ErrorActionPreference = 'Stop'
$localSdk = Join-Path $PSScriptRoot '.dotnet\dotnet.exe'
$compiler = if (Test-Path -LiteralPath $localSdk) { $localSdk } else { 'dotnet' }
$env:DOTNET_CLI_HOME = Join-Path $PSScriptRoot '.dotnet-home'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
& $compiler build "$PSScriptRoot\BobbysMods.csproj" --configuration Release "-p:GamePath=$GamePath" --nologo
if ($LASTEXITCODE -ne 0) { throw 'Build failed. See messages above.' }
Write-Host 'Built in bin\Release\netstandard2.1. See README.md for installation.'
