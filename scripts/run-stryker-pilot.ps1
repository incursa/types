param(
    [string]$MutatePattern = "**/Money.cs",
    [string]$OutputDirectory = "artifacts/stryker-pilot"
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path ".tools/dotnet-stryker.exe")) {
    dotnet tool install --tool-path .tools dotnet-stryker --version 4.9.0 | Out-Host
}

./.tools/dotnet-stryker `
    -p Incursa.Types/Incursa.Types.csproj `
    -tp Incursa.Types.Tests/Incursa.Types.Tests.csproj `
    -l Basic `
    -L `
    -b 0 `
    --threshold-low 0 `
    --threshold-high 0 `
    --break-on-initial-test-failure `
    -m $MutatePattern `
    -r Progress `
    -r Html `
    -r Markdown `
    -O $OutputDirectory `
    --skip-version-check
