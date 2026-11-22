from PIL import Image, ImageDraw, ImageFont, ImageFilter
import os

SIZE = 512
OUT = r"c:\Users\user\Downloads\AIDefendersV5-main\AIDefenders\Assets\CreatedAssets\SkillIcons"
os.makedirs(OUT, exist_ok=True)


def font(size, bold=True):
    candidates = [
        r"C:\Windows\Fonts\malgunbd.ttf" if bold else r"C:\Windows\Fonts\malgun.ttf",
        r"C:\Windows\Fonts\arialbd.ttf",
        r"C:\Windows\Fonts\segoeuib.ttf",
    ]
    for path in candidates:
        if os.path.exists(path):
            return ImageFont.truetype(path, size)
    return ImageFont.load_default()


def rounded_square(draw, box, radius, fill, outline=None, width=1):
    draw.rounded_rectangle(box, radius=radius, fill=fill, outline=outline, width=width)


def add_glow(base, mask, color, blur=18, alpha=180):
    glow = Image.new("RGBA", base.size, (0, 0, 0, 0))
    glow_mask = mask.filter(ImageFilter.GaussianBlur(blur))
    colored = Image.new("RGBA", base.size, color + (alpha,))
    glow.alpha_composite(Image.composite(colored, Image.new("RGBA", base.size), glow_mask))
    base.alpha_composite(glow)


def overdrive_icon():
    base = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    d = ImageDraw.Draw(base)
    rounded_square(d, (24, 24, 488, 488), 92, (12, 18, 38, 255), (255, 165, 45, 255), 10)
    rounded_square(d, (42, 42, 470, 470), 78, (20, 35, 68, 255))

    # Dynamic orange speed arcs
    for r, alpha in [(205, 80), (186, 115), (167, 150), (148, 180)]:
        d.arc((256-r//2, 256-r//2, 256+r//2, 256+r//2), 205, 335,
              fill=(255, 105, 20, alpha), width=14)
    d.arc((62, 62, 450, 450), 205, 335, fill=(255, 190, 74, 255), width=7)

    # Main lightning bolt
    bolt_mask = Image.new("L", (SIZE, SIZE), 0)
    md = ImageDraw.Draw(bolt_mask)
    bolt = [(266, 82), (168, 248), (238, 248), (210, 430), (352, 226), (278, 226), (318, 82)]
    md.polygon(bolt, fill=255)
    glow = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    blurred = bolt_mask.filter(ImageFilter.GaussianBlur(16))
    glow.paste((255, 86, 18, 190), (0, 0), blurred)
    base.alpha_composite(glow)

    d = ImageDraw.Draw(base)
    d.polygon(bolt, fill=(255, 210, 67, 255), outline=(255, 248, 180, 255))

    # Highlight edge
    d.line([(266, 82), (168, 248), (238, 248)], fill=(255, 255, 220, 255), width=7, joint="curve")

    # 10s badge
    rounded_square(d, (338, 352, 466, 442), 42, (255, 86, 26, 255), (255, 229, 133, 255), 7)
    text = "10초"
    f = font(58)
    bb = d.textbbox((0, 0), text, font=f)
    d.text(((402)- (bb[2]-bb[0])/2, 397-(bb[3]-bb[1])/2-bb[1]), text, font=f, fill=(255,255,255,255), stroke_width=2, stroke_fill=(120,35,10,255))

    base.save(os.path.join(OUT, "Overdrive_10s.png"))


def focus_icon():
    base = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    d = ImageDraw.Draw(base)
    rounded_square(d, (24, 24, 488, 488), 92, (10, 20, 40, 255), (86, 205, 255, 255), 10)
    rounded_square(d, (42, 42, 470, 470), 78, (18, 38, 73, 255))

    # Cyan targeting rings
    d.ellipse((92, 92, 420, 420), outline=(78, 200, 255, 255), width=12)
    d.ellipse((137, 137, 375, 375), outline=(194, 238, 255, 255), width=7)
    d.ellipse((213, 213, 299, 299), fill=(50, 179, 255, 90), outline=(255,255,255,255), width=6)

    # Crosshair with gap
    d.line((256, 58, 256, 188), fill=(151, 225, 255, 255), width=10)
    d.line((256, 324, 256, 454), fill=(151, 225, 255, 255), width=10)
    d.line((58, 256, 188, 256), fill=(151, 225, 255, 255), width=10)
    d.line((324, 256, 454, 256), fill=(151, 225, 255, 255), width=10)

    # Four corner brackets
    d.arc((66,66,446,446), 205, 295, fill=(255,255,255,255), width=8)
    d.arc((66,66,446,446), 295, 25, fill=(255,255,255,255), width=8)

    # Central turret body
    d.rounded_rectangle((201, 210, 311, 309), radius=24, fill=(24, 52, 92, 255), outline=(222,246,255,255), width=7)
    d.ellipse((218,227,294,303), fill=(67,166,218,255), outline=(255,255,255,255), width=5)
    d.polygon([(256,232),(231,277),(281,277)], fill=(13,32,66,255))

    # Barrel aimed upward
    d.rounded_rectangle((243, 140, 269, 242), radius=12, fill=(230,246,255,255), outline=(80,170,220,255), width=5)
    d.polygon([(248,140),(264,140),(271,115),(241,115)], fill=(255,255,255,255), outline=(100,200,245,255))

    # Side arrows to convey focused fire
    d.polygon([(82,244),(116,215),(116,236),(155,236),(155,252),(116,252),(116,273)], fill=(255,188,55,255))
    d.polygon([(430,244),(396,215),(396,236),(357,236),(357,252),(396,252),(396,273)], fill=(255,188,55,255))

    base.save(os.path.join(OUT, "Turret_Focus_Fire.png"))

overdrive_icon()
focus_icon()
print(OUT)
