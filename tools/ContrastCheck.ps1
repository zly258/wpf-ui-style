#Requires -Version 5.1
<#
.SYNOPSIS
    WCAG contrast check for Theme/Tokens.xaml.

.DESCRIPTION
    Reads Color.* from Tokens.xaml and checks the state pairs defined below.
    Exits 1 when any pair is below its minimum ratio.

.EXAMPLE
    pwsh ./tools/ContrastCheck.ps1

.EXAMPLE
    pwsh ./tools/ContrastCheck.ps1 -TokensPath "D:\repo\Theme\Tokens.xaml"
#>
[CmdletBinding()]
param(
    [string]$TokensPath = ''
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($TokensPath)) {
    $scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Definition }
    if ([string]::IsNullOrWhiteSpace($scriptDir)) { $scriptDir = (Get-Location).Path }
    $TokensPath = Join-Path $scriptDir '..\Theme\Tokens.xaml'
}


function ConvertTo-Linear([double]$c) {
    if ($c -le 0.04045) { return $c / 12.92 }
    return [Math]::Pow(($c + 0.055) / 1.055, 2.4)
}

function Get-Luminance([string]$hex) {
    $h = $hex.Trim().TrimStart('#')
    if ($h.Length -eq 8) { $h = $h.Substring(2) } # AARRGGBB -> RRGGBB
    if ($h.Length -ne 6) { throw "Invalid color value: $hex" }

    $r = [Convert]::ToInt32($h.Substring(0, 2), 16) / 255.0
    $g = [Convert]::ToInt32($h.Substring(2, 2), 16) / 255.0
    $b = [Convert]::ToInt32($h.Substring(4, 2), 16) / 255.0

    return 0.2126 * (ConvertTo-Linear $r) +
           0.7152 * (ConvertTo-Linear $g) +
           0.0722 * (ConvertTo-Linear $b)
}

function Get-Contrast([string]$foreground, [string]$background) {
    $lf = Get-Luminance $foreground
    $lb = Get-Luminance $background
    $hi = [Math]::Max($lf, $lb)
    $lo = [Math]::Min($lf, $lb)
    return ($hi + 0.05) / ($lo + 0.05)
}

if (-not (Test-Path $TokensPath)) {
    Write-Host "Tokens file not found: $TokensPath" -ForegroundColor Red
    exit 1
}

[xml]$tokens = Get-Content $TokensPath -Raw -Encoding UTF8
$colors = @{}
foreach ($node in $tokens.ResourceDictionary.Color) {
    $keyAttr = $node.Attributes | Where-Object { $_.LocalName -eq 'Key' } | Select-Object -First 1
    if ($null -eq $keyAttr) { continue }
    $colors[$keyAttr.Value] = $node.InnerText.Trim()
}

# ---------------------------------------------------------------------------
# 状态组合。Min: 4.5 for normal text, 3.0 for disabled text / non-text focus.
# ---------------------------------------------------------------------------
$pairs = @(
    # 正文 / 次要文字
    @{ Fg = 'Color.TextPrimary';   Bg = 'Color.WindowBackground';   Min = 4.5; Label = 'Text.Primary / WindowBackground' }
    @{ Fg = 'Color.TextPrimary';   Bg = 'Color.Surface';            Min = 4.5; Label = 'Text.Primary / Surface' }
    @{ Fg = 'Color.TextPrimary';   Bg = 'Color.SurfaceAlt';         Min = 4.5; Label = 'Text.Primary / SurfaceAlt' }
    @{ Fg = 'Color.TextPrimary';   Bg = 'Color.SurfaceSunken';      Min = 4.5; Label = 'Text.Primary / SurfaceSunken' }
    @{ Fg = 'Color.TextPrimary';   Bg = 'Color.AccentSubtle';       Min = 4.5; Label = 'Text.Primary / Accent.Subtle' }
    @{ Fg = 'Color.TextPrimary';   Bg = 'Color.AccentSubtleHover';  Min = 4.5; Label = 'Text.Primary / Accent.SubtleHover' }
    @{ Fg = 'Color.TextPrimary';   Bg = 'Color.ControlHover';       Min = 4.5; Label = 'Text.Primary / Control.Hover' }
    @{ Fg = 'Color.TextPrimary';   Bg = 'Color.ControlPressed';     Min = 4.5; Label = 'Text.Primary / Control.Pressed' }
    @{ Fg = 'Color.TextPrimary';   Bg = 'Color.Selection';          Min = 4.5; Label = 'Text.Primary / Selection' }
    @{ Fg = 'Color.TextPrimary';   Bg = 'Color.SelectionHover';     Min = 4.5; Label = 'Text.Primary / Selection.Hover' }

    @{ Fg = 'Color.TextSecondary'; Bg = 'Color.WindowBackground';   Min = 4.5; Label = 'Text.Secondary / WindowBackground' }
    @{ Fg = 'Color.TextSecondary'; Bg = 'Color.Surface';            Min = 4.5; Label = 'Text.Secondary / Surface' }
    @{ Fg = 'Color.TextSecondary'; Bg = 'Color.SurfaceAlt';         Min = 4.5; Label = 'Text.Secondary / SurfaceAlt' }
    @{ Fg = 'Color.TextSecondary'; Bg = 'Color.SurfaceSunken';      Min = 4.5; Label = 'Text.Secondary / SurfaceSunken' }

    # 禁用（WCAG 豁免，但要求至少 3:1）
    @{ Fg = 'Color.TextDisabled';  Bg = 'Color.ControlDisabled';    Min = 3.0; Label = 'Text.Disabled / Control.Disabled' }

    # 白字压品牌/危险色
    @{ Fg = 'Color.OnAccent';      Bg = 'Color.Accent';             Min = 4.5; Label = 'OnAccent / Accent' }
    @{ Fg = 'Color.OnAccent';      Bg = 'Color.AccentHover';        Min = 4.5; Label = 'OnAccent / Accent.Hover' }
    @{ Fg = 'Color.OnAccent';      Bg = 'Color.AccentPressed';      Min = 4.5; Label = 'OnAccent / Accent.Pressed' }
    @{ Fg = 'Color.OnAccent';      Bg = 'Color.Danger';             Min = 4.5; Label = 'OnAccent / Danger' }
    @{ Fg = 'Color.OnAccent';      Bg = 'Color.DangerHover';        Min = 4.5; Label = 'OnAccent / Danger.Hover' }
    @{ Fg = 'Color.OnAccent';      Bg = 'Color.DangerPressed';      Min = 4.5; Label = 'OnAccent / Danger.Pressed' }

    # 语义色浅底
    @{ Fg = 'Color.TextPrimary';   Bg = 'Color.DangerSubtle';       Min = 4.5; Label = 'Text.Primary / Danger.Subtle' }
    @{ Fg = 'Color.TextPrimary';   Bg = 'Color.SuccessSubtle';      Min = 4.5; Label = 'Text.Primary / Success.Subtle' }
    @{ Fg = 'Color.TextPrimary';   Bg = 'Color.WarningSubtle';      Min = 4.5; Label = 'Text.Primary / Warning.Subtle' }
    @{ Fg = 'Color.Danger';        Bg = 'Color.DangerSubtle';       Min = 4.5; Label = 'Danger / Danger.Subtle' }
    @{ Fg = 'Color.Success';       Bg = 'Color.SuccessSubtle';      Min = 4.5; Label = 'Success / Success.Subtle' }
    @{ Fg = 'Color.Warning';       Bg = 'Color.WarningSubtle';      Min = 4.5; Label = 'Warning / Warning.Subtle' }

    # Tooltip
    @{ Fg = 'Color.TooltipForeground'; Bg = 'Color.TooltipBackground'; Min = 4.5; Label = 'Tooltip.Foreground / Tooltip.Background' }

    # 焦点（非文字 3:1）
    @{ Fg = 'Color.Focus';         Bg = 'Color.Surface';            Min = 3.0; Label = 'Focus / Surface' }
    @{ Fg = 'Color.Focus';         Bg = 'Color.WindowBackground';   Min = 3.0; Label = 'Focus / WindowBackground' }
)

Write-Host ''
Write-Host 'WCAG contrast check' -ForegroundColor Cyan
Write-Host ("Tokens: " + (Resolve-Path $TokensPath)) -ForegroundColor DarkGray
Write-Host ''

$failures = 0
$results = foreach ($pair in $pairs) {
    $fgKey = $pair.Fg
    $bgKey = $pair.Bg

    if (-not $colors.ContainsKey($fgKey)) {
        Write-Host ("MISSING " + $fgKey) -ForegroundColor Red
        $failures++
        continue
    }
    if (-not $colors.ContainsKey($bgKey)) {
        Write-Host ("MISSING " + $bgKey) -ForegroundColor Red
        $failures++
        continue
    }

    $ratio = Get-Contrast $colors[$fgKey] $colors[$bgKey]
    $pass = $ratio -ge $pair.Min
    if (-not $pass) { $failures++ }

    [pscustomobject]@{
        Pair   = $pair.Label
        Ratio  = [Math]::Round($ratio, 2)
        Min    = $pair.Min
        Result = if ($pass) { 'PASS' } else { 'FAIL' }
    }
}

$results | Format-Table -AutoSize | Out-String | Write-Host

if ($failures -gt 0) {
    Write-Host ("FAILED: " + $failures + " contrast pair(s) below minimum.") -ForegroundColor Red
    exit 1
}

Write-Host 'All contrast checks passed.' -ForegroundColor Green
exit 0
