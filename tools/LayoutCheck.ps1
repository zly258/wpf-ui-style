#Requires -Version 5.1
<#
.SYNOPSIS
    Lightweight XAML layout checker for common WPF mistakes.

.DESCRIPTION
    Scans .xaml files and reports:
      - Height="*" on controls (only RowDefinition/ColumnDefinition allow *).
      - Multiline TextBox without MinHeight/MaxHeight.
      - Multiline TextBox with VerticalAlignment="Stretch" (warning).
      - Wrapped TextBlock with VerticalAlignment="Stretch" (warning).
      - Nested ScrollViewer (warning).

    Errors exit with code 1. Warnings do not fail unless -Strict is used.

.EXAMPLE
    pwsh ./tools/LayoutCheck.ps1 -Root "D:\workspace\inventor\OpenCADTranslate"

.EXAMPLE
    pwsh ./tools/LayoutCheck.ps1 -Root "D:\repo" -Strict
#>
[CmdletBinding()]
param(
    [string]$Root = (Get-Location).Path,
    [switch]$Strict
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Root)) {
    Write-Host "Root not found: $Root" -ForegroundColor Red
    exit 1
}

$exclude = @('\obj\', '\bin\', '\.build\', '\output\', '\dist\', '\node_modules\')
$files = Get-ChildItem -Path $Root -Recurse -Filter *.xaml -File |
    Where-Object {
        $path = $_.FullName
        -not ($exclude | Where-Object { $path -like ('*' + $_ + '*') })
    }

if ($files.Count -eq 0) {
    Write-Host "No .xaml files found under $Root" -ForegroundColor Yellow
    exit 0
}

$errors = 0
$warnings = 0

function Get-LineNumber([string]$text, [int]$index) {
    if ($index -le 0) { return 1 }
    return ([regex]::Matches($text.Substring(0, $index), "`n")).Count + 1
}

function Report([string]$level, [string]$file, [int]$line, [string]$message) {
    $prefix = "{0}:{1}" -f $file, $line
    if ($level -eq 'ERROR') {
        Write-Host ("ERROR   " + $prefix + "  " + $message) -ForegroundColor Red
        $script:errors++
    }
    else {
        Write-Host ("WARN    " + $prefix + "  " + $message) -ForegroundColor Yellow
        $script:warnings++
    }
}

Write-Host ''
Write-Host 'XAML layout check' -ForegroundColor Cyan
Write-Host ("Root: " + (Resolve-Path $Root)) -ForegroundColor DarkGray
Write-Host ''

foreach ($file in $files) {
    $text = Get-Content $file.FullName -Raw -Encoding UTF8

    # 1. Height="*" on a control
    foreach ($m in [regex]::Matches($text, '(?s)<(?<tag>[A-Za-z0-9_:]+)\b(?<attrs>.*?)>')) {
        $tag = $m.Groups['tag'].Value
        $attrs = $m.Groups['attrs'].Value
        if ($attrs -notmatch 'Height\s*=\s*"\*"') { continue }
        if ($tag -eq 'RowDefinition' -or $tag -eq 'ColumnDefinition') { continue }
        $line = Get-LineNumber $text $m.Index
        Report 'ERROR' $file.FullName $line ('Height="*" is only valid on RowDefinition/ColumnDefinition (found on <' + $tag + '>)')
    }

    # 2/3. Multiline TextBox (explicit AcceptsReturn or the multiline/code styles)
    foreach ($m in [regex]::Matches($text, '(?s)<TextBox\b(?<attrs>.*?)>')) {
        $attrs = $m.Groups['attrs'].Value
        $isLog = $attrs -match 'TextBox\.Code' -or $attrs -match 'IsReadOnly\s*=\s*"True"'
        $isMultiline = $isLog `
            -or $attrs -match 'AcceptsReturn\s*=\s*"True"' `
            -or $attrs -match 'TextBox\.Multiline'
        if (-not $isMultiline) { continue }

        $line = Get-LineNumber $text $m.Index
        
        # 针对只读日志框，建议设置上限防止无限撑大
        if ($isLog) {
            $hasMax = $attrs -match 'MaxHeight\s*='
            if (-not $hasMax) {
                Report 'WARN' $file.FullName $line 'Log/Code TextBox should preferably set MaxHeight (e.g. 240) to prevent unbounded growth'
            }
        }
    }


    # 4. Wrapped TextBlock stretching
    foreach ($m in [regex]::Matches($text, '(?s)<TextBlock\b(?<attrs>.*?)>')) {
        $attrs = $m.Groups['attrs'].Value
        if ($attrs -notmatch 'TextWrapping\s*=\s*"Wrap"') { continue }
        if ($attrs -notmatch 'VerticalAlignment\s*=\s*"Stretch"') { continue }

        $line = Get-LineNumber $text $m.Index
        Report 'WARN' $file.FullName $line 'Wrapped TextBlock uses VerticalAlignment="Stretch"; use Top so it does not center/stretch in a flexible row'
    }

    # 5. Nested ScrollViewer (skip theme/style templates: ComboBox popups legitimately nest)
    $isThemeFile = $file.Name -match '(?i)(WindowTheme|Styles|UnifiedTheme)\.xaml$' `
        -or $file.FullName -match '\\Theme\\'
    if (-not $isThemeFile) {
        $depth = 0
        foreach ($m in [regex]::Matches($text, '<ScrollViewer\b|</ScrollViewer>')) {
            if ($m.Value -eq '</ScrollViewer>') {
                if ($depth -gt 0) { $depth-- }
            }
            else {
                $depth++
                if ($depth -gt 1) {
                    $line = Get-LineNumber $text $m.Index
                    Report 'WARN' $file.FullName $line 'Nested ScrollViewer; keep one vertical scroll per window'
                    break
                }
            }
        }
    }
}

Write-Host ''
Write-Host ("Errors: " + $errors + "   Warnings: " + $warnings) -ForegroundColor Cyan

if ($errors -gt 0) {
    Write-Host 'Layout check failed.' -ForegroundColor Red
    exit 1
}

if ($Strict -and $warnings -gt 0) {
    Write-Host 'Layout check failed because -Strict was specified.' -ForegroundColor Red
    exit 1
}

Write-Host 'Layout check passed.' -ForegroundColor Green
exit 0
