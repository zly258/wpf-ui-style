#!/usr/bin/env python3
"""Lightweight XAML layout checker for common WPF mistakes.

Scans .xaml files and reports:
  - Height="*" on controls (only RowDefinition/ColumnDefinition allow *).
  - Hardcoded color literals and local FontFamily in business XAML.
  - Read-only log/code TextBox without MaxHeight (warning).
  - Wrapped TextBlock with VerticalAlignment="Stretch" (warning).
  - Nested ScrollViewer (warning).
  - Negative Margin (error) - overlapping controls.
  - Canvas used to lay out form controls (error) - absolute positioning.
  - Ellipsis trimming without a ToolTip (error) - text can never be read.
  - Fixed Height on wrapping text (error) - text is clipped vertically.
  - Fixed-width label column (warning) - long labels get cut.
  - Hardcoded Width on a TextBox (warning) - value gets truncated.
  - Window with a fixed Height and no SizeToContent (warning).
  - '*' row holding a StackPanel with no ScrollViewer (warning).
  - TabControl without a fixed Height (error in a SizeToContent="Height" window).

Errors exit with code 1. Warnings do not fail unless --strict is used.

Usage:
    python tools/LayoutCheck.py --root D:/Projects/YourApp
    python tools/LayoutCheck.py --root D:/repo --strict
"""

from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

EXCLUDE_DIRS = {"obj", "bin", ".build", "output", "dist", "node_modules"}
THEME_FILE_RE = re.compile(r"(WindowTheme|Styles|Theme)\.xaml$", re.IGNORECASE)
# Default theme file names recognized without --theme-file.
THEME_FILE_DEFAULTS = ["windowtheme.xaml", "styles.xaml", "theme.xaml"]
TAG_RE = re.compile(r"<([A-Za-z0-9_:]+)\b(.*?)>", re.DOTALL)
COMMENT_RE = re.compile(r"<!--.*?-->", re.DOTALL)
# #RGB / #RRGGBB / #AARRGGBB used as an attribute value. Requiring the leading
# quote keeps text such as "Issue #123" from being reported as a color.
COLOR_LITERAL_RE = re.compile(r'(?<=["\'])#(?:[0-9A-Fa-f]{8}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{3})\b')
FONTFAMILY_RE = re.compile(r'FontFamily\s*=\s*"[^"]{2,}"')
TEXTBOX_RE = re.compile(r"<TextBox\b(.*?)>", re.DOTALL)
TEXTBLOCK_RE = re.compile(r"<TextBlock\b(.*?)>", re.DOTALL)
SCROLLVIEWER_TAG_RE = re.compile(r"<ScrollViewer\b[^>]*?/?>|</ScrollViewer>", re.DOTALL)

# --- truncation / occlusion patterns (R14) -------------------------------
ELEMENT_RE = re.compile(r"<(/?)([A-Za-z0-9_:]+)([^>]*?)(/?)>", re.DOTALL)
MARGIN_RE = re.compile(r'Margin\s*=\s*"([^"]*)"')
# The lookbehind keeps MaxHeight / MinHeight / RowHeight from matching as "Height".
HEIGHT_RE = re.compile(r'(?<![A-Za-z])Height\s*=\s*"([0-9.]+)"')
WIDTH_RE = re.compile(r'(?<![A-Za-z])Width\s*=\s*"([0-9.]+)"')
COLUMN_DEF_RE = re.compile(r"<ColumnDefinition\b([^>]*?)/?>")
GRID_COLUMN_RE = re.compile(r'Grid\.Column\s*=\s*"(\d+)"')
ELLIPSIS_RE = re.compile(
    r'(TextTrimming|TextOverflow)\s*=\s*"(CharacterEllipsis|WordEllipsis|Ellipsis)"'
)
CANVAS_BLOCK_RE = re.compile(r"<Canvas\b[^>]*>(.*?)</Canvas>", re.DOTALL)
CANVAS_SELF_RE = re.compile(r"<Canvas\b[^>]*/>", re.DOTALL)
FORM_CONTROL_TAGS = (
    "TextBox",
    "PasswordBox",
    "ComboBox",
    "CheckBox",
    "RadioButton",
    "Button",
    "ListBox",
    "ListView",
    "DataGrid",
    "TabControl",
)

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


def is_theme_file(path: Path, theme_dirs: set[str], theme_files: set[str]) -> bool:
    if path.name.lower() in theme_files:
        return True
    return any(part.lower() in theme_dirs for part in path.parts[:-1])


def iter_element_blocks(text: str, tag_name: str):
    """Yield (start_index, end_index) for every <tag_name> element block."""
    stack = []
    for match in ELEMENT_RE.finditer(text):
        closing, name, _attrs, self_closing = match.groups()
        if name != tag_name:
            continue
        if closing:
            while stack:
                start, open_name = stack.pop()
                if open_name == tag_name:
                    yield start, match.end()
                    break
        elif not self_closing:
            stack.append((match.start(), name))


def check_negative_margin(path: Path, text: str) -> None:
    """Negative margins make neighbouring controls overlap (R14.6)."""
    for match in MARGIN_RE.finditer(text):
        raw = match.group(1)
        if any(part.strip().startswith("-") for part in raw.split(",")):
            report(
                "ERROR",
                path,
                line_number(text, match.start()),
                f'Negative Margin "{raw}"; overlapping controls. Use Padding or alignment instead.',
            )


def check_canvas_layout(path: Path, text: str) -> None:
    """Canvas positions are absolute and break as soon as the font or DPI changes."""
    for match in CANVAS_BLOCK_RE.finditer(text):
        inner = match.group(1)
        if any(f"<{tag}" in inner for tag in FORM_CONTROL_TAGS):
            report(
                "ERROR",
                path,
                line_number(text, match.start()),
                "Canvas lays out form controls absolutely; use Grid rows/columns (Auto/*) instead",
            )
        else:
            report(
                "WARN",
                path,
                line_number(text, match.start()),
                "Canvas used; absolute positioning is fragile under font scaling and DPI changes",
            )

    for match in CANVAS_SELF_RE.finditer(text):
        report(
            "WARN",
            path,
            line_number(text, match.start()),
            "Canvas used; absolute positioning is fragile under font scaling and DPI changes",
        )


def check_ellipsis_without_tooltip(path: Path, text: str) -> None:
    """Ellipsis trimming hides content; without a ToolTip the user can never read it."""
    for match in TEXTBLOCK_RE.finditer(text):
        attrs = match.group(1)
        if not ELLIPSIS_RE.search(attrs):
            continue
        if re.search(r"ToolTip\s*=", attrs):
            continue
        report(
            "ERROR",
            path,
            line_number(text, match.start()),
            "Text is trimmed with an ellipsis but has no ToolTip; the full text can never be read",
        )


def check_fixed_height_on_wrapping_text(path: Path, text: str) -> None:
    """A fixed height on wrapping text clips the text vertically."""
    for match in TEXTBLOCK_RE.finditer(text):
        attrs = match.group(1)
        if not re.search(r'TextWrapping\s*=\s*"Wrap"', attrs):
            continue
        height = HEIGHT_RE.search(attrs)
        if not height:
            continue
        report(
            "ERROR",
            path,
            line_number(text, match.start()),
            f'Wrapping TextBlock has a fixed Height="{height.group(1)}"; use Height="Auto" so the text is not clipped',
        )


def check_fixed_label_column(path: Path, text: str) -> None:
    """A label column with a fixed pixel width cuts long labels in half."""
    for start, end in iter_element_blocks(text, "Grid"):
        block = text[start:end]
        defs = re.search(
            r"<Grid\.ColumnDefinitions>(.*?)</Grid\.ColumnDefinitions>", block, re.DOTALL
        )
        if not defs:
            continue

        widths = []
        for match in COLUMN_DEF_RE.finditer(defs.group(1)):
            attrs = match.group(1)
            width = WIDTH_RE.search(attrs)
            widths.append(width.group(1) if width else None)

        if not any(widths):
            continue

        # Which columns hold a label (TextBlock)?
        label_columns = set()
        for match in TEXTBLOCK_RE.finditer(block):
            attrs = match.group(1)
            column = GRID_COLUMN_RE.search(attrs)
            label_columns.add(int(column.group(1)) if column else 0)

        for index, width in enumerate(widths):
            if width is None or index not in label_columns:
                continue
            try:
                pixels = float(width)
            except ValueError:
                continue
            # Narrow columns are spacers / gaps, not label columns.
            if pixels < 32:
                continue
            report(
                "WARN",
                path,
                line_number(text, start),
                f'Label column {index} has a fixed Width="{width}"; long labels get cut. '
                'Use Width="Auto" with MinWidth instead',
            )


def check_fixed_width_textbox(path: Path, text: str) -> None:
    """A hardcoded TextBox width truncates the value and fights the layout."""
    for match in TEXTBOX_RE.finditer(text):
        attrs = match.group(1)
        width = WIDTH_RE.search(attrs)
        if not width:
            continue
        report(
            "WARN",
            path,
            line_number(text, match.start()),
            f'TextBox has a fixed Width="{width.group(1)}"; let the column size it '
            '(* + MinWidth) and expose long values via ToolTip',
        )


def check_window_height(path: Path, text: str) -> None:
    """A fixed window height hides the bottom action bar as soon as content grows."""
    for match in ELEMENT_RE.finditer(text):
        closing, name, attrs, self_closing = match.groups()
        if closing or name != "Window" or self_closing:
            continue
        height = HEIGHT_RE.search(attrs)
        if not height:
            continue
        if re.search(r'SizeToContent\s*=\s*"Height"', attrs):
            continue
        report(
            "WARN",
            path,
            line_number(text, match.start()),
            f'Window has a fixed Height="{height.group(1)}"; use SizeToContent="Height" + MinHeight '
            "so the action bar is never pushed out of view",
        )


def _star_row_indexes(block: str) -> list[int]:
    """Indexes of the Grid rows declared with Height="*" inside a Grid block."""
    defs = re.search(r"<Grid\.RowDefinitions>(.*?)</Grid\.RowDefinitions>", block, re.DOTALL)
    if not defs:
        return []
    indexes = []
    for index, row in enumerate(re.findall(r"<RowDefinition\b[^>]*?/?>", defs.group(1))):
        if re.search(r'Height\s*=\s*"\*"', row):
            indexes.append(index)
    return indexes


# Controls that scroll their own content and may legally own a '*' row.
_SCROLLING_CONTROLS = (
    "DataGrid", "ListBox", "ListView", "TreeView", "ScrollViewer",
    "DataGridSelector", "ListBoxItem",
)


def check_star_row_without_scroll(path: Path, text: str) -> None:
    """A '*' row holding a StackPanel with no ScrollViewer clips its content.

    The check is row-aware: only the element actually placed in the '*' row is
    examined. A DataGrid / ListBox in a '*' row scrolls on its own and is fine.
    """
    reported = False
    for start, end in iter_element_blocks(text, "Grid"):
        block = text[start:end]
        star_rows = _star_row_indexes(block)
        if not star_rows:
            continue

        for index in star_rows:
            # The element placed in this row carries Grid.Row="<index>".
            occupant = None
            for match in ELEMENT_RE.finditer(block):
                closing, name, attrs, self_closing = match.groups()
                if closing or name in ("RowDefinition", "ColumnDefinition"):
                    continue
                row = re.search(r'Grid\.Row\s*=\s*"(\d+)"', attrs)
                if row and int(row.group(1)) == index:
                    occupant = name
                    break
                # No Grid.Row: the first child fills row 0.
                if row is None and index == 0 and occupant is None:
                    occupant = name
            if occupant is None:
                continue
            if occupant in _SCROLLING_CONTROLS:
                continue
            if occupant != "StackPanel":
                continue
            if "<ScrollViewer" in block:
                continue

            report(
                "WARN",
                path,
                line_number(text, start),
                f"A '*' row holds a {occupant} but the file has no ScrollViewer; "
                "long content will be clipped instead of scrolled",
            )
            reported = True
            break
        if reported:
            return


def check_tabcontrol_height(path: Path, text: str) -> None:
    """A TabControl without a fixed height resizes the window when tabs are switched."""
    matches = [m for m in ELEMENT_RE.finditer(text) if not m.group(1) and m.group(2) == "TabControl"]
    if not matches:
        return

    # A window that sizes itself to its content turns any tab height change
    # into a window height change.
    size_to_content = bool(re.search(r'<Window\b[^>]*SizeToContent\s*=\s*"Height"', text))
    level = "ERROR" if size_to_content else "WARN"

    for match in matches:
        attrs = match.group(3)
        if HEIGHT_RE.search(attrs):
            continue
        report(
            level,
            path,
            line_number(text, match.start()),
            "TabControl has no fixed Height; switching to a tab with less content "
            "changes the content height and the window jumps. Give the TabControl a "
            'fixed Height (e.g. 140) or a fixed-height content area',
        )


def check_file(path: Path, theme_dirs: set[str], theme_files: set[str]) -> None:
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
    #    - A TextBox styled with TextBox.Code is skipped: the theme style itself
    #      carries MaxHeight (Layout.LogMaxHeight).
    #    - A single-line IsReadOnly TextBox is not a log; only multiline readonly
    #      boxes are treated as logs.
    for match in TEXTBOX_RE.finditer(text):
        attrs = match.group(1)
        if "TextBox.Code" in attrs:
            continue
        is_multiline = (
            bool(re.search(r'AcceptsReturn\s*=\s*"True"', attrs))
            or "TextBox.Multiline" in attrs
        )
        is_log = is_multiline and bool(re.search(r'IsReadOnly\s*=\s*"True"', attrs))
        if not is_log:
            continue

        if not re.search(r"MaxHeight\s*=", attrs):
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
    if not is_theme_file(path, theme_dirs, theme_files):
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
    if not is_theme_file(path, theme_dirs, theme_files):
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

    # 6. Negative margin: causes overlapping controls (applies everywhere).
    check_negative_margin(path, text)

    if not is_theme_file(path, theme_dirs, theme_files):
        # 7. Truncation / occlusion rules for business XAML (R14).
        check_canvas_layout(path, text)
        check_ellipsis_without_tooltip(path, text)
        check_fixed_height_on_wrapping_text(path, text)
        check_fixed_label_column(path, text)
        check_fixed_width_textbox(path, text)
        check_window_height(path, text)
        check_star_row_without_scroll(path, text)
        check_tabcontrol_height(path, text)

    # 8. Nested ScrollViewer (skip theme/style templates: ComboBox popups legitimately nest)
    if is_theme_file(path, theme_dirs, theme_files):
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
    parser.add_argument(
        "--theme-dir", "-ThemeDir", "-theme-dir", action="append", default=[],
        help="Directory name treated as theme/style source (may contain literals). Repeatable.")
    parser.add_argument(
        "--theme-file", "-ThemeFile", "-theme-file", action="append", default=[],
        help="File name treated as theme/style source (may contain literals). Repeatable.")
    parser.add_argument("--strict", "-Strict", "-strict", action="store_true")
    args = parser.parse_args()

    theme_dirs = {d.lower() for d in args.theme_dir} | {"theme"}
    theme_files = {f.lower() for f in args.theme_file} | {m.lower() for m in THEME_FILE_DEFAULTS}

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
        check_file(path, theme_dirs, theme_files)

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
