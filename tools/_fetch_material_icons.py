import re, math, urllib.request, json
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent

ICONS = {
    "Search": "search",
    "Close": "close",
    "Check": "check",
    "Plus": "add",
    "Edit": "edit",
    "Trash": "delete",
    "Refresh": "refresh",
    "Settings": "settings",
    "Filter": "filter_alt",
    "Download": "download",
    "Upload": "upload",
    "Copy": "content_copy",
    "Folder": "folder",
    "File": "description",
    "ChevronDown": "expand_more",
    "ChevronUp": "expand_less",
    "ChevronLeft": "chevron_left",
    "ChevronRight": "chevron_right",
    "Info": "info",
    "SuccessCircle": "check_circle",
    "WarningTriangle": "warning",
    "ErrorCircle": "cancel",
    "Translate": "translate",
    "Save": "save",
    "Play": "play_arrow",
    "Pause": "pause",
    "Stop": "stop",
    "ZoomIn": "zoom_in",
    "ZoomOut": "zoom_out",
    "Visibility": "visibility",
    "MoreVert": "more_vert",
    "DragIndicator": "drag_indicator",
}

CMD_ARGS = {'M': 2, 'L': 2, 'H': 1, 'V': 1, 'C': 6, 'S': 4, 'Q': 4, 'T': 2, 'A': 7, 'Z': 0}
TOKEN_RE = re.compile(r'[MmLlHhVvCcSsQqTtAaZz]|[-+]?(?:\d*\.\d+|\d+\.?)(?:[eE][-+]?\d+)?')

def fetch(name):
    url = f"https://fonts.gstatic.com/s/i/short-term/release/materialsymbolsoutlined/{name}/default/24px.svg"
    with urllib.request.urlopen(url, timeout=30) as r:
        svg = r.read().decode('utf-8')
    m = re.search(r'<path\s+d="([^"]+)"', svg)
    if not m:
        raise RuntimeError("no path for " + name)
    return m.group(1)

def fmt(v):
    s = f"{v:.3f}".rstrip('0').rstrip('.')
    return "0" if s in ("-0", "") else s

def transform(x, y):
    # viewBox 0 -960 960 960  ->  0 0 24 24
    return x / 40.0, (y + 960.0) / 40.0

def convert(d):
    tokens = TOKEN_RE.findall(d)
    i = 0
    cmd = None
    cx = cy = sx = sy = 0.0
    lcx = lcy = None   # last cubic control
    lqx = lqy = None   # last quad control
    out = []

    def nums(n):
        nonlocal i
        vals = [float(tokens[i + k]) for k in range(n)]
        i += n
        return vals

    while i < len(tokens):
        t = tokens[i]
        if t.isalpha():
            cmd = t
            i += 1
            if cmd in 'Zz':
                out.append('Z')
                cx, cy = sx, sy
                lcx = lcy = lqx = lqy = None
                continue
        if cmd is None:
            raise RuntimeError("path starts with number")
        up = cmd.upper()
        rel = cmd.islower()

        if up == 'M':
            x, y = nums(2)
            if rel:
                x, y = cx + x, cy + y
            cx, cy = x, y
            sx, sy = x, y
            X, Y = transform(cx, cy)
            out.append(f"M{fmt(X)},{fmt(Y)}")
            cmd = 'l' if rel else 'L'
            lcx = lcy = lqx = lqy = None
        elif up == 'L':
            x, y = nums(2)
            if rel:
                x, y = cx + x, cy + y
            cx, cy = x, y
            X, Y = transform(cx, cy)
            out.append(f"L{fmt(X)},{fmt(Y)}")
            lcx = lcy = lqx = lqy = None
        elif up == 'H':
            (x,) = nums(1)
            if rel:
                x = cx + x
            cx = x
            X, Y = transform(cx, cy)
            out.append(f"L{fmt(X)},{fmt(Y)}")
            lcx = lcy = lqx = lqy = None
        elif up == 'V':
            (y,) = nums(1)
            if rel:
                y = cy + y
            cy = y
            X, Y = transform(cx, cy)
            out.append(f"L{fmt(X)},{fmt(Y)}")
            lcx = lcy = lqx = lqy = None
        elif up == 'C':
            x1, y1, x2, y2, x, y = nums(6)
            if rel:
                x1, y1, x2, y2, x, y = cx + x1, cy + y1, cx + x2, cy + y2, cx + x, cy + y
            X1, Y1 = transform(x1, y1)
            X2, Y2 = transform(x2, y2)
            X, Y = transform(x, y)
            out.append(f"C{fmt(X1)},{fmt(Y1)} {fmt(X2)},{fmt(Y2)} {fmt(X)},{fmt(Y)}")
            lcx, lcy = x2, y2
            lqx = lqy = None
            cx, cy = x, y
        elif up == 'S':
            x2, y2, x, y = nums(4)
            if rel:
                x2, y2, x, y = cx + x2, cy + y2, cx + x, cy + y
            X2, Y2 = transform(x2, y2)
            X, Y = transform(x, y)
            out.append(f"S{fmt(X2)},{fmt(Y2)} {fmt(X)},{fmt(Y)}")
            lcx, lcy = x2, y2
            lqx = lqy = None
            cx, cy = x, y
        elif up == 'Q':
            x1, y1, x, y = nums(4)
            if rel:
                x1, y1, x, y = cx + x1, cy + y1, cx + x, cy + y
            X1, Y1 = transform(x1, y1)
            X, Y = transform(x, y)
            out.append(f"Q{fmt(X1)},{fmt(Y1)} {fmt(X)},{fmt(Y)}")
            lqx, lqy = x1, y1
            lcx = lcy = None
            cx, cy = x, y
        elif up == 'T':
            x, y = nums(2)
            if rel:
                x, y = cx + x, cy + y
            X, Y = transform(x, y)
            out.append(f"T{fmt(X)},{fmt(Y)}")
            lqx = lqy = None
            lcx = lcy = None
            cx, cy = x, y
        elif up == 'A':
            rx, ry, rot, large, sweep, x, y = nums(7)
            if rel:
                x, y = cx + x, cy + y
            X, Y = transform(x, y)
            out.append(f"A{fmt(rx/40)},{fmt(ry/40)} {fmt(rot)} {int(large)} {int(sweep)} {fmt(X)},{fmt(Y)}")
            lcx = lcy = lqx = lqy = None
            cx, cy = x, y
        else:
            raise RuntimeError("unsupported " + cmd)

    return " ".join(out)

def bounds(d):
    # crude numeric bounds from absolute coords after conversion
    vals = [float(x) for x in re.findall(r'[-+]?(?:\d*\.\d+|\d+\.?)(?:[eE][-+]?\d+)?', d)]
    xs = vals[0::2]
    ys = vals[1::2]
    return min(xs), min(ys), max(xs), max(ys)

results = {}
for key, name in ICONS.items():
    try:
        raw = fetch(name)
        conv = convert(raw)
        results[key] = conv
        b = bounds(conv)
        print(f"{key:16s} {name:16s} bounds=({b[0]:.2f},{b[1]:.2f})-({b[2]:.2f},{b[3]:.2f}) len={len(conv)}")
    except Exception as e:
        print(f"FAIL {key} {name}: {e}")

with open(ROOT / "icon_paths.json", "w", encoding="utf-8") as f:
    json.dump(results, f, ensure_ascii=False, indent=1)
print("saved", len(results), "->", ROOT / "icon_paths.json")
