#!/usr/bin/env python3
"""Generate tileable lobby PBR albedo textures for Stage1 (Nexa Core reference look)."""
from __future__ import annotations

import math
import os
import random

import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Textures")
SIZE = 1024


def save_rgba(name: str, arr: np.ndarray) -> None:
    path = os.path.join(OUT, name)
    Image.fromarray(arr.astype(np.uint8), mode="RGBA").save(path)
    print("wrote", path)


def noise2d(shape, scale=8.0, seed=0):
    rng = np.random.default_rng(seed)
    h, w = shape
    gh = max(2, int(h / scale))
    gw = max(2, int(w / scale))
    coarse = rng.random((gh, gw), dtype=np.float32)
    img = Image.fromarray((coarse * 255).astype(np.uint8), mode="L").resize((w, h), Image.Resampling.BICUBIC)
    return np.asarray(img, dtype=np.float32) / 255.0


def marble(seed=1):
    n1 = noise2d((SIZE, SIZE), 12, seed)
    n2 = noise2d((SIZE, SIZE), 28, seed + 7)
    veins = np.abs(np.sin((n1 * 6.0 + n2 * 3.0) * math.pi * 2.0))
    veins = np.power(veins, 3.2)
    base = np.full((SIZE, SIZE), 0.93, dtype=np.float32)
    tint = veins * 0.22 + n2 * 0.06
    rgb = np.stack([
        np.clip(base - tint * 0.35, 0.78, 1.0),
        np.clip(base - tint * 0.38, 0.78, 1.0),
        np.clip(base - tint * 0.42, 0.80, 1.0),
    ], axis=-1)
    return (rgb * 255).astype(np.uint8)


def wall_panels():
    img = Image.new("RGB", (SIZE, SIZE), (42, 44, 48))
    draw = ImageDraw.Draw(img)
    cols, rows = 4, 3
    gw = SIZE // cols
    gh = SIZE // rows
    rng = random.Random(42)
    for y in range(rows):
        for x in range(cols):
            ox, oy = x * gw + 3, y * gh + 3
            shade = rng.randint(-8, 8)
            c = (48 + shade, 50 + shade, 54 + shade)
            draw.rectangle([ox, oy, ox + gw - 6, oy + gh - 6], fill=c)
            draw.line([ox, oy, ox + gw - 6, oy], fill=(28, 30, 33), width=2)
            draw.line([ox, oy, ox, oy + gh - 6], fill=(28, 30, 33), width=2)
    arr = np.asarray(img.filter(ImageFilter.GaussianBlur(0.6)))
    n = noise2d((SIZE, SIZE), 64, 3)[:, :, None] * 8
    arr = np.clip(arr.astype(np.float32) + n, 0, 255).astype(np.uint8)
    return np.dstack([arr, np.full((SIZE, SIZE), 255, dtype=np.uint8)])


def brushed_metal():
    arr = np.zeros((SIZE, SIZE, 3), dtype=np.float32)
    for y in range(SIZE):
        v = 0.55 + 0.08 * math.sin(y * 0.35) + 0.03 * math.sin(y * 1.7)
        arr[y, :, 0] = v + 0.02
        arr[y, :, 1] = v + 0.01
        arr[y, :, 2] = v
    n = noise2d((SIZE, SIZE), 4, 11)[:, :, None] * 0.04
    arr = np.clip((arr + n) * 255, 0, 255).astype(np.uint8)
    return np.dstack([arr, np.full((SIZE, SIZE), 255, dtype=np.uint8)])


def ceiling():
    base = np.full((SIZE, SIZE, 3), 245, dtype=np.float32)
    n = noise2d((SIZE, SIZE), 32, 5)[:, :, None] * 6
    arr = np.clip(base - n, 230, 255).astype(np.uint8)
    return np.dstack([arr, np.full((SIZE, SIZE), 255, dtype=np.uint8)])


def column_marble():
    m = marble(9)
    return np.dstack([m, np.full((SIZE, SIZE), 255, dtype=np.uint8)])


def nexa_sign():
    img = Image.new("RGBA", (1024, 512), (18, 22, 28, 255))
    draw = ImageDraw.Draw(img)
    # geometric N logo
    cx, cy = 512, 210
    s = 120
    pts = [
        (cx - s, cy + s), (cx - s, cy - s), (cx - s * 0.15, cy - s),
        (cx + s * 0.85, cy + s * 0.15), (cx + s * 0.85, cy - s),
        (cx + s, cy - s), (cx + s, cy + s), (cx + s * 0.15, cy + s),
        (cx - s * 0.85, cy - s * 0.15), (cx - s * 0.85, cy + s),
    ]
    draw.polygon(pts, fill=(235, 242, 255, 255))
    try:
        font_l = ImageFont.truetype("/System/Library/Fonts/Supplemental/Arial Bold.ttf", 72)
        font_s = ImageFont.truetype("/System/Library/Fonts/Supplemental/Arial.ttf", 28)
    except OSError:
        font_l = ImageFont.load_default()
        font_s = font_l
    draw.text((512, 360), "NEXA CORE", fill=(235, 242, 255, 255), anchor="mm", font=font_l)
    draw.text((512, 430), "GLOBAL ADVANCED TECHNOLOGY", fill=(160, 175, 195, 255), anchor="mm", font=font_s)
    return np.asarray(img)


def aegis_screen():
    img = Image.new("RGBA", (1024, 576), (8, 14, 28, 255))
    draw = ImageDraw.Draw(img)
    for i in range(0, 1024, 48):
        draw.line([(i, 0), (i, 576)], fill=(16, 40, 72, 120), width=1)
    for j in range(0, 576, 48):
        draw.line([(0, j), (1024, j)], fill=(16, 40, 72, 120), width=1)
    try:
        font_h = ImageFont.truetype("/System/Library/Fonts/Supplemental/Arial Bold.ttf", 34)
        font_b = ImageFont.truetype("/System/Library/Fonts/Supplemental/Arial.ttf", 20)
    except OSError:
        font_h = ImageFont.load_default()
        font_b = font_h
    draw.text((512, 48), "AEGIS: NEXT-GENERATION AI CITY SAFETY SYSTEM", fill=(120, 220, 255, 255), anchor="mm", font=font_h)
    draw.text((512, 520), "DEVELOPED BY NEXA CORE", fill=(80, 160, 220, 200), anchor="mm", font=font_b)
    # glow nodes
    rng = random.Random(7)
    for _ in range(18):
        x, y = rng.randint(80, 944), rng.randint(120, 460)
        r = rng.randint(6, 16)
        draw.ellipse([x - r, y - r, x + r, y + r], fill=(40, 200, 120, 180))
        draw.line([(x, y), (x + rng.randint(-80, 80), y + rng.randint(-60, 60))], fill=(30, 140, 90, 140), width=2)
    return np.asarray(img)


def city_twilight_backdrop():
    w, h = 2048, 1024
    img = np.zeros((h, w, 3), dtype=np.float32)
    for y in range(h):
        t = y / (h - 1)
        sky = np.array([0.05, 0.08, 0.18]) * (1 - t) + np.array([0.35, 0.22, 0.45]) * t * 0.55
        if t > 0.55:
            sky = sky * (1 - (t - 0.55) * 1.2)
        img[y, :, :] = sky
    pil = Image.fromarray(np.clip(img * 255, 0, 255).astype(np.uint8), "RGB")
    draw = ImageDraw.Draw(pil)
    rng = random.Random(99)
    base_y = int(h * 0.62)
    for bx in range(0, w, rng.randint(40, 90)):
        bw = rng.randint(35, 85)
        bh = rng.randint(120, 340)
        shade = rng.randint(12, 32)
        draw.rectangle([bx, base_y - bh, bx + bw, base_y], fill=(shade, shade + 2, shade + 6))
        for _ in range(rng.randint(8, 22)):
            wx = bx + rng.randint(4, max(5, bw - 8))
            wy = base_y - rng.randint(20, bh - 10)
            wh = rng.randint(6, 18)
            ww = rng.randint(3, 8)
            glow = rng.choice([(255, 220, 160), (180, 210, 255), (120, 200, 255)])
            draw.rectangle([wx, wy, wx + ww, wy + wh], fill=glow)
    # monorail
    for x in range(w):
        y = int(base_y - 180 + math.sin(x * 0.012) * 18)
        draw.line([(x, y), (x, y + 3)], fill=(80, 200, 255), width=1)
    draw.line([(0, base_y - 178), (w, base_y - 190)], fill=(120, 230, 255), width=4)
    arr = np.asarray(pil)
    return np.dstack([arr, np.full((h, w), 255, np.uint8)])


def sky_twilight_panorama():
    w, h = 2048, 1024
    img = np.zeros((h, w, 3), dtype=np.float32)
    for y in range(h):
        t = y / (h - 1)
        top = np.array([0.03, 0.05, 0.14])
        mid = np.array([0.25, 0.18, 0.38])
        hor = np.array([0.55, 0.35, 0.28])
        if t < 0.5:
            c = top * (1 - t * 2) + mid * (t * 2)
        else:
            c = mid * (1 - (t - 0.5) * 2) + hor * ((t - 0.5) * 2)
        img[y, :, :] = c
    n = noise2d((h, w), 48, 12)[:, :, None] * 0.04
    arr = np.clip((img + n) * 255, 0, 255).astype(np.uint8)
    return arr


def plaza_stone():
    m = marble(3)
    arr = m.astype(np.float32)
    arr = np.clip(arr * 0.92 + 8, 0, 255).astype(np.uint8)
    return np.dstack([arr, np.full((SIZE, SIZE), 255, np.uint8)])


def led_strip():
    img = Image.new("RGBA", (512, 64), (0, 0, 0, 255))
    draw = ImageDraw.Draw(img)
    for y in range(64):
        t = y / 63
        c = (int(40 + 180 * (1 - t)), int(160 + 80 * (1 - t)), 255, 255)
        draw.line([(0, y), (512, y)], fill=c)
    return np.asarray(img)


def main():
    os.makedirs(OUT, exist_ok=True)
    save_rgba("T_Lobby_Marble_Floor.png", np.dstack([marble(1), np.full((SIZE, SIZE), 255, np.uint8)]))
    save_rgba("T_Lobby_Plaza_Stone.png", plaza_stone())
    save_rgba("T_Lobby_Wall_Panel.png", wall_panels())
    save_rgba("T_Lobby_Metal_Brushed.png", brushed_metal())
    save_rgba("T_Lobby_Column_Marble.png", column_marble())
    save_rgba("T_Lobby_Ceiling.png", ceiling())
    save_rgba("T_Lobby_City_Twilight.png", city_twilight_backdrop())
    Image.fromarray(sky_twilight_panorama(), mode="RGB").save(os.path.join(OUT, "T_Lobby_Sky_Twilight.png"))
    Image.fromarray(led_strip(), mode="RGBA").save(os.path.join(OUT, "T_Lobby_LED_Strip.png"))
    Image.fromarray(nexa_sign(), mode="RGBA").save(os.path.join(OUT, "T_Lobby_NexaCore_Sign.png"))
    Image.fromarray(aegis_screen(), mode="RGBA").save(os.path.join(OUT, "T_Lobby_Aegis_Screen.png"))
    print("done")


if __name__ == "__main__":
    main()
