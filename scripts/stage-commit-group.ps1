param(
    [Parameter(Mandatory = $true)]
    [ValidateSet(
        'tooling-and-normalization',
        'runtime-hardening',
        'test-hardening',
        'specs-and-documentation',
        'ci-quality-gates'
    )]
    [string]$Group
)

$ErrorActionPreference = 'Stop'

function Add-Paths([string[]]$paths) {
    if ($paths.Count -eq 0) {
        return
    }

    & git add -- $paths
}

switch ($Group) {
    'tooling-and-normalization' {
        Add-Paths @(
            '.editorconfig',
            '.gitignore',
            '.vscode/tasks.json',
            '.pre-commit-config.yaml',
            'cleanup.ps1',
            'cleanup.sh'
        )
    }
    'runtime-hardening' {
        Add-Paths @(
            'Incursa.Types'
        )
    }
    'test-hardening' {
        Add-Paths @(
            'Incursa.Types.Tests'
        )
    }
    'specs-and-documentation' {
        Add-Paths @(
            'docs/spec',
            'docs/types.md',
            'docs/fastid.md',
            'docs/serialization-and-ef-samples.md',
            'docs/coverage-ratchet-plan.md',
            'docs/migration-v2.md',
            'docs/quality-gate-execution-plan.md',
            'CHANGELOG.md',
            'README.md',
            'Incursa.Types/README.md',
            '.github/pull_request_template.md'
        )
    }
    'ci-quality-gates' {
        Add-Paths @(
            '.github/workflows/quality.yml',
            '.github/workflows/commit-ci.yml',
            '.github/workflows/main-release-ci-cd.yml',
            'scripts',
            'Directory.Build.props',
            'Incursa.Core.slnx'
        )
    }
}

Write-Host "Staged commit group: $Group"
& git diff --cached --name-status
