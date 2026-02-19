param(
    [string]$SpecDirectory = "docs/spec/types",
    [string]$SpecRootDirectory = "docs/spec"
)

$ErrorActionPreference = 'Stop'

$requiredSpecs = @(
    'Money',
    'Percentage',
    'FastId',
    'Duration',
    'Period',
    'RecurringPeriod',
    'VirtualPath',
    'ShortCode',
    'BvFile',
    'Maybe',
    'EmailAddress',
    'PhoneNumber',
    'CountryCode',
    'CurrencyCode',
    'Locale',
    'TimeZoneId',
    'Url',
    'IpAddress',
    'CidrRange',
    'UsaState',
    'JsonContext',
    'MonthOnly',
    'EncryptedString'
)

$requiredGovernanceFiles = @(
    (Join-Path $SpecRootDirectory 'README.md'),
    (Join-Path $SpecRootDirectory 'compat-decisions.md'),
    (Join-Path $SpecRootDirectory 'test-traceability.md'),
    'docs/spec/templates/type-spec-template.md'
)

$missing = @()
foreach ($typeName in $requiredSpecs) {
    $file = Join-Path $SpecDirectory "$typeName.md"
    if (-not (Test-Path $file)) {
        $missing += $file
    }
}

foreach ($file in $requiredGovernanceFiles) {
    if (-not (Test-Path $file)) {
        $missing += $file
    }
}

if ($missing.Count -gt 0) {
    Write-Host 'Missing required specification files:'
    $missing | ForEach-Object { Write-Host " - $_" }
    exit 1
}

$requiredSections = @(
    '## Domain Purpose',
    '## Canonical Value Model',
    '## Input Contract',
    '## Normalization Rules',
    '## Public API Behavior',
    '## Error Contracts',
    '## Compatibility Notes',
    '## Test Requirements'
)

$validationErrors = @()
foreach ($typeName in $requiredSpecs) {
    $file = Join-Path $SpecDirectory "$typeName.md"
    $content = Get-Content -Path $file -Raw
    $lines = Get-Content -Path $file

    if ($content -match '\$\(' -or $content -match '\$today') {
        $validationErrors += "$file contains unresolved template placeholders."
    }

    if ($content.ToCharArray() | Where-Object { [int]$_ -lt 32 -and $_ -notin @("`r", "`n", "`t") }) {
        $validationErrors += "$file contains unexpected control characters."
    }

    if ($lines.Count -lt 6) {
        $validationErrors += "$file is too short to be a valid type specification."
        continue
    }

    if ($lines[0] -notmatch '^# .+ Behavioral Specification$') {
        $validationErrors += "$file has an invalid title format."
    }

    $typeValue = ($lines[2] -replace '^- Type:\s*', '' -replace '`', '').Trim()
    if ($typeValue -ne $typeName) {
        $validationErrors += "$file has invalid type metadata. Expected '$typeName'."
    }

    $namespaceValue = ($lines[3] -replace '^- Namespace:\s*', '' -replace '`', '').Trim()
    if ($namespaceValue -ne 'Incursa') {
        $validationErrors += "$file has invalid namespace metadata."
    }

    $statusValue = ($lines[4] -replace '^- Status:\s*', '' -replace '`', '').Trim()
    if ($statusValue -notin @('Draft', 'Approved')) {
        $validationErrors += "$file has invalid status metadata. Must be Draft or Approved."
    }

    $lastUpdatedValue = ($lines[5] -replace '^- Last Updated:\s*', '' -replace '`', '').Trim()
    if ($lastUpdatedValue -notmatch '^\d{4}-\d{2}-\d{2}$') {
        $validationErrors += "$file has invalid Last Updated metadata. Use YYYY-MM-DD."
    }

    foreach ($section in $requiredSections) {
        if ($content -notmatch [regex]::Escape($section)) {
            $validationErrors += "$file is missing required section '$section'."
        }
    }
}

if ($validationErrors.Count -gt 0) {
    Write-Host 'Specification validation errors:'
    $validationErrors | ForEach-Object { Write-Host " - $_" }
    exit 1
}

Write-Host "All required specification files are present and structurally valid in '$SpecRootDirectory'."
