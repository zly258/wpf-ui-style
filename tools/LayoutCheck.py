#!/usr/bin/env python3
"""Lightweight XAML layout checker for common WPF mistakes.

Scans .xaml files and reports:
  - Height="*" on controls (only RowDefinition/ColumnDefinition allow *).
  - Hardcoded color literals and local FontFamily in business XAML.
  - Read-only log/code TextBox without MaxHeight (warning).
  - Wrapped TextBlock with VerticalAlignment="Stretch" (warning).
  - Nested ScrollViewer (warning).

Errors exit with code 1. Warnings do not fail unless --strict is used.

Usage:
    python tools/LayoutCheck.py --root D:/workspace/inventor/OpenCADTranslate
    python tools/LayoutCheck.py --root D:/repo --strict
"""

from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

EXCLUDE_DIRS = {"obj", "bin", ".build", "output", "dist", "node_modules"}
THEME_FILE_RE = re.compile(r"(WindowTheme|Styles|UnifiedTheme)\.xaml$", re.IGNORECASE)
TAG_RE = re.compile(r"<([A-Za-z0-9_:]+)\b(.*?)>", re.DOTALL)
COMMENT_RE = re.compile(r"<!--.*?-->", re.DOTALL)
# #RGB / #RRGGBB / #AARRGGBB used as an attribute value. Requiring the leading
# quote keeps text such as "Issue #123" from being reported as a color.
COLOR_LITERAL_RE = re.compile(r'(?<=["\'])#(?:[0-9A-Fa-f]{8}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{3})\b')
FONTFAMILY_RE = re.compile(r'FontFamily\s*=\s*"[^"]{2,}"')
TEXTBOX_RE = re.compile(r"<TextBox\b(.*?)>", re.DOTALL)
TEXTBLOCK_RE = re.compile(r"<TextBlock\b(.*?)>", re.DOTALL)
SCROLLVIEWER_TAG_RE = re.compile(r"<ScrollViewer\b[^>]*?/?>|</ScrollViewer>", re.DOTALL)

errors = 0
warnings = 0


def line_number(text: str, index: int) -> int:
    return text.count("\n", 0, index) + 1


def comment_ranges(text: str) -> list[tuple[int, int]]:
    """Character ranges covered by XML comments, including multi-line ones."""
    return [(m.start(), m.end()) for m in COMMENT_RE.finditer(text)]


def in_ranges(index: int, ranges: list[tuple[int, int]]) -> bool:
    for start, end in ranges:
        if start > index:
            break
        if start <= index < end:
            return True
    return False


def report(level: str, path: Path, line: int, message: str) -> None:
    global errors, warnings
    prefix = f"{path}:{line}"
    if level == "ERROR":
        print(f"ERROR   {prefix}  {message}")
        errors += 1
    else:
        print(f"WARN    {prefix}  {message}")
        warnings += 1


def iter_xaml_files(root: Path):
    for path in sorted(root.rglob("*.xaml")):
        if any(part.lower() in EXCLUDE_DIRS for part in path.parts):
            continue
        yield path


def is_theme_file(path: Path) -> bool:
    if THEME_FILE_RE.search(path.name):
        return True
    return any(part.lower() == "theme" for part in path.parts[:-1])


def check_file(path: Path) -> None:
    text = path.read_text(encoding="utf-8", errors="replace")
    comments = comment_ranges(text)

    # 1. Height="*" on a control
    for match in TAG_RE.finditer(text):
        tag = match.group(1)
        attrs = match.group(2)
        if not re.search(r'Height\s*=\s*"\*"', attrs):
            continue
        if tag in ("RowDefinition", "ColumnDefinition"):
            continue
        report(
            "ERROR",
            path,
            line_number(text, match.start()),
            f'Height="*" is only valid on RowDefinition/ColumnDefinition (found on <{tag}>)',
        )

    # 2. Read-only log/code TextBox without MaxHeight
    for match in TEXTBOX_RE.finditer(text):
        attrs = match.group(1)
        is_log = bool(re.search(r"TextBox\.Code", attrs) or re.search(r'IsReadOnly\s*=\s*"True"', attrs))
        is_multiline = (
            is_log
            or bool(re.search(r'AcceptsReturn\s*=\s*"True"', attrs))
            or "TextBox.Multiline" in attrs
        )
        if not is_multiline:
            continue

        if is_log and not re.search(r"MaxHeight\s*=", attrs):
            report(
                "WARN",
                path,
                line_number(text, match.start()),
                "Log/Code TextBox should preferably set MaxHeight (e.g. 240) to prevent unbounded growth",
            )

    # 3. Wrapped TextBlock stretching
    for match in TEXTBLOCK_RE.finditer(text):
        attrs = match.group(1)
        if not re.search(r'TextWrapping\s*=\s*"Wrap"', attrs):
            continue
        if not re.search(r'VerticalAlignment\s*=\s*"Stretch"', attrs):
            continue
        report(
            "WARN",
            path,
            line_number(text, match.start()),
            'Wrapped TextBlock uses VerticalAlignment="Stretch"; use Top so it does not center/stretch in a flexible row',
        )

    # 4. Hardcoded color literals in business XAML.
    #    Theme files are the single source of truth for color and may contain them;
    #    business windows must reference Brush.* / Color.* tokens instead.
    if not is_theme_file(path):
        for match in COLOR_LITERAL_RE.finditer(text):
            if in_ranges(match.start(), comments):
                continue
            line = line_number(text, match.start())
            report(
                "ERROR",
                path,
                line,
                f"Hardcoded color '{match.group(0)}'; use a Brush.* / Color.* token from Theme/Tokens.xaml",
            )

    # 5. Local FontFamily in business XAML: the Window style provides Font.Ui.
    if not is_theme_file(path):
        for match in FONTFAMILY_RE.finditer(text):
            if in_ranges(match.start(), comments):
                continue
            line = line_number(text, match.start())
            report(
                "ERROR",
                path,
                line,
                "Local FontFamily; inherit Font.Ui from the Window style and do not override the font stack",
            )

    # 6. Nested ScrollViewer (skip theme/style templates: ComboBox popups legitimately nest)
    if is_theme_file(path):
        return

    depth = 0
    for match in SCROLLVIEWER_TAG_RE.finditer(text):
        token = match.group(0)
        if token.startswith("</"):
            if depth > 0:
                depth -= 1
            continue
        # A self-closing <ScrollViewer ... /> does not open a scope.
        if token.rstrip().endswith("/>"):
            continue
        depth += 1
        if depth > 1:
            report(
                "WARN",
                path,
                line_number(text, match.start()),
                "Nested ScrollViewer; keep one vertical scroll per window",
            )
            break


def main() -> int:
    parser = argparse.ArgumentParser(description="XAML layout checker for common WPF mistakes")
    parser.add_argument("--root", "-Root", "-root", default=".")
    parser.add_argument("--strict", "-Strict", "-strict", action="store_true")
    args = parser.parse_args()

    root = Path(args.root)
    if not root.exists():
        print(f"Root not found: {root}", file=sys.stderr)
        return 1

    files = list(iter_xaml_files(root))
    if not files:
        print(f"No .xaml files found under {root}")
        return 0

    print()
    print("XAML layout check")
    print(f"Root: {root.resolve()}")
    print()

    for path in files:
        check_file(path)

    print()
    print(f"Errors: {errors}   Warnings: {warnings}")

    if errors > 0:
        sys.stdout.flush()
        print("Layout check failed.", file=sys.stderr)
        return 1

    if args.strict and warnings > 0:
        sys.stdout.flush()
        print("Layout check failed because --strict was specified.", file=sys.stderr)
        return 1

    print("Layout check passed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
