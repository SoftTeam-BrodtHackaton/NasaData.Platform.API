$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$dotnetRoot = Split-Path (Get-Command dotnet).Source -Parent
$sdk = (dotnet --version).Trim()
$referencePack = Get-ChildItem "$dotnetRoot/packs/Microsoft.NETCore.App.Ref" -Directory | Where-Object Name -Like '9.*' | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
$output = Join-Path $root 'NasaDataPlatform.API/obj/MvpChecks'
New-Item $output -ItemType Directory -Force | Out-Null
$references = Get-ChildItem "$($referencePack.FullName)/ref/net9.0/*.dll" | ForEach-Object { '/reference:' + $_.FullName }
$api = Join-Path $root 'NasaDataPlatform.API/bin/Debug/net9.0/NasaDataPlatform.API.dll'
& dotnet "$dotnetRoot/sdk/$sdk/Roslyn/bincore/csc.dll" /nologo /target:exe "/out:$output/DomainChecks.dll" $references "/reference:$api" "$PSScriptRoot/DomainChecks.cs"
if ($LASTEXITCODE -ne 0) { throw 'Domain checks compilation failed.' }
Copy-Item $api $output -Force
'{"runtimeOptions":{"tfm":"net9.0","framework":{"name":"Microsoft.NETCore.App","version":"9.0.0"}}}' | Set-Content "$output/DomainChecks.runtimeconfig.json"
& dotnet "$output/DomainChecks.dll"
if ($LASTEXITCODE -ne 0) { throw 'Domain checks failed.' }
