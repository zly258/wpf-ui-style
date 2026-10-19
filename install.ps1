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

.PARAMETER Detect
    List detected clients and exit.

.PARAMETER List
    List all supported clients and their install paths, then exit.

.EXAMPLE
    pwsh ./install.ps1 -Detect

.EXAMPLE
    pwsh ./install.ps1 -Scope Global -Clients auto

.EXAMPLE
    pwsh ./install.ps1 -Scope Project -Root "D:\workspace\inventor\OpenCADTranslate" -Clients all

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
    param([string]$Path)
    $stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    $backup = "$Path.bak-$stamp"
    Copy-Item -LiteralPath $Path -Destination $backup -Force
    return $backup
}

function Copy-SkillFolder {
    param(
        [string]$SourcePath,
        [string]$TargetPath,
        [string]$Name,
        [bool]$Overwrite,
        [bool]$Preview
    )

    $target = Join-Path $TargetPath $Name

    if ([string]::Equals($target, $SourcePath, [StringComparison]::OrdinalIgnoreCase)) {
        return @{ Status = 'SKIP'; Detail = 'target is the source folder' }
    }
    if ($target.StartsWith($SourcePath.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)) {
        return @{ Status = 'SKIP'; Detail = 'target is inside source' }
    }

    if (Test-Path -LiteralPath $target) {
        if (-not $Overwrite) {
            return @{ Status = 'SKIP'; Detail = 'exists (use -Force to overwrite)' }
        }
        if (-not $Preview) {
            $backup = Backup-File $target
            Remove-Item -LiteralPath $target -Recurse -Force
        }
        $detail = 'overwritten (backup created)'
    }
    else {
        $detail = 'installed'
    }

    if (-not $Preview) {
        New-Item -ItemType Directory -Force -Path $TargetPath | Out-Null
        Copy-Item -LiteralPath $SourcePath -Destination $target -Recurse -Force
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
# WPF UI Style

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
5. Multiline TextBox: `Height="Auto"` + `MinHeight`/`MaxHeight` + `VerticalAlignment="Top"`.
   Only logs/lists/tables/ScrollViewer may occupy a `Grid` `*` row. Never use `Height="*"` on a control.
6. Before finishing, run `{{SKILL_PATH}}/tools/ContrastCheck.ps1` and
   `{{SKILL_PATH}}/tools/LayoutCheck.ps1 -Root <project>`.
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
description: WPF UI style rules for OpenCAD projects (tokens, states, layout, contrast).
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
## WPF UI Style

When editing WPF/XAML in this repository, read and follow:
`{{SKILL_PATH}}/SKILL.md`

Key rules:

- Every background state change also sets `Foreground`.
- No `Opacity` for disabled; use disabled background/foreground tokens.
- White text only on Accent/Danger/Tooltip dark backgrounds.
- Selected + hover uses `MultiTrigger`.
- Multiline TextBox: `Height="Auto"` + `MinHeight`/`MaxHeight` + `VerticalAlignment="Top"`;
  only logs/lists/tables/ScrollViewer may fill a `Grid` `*` row.
- Run `{{SKILL_PATH}}/tools/ContrastCheck.ps1` and
  `{{SKILL_PATH}}/tools/LayoutCheck.ps1 -Root <project>` before finishing.
<!-- END wpf-ui-style -->
'@
    $display = $SkillPath.Replace('\', '/')
    return $block.Replace('{{SKILL_PATH}}', $display) + "`r`n"
}

function Test-Detected {
    param([string]$Name)
    $entry = $Registry[$Name]
    if ($null -eq $entry) { return $false }
    foreach ($path in $entry.Detect) {
        $resolved = Resolve-TargetPath $path $Root
        if (Test-Path -LiteralPath $resolved) { return $true }
    }
    return $false
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
        if (Test-Detected $name) {
            $found += $name
            Write-Host ("  [x] {0,-12} {1}" -f $name, $Registry[$name].Label) -ForegroundColor Green
        }
    }
    if ($found.Count -eq 0) {
        Write-Host '  (none detected; use -Clients all or specify names)' -ForegroundColor Yellow
    }
    Write-Host ''
    Write-Host ('Detected: ' + ($found -join ', ')) -ForegroundColor DarkGray
    exit 0
}

# ---------------------------------------------------------------------------
# Select clients
# ---------------------------------------------------------------------------
$selected = @()
if ($Clients -eq 'all') {
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

# Rule-only clients need a real skill folder to point at. If the selection has
# no Skill action, add the generic Agent Skills root for each active scope.
$hasSkillAction = @($actions | Where-Object { $_.Kind -eq 'Skill' }).Count -gt 0
if (-not $hasSkillAction) {
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
                $status = Copy-SkillFolder $Source $target $SkillName ([bool]$Force) ([bool]$DryRun)
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

Write-Host ''
Write-Host 'Next steps:' -ForegroundColor Green
Write-Host '  1. Restart the client (or reload the window) so it rescans skills/rules.'
Write-Host '  2. Ask: "按 wpf-ui-style 规范检查这个 WPF 窗口".'
Write-Host '  3. Run the bundled checks:'
Write-Host ("     pwsh `"" + (Join-Path $primarySkillPath 'tools/ContrastCheck.ps1') + "`"")
Write-Host ("     pwsh `"" + (Join-Path $primarySkillPath 'tools/LayoutCheck.ps1') + "`" -Root `"<project>`"")
Write-Host ''
Write-Host 'Manual verification: see INSTALL.md in the skill folder.' -ForegroundColor DarkGray
