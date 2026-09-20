#!/usr/bin/env python3
"""Analyze PNG screenshots: blank-detection, color variety, motion between frames.
Pure-stdlib PNG decoder (8-bit RGB/RGBA/grayscale, filters 0-4)."""
import struct, zlib, sys

def decode_png(path):
    data = open(path, 'rb').read()
    assert data[:8] == b'\x89PNG\r\n\x1a\n', 'not png'
    pos, idat, meta = 8, b'', {}
    while pos < len(data):
        ln, typ = struct.unpack('>I4s', data[pos:pos+8]); pos += 8
        if typ == b'IHDR':
            w, h, bd, ct, comp, filt, inter = struct.unpack('>IIBBBBB', data[pos:pos+13])
            meta.update(w=w, h=h, bitdepth=bd, colortype=ct, interlace=inter)
        elif typ == b'IDAT':
            idat += data[pos:pos+ln]
        pos += ln + 4
    raw = zlib.decompress(idat)
    w, h, bd, ct = meta['w'], meta['h'], meta['bitdepth'], meta['colortype']
    assert bd == 8 and ct in (0, 2, 6) and meta['interlace'] == 0, f'unhandled png fmt {meta}'
    ch = 1 if ct == 0 else (3 if ct == 2 else 4)
    stride = w * ch
    out = bytearray()
    prev = bytearray(stride)
    p = 0
    for _ in range(h):
        f = raw[p]; p += 1
        line = bytearray(raw[p:p+stride]); p += stride
        if f == 0: pass
        elif f == 1:
            for i in range(ch, stride): line[i] = (line[i] + line[i-ch]) & 0xff
        elif f == 2:
            for i in range(stride): line[i] = (line[i] + prev[i]) & 0xff
        elif f == 3:
            for i in range(stride):
                a = line[i-ch] if i >= ch else 0
                line[i] = (line[i] + ((a + prev[i]) >> 1)) & 0xff
        elif f == 4:
            for i in range(stride):
                a = line[i-ch] if i >= ch else 0
                b = prev[i]
                c = prev[i-ch] if i >= ch else 0
                pa, pb, pc = abs(b-c), abs(a-c), abs(a+b-2*c)
                pr = a if (pa <= pb and pa <= pc) else (b if pb <= pc else c)
                line[i] = (line[i] + pr) & 0xff
        out += line
        prev = line
    return w, h, ch, out

def stats(path, sample=4):
    w, h, ch, px = decode_png(path)
    # sample pixels
    bg = None
    import collections
    counter = collections.Counter()
    total = (w//sample)*(h//sample)
    for y in range(0, h, sample):
        row = y * w * ch
        for x in range(0, w, sample):
            i = row + x*ch
            counter[tuple(px[i:i+3])] += 1
    most = counter.most_common(1)[0]
    bgpx = most[0]
    nonbg = 0
    uniq = 0
    lums = []
    for y in range(0, h, sample):
        row = y*w*ch
        for x in range(0, w, sample):
            i = row + x*ch
            rgb = tuple(px[i:i+3])
            if rgb != bgpx: nonbg += 1
            lum = 0.299*rgb[0] + 0.587*rgb[1] + 0.114*rgb[2]
            lums.append(lum)
    uniq = len(counter)
    mean_lum = sum(lums)/len(lums)
    dark = sum(1 for l in lums if l < 30)/len(lums)
    return {'bg': bgpx, 'bgCoverage%': 100*most[1]/total, 'uniqueColorsSample': uniq,
            'nonBgPixels%': 100*nonbg/total, 'meanLuma': round(mean_lum,1),
            'darkPixels%': round(100*dark,1), 'size': (w, h, ch)}

def diff(a, b, sample=8):
    w1,h1,c1,p1 = decode_png(a); w2,h2,c2,p2 = decode_png(b)
    assert w1==w2 and h1==h2
    changed = 0; total = 0
    for y in range(0, h1, sample):
        r1 = y*w1*c1; r2 = y*w2*c2
        for x in range(0, w1, sample):
            i1 = r1 + x*c1; i2 = r2 + x*c2
            if c1 == 3: d = sum(abs(p1[i1+k]-p2[i2+k]) for k in range(3))
            else: d = sum(abs(p1[i1+k]-p2[i2+k]) for k in range(3))
            total += 1
            if d > 24: changed += 1
    return round(100*changed/total, 2)

if __name__ == '__main__':
    frames = sorted(__import__('glob').glob('shot-*.png'))
    for f in frames:
        print(f, stats(f))
    if len(frames) >= 2:
        print('motion frame1->2:', diff(frames[0], frames[1]), '% changed')
        print('motion frame1->3:', diff(frames[0], frames[2]), '% changed')