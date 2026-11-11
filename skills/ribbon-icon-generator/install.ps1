<#
.SYNOPSIS
    Install the ribbon-icon-generator skill into local AI coding clients.

.DESCRIPTION
    Copies this folder (SKILL.md + catalog + tools + samples + references) into
    the skills directory of clients that are actually present on this machine.

.PARAMETER Scope
    Global  = user-level skill directories (default).
    Project = write into a repository, committable to the team.

.PARAMETER Root
    Repository root used by -Scope Project. Defaults to the current directory.

.PARAMETER All
    Install into every supported client, even ones not detected.

.PARAMETER Force
    Overwrite an existing installation without asking.

.EXAMPLE
    pwsh ./install.ps1
    pwsh ./install.ps1 -Scope Project -Root "D:/Demo/YourApp"
    pwsh ./install.ps1 -All -Force
#>
[CmdletBinding()]
param(
    [ValidateSet('Global', 'Project')]
    [string]$Scope = 'Global',
    [string]$Root = (Get-Location).Path,
    [switch]$All,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
$SkillName = 'ribbon-icon-generator'
$Source = $PSScriptRoot
$ExcludeDirs = @('.git', '.vs', '.idea', '__pycache__', 'shots', '.playwright-mcp', 'dist', 'bin', 'obj', 'node_modules')

if (-not (Test-Path (Join-Path $Source 'SKILL.md'))) {
    throw "SKILL.md not found next to install.ps1 ($Source)"
}

if ($Scope -eq 'Global') {
    $targets = @(
        @{ Client = 'Agents'; Dir = (Join-Path $HOME '.agents/skills'); Detect = (Join-Path $HOME '.agents') },
        @{ Client = 'ZCode';  Dir = (Join-Path $HOME '.zcode/skills');  Detect = (Join-Path $HOME '.zcode') },
        @{ Client = 'Claude'; Dir = (Join-Path $HOME '.claude/skills'); Detect = (Join-Path $HOME '.claude') },
        @{ Client = 'Codex';  Dir = (Join-Path $HOME '.codex/skills');  Detect = (Join-Path $HOME '.codex') },
        @{ Client = 'Cursor'; Dir = (Join-Path $HOME '.cursor/skills'); Detect = (Join-Path $HOME '.cursor') }
    )
}
else {
    $Root = (Resolve-Path -LiteralPath $Root).Path
    $targets = @(
        @{ Client = 'Agents'; Dir = (Join-Path $Root '.agents/skills'); Detect = $Root },
        @{ Client = 'ZCode';  Dir = (Join-Path $Root '.zcode/skills');  Detect = $Root }
    )
}

function Copy-SkillTree {
    param([string]$From, [string]$To)

    New-Item -ItemType Directory -Force -Path $To | Out-Null
    foreach ($item in Get-ChildItem -LiteralPath $From -Force) {
        $name = $item.Name.ToLowerInvariant()
        if ($item.PSIsContainer) {
            if ($ExcludeDirs -contains $name) { continue }
            if ($name -eq $SkillName) { continue }   # never recurse into an installed copy
            Copy-SkillTree -From $item.FullName -To (Join-Path $To $item.Name)
        }
        else {
            Copy-Item -LiteralPath $item.FullName -Destination (Join-Path $To $item.Name) -Force
        }
    }
}

$installed = 0
foreach ($target in $targets) {
    $present = Test-Path -LiteralPath $target.Detect
    if (-not $present -and -not $All) {
        Write-Host ("skip  {0,-7} (client not found: {1})" -f $target.Client, $target.Detect) -ForegroundColor DarkGray
        continue
    }

    $destination = Join-Path $target.Dir $SkillName
    if ((Test-Path -LiteralPath $destination) -and -not $Force) {
        Write-Host ("exists {0,-7} {1}  (use -Force to overwrite)" -f $target.Client, $destination) -ForegroundColor Yellow
        continue
    }
    if (Test-Path -LiteralPath $destination) {
        Remove-Item -LiteralPath $destination -Recurse -Force
    }

    Copy-SkillTree -From $Source -To $destination
    Write-Host ("ok    {0,-7} {1}" -f $target.Client, $destination) -ForegroundColor Green
    $installed++
}

Write-Host ""
Write-Host ("Installed {0} copy/copies. Try: python tools/IconGen.py list" -f $installed) -ForegroundColor Cyan