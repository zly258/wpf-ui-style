#Requires -Version 5.1
<#
.SYNOPSIS
    Shim that runs tools/ContrastCheck.py with whatever Python 3 is available.

.DESCRIPTION
    The actual logic lives in ContrastCheck.py (stdlib only). This wrapper keeps
    the documented `pwsh ./tools/ContrastCheck.ps1` command working on machines
    where only PowerShell is guaranteed.

    Python lookup order: py -3 -> python3 -> python.

.EXAMPLE
    pwsh ./tools/ContrastCheck.ps1

.EXAMPLE
    pwsh ./tools/ContrastCheck.ps1 -TokensPath "D:\repo\Theme\Tokens.xaml"
#>
[CmdletBinding()]
param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$RemainingArgs
)

$ErrorActionPreference = 'Stop'

$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Definition }
$pyScript = Join-Path $scriptDir 'ContrastCheck.py'

function Resolve-Python {
    $candidates = @(
        @{ Exe = 'py'; Prefix = @('-3') },
        @{ Exe = 'python3'; Prefix = @() },
        @{ Exe = 'python'; Prefix = @() }
    )

    foreach ($candidate in $candidates) {
        if (-not (Get-Command $candidate.Exe -ErrorAction SilentlyContinue)) { continue }
        try {
            & $candidate.Exe @($candidate.Prefix + '--version') *> $null
            if ($LASTEXITCODE -eq 0) {
                return $candidate
            }
        }
        catch {
            continue
        }
    }
    return $null
}

$python = Resolve-Python
if ($null -eq $python) {
    Write-Host 'Python 3 not found. Install Python 3 or run the .py file directly.' -ForegroundColor Red
    exit 2
}

& $python.Exe @($python.Prefix + $pyScript + $RemainingArgs)
exit $LASTEXITCODE
