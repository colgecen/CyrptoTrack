#!/usr/bin/env python3
"""Regenerate every README screenshot from the real Avalonia views.

The C# helper in Tools/Screenshots renders the actual MainWindow headlessly
with English demo data and writes three 1440x900 PNG frames. This script wraps
each frame in a macOS-style window (rounded corners, transparent outside the
frame, title bar and traffic lights) and stores the result as an SVG in
docs/img/. Run it from the repository root:

    python3 scripts/make_screenshots.py
"""

from __future__ import annotations

import base64
import os
import struct
import subprocess
import sys
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
OUT_DIR = ROOT / "docs" / "img"
RUNNER = ROOT / "Tools" / "Screenshots" / "Screenshots.csproj"

CONTENT_WIDTH = 1440
CONTENT_HEIGHT = 900
TITLE_BAR = 44
RADIUS = 10

SHOTS = [
    ("dashboard", "CryptoTrack \u2014 Dashboard"),
    ("portfolio", "CryptoTrack \u2014 Portfolio"),
    ("news", "CryptoTrack \u2014 News"),
]


def png_size(data: bytes) -> tuple[int, int]:
    if data[:8] != b"\x89PNG\r\n\x1a\n" or data[12:16] != b"IHDR":
        raise ValueError("not a PNG file")
    return struct.unpack(">II", data[16:24])


def svg_frame(png: bytes, title: str) -> str:
    width, height = png_size(png)
    if (width, height) != (CONTENT_WIDTH, CONTENT_HEIGHT):
        raise ValueError(f"expected {CONTENT_WIDTH}x{CONTENT_HEIGHT}, got {width}x{height}")

    total_height = TITLE_BAR + CONTENT_HEIGHT
    encoded = base64.b64encode(png).decode("ascii")

    # Keep the real capture at its native size: only the window frame is added
    # around it. Nothing is stretched, cropped or resized.
    return f"""<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink"
     width="{CONTENT_WIDTH}" height="{total_height}"
     viewBox="0 0 {CONTENT_WIDTH} {total_height}">
  <defs>
    <clipPath id="window">
      <rect x="0" y="0" width="{CONTENT_WIDTH}" height="{total_height}" rx="{RADIUS}" ry="{RADIUS}"/>
    </clipPath>
    <clipPath id="content">
      <rect x="0" y="{TITLE_BAR}" width="{CONTENT_WIDTH}" height="{CONTENT_HEIGHT}"/>
    </clipPath>
  </defs>
  <g clip-path="url(#window)">
    <rect x="0" y="0" width="{CONTENT_WIDTH}" height="{total_height}" fill="#1a1e26"/>
    <rect x="0" y="0" width="{CONTENT_WIDTH}" height="{TITLE_BAR}" fill="#1a1e26"/>
    <line x1="0" y1="{TITLE_BAR - 0.5}" x2="{CONTENT_WIDTH}" y2="{TITLE_BAR - 0.5}"
          stroke="#2a303c" stroke-width="1"/>
    <circle cx="22" cy="22" r="6.5" fill="#ff5f57"/>
    <circle cx="44" cy="22" r="6.5" fill="#febc2e"/>
    <circle cx="66" cy="22" r="6.5" fill="#28c840"/>
    <text x="{CONTENT_WIDTH / 2}" y="27" text-anchor="middle"
          font-family="-apple-system, BlinkMacSystemFont, 'Segoe UI', Inter, sans-serif"
          font-size="13.5" font-weight="600" fill="#c9d1d9">{title}</text>
    <image x="0" y="{TITLE_BAR}" width="{CONTENT_WIDTH}" height="{CONTENT_HEIGHT}"
           clip-path="url(#content)"
           xlink:href="data:image/png;base64,{encoded}"/>
  </g>
  <rect x="0.5" y="0.5" width="{CONTENT_WIDTH - 1}" height="{total_height - 1}"
        rx="{RADIUS - 0.5}" ry="{RADIUS - 0.5}" fill="none"
        stroke="#2a303c" stroke-opacity="0.85" stroke-width="1"/>
</svg>
"""


def main() -> int:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix="cryptotrack-shots-") as tmp:
        env = {
            **os.environ,
            "DOTNET_ROLL_FORWARD": "Major",
            "DOTNET_CLI_TELEMETRY_OPTOUT": "1",
        }
        subprocess.run(
            ["dotnet", "run", "-c", "Release", "--project", str(RUNNER), "--", tmp],
            cwd=ROOT,
            env=env,
            check=True,
        )

        for name, title in SHOTS:
            png = Path(tmp, f"{name}.png").read_bytes()
            svg = svg_frame(png, title)
            target = OUT_DIR / f"{name}.svg"
            target.write_text(svg, encoding="utf-8")
            print(f"wrote {target.relative_to(ROOT)}")

    return 0


if __name__ == "__main__":
    sys.exit(main())
