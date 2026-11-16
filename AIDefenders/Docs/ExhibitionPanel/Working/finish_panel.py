from pathlib import Path
import json
import struct
import zipfile
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parent.parent
content = json.loads((root / 'Working/content.json').read_text(encoding='utf-8'))
submission = root / 'Submission'
ppt = submission / (content['fileName'] + '.pptx')
ns = {'p': 'http://schemas.openxmlformats.org/presentationml/2006/main',
      'a': 'http://schemas.openxmlformats.org/drawingml/2006/main'}
with zipfile.ZipFile(ppt) as deck:
    presentation = ET.fromstring(deck.read('ppt/presentation.xml'))
    assert len(presentation.find('p:sldIdLst', ns)) == 1
    size = presentation.find('p:sldSz', ns)
    assert abs(int(size.attrib['cx']) / 36000 - 594) < .1
    assert abs(int(size.attrib['cy']) / 36000 - 719) < .1
    slide = ET.fromstring(deck.read('ppt/slides/slide1.xml'))
    assert len(slide.findall('.//p:pic', ns)) == 2
    for transform in slide.findall('.//a:xfrm', ns):
        offset, extent = transform.find('a:off', ns), transform.find('a:ext', ns)
        if offset is None or extent is None:
            continue
        x, y = int(offset.attrib['x']) / 36000, int(offset.attrib['y']) / 36000
        w, h = int(extent.attrib['cx']) / 36000, int(extent.attrib['cy']) / 36000
        if w == 0 and h == 0:
            continue
        assert x >= 39.9 and y >= 29.9 and x + w <= 554.1 and y + h <= 689.1, (x, y, w, h)
    colors = [element.attrib['val'] for element in slide.findall('.//a:rPr/a:solidFill/a:srgbClr', ns)]
    assert all(color == '000000' for color in colors)
    font_files = [name for name in deck.namelist() if name.startswith('ppt/fonts/')]

checks = json.loads((root / 'Preview/layout_check.json').read_text(encoding='utf-8-sig'))
assert not any(row['Overflow'] for row in checks)
images = []
for image in sorted((submission / 'Originals').glob('*.png')):
    width, height = struct.unpack('>II', image.read_bytes()[16:24])
    assert width >= 3000 and height >= 1800
    images.append({'file': image.name, 'pixels': [width, height], 'dpi_at_514mm': round(width / (514 / 25.4))})
assert len(images) == 2
archive = root / (content['fileName'] + '.zip')
with zipfile.ZipFile(archive, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as bundle:
    bundle.write(ppt, ppt.name)
    for image in sorted((submission / 'Originals').glob('*.png')):
        bundle.write(image, 'Originals/' + image.name)
with zipfile.ZipFile(archive) as bundle:
    assert bundle.testzip() is None
report = {'slide_count': 1, 'size_mm': [594, 719], 'black_text_only': True,
          'margins_mm': {'horizontal': 40, 'vertical': 30}, 'text_overflow': False,
          'images': images, 'embedded_font_parts': len(font_files), 'zip': archive.name}
(root / 'Preview/validation.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(report, ensure_ascii=False, indent=2))
