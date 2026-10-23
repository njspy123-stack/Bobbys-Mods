param(
 [string]$GamePath='C:\Program Files (x86)\Steam\steamapps\common\Gorilla Tag',
 [string]$CodeQLPath=(Join-Path $PSScriptRoot '.security-tools\codeql\codeql.exe'),
 [string]$DotnetPath=(Join-Path $PSScriptRoot '.dotnet\dotnet.exe'),
 [string]$GhPath=(Join-Path $PSScriptRoot '.security-tools\gh\bin\gh.exe'),
 [switch]$Upload
)
$ErrorActionPreference='Stop'
foreach($p in @($GamePath,$CodeQLPath,$DotnetPath)){if($p -match '["\r\n]'){throw 'Paths cannot contain quotes or line breaks.'};if(!(Test-Path -LiteralPath $p)){throw "Required path is missing: $p"}}
$work=Join-Path $PSScriptRoot ('.security-scans\run-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $work | Out-Null
$source=Join-Path $work 'source'
git -c credential.helper= clone --depth 1 --branch main https://github.com/njspy123-stack/Bobbys-Mods.git $source
if($LASTEXITCODE -ne 0){throw 'Clean source checkout failed'}
$commit=git -C $source rev-parse HEAD
$env:DOTNET_ROOT=Split-Path -Parent $DotnetPath
$env:PATH=$env:DOTNET_ROOT+';'+$env:PATH
$env:DOTNET_CLI_HOME=Join-Path $work 'dotnet-home'
$env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
$lines=[Collections.Generic.List[string]]::new();$lines.Add('@echo off')
$lines.Add(('"'+$DotnetPath+'" build "'+(Join-Path $source 'BobbysMods.csproj')+'" --configuration Release -t:Rebuild -p:UseSharedCompilation=false -nodeReuse:false "-p:GamePath='+$GamePath+'"'))
$lines.Add('if errorlevel 1 exit /b 1')
$lines.Add(('"'+$DotnetPath+'" build "'+(Join-Path $source 'tests\FreezeTests.csproj')+'" --configuration Release -t:Rebuild -p:UseSharedCompilation=false -nodeReuse:false'))
$command=Join-Path $work 'build.cmd';[IO.File]::WriteAllLines($command,$lines,[Text.Encoding]::ASCII)
$db=Join-Path $work 'database';$sarif=Join-Path $work 'results.sarif'
& $CodeQLPath database create $db --language=csharp "--source-root=$source" ('--command=cmd.exe /d /c "'+$command+'"') --threads=2 --ram=4096
if($LASTEXITCODE -ne 0){throw 'CodeQL extraction failed'}
& $CodeQLPath database analyze $db codeql/csharp-queries:codeql-suites/csharp-security-extended.qls --format=sarifv2.1.0 "--output=$sarif" --sarif-category=local-build/csharp --threads=2 --ram=4096 --no-sarif-add-file-contents --no-sarif-add-snippets
if($LASTEXITCODE -ne 0){throw 'CodeQL analysis failed'}
@{commit=$commit;database=$db;sarif=$sarif} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $work 'scan.json') -Encoding utf8
if($Upload){
 if(!(Test-Path -LiteralPath $GhPath)){throw 'GitHub CLI not found; local results retained.'}
 $login=& $GhPath api user --jq .login
 if($LASTEXITCODE -ne 0 -or $login -ne 'njspy123-stack'){throw 'Sign GitHub CLI into njspy123-stack before uploading.'}
 & $GhPath auth token | & $CodeQLPath github upload-results "--sarif=$sarif" --repository=njspy123-stack/Bobbys-Mods --ref=refs/heads/main "--commit=$commit" "--checkout-path=$source" --github-auth-stdin
 if($LASTEXITCODE -ne 0){throw 'Upload failed; local results retained.'}
}
Write-Output "Completed scan of $commit. Results: $sarif"
