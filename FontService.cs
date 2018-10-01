using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpFont;
using System;
using System.Collections.Generic;

namespace TextRenderingTest
{
    public class FontService : IDisposable
    {
        struct GlyphInfo
        {
            public int PenX { get; }
            public int PenY { get; }
            public int X { get; }
            public int Y { get; }

            public GlyphInfo(int penX, int penY, int x, int y)
            {
                PenX = penX;
                PenY = penY;
                X = x;
                Y = y;
            }
        }
        private Library lib;

        public Face FontFace { get => _fontFace; set => SetFont(value); }
        private Face _fontFace;

        public float Size { get => _size; set => SetSize(value); }
        public CharRecord[] crs { get; private set; }

        private float _size;

        /// <summary>
        /// If multithreading, each thread should have its own FontService.
        /// </summary>
        public FontService(Game game)
        {
            this.game = game;
            lib = new Library();
            _size = 8.25f;
        }

        public void SetFont(Face face)
        {
            _fontFace = face;
            SetSize(Size);
        }

        public void SetFont(string filename)
        {
            FontFace = new Face(lib, filename);
            SetSize(Size);
        }

        public void SetFont(Byte[] fontData)
        {
            FontFace = new Face(lib, fontData, 0);
            SetSize(Size);
        }

        public void SetSize(float size)
        {
            _size = size;
            if (FontFace != null)
                FontFace.SetCharSize(0, size, 0, 96);
        }

        /// <summary>
        /// Render the string into an 8-bit alpha texture.
        /// </summary>
        /// <param name="text">The string to render.</param>
        /// <returns></returns>
        public Texture2D RenderString(string text)
        {
            int penX = 0, penY = 0, width = 0, height = 0;
            FTBitmap[] bitmaps = new FTBitmap[text.Length];
            GlyphInfo[] glyphs = new GlyphInfo[text.Length];
            crs = new CharRecord[text.Length];
            List<byte[]> buffers = new List<byte[]>(text.Length);

            // Measure the size of the string before rendering it. We need to do this so
            // we can create the proper size of bitmap (canvas) to draw the characters on.
            for (int i = 0; i < text.Length; i++)
            {
                // Look up the glyph index for this character.
                uint glyphIndex = FontFace.GetCharIndex(text[i]);

                // Load the glyph into the font's glyph slot. There is usually only one slot in the font.
                FontFace.LoadGlyph(glyphIndex, LoadFlags.Render, LoadTarget.Normal);
                buffers.Add(new byte[FontFace.Glyph.Bitmap.Width * FontFace.Glyph.Bitmap.Rows]);

                // Whitespace characters sometimes have a bitmap of zero size, but a non-zero advance.
                // We can't draw a 0-size bitmap, but the pen position will still get advanced (below).
                if (buffers[i].Length != 0)
                    System.Runtime.InteropServices.Marshal.Copy(FontFace.Glyph.Bitmap.Buffer, buffers[i], 0, buffers[i].Length);
                bitmaps[i] = FontFace.Glyph.Bitmap;

                // Refer to the diagram entitled "Glyph Metrics" at http://www.freetype.org/freetype2/docs/tutorial/step2.html.
                // The metrics below are for the glyph loaded in the slot.
                int gAdvanceX = FontFace.Glyph.Advance.X.Ceiling();
                int ascender = FontFace.Size.Metrics.Ascender.Ceiling();

                // If this character goes higher or lower than any previous character, adjust
                // the overall height of the bitmap.
                height = Math.Max(ascender - FontFace.Size.Metrics.Descender.Ceiling(), height);

                // Accumulate the distance between the origin of each character (simple width).
                width += gAdvanceX;

                int x = Math.Max(0, penX + FontFace.Glyph.BitmapLeft);
                int y = Math.Max(0, penY + ascender - FontFace.Glyph.Metrics.HorizontalBearingY.Ceiling());

                glyphs[i] = new GlyphInfo(penX, penY, x, y);
                crs[i] = new CharRecord(text[i], penX, penY, FontFace.Glyph.Bitmap.Width, height, 0, 0, gAdvanceX);

                // Advance pen positions for drawing the next character.
                penX += gAdvanceX;
                penY += FontFace.Glyph.Advance.Y.Ceiling();
            }

            // If any dimension is 0, we can't create a bitmap
            if (width == 0 || height == 0)
                return null;
            byte[] charmap = new byte[width * height];
            var tex = new Texture2D(game.GraphicsDevice, width, height, false, SurfaceFormat.Alpha8);
            // Draw the string into the bitmap.
            for (int i = 0; i < text.Length; i++)
                if (buffers[i].Length != 0)
                    StitchGlyph(bitmaps[i], buffers[i], glyphs[i].X, glyphs[i].Y, width, height, ref charmap);

            tex.SetData(charmap);
            return tex;
        }

        public bool HasChar(char i) => FontFace.GetCharIndex(i) != 0;

        private void StitchGlyph(FTBitmap g, byte[] buffer, int px, int py, int width, int height, ref byte[] charmap)
        {
            for (int y = 0; y < g.Rows; y++)
            {
                for (int x = 0; x < g.Width; x++)
                {
                    if (px + x >= width || py + y >= height)
                        continue;

                    charmap[(py + y) * width + (px + x)] = buffer[y * g.Width + x];
                }
            }
        }

        private Game game;

        public void Dispose()
        {
            if (FontFace != null && !FontFace.IsDisposed)
                try
                {
                    FontFace.Dispose();
                    lib.Dispose();
                }
                catch { }
        }
    }
}
