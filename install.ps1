#Requires -Version 5.1
<#
.SYNOPSIS
    Install the wpf-ui-style skill into mainstream AI coding clients.

.DESCRIPTION
    Installs this folder as a skill (SKILL.md + references + Theme + tools)
    and, for clients that use rules/instructions instead of skills, writes a
    concise rule file or appends a marked block to AGENTS.md / global rules.

    Supported clients include:
      ZCode, generic Agent Skills, Claude Code, GitHub Copilot / VS Code,
      Cursor, Windsurf / Codeium, Cline, Roo Code, Kilo Code, Continue,
      OpenAI Codex, Gemini CLI, Qwen Code, Grok CLI, Kiro, Trae, Trae CN,
      WorkBuddy, CodeBuddy, CodeBuddy CN, DSH (DeepSeek Harness),
      Antigravity, MarsCode, Zed, Aider, OpenHands, JetBrains AI Assistant.

.PARAMETER Scope
    Global (default), Project, or Both.
      Global  = user-level skill/rule directories (available in all projects).
      Project = current project root (or -Root), shareable with the repo.
      Both    = install both.

.PARAMETER Clients
    auto (default), all, or a comma-separated list.
      auto = only clients detected on this machine; falls back to 'agents'.
      all  = every supported client (creates directories as needed).
      Example: -Clients zcode,cursor,copilot,claude

.PARAMETER Root
    Project root for -Scope Project/Both. Default: current directory.

.PARAMETER SkillName
    Skill folder name. Default: wpf-ui-style.

.PARAMETER Source
    Skill source folder. Default: the folder containing this script.

.PARAMETER Force
    Overwrite existing skill folders / rule files. Shared files (AGENTS.md,
    global_rules.md) are backed up before the marked block is replaced.

.PARAMETER DryRun
    Print actions without writing anything.

.PARAMETER Cleanup
    Remove this skill's artifacts from clients that are not installed on this
    machine. Implied by -Clients auto unless -NoCleanup is passed.

.PARAMETER NoCleanup
    Skip the automatic cleanup pass under -Clients auto.

.PARAMETER Detect
    List detected clients and exit. A client counts as installed when its
    software is present (executable, install directory, uninstall entry, MSIX
    package) or when its config profile was written to recently. Directories
    that contain nothing but files this installer wrote are reported as stale
    installs instead.

.PARAMETER MaxAgeDays
    How recent a config profile must be to count as evidence that the client is
    still installed. A profile untouched for longer than this is a leftover of
    an uninstalled client. Default: 90.

.PARAMETER List
    List all supported clients and their install paths, then exit.

.EXAMPLE
    pwsh ./install.ps1 -Detect

.EXAMPLE
    pwsh ./install.ps1 -Scope Global -Clients auto

.EXAMPLE
    pwsh ./install.ps1 -Scope Project -Root "D:/Demo/YourApp" -Clients all

.EXAMPLE
    pwsh ./install.ps1 -Scope Both -Clients zcode,cursor,copilot,claude,workbuddy,trae-cn
#>
[CmdletBinding()]
param(
    [ValidateSet('Global', 'Project', 'Both')]
    [string]$Scope = 'Global',

    [string]$Clients = 'auto',

    [string]$Root = (Get-Location).Path,

    [string]$SkillName = 'wpf-ui-style',

    [string]$Source = '',

    [switch]$Force,


    [switch]$DryRun,

    [switch]$Cleanup,

    [switch]$NoCleanup,

    [ValidateRange(1, 3650)]
    [int]$MaxAgeDays = 90,

    [switch]$Detect,

    [switch]$List
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($Source)) {
    $scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Definition }
    if ([string]::IsNullOrWhiteSpace($scriptDir)) { $scriptDir = (Get-Location).Path }
    $Source = $scriptDir
}

# ---------------------------------------------------------------------------
# Client registry.
# Kind:
#   Skill       copy the skill folder into <Path>\<SkillName>
#   Rule        write a standalone rule file (optional frontmatter)
#   Instruction write a Copilot .instructions.md file (applyTo frontmatter)
#   Agent       append/replace a marked block in AGENTS.md / CONVENTIONS.md
#   Memory      append/replace a marked block in a global rules file
# BestEffort:
#   path is a community/observed convention; verify in the client if needed.
# ---------------------------------------------------------------------------
$Registry = [ordered]@{
    'agents' = @{
        Label = 'Generic Agent Skills'
        Detect = @("$Home/.agents")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.agents/skills" }
        )
        Project = @(
            @{ Kind = 'Skill'; Path = '.agents/skills' }
        )
    }

    'zcode' = @{
        Label = 'ZCode'
        Detect = @("$Home/.zcode")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.zcode/skills" }
        )
        Project = @(
            @{ Kind = 'Skill'; Path = '.zcode/skills' }
        )
    }

    'claude' = @{
        Label = 'Claude Code'
        Detect = @("$Home/.claude")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.claude/skills" }
        )
        Project = @(
            @{ Kind = 'Skill'; Path = '.claude/skills' }
        )
    }

    'copilot' = @{
        Label = 'GitHub Copilot / VS Code'
        Detect = @("$Home/.copilot", "$Home/.vscode")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.copilot/skills" },
            @{ Kind = 'Skill'; Path = "$Home/.agents/skills" }
        )
        Project = @(
            @{ Kind = 'Skill'; Path = '.github/skills' },
            @{ Kind = 'Skill'; Path = '.agents/skills' },
            @{ Kind = 'Instruction'; Path = '.github/instructions/wpf-ui-style.instructions.md' },
            @{ Kind = 'Agent'; Path = '.github/copilot-instructions.md' }
        )
    }

    'cursor' = @{
        Label = 'Cursor'
        Detect = @("$Home/.cursor")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.cursor/skills" },
            @{ Kind = 'Rule'; Path = "$Home/.cursor/rules/wpf-ui-style.mdc"; Frontmatter = 'cursor'; BestEffort = $true }
        )
        Project = @(
            @{ Kind = 'Skill'; Path = '.cursor/skills' },
            @{ Kind = 'Rule'; Path = '.cursor/rules/wpf-ui-style.mdc'; Frontmatter = 'cursor' },
            @{ Kind = 'Agent'; Path = 'AGENTS.md' }
        )
    }

    'windsurf' = @{
        Label = 'Windsurf / Codeium'
        Detect = @("$Home/.windsurf", "$Home/.codeium")
        Global = @(
            @{ Kind = 'Rule'; Path = "$Home/.windsurf/rules/wpf-ui-style.md"; BestEffort = $true },
            @{ Kind = 'Memory'; Path = "$Home/.codeium/windsurf/memories/global_rules.md" }
        )
        Project = @(
            @{ Kind = 'Rule'; Path = '.windsurf/rules/wpf-ui-style.md' }
        )
    }

    'cline' = @{
        Label = 'Cline'
        Detect = @("$Home/.cline")
        Global = @(
            @{ Kind = 'Rule'; Path = "$Home/Documents/Cline/Rules/wpf-ui-style.md" }
        )
        Project = @(
            @{ Kind = 'Rule'; Path = '.clinerules/wpf-ui-style.md' }
        )
    }

    'roo' = @{
        Label = 'Roo Code'
        Detect = @("$Home/.roo")
        Global = @(
            @{ Kind = 'Rule'; Path = "$Home/.roo/rules/wpf-ui-style.md" }
        )
        Project = @(
            @{ Kind = 'Rule'; Path = '.roo/rules/wpf-ui-style.md' }
        )
    }

    'kilo' = @{
        Label = 'Kilo Code'
        Detect = @("$Home/.kilocode")
        Global = @(
            @{ Kind = 'Rule'; Path = "$Home/.kilocode/rules/wpf-ui-style.md" }
        )
        Project = @(
            @{ Kind = 'Rule'; Path = '.kilocode/rules/wpf-ui-style.md' }
        )
    }

    'continue' = @{
        Label = 'Continue'
        Detect = @("$Home/.continue")
        Global = @(
            @{ Kind = 'Rule'; Path = "$Home/.continue/rules/wpf-ui-style.md" }
        )
        Project = @(
            @{ Kind = 'Rule'; Path = '.continue/rules/wpf-ui-style.md' }
        )
    }

    'codex' = @{
        Label = 'OpenAI Codex'
        Detect = @("$Home/.codex")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.codex/skills" },
            @{ Kind = 'Agent'; Path = "$Home/.codex/AGENTS.md" }
        )
        Project = @(
            @{ Kind = 'Agent'; Path = 'AGENTS.md' }
        )
    }

    'gemini' = @{
        Label = 'Gemini CLI'
        Detect = @("$Home/.gemini")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.gemini/config/skills" }
        )
        Project = @(
            @{ Kind = 'Skill'; Path = '.gemini/skills' }
        )
    }

    'qwen' = @{
        Label = 'Qwen Code'
        Detect = @("$Home/.qwen")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.qwen/skills" }
        )
        Project = @(
            @{ Kind = 'Skill'; Path = '.qwen/skills' }
        )
    }

    'grok' = @{
        Label = 'Grok CLI'
        Detect = @("$Home/.grok")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.grok/skills" },
            @{ Kind = 'Rule'; Path = "$Home/.grok/rules/wpf-ui-style.md" }
        )
        Project = @(
            @{ Kind = 'Skill'; Path = '.grok/skills' },
            @{ Kind = 'Rule'; Path = '.grok/rules/wpf-ui-style.md' }
        )
    }

    'kiro' = @{
        Label = 'Kiro'
        Detect = @("$Home/.kiro")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.kiro/skills" },
            @{ Kind = 'Rule'; Path = "$Home/.kiro/steering/wpf-ui-style.md" }
        )
        Project = @(
            @{ Kind = 'Skill'; Path = '.kiro/skills' },
            @{ Kind = 'Rule'; Path = '.kiro/steering/wpf-ui-style.md' }
        )
    }

    'trae' = @{
        Label = 'Trae'
        Detect = @("$Home/.trae")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.trae/skills" }
        )
        Project = @(
            @{ Kind = 'Skill'; Path = '.trae/skills' }
        )
    }

    'trae-cn' = @{
        Label = 'Trae CN'
        Detect = @("$Home/.trae-cn")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.trae-cn/skills" }
        )
        Project = @(
            @{ Kind = 'Skill'; Path = '.trae-cn/skills' }
        )
    }

    'workbuddy' = @{
        Label = 'WorkBuddy'
        Detect = @("$Home/.workbuddy")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.workbuddy/skills" }
        )
        Project = @(
            @{ Kind = 'Skill'; Path = '.workbuddy/skills' }
        )
    }

    'codebuddy' = @{
        Label = 'CodeBuddy'
        Detect = @("$Home/.codebuddy")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.codebuddy/skills" }
        )
        Project = @(
            @{ Kind = 'Skill'; Path = '.codebuddy/skills' }
        )
    }

    'codebuddycn' = @{
        Label = 'CodeBuddy CN'
        Detect = @("$Home/.codebuddycn")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.codebuddycn/skills" }
        )
        Project = @(
            @{ Kind = 'Skill'; Path = '.codebuddycn/skills' }
        )
    }

    'dsh' = @{
        Label = 'DeepSeek Harness (DSH)'
        Detect = @("$Home/.dsh")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.dsh/skills"; BestEffort = $true },
            @{ Kind = 'Agent'; Path = "$Home/.dsh/AGENTS.md" }
        )
        Project = @(
            @{ Kind = 'Agent'; Path = 'AGENTS.md' }
        )
    }

    'antigravity' = @{
        Label = 'Antigravity'
        Detect = @("$Home/.antigravity-ide")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.antigravity-ide/skills"; BestEffort = $true },
            @{ Kind = 'Agent'; Path = "$Home/.antigravity-ide/AGENTS.md" }
        )
        Project = @(
            @{ Kind = 'Agent'; Path = 'AGENTS.md' }
        )
    }

    'marscode' = @{
        Label = 'MarsCode'
        Detect = @("$Home/.marscode")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.marscode/skills"; BestEffort = $true },
            @{ Kind = 'Rule'; Path = "$Home/.marscode/rules/wpf-ui-style.md" }
        )
        Project = @(
            @{ Kind = 'Skill'; Path = '.marscode/skills'; BestEffort = $true },
            @{ Kind = 'Rule'; Path = '.marscode/rules/wpf-ui-style.md' }
        )
    }

    'zed' = @{
        Label = 'Zed'
        Detect = @("$Home/.config/zed")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.config/zed/skills"; BestEffort = $true },
            @{ Kind = 'Agent'; Path = "$Home/.config/zed/AGENTS.md" }
        )
        Project = @(
            @{ Kind = 'Skill'; Path = '.zed/skills'; BestEffort = $true },
            @{ Kind = 'Agent'; Path = 'AGENTS.md' }
        )
    }

    'aider' = @{
        Label = 'Aider'
        Detect = @("$Home/.aider")
        Global = @(
            @{ Kind = 'Agent'; Path = "$Home/.aider/CONVENTIONS.md" }
        )
        Project = @(
            @{ Kind = 'Agent'; Path = 'CONVENTIONS.md' }
        )
    }

    'openhands' = @{
        Label = 'OpenHands'
        Detect = @("$Home/.openhands")
        Global = @(
            @{ Kind = 'Skill'; Path = "$Home/.openhands/skills"; BestEffort = $true }
        )
        Project = @(
            @{ Kind = 'Skill'; Path = '.openhands/skills'; BestEffort = $true },
            @{ Kind = 'Agent'; Path = 'AGENTS.md' }
        )
    }

    'jetbrains' = @{
        Label = 'JetBrains AI Assistant'
        Detect = @("$Home/.aiassistant")
        Global = @(
            @{ Kind = 'Rule'; Path = "$Home/.aiassistant/rules/wpf-ui-style.md" }
        )
        Project = @(
            @{ Kind = 'Rule'; Path = '.aiassistant/rules/wpf-ui-style.md' }
        )
    }
}

# Positive evidence that the client software itself is installed:
#   * a command on PATH
#   * an app install directory that still contains the executable
#   * an uninstall-registry entry (Windows)
#   * an installed MSIX/AppX package (Windows)
# A config directory on its own is never enough: `-Clients all` creates one for
# every supported client, and uninstalling an app usually leaves its profile
# folder behind. Config data only counts when it is recent (see -MaxAgeDays).
# ---------------------------------------------------------------------------
$LocalPrograms = if ($env:LOCALAPPDATA) { Join-Path $env:LOCALAPPDATA 'Programs' } else { $null }
$Probes = @{
    'zcode'      = @{ Commands = @('zcode'); AppDirs = @("$LocalPrograms\ZCode"); Uninstall = @('^ZCode') }
    'claude'     = @{ Commands = @('claude'); AppDirs = @("$Home\.claude\local"); Uninstall = @('Claude Code') }
    'copilot'    = @{ Commands = @('copilot'); Uninstall = @('Microsoft Visual Studio Code') }
    'cursor'     = @{ Commands = @('cursor'); AppDirs = @("$LocalPrograms\cursor"); Uninstall = @('^Cursor') }
    'windsurf'   = @{ Commands = @('windsurf'); AppDirs = @("$LocalPrograms\Windsurf"); Uninstall = @('Windsurf') }
    'cline'      = @{ Uninstall = @('Cline') }
    'roo'        = @{ Uninstall = @('Roo Code') }
    'kilo'       = @{ Uninstall = @('Kilo Code') }
    'continue'   = @{ Uninstall = @('^Continue') }
    'codex'      = @{ Commands = @('codex'); AppDirs = @("$env:LOCALAPPDATA\OpenAI\Codex", "$env:APPDATA\Codex"); Uninstall = @('Codex'); Appx = @('OpenAI.Codex') }
    'gemini'     = @{ Commands = @('gemini'); Uninstall = @('Gemini CLI') }
    'qwen'       = @{ Commands = @('qwen'); Uninstall = @('Qwen') }
    'grok'       = @{ Commands = @('grok'); AppDirs = @("$Home\.grok\bin"); Uninstall = @('Grok Bot') }
    'kiro'       = @{ Commands = @('kiro'); AppDirs = @("$LocalPrograms\Kiro"); Uninstall = @('Kiro') }
    'trae'       = @{ Commands = @('trae'); AppDirs = @("$LocalPrograms\Trae"); Uninstall = @('TRAE') }
    'trae-cn'    = @{ Commands = @('trae-cn'); AppDirs = @("$LocalPrograms\TRAE CN", "$LocalPrograms\Trae CN"); Uninstall = @('TRAE') }
    'workbuddy'  = @{ AppDirs = @("$LocalPrograms\WorkBuddy", "$LocalPrograms\WorkBuddyAI"); Uninstall = @('WorkBuddy'); Appx = @('WorkBuddy') }
    'codebuddy'  = @{ AppDirs = @("$LocalPrograms\CodeBuddy"); Uninstall = @('^CodeBuddy') }
    'codebuddycn'= @{ AppDirs = @("$LocalPrograms\CodeBuddy CN"); Uninstall = @('CodeBuddy CN') }
    'dsh'        = @{ AppDirs = @("$LocalPrograms\DeepSeek Harness"); Uninstall = @('DeepSeek Harness') }
    'antigravity'= @{ AppDirs = @("$LocalPrograms\antigravity"); Uninstall = @('Antigravity') }
    'marscode'   = @{ AppDirs = @("$LocalPrograms\MarsCode"); Uninstall = @('MarsCode') }
    'zed'        = @{ Commands = @('zed'); AppDirs = @("$LocalPrograms\Zed"); Uninstall = @('^Zed') }
    'aider'      = @{ Commands = @('aider'); Uninstall = @('Aider') }
    'openhands'  = @{ Commands = @('openhands'); Uninstall = @('OpenHands') }
    'jetbrains'  = @{ Uninstall = @('JetBrains') }
}

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------
function Resolve-TargetPath {
    param([string]$Path, [string]$BaseRoot)

    if ([string]::IsNullOrWhiteSpace($Path)) { return $Path }
    $value = $Path.Trim()

    if ($value.StartsWith('~')) {
        $value = Join-Path $Home $value.Substring(1).TrimStart('\', '/')
    }
    if (-not [IO.Path]::IsPathRooted($value)) {
        $value = Join-Path $BaseRoot $value
    }
    return [IO.Path]::GetFullPath($value)
}

function Backup-File {
    param([string]$Path, [string]$BackupRoot)
    $stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    if ($BackupRoot) {
        # Skill folders live inside a directory the client scans for skills.
        # A sibling "<name>.bak-<stamp>" folder would be picked up as a stale
        # skill, so keep backups outside that tree.
        New-Item -ItemType Directory -Force -Path $BackupRoot | Out-Null
        $backup = Join-Path $BackupRoot ((Split-Path -Leaf $Path) + '.' + $stamp)
    }
    else {
        $backup = "$Path.bak-$stamp"
    }
    # -Recurse is required for directories; without it the backup is an empty
    # folder and a failed reinstall cannot be rolled back.
    Copy-Item -LiteralPath $Path -Destination $backup -Recurse -Force

    # Keep only the newest few backups of the same target; older ones pile up
    # in the client's rule directory and clutter it.
    $dir = Split-Path -Parent $Path
    $leaf = Split-Path -Leaf $Path
    $siblings = Get-ChildItem -LiteralPath $dir -Force -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like ($leaf + '.bak-*') } |
        Sort-Object Name -Descending
    for ($i = 3; $i -lt $siblings.Count; $i++) {
        Remove-Item -LiteralPath $siblings[$i].FullName -Recurse -Force -ErrorAction SilentlyContinue
    }

    return $backup
}

# Removes stale skill-folder backups left by older installer versions.
# Such folders sit next to the real skill and get advertised as outdated skills.
function Remove-StaleSkillBackups {
    param([string]$SkillsRoot, [string]$Name, [bool]$Preview)

    if (-not (Test-Path -LiteralPath $SkillsRoot)) { return @() }
    $stale = Get-ChildItem -LiteralPath $SkillsRoot -Directory -Force -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like ($Name + '.bak-*') }
    $removed = @()
    foreach ($dir in $stale) {
        if (-not $Preview) {
            Remove-Item -LiteralPath $dir.FullName -Recurse -Force -ErrorAction SilentlyContinue
        }
        $removed += $dir.FullName
    }
    return $removed
}

# Directories that must never be copied into an installed skill folder.
# `dist` holds the built preview executable and launcher .bat files (~450 KB);
# it is a local build artifact, not part of the skill.
$script:SkillCopyExcludeDirs = @('.git', '.vs', '.idea', 'bin', 'obj', '__pycache__', '.zcode', 'dist', '.tmp-shots', '.tmp')

function Copy-SkillTree {
    param([string]$SourcePath, [string]$TargetPath)

    $sourceRoot = (Resolve-Path -LiteralPath $SourcePath).ProviderPath.TrimEnd('\')
    New-Item -ItemType Directory -Force -Path $TargetPath | Out-Null

    foreach ($file in Get-ChildItem -LiteralPath $sourceRoot -Recurse -File -Force) {
        $relative = $file.FullName.Substring($sourceRoot.Length).TrimStart('\')
        $parts = $relative -split '[\\/]'
        $excluded = $false
        for ($i = 0; $i -lt $parts.Count - 1; $i++) {
            if ($script:SkillCopyExcludeDirs -contains $parts[$i].ToLowerInvariant()) {
                $excluded = $true
                break
            }
        }
        if ($excluded) { continue }
        if ($file.Name -like '*.bak-*' -or $file.Name -like '*.user') { continue }

        $destination = Join-Path $TargetPath $relative
        $destinationDir = Split-Path -Parent $destination
        if (-not (Test-Path -LiteralPath $destinationDir)) {
            New-Item -ItemType Directory -Force -Path $destinationDir | Out-Null
        }
        Copy-Item -LiteralPath $file.FullName -Destination $destination -Force
    }
}

function Copy-SkillFolder {
    param(
        [string]$SourcePath,
        [string]$TargetPath,
        [string]$Name,
        [bool]$Overwrite,
        [bool]$Preview,
        [string]$BackupRoot
    )

    $target = Join-Path $TargetPath $Name

    if ([string]::Equals($target, $SourcePath, [StringComparison]::OrdinalIgnoreCase)) {
        return @{ Status = 'SKIP'; Detail = 'target is the source folder' }
    }
    if ($target.StartsWith($SourcePath.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)) {
        return @{ Status = 'SKIP'; Detail = 'target is inside source' }
    }

    # Clear backups that older installer versions left inside the skills root.
    $null = Remove-StaleSkillBackups $TargetPath $Name $Preview

    if (Test-Path -LiteralPath $target) {
        if (-not $Overwrite) {
            return @{ Status = 'SKIP'; Detail = 'exists (use -Force to overwrite)' }
        }
        if (-not $Preview) {
            $backup = Backup-File $target $BackupRoot
            Remove-Item -LiteralPath $target -Recurse -Force
        }
        $detail = 'overwritten (backup created)'
    }
    else {
        $detail = 'installed'
    }

    if (-not $Preview) {
        New-Item -ItemType Directory -Force -Path $TargetPath | Out-Null
        Copy-SkillTree $SourcePath $target
    }

    return @{ Status = if ($Preview) { 'DRY' } else { 'OK' }; Detail = $detail }
}

function Write-RuleFile {
    param(
        [string]$Path,
        [string]$Content,
        [bool]$Overwrite,
        [bool]$Shared,
        [bool]$Preview
    )

    $parent = Split-Path -Parent $Path
    if ($parent -and -not (Test-Path -LiteralPath $parent)) {
        if (-not $Preview) { New-Item -ItemType Directory -Force -Path $parent | Out-Null }
    }

    if (Test-Path -LiteralPath $Path) {
        $existing = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
        $hasMarker = $existing -match '<!-- BEGIN wpf-ui-style -->'

        if ($hasMarker -and $Shared) {
            $pattern = '(?s)<!-- BEGIN wpf-ui-style -->.*?<!-- END wpf-ui-style -->'
            $newContent = [regex]::Replace($existing, $pattern, $Content)
            if (-not $Preview) {
                $null = Backup-File $Path
                [IO.File]::WriteAllText($Path, $newContent, (New-Object Text.UTF8Encoding($false)))
            }
            return @{ Status = if ($Preview) { 'DRY' } else { 'OK' }; Detail = 'marker block replaced' }
        }

        if ($hasMarker -and -not $Shared) {
            # Generated rule/instruction file: replace the whole file so frontmatter
            # is not duplicated on re-install.
            if (-not $Preview) {
                $null = Backup-File $Path
                [IO.File]::WriteAllText($Path, $Content, (New-Object Text.UTF8Encoding($false)))
            }
            return @{ Status = if ($Preview) { 'DRY' } else { 'OK' }; Detail = 'rule file replaced' }
        }

        if ($Shared) {
            $newContent = $existing.TrimEnd() + "`r`n`r`n" + $Content
            if (-not $Preview) {
                $null = Backup-File $Path
                [IO.File]::WriteAllText($Path, $newContent, (New-Object Text.UTF8Encoding($false)))
            }
            return @{ Status = if ($Preview) { 'DRY' } else { 'OK' }; Detail = 'marker block appended' }
        }

        if (-not $Overwrite) {
            return @{ Status = 'SKIP'; Detail = 'exists (use -Force to overwrite)' }
        }

        if (-not $Preview) {
            $null = Backup-File $Path
            [IO.File]::WriteAllText($Path, $Content, (New-Object Text.UTF8Encoding($false)))
        }
        return @{ Status = if ($Preview) { 'DRY' } else { 'OK' }; Detail = 'overwritten (backup created)' }
    }

    if (-not $Preview) {
        [IO.File]::WriteAllText($Path, $Content, (New-Object Text.UTF8Encoding($false)))
    }
    return @{ Status = if ($Preview) { 'DRY' } else { 'OK' }; Detail = 'created' }
}

function Get-RuleBody {
    param([string]$SkillPath)
    $body = @'
<!-- BEGIN wpf-ui-style -->
# WPF UI Skills

When editing WPF/XAML in this project, follow the `wpf-ui-style` skill.

- Skill: `{{SKILL_PATH}}/SKILL.md`
- Theme: `{{SKILL_PATH}}/Theme/Theme.xaml`
- State matrix: `{{SKILL_PATH}}/docs/02-control-states.md`
- Layout: `{{SKILL_PATH}}/docs/03-layout.md`
- Pitfalls: `{{SKILL_PATH}}/docs/04-common-pitfalls.md`

Non-negotiable rules:

1. Every background state change must also set `Foreground`.
2. Never use `Opacity` for disabled; use `Brush.Control.Disabled` + `Brush.Text.Disabled`.
3. White text only on Accent / Accent.Hover / Accent.Pressed / Danger / Danger.Hover / Danger.Pressed / Tooltip dark backgrounds.
4. Selected + hover must use `MultiTrigger`; selected list rows use `Brush.Selection` + `Brush.Text.Primary`.
5. Multiline TextBox and Grid: no forced MinHeight (no MinHeight >= 180). Forms use `Height="Auto"` +
   `VerticalAlignment="Top"`; tables/logs/comparison panels use `VerticalAlignment="Stretch"` in a dedicated
   `Grid` `*` row. Only logs/lists/tables/ScrollViewer may occupy a `*` row. Never use `Height="*"` on a control.
6. Before finishing, run `{{SKILL_PATH}}/tools/ContrastCheck.ps1`,
   `{{SKILL_PATH}}/tools/LayoutCheck.ps1 -Root <project>`, and
   `{{SKILL_PATH}}/tools/UiTest.ps1 -Window editable-table -TestClicks`.
7. No truncation, no occlusion: label columns use `Width="Auto"` + `MinWidth`, input columns
   use `Width="*"` + `MinWidth` (never a hardcoded `Width` on a TextBox); every ellipsis needs a
   `ToolTip`; form dialogs use `SizeToContent="Height"` + `MinHeight` with a single `ScrollViewer`,
   while DataGrid/workbench viewports use explicit initial size (e.g. 1180x680) with low `MinHeight` (360-420);
   the bottom action bar owns its own row (long status text on its own row above it);
   no negative `Margin`, no `Canvas` absolute layout.
8. No multi-tab navigation in a window: use one window per task, or `GroupBox`/`Expander`
   sections. If a `TabControl` is used, give it a fixed `Height` and scroll inside the tab,
   so switching tabs never changes the window height.
9. Compact by default: body text 12px, control height 32px, form row spacing 3px, window
   padding 12px. Do not enlarge fonts or spacing to create hierarchy — show more rows and
   columns per screen instead.
10. Strict 4-zone layout and single action entry: Zone 1 Header (title + global Save/Export),
    Zone 2 Filter (pure search inputs, <=4 filters), Zone 3 Workspace/Toolbar (batch actions enabled
    only with selection; view toggles), Zone 4 Footer (metrics & pagination). Never duplicate
    actions like "Export" or "Save" across multiple zones.
11. Zero redundancy text: no AI tutorial chatter (never write "Here you can..." or "Demo...").
    Keep labels and table column headers crisp and non-repetitive.
<!-- END wpf-ui-style -->
'@
    $display = $SkillPath.Replace('\', '/')
    return $body.Replace('{{SKILL_PATH}}', $display) + "`r`n"
}

function Get-RuleContent {
    param([string]$SkillPath, [string]$Frontmatter)
    $body = Get-RuleBody $SkillPath
    switch ($Frontmatter) {
        'cursor' {
            return @"
---
description: WPF UI Skills rules (tokens, states, layout, contrast).
globs: "**/*.xaml"
alwaysApply: false
---

$body
"@
        }
        'copilot' {
            return @"
---
applyTo: "**/*.xaml"
---

$body
"@
        }
        default { return $body }
    }
}

function Get-AgentBlock {
    param([string]$SkillPath)
    $block = @'
<!-- BEGIN wpf-ui-style -->
## WPF UI Skills

When editing WPF/XAML in this repository, read and follow:
`{{SKILL_PATH}}/SKILL.md`

Key rules:

- Every background state change also sets `Foreground`.
- No `Opacity` for disabled; use disabled background/foreground tokens.
- White text only on Accent/Danger/Tooltip dark backgrounds.
- Selected + hover uses `MultiTrigger`.
- Multiline TextBox: no global `MinHeight`; use `VerticalAlignment="Stretch"` in a `*` row for
  comparison/log panels, or `VerticalAlignment="Top"` in an `Auto` row for forms.
- No truncation, no occlusion: label columns `Auto` + `MinWidth`, input columns `*` + `MinWidth`,
  ellipsis always with a `ToolTip`, window `SizeToContent="Height"` + one `ScrollViewer`,
  bottom action bar on its own row, no negative `Margin` and no `Canvas` layout.
- Run `{{SKILL_PATH}}/tools/ContrastCheck.ps1` and
  `{{SKILL_PATH}}/tools/LayoutCheck.ps1 -Root <project>` before finishing.
<!-- END wpf-ui-style -->
'@
    $display = $SkillPath.Replace('\', '/')
    return $block.Replace('{{SKILL_PATH}}', $display) + "`r`n"
}

# ---------------------------------------------------------------------------
# Client detection.
# A config directory on its own is not proof that the client is installed:
# `-Clients all` creates one for every supported client. A directory counts as
# a real client only when it holds something this installer did not write.
# ---------------------------------------------------------------------------
$script:SharedRuleNames = @('AGENTS.md', 'CONVENTIONS.md', 'copilot-instructions.md', 'global_rules.md')

function Test-InstallerArtifact {
    param([string]$Path)

    $leaf = Split-Path -Leaf $Path
    $base = $leaf -replace '\.bak-\d{8}-\d{6}$', ''

    if ($base -eq $SkillName) {
        return (Test-Path -LiteralPath (Join-Path $Path 'SKILL.md') -PathType Leaf)
    }

    if ($base -eq ($SkillName + '.md') -or
        $base -eq ($SkillName + '.mdc') -or
        $base -eq ($SkillName + '.instructions.md')) {
        return $true
    }

    if ($script:SharedRuleNames -contains $base) {
        if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $false }
        $text = Get-Content -LiteralPath $Path -Raw -ErrorAction SilentlyContinue
        if ($null -eq $text -or $text -notmatch '<!-- BEGIN wpf-ui-style -->') { return $false }
        # A shared file is "ours" only when our marked block is all it contains;
        # any extra content means the user owns that file too.
        $rest = [regex]::Replace($text, '(?s)<!-- BEGIN wpf-ui-style -->.*?<!-- END wpf-ui-style -->', '').Trim()
        return [string]::IsNullOrWhiteSpace($rest)
    }

    return $false
}

function Test-ForeignContent {
    param([string]$Path)

    foreach ($child in @(Get-ChildItem -LiteralPath $Path -Force -ErrorAction SilentlyContinue)) {
        if (Test-InstallerArtifact $child.FullName) { continue }
        if ($child.PSIsContainer) {
            # `skills`, `rules`, `steering`... hold only our files when the
            # whole tree was created by this installer.
            if (Test-ForeignContent $child.FullName) { return $true }
            continue
        }
        return $true
    }
    return $false
}

# True for any file this installer wrote, including a marked block inside a
# file the user also owns. Used to decide whether a file counts as client
# activity: our own write must never make a client look "recently used".
function Test-WrittenByInstaller {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $false }

    $leaf = Split-Path -Leaf $Path
    $base = $leaf -replace '\.bak-\d{8}-\d{6}$', ''

    if ($base -eq ($SkillName + '.md') -or
        $base -eq ($SkillName + '.mdc') -or
        $base -eq ($SkillName + '.instructions.md')) {
        return $true
    }
    if ($script:SharedRuleNames -contains $base) {
        $text = Get-Content -LiteralPath $Path -Raw -ErrorAction SilentlyContinue
        return ($null -ne $text -and $text -match '<!-- BEGIN wpf-ui-style -->')
    }
    return $false
}

# Newest file in the tree that this installer did not write, or $null when the
# tree holds nothing but our own artifacts. Used to tell a live client profile
# from a leftover one. The scan stops as soon as it finds a file that is
# already newer than the cutoff, so live clients stay cheap to check.
function Get-NewestForeignFile {
    param([string]$Path, [datetime]$Cutoff)

    $newest = $null
    $queue = New-Object System.Collections.Queue
    $queue.Enqueue(@($Path, 0))

    while ($queue.Count -gt 0) {
        $entry = $queue.Dequeue()
        $dir = $entry[0]
        $depth = [int]$entry[1]
        if ($depth -gt 6) { continue }

        foreach ($child in @(Get-ChildItem -LiteralPath $dir -Force -ErrorAction SilentlyContinue)) {
            if ($child.PSIsContainer) {
                if (Test-InstallerArtifact $child.FullName) { continue }
                $queue.Enqueue(@($child.FullName, $depth + 1))
                continue
            }
            if (Test-InstallerArtifact $child.FullName) { continue }
            if (Test-WrittenByInstaller $child.FullName) { continue }
            if ($null -eq $newest -or $child.LastWriteTime -gt $newest.LastWriteTime) { $newest = $child }
            # Already newer than the cutoff: nothing older can change the
            # verdict, so stop walking this branch.
            if ($newest.LastWriteTime -ge $Cutoff) { return $newest }
        }
    }
    return $newest
}

# ---------------------------------------------------------------------------
# Windows installation evidence.
# An app directory that still holds its executable, an uninstall-registry
# entry, or an MSIX package all prove the client software is present. A
# directory that only holds leftovers proves nothing.
# ---------------------------------------------------------------------------
$script:UninstallIndex = $null
$script:AppxIndex = $null

function Get-UninstallIndex {
    if ($null -ne $script:UninstallIndex) { return $script:UninstallIndex }

    $entries = @()
    $roots = @(
        'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall',
        'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall',
        'HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall'
    )
    foreach ($root in $roots) {
        if (-not (Test-Path -LiteralPath $root)) { continue }
        foreach ($key in @(Get-ChildItem -LiteralPath $root -ErrorAction SilentlyContinue)) {
            $props = Get-ItemProperty -LiteralPath $key.PSPath -ErrorAction SilentlyContinue
            if ($props -and $props.DisplayName) {
                $entries += [pscustomobject]@{
                    Name = $props.DisplayName
                    Location = $props.InstallLocation
                }
            }
        }
    }
    $script:UninstallIndex = $entries
    return $entries
}

function Get-AppxIndex {
    if ($null -ne $script:AppxIndex) { return $script:AppxIndex }

    $names = @()
    if ($env:OS -eq 'Windows_NT') {
        try {
            $raw = & powershell.exe -NoProfile -Command `
                "Get-AppxPackage | ForEach-Object { `$_.Name }" 2>$null
            foreach ($line in @($raw)) {
                $name = "$line".Trim()
                if ($name) { $names += $name }
            }
        }
        catch {
            $names = @()
        }
    }
    $script:AppxIndex = $names
    return $names
}

# An app directory only proves the client is installed when it still holds an
# executable. A folder kept after an uninstall only holds leftovers.
function Test-AppDirHasExecutable {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Container)) { return $false }
    foreach ($exe in @(Get-ChildItem -LiteralPath $Path -Filter *.exe -File -Force -ErrorAction SilentlyContinue)) {
        return $true
    }
    # Electron / VS Code forks keep the binary in a nested folder.
    foreach ($child in @(Get-ChildItem -LiteralPath $Path -Directory -Force -ErrorAction SilentlyContinue)) {
        if (Test-AppDirHasExecutable $child.FullName) { return $true }
    }
    return $false
}

function Get-ClientEvidence {
    param([string]$Name)

    $entry = $Registry[$Name]
    if ($null -eq $entry) { return @() }

    $evidence = @()
    $probe = $Probes[$Name]
    if ($probe) {
        foreach ($cmd in @($probe.Commands)) {
            if ($cmd -and (Get-Command $cmd -ErrorAction SilentlyContinue)) { $evidence += "command $cmd" }
        }
        foreach ($dir in @($probe.AppDirs)) {
            if (-not $dir) { continue }
            $resolved = Resolve-TargetPath $dir $Root
            if (Test-AppDirHasExecutable $resolved) { $evidence += "app $resolved" }
        }
        foreach ($pattern in @($probe.Uninstall)) {
            if (-not $pattern) { continue }
            foreach ($item in @(Get-UninstallIndex)) {
                if ($item.Name -match $pattern) { $evidence += "installed $($item.Name)"; break }
            }
        }
        foreach ($pattern in @($probe.Appx)) {
            if (-not $pattern) { continue }
            foreach ($appxName in @(Get-AppxIndex)) {
                if ($appxName -like "*$pattern*") { $evidence += "appx $appxName"; break }
            }
        }
    }

    # A config profile proves the client ran, but only while it is still being
    # written to. An old profile is a leftover of an uninstalled client.
    $cutoff = (Get-Date).AddDays(-$MaxAgeDays)
    foreach ($path in @($entry.Detect)) {
        $resolved = Resolve-TargetPath $path $Root
        if (-not (Test-Path -LiteralPath $resolved -PathType Container)) { continue }
        $newest = Get-NewestForeignFile $resolved $cutoff
        if ($null -eq $newest) { continue }
        $ageDays = ((Get-Date) - $newest.LastWriteTime).TotalDays
        if ($ageDays -le $MaxAgeDays) {
            $evidence += "data $resolved (written $([int]$ageDays)d ago)"
        }
    }
    return $evidence
}

function Test-ClientInstalled {
    param([string]$Name)
    return (@(Get-ClientEvidence $Name).Count -gt 0)
}

# Backwards-compatible name used by the rest of the script.
function Test-Detected {
    param([string]$Name)
    return (Test-ClientInstalled $Name)
}

function Get-InstalledArtifactPaths {
    param([string]$Name, [string]$ScopeName)

    $entry = $Registry[$Name]
    if ($null -eq $entry) { return @() }
    $base = if ($ScopeName -eq 'Global') { $Home } else { $Root }
    $specs = if ($ScopeName -eq 'Global') { $entry.Global } else { $entry.Project }
    $found = @()

    foreach ($spec in $specs) {
        $target = Resolve-TargetPath $spec.Path $base
        if ($spec.Kind -eq 'Skill') {
            $skillDir = Join-Path $target $SkillName
            if (Test-Path -LiteralPath (Join-Path $skillDir 'SKILL.md')) { $found += $skillDir }
        }
        elseif ((Test-Path -LiteralPath $target -PathType Leaf) -and (Test-InstallerArtifact $target)) {
            $found += $target
        }
    }
    return $found
}

# Removes empty directories that existed only for this skill: e.g. ~/.roo/rules
# left behind after the rule file is deleted. Stops at $StopAt and never touches
# a directory that still has content.
function Remove-EmptyParents {
    param([string]$Path, [string]$StopAt, [bool]$Preview)

    $stop = $StopAt.TrimEnd('\')
    $current = $Path
    while ($current -and (Test-Path -LiteralPath $current -PathType Container)) {
        $trimmed = $current.TrimEnd('\')
        if ($trimmed -eq $stop -or -not $trimmed.StartsWith($stop + '\', [StringComparison]::OrdinalIgnoreCase)) { break }
        if (@(Get-ChildItem -LiteralPath $current -Force -ErrorAction SilentlyContinue).Count -gt 0) { break }
        if (-not $Preview) {
            try { Remove-Item -LiteralPath $current -Force -ErrorAction Stop } catch { break }
        }
        $script:CleanupPruned += $current
        $current = Split-Path -Parent $current
    }
}

# Deletes the skill folder / rule file / marked block this installer wrote for
# one client. Anything that cannot be verified as ours is left untouched.
function Remove-ClientArtifacts {
    param([string]$Name, [string]$ScopeName, [bool]$Preview)

    $entry = $Registry[$Name]
    if ($null -eq $entry) { return @() }
    $base = if ($ScopeName -eq 'Global') { $Home } else { $Root }
    $specs = if ($ScopeName -eq 'Global') { $entry.Global } else { $entry.Project }
    $removed = @()

    foreach ($spec in $specs) {
        $target = Resolve-TargetPath $spec.Path $base

        if ($spec.Kind -eq 'Skill') {
            $skillDir = Join-Path $target $SkillName
            if (Test-Path -LiteralPath (Join-Path $skillDir 'SKILL.md')) {
                if (-not $Preview) { Remove-Item -LiteralPath $skillDir -Recurse -Force }
                $removed += $skillDir
                Remove-EmptyParents $target $base $Preview
            }
            foreach ($bak in @(Get-ChildItem -LiteralPath $target -Force -ErrorAction SilentlyContinue |
                    Where-Object { $_.Name -like ($SkillName + '.bak-*') })) {
                if (-not $Preview) { Remove-Item -LiteralPath $bak.FullName -Recurse -Force -ErrorAction SilentlyContinue }
                $removed += $bak.FullName
            }
            continue
        }

        if (-not (Test-Path -LiteralPath $target -PathType Leaf)) { continue }
        $text = Get-Content -LiteralPath $target -Raw -ErrorAction SilentlyContinue
        if ($null -eq $text -or $text -notmatch '<!-- BEGIN wpf-ui-style -->') { continue }

        if ($spec.Kind -eq 'Agent' -or $spec.Kind -eq 'Memory') {
            $stripped = [regex]::Replace($text, '(?s)\r?\n?<!-- BEGIN wpf-ui-style -->.*?<!-- END wpf-ui-style -->\r?\n?', "`r`n").Trim()
            if ([string]::IsNullOrWhiteSpace($stripped)) {
                if (-not $Preview) { Remove-Item -LiteralPath $target -Force }
                $removed += $target
            }
            else {
                if (-not $Preview) {
                    # Keep the original mtime: this is a cleanup of our own
                    # block, not client activity, and a refreshed timestamp
                    # would make an uninstalled client look recently used.
                    $keep = (Get-Item -LiteralPath $target).LastWriteTime
                    [IO.File]::WriteAllText($target, $stripped + "`r`n", (New-Object Text.UTF8Encoding($false)))
                    (Get-Item -LiteralPath $target).LastWriteTime = $keep
                }
                $removed += "$target (block removed)"
            }
        }
        else {
            if (-not $Preview) { Remove-Item -LiteralPath $target -Force }
            $removed += $target
        }

        $parent = Split-Path -Parent $target
        foreach ($bak in @(Get-ChildItem -LiteralPath $parent -Force -ErrorAction SilentlyContinue |
                Where-Object { $_.Name -like ((Split-Path -Leaf $target) + '.bak-*') })) {
            if (-not $Preview) { Remove-Item -LiteralPath $bak.FullName -Recurse -Force -ErrorAction SilentlyContinue }
            $removed += $bak.FullName
        }
        Remove-EmptyParents $parent $base $Preview
    }
    return $removed
}

# ---------------------------------------------------------------------------
# Validate source
# ---------------------------------------------------------------------------
if (-not (Test-Path -LiteralPath (Join-Path $Source 'SKILL.md'))) {
    Write-Host "SKILL.md not found in source: $Source" -ForegroundColor Red
    exit 1
}
$Source = (Resolve-Path -LiteralPath $Source).Path
$Root = (Resolve-Path -LiteralPath $Root).Path

# ---------------------------------------------------------------------------
# List / detect
# ---------------------------------------------------------------------------
if ($List) {
    Write-Host ''
    Write-Host 'Supported clients' -ForegroundColor Cyan
    Write-Host ''
    $rows = foreach ($name in $Registry.Keys) {
        $entry = $Registry[$name]
        $globalSkills = ($entry.Global | Where-Object { $_.Kind -eq 'Skill' } |
            ForEach-Object { Resolve-TargetPath $_.Path $Home }) -join '; '
        $projectSkills = ($entry.Project | Where-Object { $_.Kind -eq 'Skill' } |
            ForEach-Object { Resolve-TargetPath $_.Path $Root }) -join '; '
        [pscustomobject]@{
            Client = $name
            Label = $entry.Label
            GlobalSkill = $globalSkills
            ProjectSkill = $projectSkills
        }
    }
    $rows | Format-Table -AutoSize | Out-String | Write-Host
    Write-Host 'Rule/Agent-only clients have empty skill columns; they receive a rule file or AGENTS.md block.' -ForegroundColor DarkGray
    exit 0
}

if ($Detect) {
    Write-Host ''
    Write-Host 'Detected clients' -ForegroundColor Cyan
    Write-Host ''
    $found = @()
    foreach ($name in $Registry.Keys) {
        if ($name -eq 'agents') { continue }
        $evidence = @(Get-ClientEvidence $name)
        if ($evidence.Count -eq 0) { continue }
        $found += $name
        Write-Host ("  [x] {0,-12} {1}" -f $name, $Registry[$name].Label) -ForegroundColor Green
        Write-Host ("      " + ($evidence -join '; ')) -ForegroundColor DarkGray
    }
    if ($found.Count -eq 0) {
        Write-Host '  (none detected; use -Clients all or specify names)' -ForegroundColor Yellow
    }

    # A client that is gone but still has our skill folder is not "installed";
    # report it so the stale copy can be removed instead of silently reused.
    $stale = @()
    foreach ($name in $Registry.Keys) {
        if ($name -eq 'agents' -or $found -contains $name) { continue }
        $paths = @(Get-InstalledArtifactPaths $name 'Global') + @(Get-InstalledArtifactPaths $name 'Project')
        if ($paths.Count -eq 0) { continue }
        $stale += $name
        Write-Host ("  [!] {0,-12} {1}  (skill present, client not found)" -f $name, $Registry[$name].Label) -ForegroundColor Yellow
    }
    if ($stale.Count -gt 0) {
        Write-Host ''
        Write-Host 'Stale installs are removed by "pwsh ./install.ps1 -Cleanup".' -ForegroundColor Yellow
    }

    Write-Host ''
    Write-Host ('Detected: ' + ($found -join ', ')) -ForegroundColor DarkGray
    if ($stale.Count -gt 0) {
        Write-Host ('Stale   : ' + ($stale -join ', ')) -ForegroundColor DarkGray
    }
    exit 0
}

# ---------------------------------------------------------------------------
# Select clients
# ---------------------------------------------------------------------------
# `-Cleanup` without `-Clients` means "only clean up stale installs"; passing
# a client selection (or relying on the `auto` default) installs as well.
$cleanupOnly = $Cleanup -and -not $PSBoundParameters.ContainsKey('Clients')

$selected = @()
if ($cleanupOnly) {
    $selected = @()
}
elseif ($Clients -eq 'all') {
    $selected = @($Registry.Keys)
}
elseif ($Clients -eq 'auto') {
    $selected = @($Registry.Keys | Where-Object { Test-Detected $_ })
    if ($selected.Count -eq 0) {
        Write-Host 'No client detected; falling back to generic Agent Skills (agents).' -ForegroundColor Yellow
        $selected = @('agents')
    }
}
else {
    $selected = @($Clients -split '[,; ]+' | Where-Object { $_ })
}

foreach ($name in $selected) {
    if (-not $Registry.Contains($name)) {
        Write-Host "Unknown client: $name (use -List to see supported names)" -ForegroundColor Red
        exit 1
    }
}

# `auto` detection often matches both a native client (ZCode/Claude/Copilot)
# and the generic ~/.agents root. Installing both makes those clients list the
# same skill twice, so prefer the native directory when one is available.
if ($Clients -eq 'auto' -and -not $cleanupOnly) {
    $selected = @($selected | Where-Object { $_ -ne 'agents' })
    if ($selected.Count -eq 0) { $selected = @('agents') }
}

# ---------------------------------------------------------------------------
# Build actions
# ---------------------------------------------------------------------------
$scopes = @()
if ($Scope -eq 'Global' -or $Scope -eq 'Both') { $scopes += 'Global' }
if ($Scope -eq 'Project' -or $Scope -eq 'Both') { $scopes += 'Project' }

$actions = @()
foreach ($scopeName in $scopes) {
    foreach ($name in $selected) {
        $entry = $Registry[$name]
        $clientActions = if ($scopeName -eq 'Global') { $entry.Global } else { $entry.Project }
        foreach ($action in $clientActions) {
            $actions += [pscustomobject]@{
                Client = $name
                Label = $entry.Label
                Scope = $scopeName
                Kind = $action.Kind
                RawPath = $action.Path
                Frontmatter = $action.Frontmatter
                BestEffort = [bool]$action.BestEffort
            }
        }
    }
}

# Under `auto`, drop the generic ~/.agents/skills fallback for a scope when a
# native skill directory is also being installed (Copilot lists both). Keeping
# both makes the client show the same skill twice.
if ($Clients -eq 'auto') {
    foreach ($scopeName in $scopes) {
        $scopeSkills = @($actions | Where-Object { $_.Scope -eq $scopeName -and $_.Kind -eq 'Skill' })
        $nativeSkills = @($scopeSkills | Where-Object { $_.RawPath -notmatch '(^|[\\/])\.agents[\\/]skills$' })
        if ($nativeSkills.Count -gt 0) {
            $actions = @($actions | Where-Object {
                -not ($_.Scope -eq $scopeName -and $_.Kind -eq 'Skill' -and $_.RawPath -match '(^|[\\/])\.agents[\\/]skills$')
            })
        }
    }
}

# Rule-only clients need a real skill folder to point at. If the selection has
# no Skill action, add the generic Agent Skills root for each active scope.
$hasSkillAction = @($actions | Where-Object { $_.Kind -eq 'Skill' }).Count -gt 0
if (-not $hasSkillAction -and -not $cleanupOnly) {
    foreach ($scopeName in $scopes) {
        $fallbackPath = if ($scopeName -eq 'Global') { "$Home/.agents/skills" } else { '.agents/skills' }
        $actions += [pscustomobject]@{
            Client = 'agents'
            Label = 'Generic Agent Skills'
            Scope = $scopeName
            Kind = 'Skill'
            RawPath = $fallbackPath
            Frontmatter = $null
            BestEffort = $false
        }
    }
}

# Deduplicate identical scope/kind/path actions.
$seen = New-Object 'System.Collections.Generic.HashSet[string]'
$uniqueActions = @()
foreach ($action in $actions) {
    $key = $action.Scope + '|' + $action.Kind + '|' + $action.RawPath
    if ($seen.Add($key)) { $uniqueActions += $action }
}
$actions = $uniqueActions

# ---------------------------------------------------------------------------
# Determine a primary skill path for rule/agent content.
# ---------------------------------------------------------------------------
$primarySkillPath = $Source
$firstSkill = $actions | Where-Object { $_.Kind -eq 'Skill' } | Select-Object -First 1
if ($firstSkill) {
    $base = if ($firstSkill.Scope -eq 'Global') { $Home } else { $Root }
    $primarySkillPath = Join-Path (Resolve-TargetPath $firstSkill.RawPath $base) $SkillName
}

# ---------------------------------------------------------------------------
# Execute
# ---------------------------------------------------------------------------
$results = @()
foreach ($action in $actions) {
    $base = if ($action.Scope -eq 'Global') { $Home } else { $Root }
    $target = Resolve-TargetPath $action.RawPath $base

    $status = @{ Status = 'SKIP'; Detail = '' }
    try {
        switch ($action.Kind) {
            'Skill' {
                $backupRoot = Join-Path $Home ('.wpf-ui-style-backups\' + $action.Client)
                $status = Copy-SkillFolder $Source $target $SkillName ([bool]$Force) ([bool]$DryRun) $backupRoot
            }
            'Rule' {
                $content = Get-RuleContent $primarySkillPath $action.Frontmatter
                $status = Write-RuleFile $target $content ([bool]$Force) $false ([bool]$DryRun)
            }
            'Instruction' {
                $content = Get-RuleContent $primarySkillPath 'copilot'
                $status = Write-RuleFile $target $content ([bool]$Force) $false ([bool]$DryRun)
            }
            'Agent' {
                $content = Get-AgentBlock $primarySkillPath
                $status = Write-RuleFile $target $content ([bool]$Force) $true ([bool]$DryRun)
            }
            'Memory' {
                $content = Get-AgentBlock $primarySkillPath
                $status = Write-RuleFile $target $content ([bool]$Force) $true ([bool]$DryRun)
            }
            default {
                $status = @{ Status = 'ERROR'; Detail = 'unknown action kind' }
            }
        }
    }
    catch {
        $status = @{ Status = 'ERROR'; Detail = $_.Exception.Message }
    }

    $results += [pscustomobject]@{
        Client = $action.Client
        Scope = $action.Scope
        Kind = $action.Kind
        Target = if ($action.Kind -eq 'Skill') { Join-Path $target $SkillName } else { $target }
        Status = $status.Status
        Detail = $status.Detail
        BestEffort = if ($action.BestEffort) { 'yes' } else { '' }
    }
}

# ---------------------------------------------------------------------------
# Summary
# ---------------------------------------------------------------------------
if (-not $cleanupOnly) {
Write-Host ''
Write-Host 'wpf-ui-style install' -ForegroundColor Cyan
Write-Host ("Source : " + $Source) -ForegroundColor DarkGray
Write-Host ("Scope  : " + $Scope) -ForegroundColor DarkGray
Write-Host ("Clients: " + ($selected -join ', ')) -ForegroundColor DarkGray
if ($DryRun) { Write-Host 'Mode   : DRY RUN (no files written)' -ForegroundColor Yellow }
Write-Host ''

$results | Format-Table Client, Scope, Kind, Status, Target, Detail -AutoSize | Out-String | Write-Host

$ok = @($results | Where-Object { $_.Status -eq 'OK' }).Count
$skipped = @($results | Where-Object { $_.Status -eq 'SKIP' }).Count
$errors = @($results | Where-Object { $_.Status -eq 'ERROR' }).Count

Write-Host ("Installed: {0}   Skipped: {1}   Errors: {2}" -f $ok, $skipped, $errors) -ForegroundColor Cyan

if ($errors -gt 0) {
    Write-Host 'Some actions failed. Review the table above.' -ForegroundColor Red
    exit 1
}

if ($skipped -gt 0) {
    Write-Host 'Existing items were skipped. Re-run with -Force to overwrite.' -ForegroundColor Yellow
}
}

# ---------------------------------------------------------------------------
# Cleanup: remove this skill from clients that are not installed here.
# `-Clients all` used to create a config directory for every supported client;
# those leftover copies must not survive an `auto` install.
# ---------------------------------------------------------------------------
$doCleanup = $Cleanup -or ($Clients -eq 'auto' -and -not $NoCleanup)
if ($doCleanup) {
    $script:CleanupPruned = @()
    $cleanupRows = @()
    foreach ($name in $Registry.Keys) {
        if ($selected -contains $name) { continue }
        if ($name -ne 'agents' -and (Test-ClientInstalled $name)) { continue }
        foreach ($scopeName in $scopes) {
            foreach ($path in @(Remove-ClientArtifacts $name $scopeName ([bool]$DryRun))) {
                $cleanupRows += [pscustomobject]@{ Client = $name; Scope = $scopeName; Removed = $path }
            }
        }
    }

    if ($cleanupRows.Count -gt 0) {
        Write-Host ''
        Write-Host $(if ($DryRun) { 'Cleanup (dry run: nothing deleted)' } else { 'Cleanup (clients not installed)' }) -ForegroundColor Cyan
        Write-Host ''
        $cleanupRows | Format-Table Client, Scope, Removed -AutoSize | Out-String | Write-Host
        $pruned = @($script:CleanupPruned | Select-Object -Unique)
        if ($pruned.Count -gt 0) {
            Write-Host ("Empty directories {0}:" -f $(if ($DryRun) { 'that would be removed' } else { 'removed' })) -ForegroundColor DarkGray
            foreach ($dir in $pruned) { Write-Host ("  " + $dir) -ForegroundColor DarkGray }
        }
        Write-Host ("Removed {0} leftover item(s) from clients that are not installed." -f $cleanupRows.Count) -ForegroundColor Cyan
    }
    else {
        Write-Host ''
        Write-Host 'Cleanup: no leftover installs in clients that are not present.' -ForegroundColor DarkGray
    }
}

if (-not $cleanupOnly) {
Write-Host ''
Write-Host 'Next steps:' -ForegroundColor Green
Write-Host '  1. Restart the client (or reload the window) so it rescans skills/rules.'
Write-Host '  2. Ask: "按 wpf-ui-style 规范检查这个 WPF 窗口".'
Write-Host '  3. Run the bundled checks:'
Write-Host ("     pwsh `"" + (Join-Path $primarySkillPath 'tools/ContrastCheck.ps1') + "`"")
Write-Host ("     pwsh `"" + (Join-Path $primarySkillPath 'tools/LayoutCheck.ps1') + "`" -Root `"<project>`"")
Write-Host ("     pwsh `"" + (Join-Path $primarySkillPath 'tools/UiTest.ps1') + "`" -Window editable-table -TestClicks")
Write-Host ''
Write-Host 'Manual verification: see INSTALL.md in the skill folder.' -ForegroundColor DarkGray
}
