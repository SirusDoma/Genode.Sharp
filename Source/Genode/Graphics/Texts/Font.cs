using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

using SharpFont;

using Genode;

namespace Genode.Graphics
{

    /// <summary>
    /// Provides functionality to load and manipulate character font.
    /// </summary>
    public class Font : IDisposable
    {
        /// <summary>
        /// Represents a <see cref="Font"/> style.
        /// </summary>
        [Flags]
        public enum Style
        {
            /// <summary>
            /// Regular characters, no style.
            /// </summary>
            Regular = 0,

            /// <summary>
            /// Bold characters.
            /// </summary>
            Bold = 1 << 0,

            /// <summary>
            /// Italic characters.
            /// </summary>
            Italic = 1 << 1,

            /// <summary>
            /// Underlined characters.
            /// </summary>
            Underlined = 1 << 2,

            /// <summary>
            /// Strike through characters.
            /// </summary>
            StrikeThrough = 1 << 3
        };

        private SharpFont.Library _library;
        private SharpFont.Face _face;
        private SharpFont.Stroker _stroker;
        private string _familyName;
        private Dictionary<int, Page> _pages;
        private byte[] _fontData;

        /// <summary>
        /// Gets the family name of current <see cref="Font"/> object.
        /// </summary>
        public string FamilyName
        {
            get { return _familyName; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Font"/> class.
        /// </summary>
        public Font()
        {
            _library     = null;
            _face        = null;
            _stroker     = null;
            _familyName  = string.Empty;
            _pages       = new Dictionary<int, Page>();
            _fontData    = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Font"/> class
        /// from a file on disk.
        /// </summary>
        /// <param name="filename">The path of the font file.</param>
        public Font(string filename)
            : this(File.ReadAllBytes(filename))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Font"/> class
        /// from an array of byte containing font data.
        /// </summary>
        /// <param name="fontData">An array of byte that contains font data.</param>
        public Font(byte[] fontData)
            : this()
        {
            _fontData = fontData;
            Initialize();
        }
        
        private void Initialize()
        {
            // Initialize FreeType
            // Note: we initialize FreeType for every font instance in order to avoid having a single
            // global manager that would create a lot of issues regarding creation and destruction order.
            _library = new SharpFont.Library();

            // Load the new font face from the specified data
            _face = new SharpFont.Face(_library, _fontData, 0);

            // Load the stroker that will be used to outline the font
            _stroker = new SharpFont.Stroker(_library);

            // Select the unicode character map
            _face.SelectCharmap(SharpFont.Encoding.Unicode);

            // Store the font family name
            _familyName = _face.FamilyName;
        }


        private Glyph LoadGlyph(int codePoint, int characterSize, bool bold, float outlineThickness)
        {
            Glyph glyph = new Glyph();

            if (_face == null)
            {
                return null;
            }

            // Set the character size
            if (!SetCurrentSize(characterSize))
            {
                return glyph;
            }

            // Load the glyph corresponding to the code point
            var flags = LoadFlags.ForceAutohint;
            if (outlineThickness != 0)
                flags |= LoadFlags.NoBitmap;
            
            _face.LoadChar((uint)codePoint, flags, SharpFont.LoadTarget.Normal);

            // Retrieve the glyph
            SharpFont.Glyph glyphDesc = _face.Glyph.GetGlyph();

            // Apply bold if necessary -- first technique using outline (highest quality)
            SharpFont.Fixed26Dot6 weight = new SharpFont.Fixed26Dot6(1);
            bool outline = glyphDesc.Format == SharpFont.GlyphFormat.Outline;
            if (outline)
            {
                if (bold)
                {
                    SharpFont.OutlineGlyph outlineGlyph = glyphDesc.ToOutlineGlyph();
                    outlineGlyph.Outline.Embolden(weight);
                }

                if (outlineThickness != 0)
                {
                    _stroker.Set((int)(outlineThickness * Fixed26Dot6.FromInt32(1).Value), 
                        StrokerLineCap.Round, StrokerLineJoin.Round, Fixed16Dot16.FromSingle(0));

                    // This function returning a new instance of Glyph
                    // Because the pointer may changed upon applying stroke to the glyph
                    glyphDesc = glyphDesc.Stroke(_stroker, false);
                }
            }

            // Convert the glyph to a bitmap (i.e. rasterize it)
            glyphDesc.ToBitmap(SharpFont.RenderMode.Normal, new FTVector26Dot6(0, 0), true);
            SharpFont.FTBitmap bitmap = glyphDesc.ToBitmapGlyph().Bitmap;
            
            // Apply bold if necessary -- fallback technique using bitmap (lower quality)
            if (!outline)
            {
                if (bold)
                    bitmap.Embolden(_library, weight, weight);

                if (outlineThickness != 0)
                    Logger.Warning("Failed to outline glyph (no fallback available)");
            }

            // Compute the glyph's advance offset
            glyph.Advance = _face.Glyph.Metrics.HorizontalAdvance.ToSingle();
            if (bold)
                glyph.Advance += weight.ToSingle();

            int width = bitmap.Width;
            int height = bitmap.Rows;

            if ((width > 0) && (height > 0))
            {
                // Leave a small padding around characters, so that filtering doesn't
                // pollute them with pixels from neighbors
                int padding = 1;

                // Get the glyphs page corresponding to the character size
                Page page = _pages[characterSize];

                // Find a good position for the new glyph into the texture
                glyph.TexCoords = FindGlyphRectangle(page, width + 2 * padding, height + 2 * padding);
                var texRect = glyph.TexCoords;

                // Make sure the texture data is positioned in the center
                // of the allocated texture rectangle
                glyph.TexCoords = new Rectangle(texRect.X + padding, texRect.Y + padding,
                    texRect.Width - 2 * padding, texRect.Height - 2 * padding);

                // Compute the glyph's bounding box
                float boundsX      =  (float)(_face.Glyph.Metrics.HorizontalBearingX);
                float boundsY      = -(float)(_face.Glyph.Metrics.HorizontalBearingY);
                float boundsWidth  =  (float)(_face.Glyph.Metrics.Width) + outlineThickness * 2;
                float boundsHeight =  (float)(_face.Glyph.Metrics.Height) + outlineThickness * 2;
                glyph.Bounds = new RectangleF(boundsX, boundsY, boundsWidth, boundsHeight);

                // Extract the glyph's pixels from the bitmap
                byte[] pixelBuffer = new byte[width * height * 4];
                for (int i = 0; i < pixelBuffer.Length; i++)
                {
                    pixelBuffer[i] = 255;
                }

                unsafe
                {
                    byte* pixels = (byte*)bitmap.Buffer.ToPointer();
                    if (bitmap.PixelMode == SharpFont.PixelMode.Mono)
                    {
                        // Pixels are 1 bit monochrome values
                        for (int y = 0; y < height; ++y)
                        {
                            for (int x = 0; x < width; ++x)
                            {
                                // The color channels remain white, just fill the alpha channel
                                int index = (x + y * width) * 4 + 3;
                                pixelBuffer[index] = (byte)((((pixels[x / 8]) & (1 << (7 - (x % 8)))) > 0) ? 255 : 0);
                            }
                            pixels += bitmap.Pitch;
                        }
                    }
                    else
                    {
                        // Pixels are 8 bits gray levels
                        for (int y = 0; y < height; ++y)
                        {
                            for (int x = 0; x < width; ++x)
                            {
                                // The color channels remain white, just fill the alpha channel
                                int index = (x + y * width) * 4 + 3;
                                pixelBuffer[index] = pixels[x];
                            }
                            pixels += bitmap.Pitch;
                        }
                    }
                }

                // Write the pixels to the texture
                int tx = glyph.TexCoords.Left;
                int ty = glyph.TexCoords.Top;
                int tw = glyph.TexCoords.Width;
                int th = glyph.TexCoords.Height;
                page.texture.Update(pixelBuffer, tx, ty, tw, th);
            }

            // Delete the FT glyph
            glyphDesc.Dispose();

            return glyph;
        }

        private Rectangle FindGlyphRectangle(Page page, int width, int height)
        {
            // Find the line that fits well the glyph
            Row row = null;
            float bestRatio = 0;

            foreach (Row it in page.rows)
            {
                float ratio = (float)(height) / it.height;

                // Ignore rows that are either too small or too high
                if ((ratio < 0.7f) || (ratio > 1f))
                {
                    continue;
                }

                // Check if there's enough horizontal space left in the row
                if (width > page.texture.Width - it.width)
                {
                    continue;
                }

                // Make sure that this new row is the best found so far
                if (ratio < bestRatio)
                {
                    continue;
                }

                // The current row passed all the tests: we can select it
                row = it;
                bestRatio = ratio;
            }

            // If we didn't find a matching row, create a new one (10% taller than the glyph)
            if (row == null)
            {
                int rowHeight = height + height / 10;
                while ((page.nextRow + rowHeight >= page.texture.Height) || (width >= page.texture.Width))
                {
                    // Not enough space: resize the texture if possible
                    int textureWidth = page.texture.Width;
                    int textureHeight = page.texture.Height;
                    if ((textureWidth * 2 <= Texture.GetMaximumSize()) && (textureHeight * 2 <= Texture.GetMaximumSize()))
                    {
                        // Make the texture 2 times bigger
                        Bitmap newImage = new Bitmap(textureWidth * 2, textureHeight * 2);
                        Bitmap source = new Bitmap(page.texture.ToImage());

                        for (int x = 0; x < source.Width; x++)
                        {
                            for (int y = 0; y < source.Height; y++)
                            {
                                newImage.SetPixel(x, y, source.GetPixel(x, y));
                            }
                        }

                        page.texture = new Graphics.Texture(newImage);
                    }
                    else
                    {
                        // Oops, we've reached the maximum texture size...
                        throw new OutOfMemoryException("Failed to add a new character to the font: the maximum texture size has been reached");
                    }
                }

                // We can now create the new row
                page.rows.Add(new Row(page.nextRow, rowHeight));
                page.nextRow += rowHeight;
                row = page.rows[page.rows.Count - 1];
            }

            // Find the glyph's rectangle on the selected row
            Rectangle rect = new Rectangle(row.width, row.top, width, height);

            // Update the row informations
            row.width += width;

            return rect;
        }

        private bool SetCurrentSize(int characterSize)
        {
            // FT_Set_Pixel_Sizes is an expensive function, so we must call it
            // only when necessary to avoid killing performances
            ushort currentSize = _face.Size.Metrics.NominalWidth;
            if (currentSize != characterSize)
            {
                try
                {
                    _face.SetPixelSizes(0, (uint)characterSize);
                }
                catch (SharpFont.FreeTypeException ex)
                {
                    SharpFont.Error result = ex.Error;
                    if (result == SharpFont.Error.InvalidPixelSize)
                    {
                        // In the case of bitmap fonts, resizing can
                        // fail if the requested size is not available
                        if (!_face.IsScalable)
                        {
                            string exceptionMessage;
                            exceptionMessage = "Failed to set bitmap font size to " + characterSize + Environment.NewLine;
                            exceptionMessage += "Available sizes are: ";
                            for (int i = 0; i < _face.FixedSizesCount; ++i)
                                exceptionMessage += _face.AvailableSizes[i].Height.ToString() + " ";
                            exceptionMessage += Environment.NewLine;

                            Logger.Warning(exceptionMessage);
                        }
                    }

                    return result == SharpFont.Error.Ok;
                }

                return true;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Retrieve a glyph of the font.
        /// </summary>
        /// <param name="codePoint">Unicode code point of the character to get.</param>
        /// <param name="characterSize">Reference character size.</param>
        /// <param name="bold">Specify whether the glyph should be bold version.</param>
        /// <param name="outlineThickness">Thickness of outline (when != 0 the glyph will not be filled).</param>
        /// <returns>The <see cref="Glyph"/> corresponding to a codePoint and a characterSize.</returns>
        public Glyph GetGlyph(int codePoint, int characterSize, bool bold, float outlineThickness = 0f)
        {
            if (!_pages.ContainsKey(characterSize))
            {
                _pages.Add(characterSize, new Page());
            }

            var glyphs = _pages[characterSize].glyphs;
            long key =   ((long)((int)(outlineThickness)) << 32)
                       | ((long)(bold ? 1 : 0) << 31)
                       |  (long)(codePoint);

            if (glyphs.ContainsKey(key))
            {
                return glyphs[key];
            }
            else
            {
                Glyph newGlyph = LoadGlyph(codePoint, characterSize, bold, outlineThickness);
                _pages[characterSize].glyphs.Add(key, newGlyph);

                return newGlyph;
            }
        }

        /// <summary>
        /// Get the kerning offset between two <see cref="Glyph"/>.
        /// </summary>
        /// <param name="first">The unicode code point of the first character.</param>
        /// <param name="second">The unicode code point of the second character.</param>
        /// <param name="characterSize">Reference character size.</param>
        /// <returns>Kerning value for \a first and \a second, in pixels.</returns>
        public float GetKerning(int first, int second, int characterSize)
        {
            // Special case where first or second is 0 (null character)
            if (first == 0 || second == 0)
            {
                return 0f;
            }

            if (_face != null && _face.HasKerning && SetCurrentSize(characterSize))
            {
                // Convert the characters to indices
                uint index1 = _face.GetCharIndex((uint)first);
                uint index2 = _face.GetCharIndex((uint)second);

                // Get the kerning vector
                var kerning = _face.GetKerning(index1, index2, SharpFont.KerningMode.Default);

                // X advance is already in pixels for bitmap fonts
                if (!_face.IsScalable)
                {
                    return kerning.X.ToInt32();
                }

                // Return the X advance
                return kerning.X.ToSingle();
            }
            else
            {
                // Invalid font, or no kerning
                return 0f;
            }
        }

        /// <summary>
        /// Get the line spacing.
        /// </summary>
        /// <param name="characterSize">Reference character size.</param>
        /// <returns>Line spacing, in pixels.</returns>
        public float GetLineSpacing(int characterSize)
        {
            if (_face != null && SetCurrentSize(characterSize))
            {
                return _face.Size.Metrics.Height.ToSingle();
            }
            else
            {
                return 0f;
            }
        }

        /// <summary>
        /// Get the position of the underline.
        /// </summary>
        /// <param name="characterSize">Reference character size.</param>
        /// <returns>Underline position, in pixels.</returns>
        public float GetUnderlinePosition(int characterSize)
        {
            if (_face != null && SetCurrentSize(characterSize))
            {
                // Return a fixed position if font is a bitmap font
                if (!_face.IsScalable)
                {
                    return characterSize / 10f;
                }

                return -(Fixed16Dot16.Multiply(_face.UnderlinePosition, _face.Size.Metrics.ScaleY).ToSingle()); // / (float)(1 << 6);
            }
            else
            {
                return 0f;
            }
        }

        /// <summary>
        /// Get the thickness of the underline.
        /// </summary>
        /// <param name="characterSize">Reference character size.</param>
        /// <returns>Underline thickness, in pixels.</returns>
        public float GetUnderlineThickness(int characterSize)
        {
            if (_face != null && SetCurrentSize(characterSize))
            {
                // Return a fixed thickness if font is a bitmap font
                if (!_face.IsScalable)
                {
                    return characterSize / 14f;
                }

                return (Fixed16Dot16.Multiply(_face.UnderlineThickness, _face.Size.Metrics.ScaleY).ToSingle());
            }
            else
            {
                return 0f;
            }
        }

        /// <summary>
        /// Get the <see cref="Texture"/> containing the loaded glyphs of a certain size.
        /// </summary>
        /// <param name="characterSize">Reference character size.</param>
        /// <returns><see cref="Texture"/> containing the glyphs of the requested size.</returns>
        public Texture GetTexture(int characterSize)
        {
            return _pages[characterSize].texture;
        }

        /// <summary>
        /// Releases all resources used by the <see cref="Text"/>.
        /// </summary>
        public void Dispose()
        {
            if (_stroker != null && !_stroker.IsDisposed)
                _stroker.Dispose();

            if (_face != null && !_face.IsDisposed)
                _face.Dispose();

            if (_library != null && !_library.IsDisposed)
                _library.Dispose();

            _pages       = null;
            _fontData    = null;
        }

        internal byte[] GetFontData()
        {
            return _fontData;
        }
    }
}
