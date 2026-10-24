#!/usr/bin/env python3
"""WCAG contrast check for Theme/Tokens.xaml.

Reads Color.* entries from Tokens.xaml and checks the state pairs defined below.
Exits 1 when any pair is below its minimum ratio.

Usage:
    python tools/ContrastCheck.py
    python tools/ContrastCheck.py --tokens D:/repo/Theme/Tokens.xaml
"""

from __future__ import annotations

import argparse
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

XAML_NS = "http://schemas.microsoft.com/winfx/2006/xaml"
KEY_ATTR = f"{{{XAML_NS}}}Key"

# (foreground key, background key, minimum ratio, label)
# Min: 4.5 for normal text, 3.0 for disabled text / non-text focus.
# Not checked (decorative, WCAG-exempt): ScrollThumb*, OverlayChip*.
PAIRS = [
    # 正文 / 次要文字
    ("Color.TextPrimary", "Color.WindowBackground", 4.5, "Text.Primary / WindowBackground"),
    ("Color.TextPrimary", "Color.Surface", 4.5, "Text.Primary / Surface"),
    ("Color.TextPrimary", "Color.SurfaceAlt", 4.5, "Text.Primary / SurfaceAlt"),
    ("Color.TextPrimary", "Color.SurfaceSunken", 4.5, "Text.Primary / SurfaceSunken"),
    ("Color.TextPrimary", "Color.AccentSubtle", 4.5, "Text.Primary / Accent.Subtle"),
    ("Color.TextPrimary", "Color.AccentSubtleHover", 4.5, "Text.Primary / Accent.SubtleHover"),
    ("Color.TextPrimary", "Color.ControlHover", 4.5, "Text.Primary / Control.Hover"),
    ("Color.TextPrimary", "Color.ControlPressed", 4.5, "Text.Primary / Control.Pressed"),
    ("Color.TextPrimary", "Color.Selection", 4.5, "Text.Primary / Selection"),
    ("Color.TextPrimary", "Color.SelectionHover", 4.5, "Text.Primary / Selection.Hover"),
    ("Color.TextSecondary", "Color.WindowBackground", 4.5, "Text.Secondary / WindowBackground"),
    ("Color.TextSecondary", "Color.Surface", 4.5, "Text.Secondary / Surface"),
    ("Color.TextSecondary", "Color.SurfaceAlt", 4.5, "Text.Secondary / SurfaceAlt"),
    ("Color.TextSecondary", "Color.SurfaceSunken", 4.5, "Text.Secondary / SurfaceSunken"),
    # 禁用（WCAG 豁免，但要求至少 3:1）
    ("Color.TextDisabled", "Color.ControlDisabled", 3.0, "Text.Disabled / Control.Disabled"),
    # 白字压品牌/危险色
    ("Color.OnAccent", "Color.Accent", 4.5, "OnAccent / Accent"),
    ("Color.OnAccent", "Color.AccentHover", 4.5, "OnAccent / Accent.Hover"),
    ("Color.OnAccent", "Color.AccentPressed", 4.5, "OnAccent / Accent.Pressed"),
    ("Color.OnAccent", "Color.Danger", 4.5, "OnAccent / Danger"),
    ("Color.OnAccent", "Color.DangerHover", 4.5, "OnAccent / Danger.Hover"),
    ("Color.OnAccent", "Color.DangerPressed", 4.5, "OnAccent / Danger.Pressed"),
    # 语义色浅底
    ("Color.TextPrimary", "Color.DangerSubtle", 4.5, "Text.Primary / Danger.Subtle"),
    ("Color.TextPrimary", "Color.SuccessSubtle", 4.5, "Text.Primary / Success.Subtle"),
    ("Color.TextPrimary", "Color.WarningSubtle", 4.5, "Text.Primary / Warning.Subtle"),
    ("Color.Danger", "Color.DangerSubtle", 4.5, "Danger / Danger.Subtle"),
    ("Color.Success", "Color.SuccessSubtle", 4.5, "Success / Success.Subtle"),
    ("Color.Warning", "Color.WarningSubtle", 4.5, "Warning / Warning.Subtle"),
    # Tooltip
    ("Color.TooltipForeground", "Color.TooltipBackground", 4.5, "Tooltip.Foreground / Tooltip.Background"),
    # 徽章（状态标签前景/背景对）
    ("Color.BadgeIdleForeground", "Color.BadgeIdleBackground", 4.5, "Badge.Idle fg/bg"),
    ("Color.BadgeActiveForeground", "Color.BadgeActiveBackground", 4.5, "Badge.Active fg/bg"),
    ("Color.BadgeDoneForeground", "Color.BadgeDoneBackground", 4.5, "Badge.Done fg/bg"),
    ("Color.BadgeStoppedForeground", "Color.BadgeStoppedBackground", 4.5, "Badge.Stopped fg/bg"),
    ("Color.BadgeFailedForeground", "Color.BadgeFailedBackground", 4.5, "Badge.Failed fg/bg"),
    # 禁用文字在其他常见表面上（WCAG 豁免，但要求至少 3:1）
    ("Color.TextDisabled", "Color.Surface", 3.0, "Text.Disabled / Surface"),
    ("Color.TextDisabled", "Color.SurfaceAlt", 3.0, "Text.Disabled / SurfaceAlt"),
    ("Color.TextDisabled", "Color.SurfaceSunken", 3.0, "Text.Disabled / SurfaceSunken"),
    # 次要文字在交互/选中浅底上
    ("Color.TextSecondary", "Color.AccentSubtle", 4.5, "Text.Secondary / Accent.Subtle"),
    ("Color.TextSecondary", "Color.Selection", 4.5, "Text.Secondary / Selection"),
    ("Color.TextSecondary", "Color.ControlHover", 4.5, "Text.Secondary / Control.Hover"),
    # 焦点（非文字 3:1）
    ("Color.Focus", "Color.Surface", 3.0, "Focus / Surface"),
    ("Color.Focus", "Color.WindowBackground", 3.0, "Focus / WindowBackground"),
]


def linear(channel: float) -> float:
    return channel / 12.92 if channel <= 0.04045 else ((channel + 0.055) / 1.055) ** 2.4


def luminance(hex_value: str) -> float:
    value = hex_value.strip().lstrip("#")
    if len(value) == 8:  # AARRGGBB -> RRGGBB
        value = value[2:]
    if len(value) != 6:
        raise ValueError(f"Invalid color value: {hex_value}")

    r = int(value[0:2], 16) / 255.0
    g = int(value[2:4], 16) / 255.0
    b = int(value[4:6], 16) / 255.0
    return 0.2126 * linear(r) + 0.7152 * linear(g) + 0.0722 * linear(b)


def contrast(foreground: str, background: str) -> float:
    lf = luminance(foreground)
    lb = luminance(background)
    hi, lo = max(lf, lb), min(lf, lb)
    return (hi + 0.05) / (lo + 0.05)


def load_colors(tokens_path: Path) -> dict[str, str]:
    root = ET.parse(tokens_path).getroot()
    colors: dict[str, str] = {}
    for node in root.iter():
        if node.tag.split("}")[-1] != "Color":
            continue
        key = node.get(KEY_ATTR)
        if not key:
            continue
        colors[key] = (node.text or "").strip()
    return colors


def main() -> int:
    default_tokens = Path(__file__).resolve().parent.parent / "Theme" / "Tokens.xaml"
    parser = argparse.ArgumentParser(description="WCAG contrast check for Theme/Tokens.xaml")
    parser.add_argument("--tokens", "-TokensPath", "-tokens", default=str(default_tokens))
    args = parser.parse_args()

    tokens_path = Path(args.tokens)
    if not tokens_path.exists():
        print(f"Tokens file not found: {tokens_path}", file=sys.stderr)
        return 1

    colors = load_colors(tokens_path)

    print()
    print("WCAG contrast check")
    print(f"Tokens: {tokens_path.resolve()}")
    print()

    failures = 0
    rows = []
    for fg_key, bg_key, minimum, label in PAIRS:
        if fg_key not in colors:
            print(f"MISSING {fg_key}", file=sys.stderr)
            failures += 1
            continue
        if bg_key not in colors:
            print(f"MISSING {bg_key}", file=sys.stderr)
            failures += 1
            continue

        ratio = contrast(colors[fg_key], colors[bg_key])
        passed = ratio >= minimum
        if not passed:
            failures += 1
        rows.append((label, ratio, minimum, "PASS" if passed else "FAIL"))

    header = f"{'Pair':<40} {'Ratio':>6} {'Min':>5} {'Result':<6}"
    print(header)
    print("-" * len(header))
    for label, ratio, minimum, result in rows:
        print(f"{label:<40} {ratio:>6.2f} {minimum:>5.1f} {result:<6}")
    print()

    if failures:
        sys.stdout.flush()
        print(f"FAILED: {failures} contrast pair(s) below minimum.", file=sys.stderr)
        return 1

    print("All contrast checks passed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
