#Requires -Version 5.1
<#
.SYNOPSIS
    WPF UI 渲染、交互点击测试与视觉规范自检工具脚本。

.DESCRIPTION
    薄包装脚本：探测 py -3 -> python3 -> python 并转调 tools/UiTest.py。
    如未找到 Python 3，则回退调用 dotnet 驱动 preview/WpfStylePreview。

.EXAMPLE
    pwsh ./tools/UiTest.ps1 -Window editable-table -TestClicks
    pwsh ./tools/UiTest.ps1 -Window wizard -TestClicks -ShotDir shots/wizard
    pwsh ./tools/UiTest.ps1 -Xaml "preview/Views/SafeFormView.xaml" -Shot "shots/safeform.png"
    pwsh ./tools/UiTest.ps1 -All -TestClicks
    pwsh ./tools/UiTest.ps1 -List
#>
[CmdletBinding()]
param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$RemainingArgs
)

$ErrorActionPreference = 'Stop'

$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Definition }
$pyScript = Join-Path $scriptDir 'UiTest.py'

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
if ($null -ne $python) {
    & $python.Exe @($python.Prefix + $pyScript + $RemainingArgs)
    exit $LASTEXITCODE
}

# 回退：使用 dotnet 直接运行
$dotnet = Get-Command 'dotnet' -ErrorAction SilentlyContinue
if ($null -ne $dotnet) {
    $proj = Join-Path (Split-Path -Parent $scriptDir) 'preview\WpfStylePreview.csproj'
    & dotnet run --project $proj -- $RemainingArgs
    exit $LASTEXITCODE
}

Write-Host 'Neither Python 3 nor .NET SDK found.' -ForegroundColor Red
exit 2
