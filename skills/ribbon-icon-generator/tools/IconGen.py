#!/usr/bin/env python3
"""Ribbon icon generator — 24x24 monoline icons for CAD/IDE ribbon UIs.

Single source of truth: catalog/icons.json. Everything else (SVG, XAML,
preview sheet, image prompt) is generated from it, so the style stays uniform.

Usage:
    python tools/IconGen.py list
    python tools/IconGen.py search 框选
    python tools/IconGen.py svg box-select --size 24
    python tools/IconGen.py all --out samples/svg --size 32
    python tools/IconGen.py xaml --out samples/RibbonIcons.xaml
    python tools/IconGen.py preview --out preview/icons.html
    python tools/IconGen.py check --strict
    python tools/IconGen.py prompt "批量导出" --theme dark

Python 3.8+, standard library only.
"""

from __future__ import annotations

import argparse
import json
import math
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
CATALOG = ROOT / "catalog" / "icons.json"

ALLOWED_KINDS = {
    "path": {"d"},
    "rect": {"x", "y", "w", "h", "r"},
    "circle": {"cx", "cy", "r"},
    "line": {"x1", "y1", "x2", "y2"},
}
ALLOWED_DASH = {"2 2", "2 3"}
GRID = 24
STROKE = 2
SAFE_MIN, SAFE_MAX = 2.0, 22.0        # hard limit: stroke must stay inside the 24 box
TIDY_MIN, TIDY_MAX = 3.0, 21.0        # soft limit: keeps a full 1px optical margin
MIN_CONTRAST = 3.0                    # WCAG non-text contrast


def fmt(value: float) -> str:
    """Compact number formatting for SVG/XAML output."""
    if abs(value - round(value)) < 1e-9:
        return str(int(round(value)))
    return f"{value:.2f}".rstrip("0").rstrip(".")


def load_catalog(path: Path = CATALOG) -> dict:
    with path.open(encoding="utf-8") as fh:
        return json.load(fh)


# ---------------------------------------------------------------------------
# SVG rendering
# ---------------------------------------------------------------------------

def shape_svg(shape: dict) -> str:
    kind = shape["kind"]
    if kind == "path":
        attrs = [("d", shape["d"])]
    elif kind == "rect":
        attrs = [("x", shape["x"]), ("y", shape["y"]),
                 ("width", shape["w"]), ("height", shape["h"])]
        if shape.get("r"):
            attrs.append(("rx", shape["r"]))
    elif kind == "circle":
        attrs = [("cx", shape["cx"]), ("cy", shape["cy"]), ("r", shape["r"])]
    elif kind == "line":
        attrs = [("x1", shape["x1"]), ("y1", shape["y1"]),
                 ("x2", shape["x2"]), ("y2", shape["y2"])]
    else:
        raise ValueError(f"unsupported shape kind: {kind}")

    parts = [f'{name}="{fmt(value) if not isinstance(value, str) else value}"'
             for name, value in attrs]
    if shape.get("dash"):
        parts.append(f'stroke-dasharray="{shape["dash"]}"')
    return f"<{kind} {' '.join(parts)} />"


def render_svg(icon: dict, stroke: str = "currentColor",
               background: str | None = None, size: int = 24) -> str:
    lines = [
        f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" '
        f'width="{size}" height="{size}" fill="none" stroke="{stroke}" '
        f'stroke-width="2" stroke-linecap="round" stroke-linejoin="round">'
    ]
    if background:
        lines.append(f'  <rect x="0" y="0" width="24" height="24" fill="{background}" stroke="none" />')
    lines.extend(f"  {shape_svg(shape)}" for shape in icon["shapes"])
    lines.append("</svg>")
    return "\n".join(lines)


# ---------------------------------------------------------------------------
# Path geometry: parse SVG path data -> bounding box and WPF geometry
# ---------------------------------------------------------------------------

NUMBER_RE = re.compile(r"[-+]?(?:\d*\.\d+|\d+\.?)(?:[eE][-+]?\d+)?")


def arc_points(x1, y1, rx, ry, phi, large, sweep, x2, y2):
    """Endpoint -> center parameterization (SVG spec F.6.5); returns arc points."""
    if rx == 0 or ry == 0:
        return [(x2, y2)]
    rx, ry = abs(rx), abs(ry)
    phi_r = math.radians(phi)
    cos_phi, sin_phi = math.cos(phi_r), math.sin(phi_r)

    dx, dy = (x1 - x2) / 2.0, (y1 - y2) / 2.0
    x1p = cos_phi * dx + sin_phi * dy
    y1p = -sin_phi * dx + cos_phi * dy

    lam = (x1p ** 2) / (rx ** 2) + (y1p ** 2) / (ry ** 2)
    if lam > 1:
        scale = math.sqrt(lam)
        rx, ry = rx * scale, ry * scale

    num = rx ** 2 * ry ** 2 - rx ** 2 * y1p ** 2 - ry ** 2 * x1p ** 2
    den = rx ** 2 * y1p ** 2 + ry ** 2 * x1p ** 2
    coef = math.sqrt(max(0.0, num / den)) if den else 0.0
    if large == sweep:
        coef = -coef

    cxp = coef * (rx * y1p / ry)
    cyp = coef * -(ry * x1p / rx)
    cx = cos_phi * cxp - sin_phi * cyp + (x1 + x2) / 2.0
    cy = sin_phi * cxp + cos_phi * cyp + (y1 + y2) / 2.0

    def angle(ux, uy, vx, vy):
        dot = ux * vx + uy * vy
        norm = math.hypot(ux, uy) * math.hypot(vx, vy)
        a = math.acos(max(-1.0, min(1.0, dot / norm))) if norm else 0.0
        return -a if ux * vy - uy * vx < 0 else a

    theta1 = angle(1, 0, (x1p - cxp) / rx, (y1p - cyp) / ry)
    dtheta = angle((x1p - cxp) / rx, (y1p - cyp) / ry,
                   (-x1p - cxp) / rx, (-y1p - cyp) / ry)
    if sweep == 0 and dtheta > 0:
        dtheta -= 2 * math.pi
    elif sweep == 1 and dtheta < 0:
        dtheta += 2 * math.pi

    points = []
    steps = max(2, int(abs(dtheta) / (math.pi / 12)) + 1)
    for i in range(steps + 1):
        t = theta1 + dtheta * i / steps
        px = cx + rx * math.cos(t) * cos_phi - ry * math.sin(t) * sin_phi
        py = cy + rx * math.cos(t) * sin_phi + ry * math.sin(t) * cos_phi
        points.append((px, py))
    return points


def path_points(d: str):
    """Walk an SVG path string and collect on-curve/control points for bounds."""
    tokens = re.findall(r"[MmLlHhVvCcSsQqTtAaZz]|[-+]?(?:\d*\.\d+|\d+\.?)(?:[eE][-+]?\d+)?", d)
    points = []
    i = 0
    cmd = None
    cur = (0.0, 0.0)
    start = (0.0, 0.0)

    def take(n):
        nonlocal i
        values = [float(tokens[i + k]) for k in range(n)]
        i += n
        return values

    while i < len(tokens):
        token = tokens[i]
        if re.match(r"[A-Za-z]", token):
            cmd = token
            i += 1
            if cmd in "Zz":
                cur = start
                points.append(cur)
                continue
        elif cmd is None:
            raise ValueError(f"path data starts with a number: {d}")

        rel = cmd.islower()
        kind = cmd.upper()

        if kind == "M":
            x, y = take(2)
            cur = (cur[0] + x, cur[1] + y) if rel else (x, y)
            start = cur
            points.append(cur)
            cmd = "l" if rel else "L"
        elif kind == "L":
            x, y = take(2)
            cur = (cur[0] + x, cur[1] + y) if rel else (x, y)
            points.append(cur)
        elif kind == "H":
            (x,) = take(1)
            cur = (cur[0] + x if rel else x, cur[1])
            points.append(cur)
        elif kind == "V":
            (y,) = take(1)
            cur = (cur[0], cur[1] + y if rel else y)
            points.append(cur)
        elif kind in ("C", "S", "Q", "T"):
            n = {"C": 6, "S": 4, "Q": 4, "T": 2}[kind]
            vals = take(n)
            pts = [(vals[k], vals[k + 1]) for k in range(0, n, 2)]
            if rel:
                pts = [(cur[0] + px, cur[1] + py) for px, py in pts]
            points.extend(pts)
            cur = pts[-1]
        elif kind == "A":
            rx, ry, rot, large, sweep, x, y = take(7)
            end = (cur[0] + x, cur[1] + y) if rel else (x, y)
            points.extend(arc_points(cur[0], cur[1], rx, ry, rot,
                                     int(large), int(sweep), end[0], end[1]))
            cur = end
        else:
            raise ValueError(f"unsupported path command: {cmd}")
    return points


def shape_bbox(shape: dict):
    kind = shape["kind"]
    if kind == "path":
        pts = path_points(shape["d"])
        xs = [p[0] for p in pts]
        ys = [p[1] for p in pts]
        return min(xs), min(ys), max(xs), max(ys)
    if kind == "rect":
        x, y, w, h = shape["x"], shape["y"], shape["w"], shape["h"]
        return x, y, x + w, y + h
    if kind == "circle":
        cx, cy, r = shape["cx"], shape["cy"], shape["r"]
        return cx - r, cy - r, cx + r, cy + r
    if kind == "line":
        return (min(shape["x1"], shape["x2"]), min(shape["y1"], shape["y2"]),
                max(shape["x1"], shape["x2"]), max(shape["y1"], shape["y2"]))
    raise ValueError(f"unsupported shape kind: {kind}")


def rect_path(shape: dict) -> str:
    x, y, w, h = (float(shape[k]) for k in ("x", "y", "w", "h"))
    r = float(shape.get("r") or 0)
    if r <= 0:
        return f"M{fmt(x)} {fmt(y)}H{fmt(x + w)}V{fmt(y + h)}H{fmt(x)}Z"
    r = min(r, w / 2, h / 2)
    a = f"A{fmt(r)} {fmt(r)} 0 0 1 "
    return (
        f"M{fmt(x + r)} {fmt(y)}H{fmt(x + w - r)}{a}{fmt(x + w)} {fmt(y + r)}"
        f"V{fmt(y + h - r)}{a}{fmt(x + w - r)} {fmt(y + h)}"
        f"H{fmt(x + r)}{a}{fmt(x)} {fmt(y + h - r)}"
        f"V{fmt(y + r)}{a}{fmt(x + r)} {fmt(y)}Z"
    )


def circle_path(shape: dict) -> str:
    cx, cy, r = float(shape["cx"]), float(shape["cy"]), float(shape["r"])
    return (f"M{fmt(cx - r)} {fmt(cy)}"
            f"A{fmt(r)} {fmt(r)} 0 1 0 {fmt(cx + r)} {fmt(cy)}"
            f"A{fmt(r)} {fmt(r)} 0 1 0 {fmt(cx - r)} {fmt(cy)}Z")


def shape_geometry(shape: dict) -> str:
    kind = shape["kind"]
    if kind == "path":
        return shape["d"]
    if kind == "rect":
        return rect_path(shape)
    if kind == "circle":
        return circle_path(shape)
    if kind == "line":
        return f"M{fmt(shape['x1'])} {fmt(shape['y1'])}L{fmt(shape['x2'])} {fmt(shape['y2'])}"
    raise ValueError(f"unsupported shape kind: {kind}")


def icon_geometry(icon: dict):
    solid = [shape_geometry(s) for s in icon["shapes"] if not s.get("dash")]
    dashed = [shape_geometry(s) for s in icon["shapes"] if s.get("dash")]
    return " ".join(solid), " ".join(dashed)


# ---------------------------------------------------------------------------
# XAML output
# ---------------------------------------------------------------------------

PASCAL = re.compile(r"(^|-)([a-z0-9])")


def pascal(icon_id: str) -> str:
    return PASCAL.sub(lambda m: m.group(2).upper(), icon_id)


XAML_HEADER = """<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- ======================================================================
         Ribbon 线性图标 (24x24 / stroke 2 / round cap & join)
         由 tools/IconGen.py xaml 生成，请勿手改：改 catalog/icons.json 后重跑。
         用法见 SKILL.md「WPF 集成」一节。
         ====================================================================== -->

    <Style x:Key="Icon.Ribbon" TargetType="Path">
        <Setter Property="Fill" Value="{x:Null}" />
        <Setter Property="Stroke"
                Value="{Binding Path=(TextElement.Foreground),
                                RelativeSource={RelativeSource Self},
                                FallbackValue={StaticResource Brush.Text.Primary}}" />
        <Setter Property="StrokeThickness" Value="2" />
        <Setter Property="StrokeStartLineCap" Value="Round" />
        <Setter Property="StrokeEndLineCap" Value="Round" />
        <Setter Property="StrokeLineJoin" Value="Round" />
        <!-- 固定 24x24 设计画布：几何本身不铺满时也保持同一光学尺寸，
             外层 Viewbox 负责缩放到 Icon.Size.*，不要改成 Uniform。 -->
        <Setter Property="Width" Value="24" />
        <Setter Property="Height" Value="24" />
        <Setter Property="Stretch" Value="None" />
        <Setter Property="UseLayoutRounding" Value="True" />
        <Setter Property="SnapsToDevicePixels" Value="True" />
    </Style>

    <!-- 虚线=框选/范围/剖切面；2 3 / stroke 2 = WPF 的 1 1.5 -->
    <Style x:Key="Icon.Ribbon.Dash" TargetType="Path" BasedOn="{StaticResource Icon.Ribbon}">
        <Setter Property="StrokeDashArray" Value="1 1.5" />
    </Style>
"""


def render_xaml(catalog: dict) -> str:
    out = [XAML_HEADER]
    for category in catalog["categories"]:
        out.append(f"\n    <!-- {category['zh']} ({category['en']}) -->")
        for icon in catalog["icons"]:
            if icon["category"] != category["id"]:
                continue
            key = f"Icon.Ribbon.{pascal(icon['id'])}"
            solid, dashed = icon_geometry(icon)
            if solid:
                out.append(f'    <!-- {icon["zh"]} / {icon["en"]} -->')
                out.append(f'    <StreamGeometry x:Key="{key}">{solid}</StreamGeometry>')
            if dashed:
                out.append(f'    <StreamGeometry x:Key="{key}.Dash">{dashed}</StreamGeometry>')
    out.append("\n</ResourceDictionary>\n")
    return "\n".join(out)


# ---------------------------------------------------------------------------
# Preview sheet
# ---------------------------------------------------------------------------

PREVIEW_CSS = """
:root { color-scheme: dark; }
* { box-sizing: border-box; }
body { margin: 0; font-family: "Segoe UI", "Microsoft YaHei", system-ui, sans-serif; }
section { padding: 28px 32px 36px; }
section.dark { background: #2D323A; color: #FFFFFF; }
section.light { background: #F3F3F3; color: #4A4A4A; }
h1 { font-size: 20px; margin: 0 0 4px; font-weight: 600; letter-spacing: .2px; }
h2 { font-size: 12px; font-weight: 600; text-transform: uppercase; letter-spacing: .12em;
     opacity: .62; margin: 28px 0 10px; }
p.sub { margin: 0 0 22px; font-size: 12px; opacity: .66; }
.grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(132px, 1fr)); gap: 10px; }
.card { border: 1px solid currentColor; border-color: rgba(128,128,128,.22);
        border-radius: 8px; padding: 12px 12px 10px; display: flex; flex-direction: column; gap: 10px; }
.row { display: flex; align-items: center; gap: 10px; min-height: 40px; }
.icon { display: flex; align-items: center; justify-content: center; flex: none; }
.tile { background: rgba(128,128,128,.14); border-radius: 4px; padding: 4px; }
.name { font-size: 12px; line-height: 1.35; }
.name b { font-weight: 600; display: block; }
.name span { font-size: 10.5px; opacity: .58; letter-spacing: .02em; }
.sizes { display: flex; align-items: flex-end; gap: 14px; padding-top: 2px; opacity: .9; }
.sizes figure { margin: 0; display: flex; flex-direction: column; align-items: center; gap: 3px; }
.sizes figcaption { font-size: 9.5px; opacity: .55; }
"""


def render_preview(catalog: dict) -> str:
    themes = catalog["themes"]
    body = []
    svg_cache = {}

    def svg(icon_id: str, size: int, color: str) -> str:
        key = (icon_id, size, color)
        if key not in svg_cache:
            icon = next(i for i in catalog["icons"] if i["id"] == icon_id)
            svg_cache[key] = render_svg(icon, stroke=color, size=size)
        return svg_cache[key]

    for theme_name in ("dark", "light"):
        theme = themes[theme_name]
        fg, bg = theme["foreground"], theme["background"]
        body.append(f'<section class="{theme_name}">')
        body.append(f"  <h1>Ribbon 线性图标 · {theme_name.title()} 模式</h1>")
        body.append(f'  <p class="sub">24×24 网格 / stroke 2 / round cap / 前景 {fg} · 背景 {bg}</p>')
        for category in catalog["categories"]:
            icons = [i for i in catalog["icons"] if i["category"] == category["id"]]
            if not icons:
                continue
            body.append(f"  <h2>{category['zh']} · {category['en']}</h2>")
            body.append('  <div class="grid">')
            for icon in icons:
                body.append('    <div class="card">')
                body.append('      <div class="row">')
                body.append(f'        <div class="icon tile">{svg(icon["id"], 32, fg)}</div>')
                body.append(f'        <div class="icon tile">{svg(icon["id"], 16, fg)}</div>')
                body.append("      </div>")
                body.append(f'      <div class="name"><b>{icon["zh"]}</b><span>{icon["id"]} · {icon["en"]}</span></div>')
                body.append("    </div>")
            body.append("  </div>")
        body.append("</section>")

    return (
        "<!DOCTYPE html>\n<html lang=\"zh-CN\">\n<head>\n<meta charset=\"utf-8\">\n"
        "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n"
        "<title>Ribbon 图标预览</title>\n<style>" + PREVIEW_CSS + "</style>\n</head>\n<body>\n"
        + "\n".join(body) + "\n</body>\n</html>\n"
    )


# ---------------------------------------------------------------------------
# Validation
# ---------------------------------------------------------------------------

def relative_luminance(hex_color: str) -> float:
    hex_color = hex_color.lstrip("#")
    channels = [int(hex_color[i:i + 2], 16) / 255.0 for i in (0, 2, 4)]
    linear = [c / 12.92 if c <= 0.03928 else ((c + 0.055) / 1.055) ** 2.4 for c in channels]
    return 0.2126 * linear[0] + 0.7152 * linear[1] + 0.0722 * linear[2]


def contrast_ratio(fg: str, bg: str) -> float:
    a, b = relative_luminance(fg), relative_luminance(bg)
    high, low = max(a, b), min(a, b)
    return (high + 0.05) / (low + 0.05)


def validate(catalog: dict) -> tuple[list[str], list[str]]:
    errors, warnings = [], []
    icons = catalog.get("icons", [])

    if catalog.get("grid") != GRID:
        errors.append(f"catalog grid 必须是 {GRID}")
    if catalog.get("stroke") != STROKE:
        errors.append(f"catalog stroke 必须是 {STROKE}")

    for theme_name, theme in catalog.get("themes", {}).items():
        ratio = contrast_ratio(theme["foreground"], theme["background"])
        if ratio < MIN_CONTRAST:
            errors.append(f"{theme_name} 主题对比度 {ratio:.2f}:1 < {MIN_CONTRAST}:1")
        else:
            warnings.append(f"{theme_name} 主题对比度 {ratio:.2f}:1 ✓")

    category_ids = {c["id"] for c in catalog.get("categories", [])}
    seen_ids = set()
    seen_names = {}

    for index, icon in enumerate(icons):
        name = icon.get("id") or f"#{index}"
        for field in ("id", "zh", "en", "category", "keywords", "concept", "shapes"):
            if not icon.get(field):
                errors.append(f"{name}: 缺少字段 {field}")

        icon_id = icon.get("id", "")
        if not re.fullmatch(r"[a-z0-9]+(?:-[a-z0-9]+)*", icon_id):
            errors.append(f"{name}: id 必须是 kebab-case（小写字母、数字、连字符）")
        if icon_id in seen_ids:
            errors.append(f"{name}: id 重复")
        seen_ids.add(icon_id)

        label = icon.get("zh", name)
        if icon.get("zh") in seen_names:
            warnings.append(f"{name}: 中文名「{label}」与 {seen_names[icon['zh']]} 重复")
        seen_names[icon.get("zh")] = name

        if icon.get("category") not in category_ids:
            errors.append(f"{name}: 未知 category「{icon.get('category')}」")

        shapes = icon.get("shapes") or []
        if not shapes:
            errors.append(f"{name}: shapes 为空")
        if len(shapes) > 5:
            errors.append(f"{name}: shapes {len(shapes)} 个 > 5，24px 下会粘连")
        elif len(shapes) > 4:
            warnings.append(f"{name}: shapes {len(shapes)} 个，建议 ≤ 4")

        bbox = None
        for shape in shapes:
            kind = shape.get("kind")
            if kind not in ALLOWED_KINDS:
                errors.append(f"{name}: 不支持 shape kind「{kind}」")
                continue
            extra = set(shape) - ALLOWED_KINDS[kind] - {"kind", "dash"}
            if extra:
                errors.append(f"{name}: shape 含非法属性 {sorted(extra)}")
            if shape.get("dash") and shape["dash"] not in ALLOWED_DASH:
                errors.append(f"{name}: 虚线只能是 {sorted(ALLOWED_DASH)}，实际「{shape['dash']}」")

            for key, value in shape.items():
                if key in ("kind", "dash", "d"):
                    continue
                if not isinstance(value, (int, float)):
                    errors.append(f"{name}.{key}: 必须是数字")

            try:
                box = shape_bbox(shape)
            except Exception as exc:  # noqa: BLE001 - report a readable diagnostic
                errors.append(f"{name}: 无法解析几何（{exc}）")
                continue

            bbox = box if bbox is None else (
                min(bbox[0], box[0]), min(bbox[1], box[1]),
                max(bbox[2], box[2]), max(bbox[3], box[3]))

            if box[0] < SAFE_MIN or box[1] < SAFE_MIN or box[2] > SAFE_MAX or box[3] > SAFE_MAX:
                errors.append(
                    f"{name}: 几何越界 [{box[0]:.2f},{box[1]:.2f}]-[{box[2]:.2f},{box[3]:.2f}]，"
                    f"必须留在 {SAFE_MIN:g}..{SAFE_MAX:g} 安全区内")
            elif box[0] < TIDY_MIN or box[1] < TIDY_MIN or box[2] > TIDY_MAX or box[3] > TIDY_MAX:
                warnings.append(
                    f"{name}: 几何贴边 [{box[0]:.2f},{box[1]:.2f}]-[{box[2]:.2f},{box[3]:.2f}]，"
                    f"stroke 2 会只剩 <1px 视觉留白")

        if bbox:
            width, height = bbox[2] - bbox[0], bbox[3] - bbox[1]
            if width < 10 and height < 10:
                warnings.append(f"{name}: 图形仅 {width:.0f}×{height:.0f}，16px 渲染时偏小")

        # 密度启发式：路径命令越多，小尺寸越容易糊在一起
        numbers = sum(len(NUMBER_RE.findall(s.get("d", ""))) for s in shapes)
        if numbers > 64:
            warnings.append(f"{name}: 坐标点 {numbers} 个，细节偏多，建议在 24px 下精简")

    if len(icons) < 1:
        errors.append("catalog 里没有任何图标")

    return errors, warnings


# ---------------------------------------------------------------------------
# Image-generation prompt
# ---------------------------------------------------------------------------

NEGATIVE = (
    "complex details, 3D, isometric, gradients, shadows, textures, multiple colors, "
    "thin lines, realistic, cluttered, filled shapes, text, watermark"
)


def image_prompt(subject: str, theme: str, concept: str | None = None) -> str:
    palette = {
        "dark": "pure white lines on solid dark slate gray background (#2D323A)",
        "light": "dark charcoal gray lines (#4A4A4A) on solid light gray background (#F3F3F3)",
    }
    if theme not in palette:
        raise SystemExit(f"未知主题「{theme}」，可选：dark / light")
    idea = f"{subject}" + (f" ({concept})" if concept else "")
    return (
        f"Minimalist flat line icon of {idea}, CAD software ribbon UI style, "
        "2px uniform monoline stroke, rounded corners, extremely simple geometric silhouette, "
        f"high contrast monochrome, {palette[theme]}, centered, 2D vector, no gradients, "
        "no shadows, clean negative space."
    )


# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------

def find_icon(catalog: dict, name: str) -> dict:
    match = next((i for i in catalog["icons"] if i["id"] == name), None)
    if match is None:
        match = next((i for i in catalog["icons"] if i["zh"] == name or i["en"].lower() == name.lower()), None)
    if match is None:
        raise SystemExit(f"找不到图标「{name}」。用 list / search 查看可用 id。")
    return match


def cmd_list(catalog: dict, categories: list[str]) -> None:
    for category in catalog["categories"]:
        if categories and category["id"] not in categories:
            continue
        icons = [i for i in catalog["icons"] if i["category"] == category["id"]]
        if not icons:
            continue
        print(f"\n[{category['id']}] {category['zh']} · {category['en']}  ({len(icons)})")
        for icon in icons:
            print(f"  {icon['id']:<16} {icon['zh']:<8} {icon['en']}")
    print(f"\n合计 {len(catalog['icons'])} 个图标")


def cmd_search(catalog: dict, term: str) -> None:
    term_lower = term.lower()
    hits = []
    for icon in catalog["icons"]:
        haystack = " ".join([icon["id"], icon["zh"], icon["en"], icon["concept"]] + icon["keywords"]).lower()
        if term_lower in haystack:
            hits.append(icon)
    if not hits:
        print(f"没有匹配「{term}」的图标；可用 catalog + 绘图规范自行新增。")
        return
    for icon in hits:
        print(f"{icon['id']:<16} {icon['zh']:<8} {icon['en']:<18} {icon['concept']}")


def cmd_check(catalog: dict, strict: bool) -> int:
    errors, warnings = validate(catalog)
    for line in warnings:
        prefix = "INFO " if line.endswith("✓") else "WARN "
        print(f"{prefix} {line}")
    for line in errors:
        print(f"ERROR {line}")
    print(f"\n{len(catalog['icons'])} 个图标，{len(errors)} 个错误，"
          f"{len([w for w in warnings if not w.endswith('✓')])} 个警告")
    if errors:
        return 1
    if strict and any(w for w in warnings if not w.endswith("✓")):
        return 2
    return 0


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        prog="IconGen.py",
        description="24x24 monoline ribbon icon generator (SVG / XAML / preview / check / prompt).")
    sub = parser.add_subparsers(dest="command", required=True)

    p_list = sub.add_parser("list", help="列出图标目录")
    p_list.add_argument("--category", action="append", default=[], help="只看某分类，可重复")

    p_search = sub.add_parser("search", help="按关键字搜索图标")
    p_search.add_argument("term")

    p_svg = sub.add_parser("svg", help="输出单个图标的 SVG")
    p_svg.add_argument("id")
    p_svg.add_argument("--size", type=int, default=24)
    p_svg.add_argument("--theme", choices=("dark", "light", "none"), default="none",
                       help="dark/light 会填入对应背景与描边色")
    p_svg.add_argument("--out", help="输出目录（省略则打印到终端）")

    p_all = sub.add_parser("all", help="批量输出全部 SVG")
    p_all.add_argument("--out", default=str(ROOT / "samples" / "svg"))
    p_all.add_argument("--size", type=int, default=32)
    p_all.add_argument("--theme", choices=("dark", "light", "none"), default="none")

    p_xaml = sub.add_parser("xaml", help="输出 WPF ResourceDictionary")
    p_xaml.add_argument("--out", help=".xaml 路径（省略则打印到终端）")

    p_preview = sub.add_parser("preview", help="生成深浅双主题预览页")
    p_preview.add_argument("--out", default=str(ROOT / "preview" / "icons.html"))

    p_check = sub.add_parser("check", help="校验目录与安全边距")
    p_check.add_argument("--strict", action="store_true", help="有警告也返回非零")

    p_prompt = sub.add_parser("prompt", help="输出 AI 绘图提示词")
    p_prompt.add_argument("subject")
    p_prompt.add_argument("--theme", choices=("dark", "light"), default="dark")
    p_prompt.add_argument("--concept", help="补充几何意象，例如「漏斗 + 虚线范围框」")
    p_prompt.add_argument("--negative", action="store_true", help="同时输出负面提示词")

    return parser


def main(argv: list[str] | None = None) -> int:
    args = build_parser().parse_args(argv)
    catalog = load_catalog()

    if args.command == "list":
        cmd_list(catalog, args.category)
        return 0

    if args.command == "search":
        cmd_search(catalog, args.term)
        return 0

    if args.command == "check":
        return cmd_check(catalog, args.strict)

    if args.command == "svg":
        icon = find_icon(catalog, args.id)
        theme = catalog["themes"].get(args.theme)
        svg = render_svg(
            icon,
            stroke=theme["foreground"] if theme else "currentColor",
            background=theme["background"] if theme else None,
            size=args.size)
        if args.out:
            target = Path(args.out)
            target.mkdir(parents=True, exist_ok=True)
            path = target / f"{icon['id']}.svg"
            path.write_text(svg + "\n", encoding="utf-8")
            print(path)
        else:
            print(svg)
        return 0

    if args.command == "all":
        out = Path(args.out)
        out.mkdir(parents=True, exist_ok=True)
        theme = catalog["themes"].get(args.theme)
        for icon in catalog["icons"]:
            svg = render_svg(
                icon,
                stroke=theme["foreground"] if theme else "currentColor",
                background=theme["background"] if theme else None,
                size=args.size)
            (out / f"{icon['id']}.svg").write_text(svg + "\n", encoding="utf-8")
        print(f"{len(catalog['icons'])} 个 SVG -> {out}")
        return 0

    if args.command == "xaml":
        xaml = render_xaml(catalog)
        if args.out:
            path = Path(args.out)
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text(xaml, encoding="utf-8")
            print(path)
        else:
            print(xaml)
        return 0

    if args.command == "preview":
        path = Path(args.out)
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(render_preview(catalog), encoding="utf-8")
        print(path)
        return 0

    if args.command == "prompt":
        # 命中目录时用官方英文名，避免中文短语进入英文提示词
        subject = args.subject
        for icon in catalog["icons"]:
            if args.subject in (icon["id"], icon["zh"], icon["en"]):
                subject = icon["en"]
                break
        print(image_prompt(subject, args.theme, args.concept))
        if args.negative:
            print()
            print(f"Negative prompt: {NEGATIVE}")
        return 0

    return 1


if __name__ == "__main__":
    sys.exit(main())