using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Genode;
using Genode.Entities;

namespace Genode.Graphics
{
    public class Text : Transformable, IRenderable, IColorable
    {
        private string      _value;
        private Font        _font;
        private int         _characterSize;
        private Font.Style  _style;
        private Color       _color;
        private Color       _outlineColor;
        private float       _outlineThickness;
        private VertexArray _vertices;
        private VertexArray _outlineVertices;
        private RectangleF  _bounds;
        private bool        _isGeometryUpdated;

        /// <summary>
        /// Gets a value indicating whether the current of <see cref="Text"/> object should be visible.
        /// </summary>
        public bool Visible
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the text's string of current <see cref="Text"/> object.
        /// </summary>
        public string Value
        {
            get { return _value; }
            set
            {
                if (!_value.Equals(value))
                {
                    _value = value;
                    _isGeometryUpdated = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the text's <see cref="Graphics.Font"/> of current <see cref="Text"/> object.
        /// </summary>
        public Font Font
        {
            get { return _font; }
            set
            {
                if (_font != value)
                {
                    _font = value;
                    _isGeometryUpdated = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the character size of current <see cref="Text"/> object.
        /// </summary>
        public int CharacterSize
        {
            get { return _characterSize; }
            set
            {
                if (_characterSize != value)
                {
                    _characterSize = value;
                    _isGeometryUpdated = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the text's style of current <see cref="Text"/> object.
        /// </summary>
        public Font.Style Style
        {
            get { return _style; }
            set
            {
                if (_style != value)
                {
                    _style = value;
                    _isGeometryUpdated = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Drawing.Color"/> of current <see cref="Text"/> object.
        /// </summary>
        public Color Color
        {
            get { return _color; }
            set
            {
                if (_color != value)
                {
                    _color = value;

                    // Change vertex colors directly, no need to update whole geometry
                    // (if geometry is updated anyway, we can skip this step)
                    if (_isGeometryUpdated)
                    {
                        for (int i = 0; i < _vertices.Count; ++i)
                            _vertices[i] = new Vertex(_vertices[i].Position, _vertices[i].TexCoords, _color);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the outline <see cref="System.Drawing.Color"/> of current <see cref="Text"/> object.
        /// </summary>
        public Color OutlineColor
        {
            get { return _outlineColor; }
            set
            {
                if (_outlineColor != value)
                {
                    _outlineColor = value;

                    // Change vertex colors directly, no need to update whole geometry
                    // (if geometry is updated anyway, we can skip this step)
                    if (_isGeometryUpdated)
                    {
                        for (int i = 0; i < _outlineVertices.Count; ++i)
                            _outlineVertices[i] = new Vertex(_outlineVertices[i].Position, _outlineVertices[i].TexCoords, _color);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the outline thickness of current <see cref="Text"/> object.
        /// </summary>
        public float OutlineThickness
        {
            get { return _outlineThickness; }
            set
            {
                if (_outlineThickness != value)
                {
                    _outlineThickness = value;
                    _isGeometryUpdated = false;
                }
            }
        }

        /// <summary>
        /// Gets the local bounding rectangle of current <see cref="Text"/> object.
        /// </summary>
        public RectangleF LocalBounds
        {
            get
            {
                UpdateGeometry();
                return _bounds;
            }
        }

        /// <summary>
        /// Gets the global bounding rectangle of current <see cref="Text"/> object.
        /// </summary>
        public RectangleF GlobalBounds
        {
            get
            {
                return Transform.TransformRectangle(LocalBounds);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Text"/> class.
        /// </summary>
        public Text()
        {
            _value             = null;
            _font              = null;
            _characterSize     = 30;
            _style             = Font.Style.Regular;
            _color             = Color.White;
            _outlineColor      = Color.FromArgb(0, 0, 0, 0);
            _outlineThickness  = 0f;
            _vertices          = new VertexArray(PrimitiveType.Triangles);
            _outlineVertices   = new VertexArray(PrimitiveType.Triangles);
            _bounds            = RectangleF.Empty;
            _isGeometryUpdated = false;

            Visible            = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Text"/>
        /// with specified string, font and size of characters.
        /// </summary>
        /// <param name="value">The text assigned to the string.</param>
        /// <param name="font">The font used to draw the string.</param>
        /// <param name="characterSize">Base size of characters, in pixels.</param>
        public Text(string value, Font font, int characterSize = 30)
            : this()
        {
            _value             = value;
            _font              = font;
            _characterSize     = characterSize;
            _isGeometryUpdated = false;
        }

        /// <summary>
        /// Find the position of the a index-th character.
        /// </summary>
        /// <param name="index">The index of the character.</param>
        /// <returns>The position of character.</returns>
        public Vector2 FindCharacterPosition(int index)
        {
            // Make sure that we have a valid font 
            if (_font == null)
                return Vector2.Zero;

            // Clamp the index if it is beyond the number of characters or less than zero
            if (index >= _value.Length)
                index = _value.Length;
            else if (index < 0)
                index = 0;

            bool bold    = (_style & Font.Style.Bold) != 0;
            float hspace = _font.GetGlyph(' ', _characterSize, bold).Advance;
            float vspace = _font.GetLineSpacing(_characterSize);

            var position = Vector2.Zero;
            int prevChar = 0;

            for (int i = 0; i < index; i++)
            {
                int curChar = _value[i];

                // Apply the kerning offset
                position.X += (_font.GetKerning(prevChar, curChar, _characterSize));
                prevChar = curChar;

                // Handle special characters
                switch (curChar)
                {
                    case ' ':  position.X += hspace;                 continue;
                    case '\t': position.X += hspace * 4;             continue;
                    case '\n': position.Y += vspace; position.X = 0; continue;
                }

                // For regular characters, add the advance offset of the glyph
                position.X += (_font.GetGlyph(curChar, _characterSize, bold).Advance);
            }

            // Transform the position to global coordinates
            position = Transform.TransformPoint(position);
            return position;
        }

        /// <summary>
        /// Render the current <see cref="Text"/> object to a <see cref="RenderTarget"/>.
        /// </summary>
        /// <param name="target">The <see cref="RenderTarget"/> to render to.</param>
        /// <param name="states">The current render states.</param>
        public void Render(RenderTarget target, RenderStates states)
        {
            if (!string.IsNullOrEmpty(_value) && _font != null && Visible)
            {
                UpdateGeometry();

                states.Transform *= Transform;
                states.Texture = _font.GetTexture(_characterSize);

                // Only draw the outline if there is something to draw
                if (_outlineThickness != 0f)
                {
                    target.Render(_outlineVertices, states);
                }

                target.Render(_vertices, states);
            }
        }

        private void UpdateGeometry()
        {
            // Do nothing, if geometry has not changed
            if (_isGeometryUpdated)
                return;

            // Mark geometry as updated
            _isGeometryUpdated = true;

            // Clear the previous geometry
            _vertices.Clear();
            _outlineVertices.Clear();
            _bounds = RectangleF.Empty;

            // No font or text: nothing to draw
            if (_font == null || string.IsNullOrEmpty(_value))
                return;

            // Compute values related to the text style
            bool  bold               = (_style & Font.Style.Bold) != 0;
            bool  underlined         = (_style & Font.Style.Underlined) != 0;
            bool  strikeThrough      = (_style & Font.Style.StrikeThrough) != 0;
            float italic             = (_style & Font.Style.Italic) != 0 ? 0.208f : 0f; // 12 degrees
            float underlineOffset    = _font.GetUnderlinePosition(_characterSize);
            float underlineThickness = _font.GetUnderlineThickness(_characterSize);

            // Compute the location of the strike through dynamically
            // We use the center point of the lowercase 'x' glyph as the reference
            // We reuse the underline thickness as the thickness of the strike through as well
            var xBounds = _font.GetGlyph('x', _characterSize, bold).Bounds;
            float strikeThroughOffset = xBounds.Top + xBounds.Height / 2f;

            // Precompute the variables needed by the algorithm
            float hspace = _font.GetGlyph(' ', _characterSize, bold).Advance;
            float vspace = _font.GetLineSpacing(_characterSize);
            float x      = 0f;
            float y      = (float)_characterSize;

            // Create one quad for each character
            float minX = (float)_characterSize;
            float minY = (float)_characterSize;
            float maxX = 0f;
            float maxY = 0f;
            int prevChar = 0;

            for (int i = 0; i < _value.Length; i++)
            {
                int curChar = _value[i];

                // Apply the kerning offset
                x += _font.GetKerning(prevChar, curChar, _characterSize);
                prevChar = curChar;

                // If we're using the underlined style and there's a new line, draw a line
                if (underlined && (curChar == '\n'))
                {
                    AddLine(_vertices, x, y, _color, underlineOffset, underlineThickness);

                    if (_outlineThickness != 0)
                        AddLine(_outlineVertices, x, y, _outlineColor, underlineOffset, underlineThickness, _outlineThickness);
                }

                // If we're using the strike through style and there's a new line, draw a line across all characters
                if (strikeThrough && (curChar == '\n'))
                {
                    AddLine(_vertices, x, y, _color, strikeThroughOffset, underlineThickness);

                    if (_outlineThickness != 0)
                        AddLine(_outlineVertices, x, y, _outlineColor, strikeThroughOffset, underlineThickness, _outlineThickness);
                }

                // Handle special characters
                if ((curChar == ' ') || (curChar == '\t') || (curChar == '\n'))
                {
                    // Update the current bounds (min coordinates)
                    minX = (float)Math.Min(minX, x);
                    minY = (float)Math.Min(minY, y);

                    switch (curChar)
                    {
                        case ' ': x  += hspace;        break;
                        case '\t': x += hspace * 4;    break;
                        case '\n': y += vspace; x = 0; break;
                    }

                    // Update the current bounds (max coordinates)
                    maxX = (float)Math.Max(maxX, x);
                    maxY = (float)Math.Max(maxY, y);

                    // Next glyph, no need to create a quad for whitespace
                    continue;
                }

                var ch = (char)curChar;

                // Apply the outline
                if (_outlineThickness != 0)
                {
                    var glyphOutline = _font.GetGlyph(curChar, _characterSize, bold, _outlineThickness);

                    float left   = glyphOutline.Bounds.Left;
                    float top    = glyphOutline.Bounds.Top;
                    float right  = glyphOutline.Bounds.Left + glyphOutline.Bounds.Width;
                    float bottom = glyphOutline.Bounds.Top  + glyphOutline.Bounds.Height;

                    // Add the outline glyph to the vertices
                    AddGlyphQuad(_outlineVertices, new Vector2(x, y), _outlineColor, glyphOutline, italic, _outlineThickness);

                    // Update the current bounds with the outlined glyph bounds
                    minX = (float)Math.Min(minX, x + left   - italic * bottom - _outlineThickness);
                    maxX = (float)Math.Max(maxX, x + right  - italic * top    - _outlineThickness);
                    minY = (float)Math.Min(minY, y + top    - _outlineThickness);
                    maxY = (float)Math.Max(maxY, y + bottom - _outlineThickness);
                }

                // Extract the current glyph's description
                var glyph = _font.GetGlyph(curChar, _characterSize, bold);

                // Add the glyph to the vertices
                AddGlyphQuad(_vertices, new Vector2(x, y), _color, glyph, italic);

                // Update the current bounds with the non outlined glyph bounds
                if (_outlineThickness == 0)
                {
                    float left   = glyph.Bounds.Left;
                    float top    = glyph.Bounds.Top;
                    float right  = glyph.Bounds.Left + glyph.Bounds.Width;
                    float bottom = glyph.Bounds.Top  + glyph.Bounds.Height;

                    minX = (float)Math.Min(minX, x + left  - italic * bottom);
                    maxX = (float)Math.Max(maxX, x + right - italic * top);
                    minY = (float)Math.Min(minY, y + top);
                    maxY = (float)Math.Max(maxY, y + bottom);
                }

                // Advance to the next character
                x += glyph.Advance;
            }

            // If we're using the underlined style, add the last line
            if (underlined && (x > 0))
            {
                AddLine(_vertices, x, y, _color, underlineOffset, underlineThickness);

                if (_outlineThickness != 0)
                    AddLine(_outlineVertices, x, y, _outlineColor, underlineOffset, underlineThickness, _outlineThickness);
            }

            // If we're using the strike through style, add the last line across all characters
            if (strikeThrough && (x > 0))
            {
                AddLine(_vertices, x, y, _color, strikeThroughOffset, underlineThickness);

                if (_outlineThickness != 0)
                    AddLine(_outlineVertices, x, y, _outlineColor, strikeThroughOffset, underlineThickness, _outlineThickness);
            }

            // Update the bounding rectangle
            _bounds = new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }


        private static void AddLine(VertexArray vertices, float lineLength, float lineTop, Color color,
            float offset, float thickness, float outlineThickness = 0)
        {
            float top = (float)Math.Floor(lineTop + offset - (thickness / 2) + 0.5f);
            float bottom = top + (float)Math.Floor(thickness + 0.5f);

            vertices.Add(new Vertex(new Vector2(-outlineThickness,             top    - outlineThickness), new Vector2(1, 1), color));
            vertices.Add(new Vertex(new Vector2(lineLength + outlineThickness, top    - outlineThickness), new Vector2(1, 1), color));
            vertices.Add(new Vertex(new Vector2(-outlineThickness,             bottom + outlineThickness), new Vector2(1, 1), color));
            vertices.Add(new Vertex(new Vector2(-outlineThickness,             bottom + outlineThickness), new Vector2(1, 1), color));
            vertices.Add(new Vertex(new Vector2(lineLength + outlineThickness, top    - outlineThickness), new Vector2(1, 1), color));
            vertices.Add(new Vertex(new Vector2(lineLength + outlineThickness, bottom + outlineThickness), new Vector2(1, 1), color));
        }

        private static void AddGlyphQuad(VertexArray vertices, Vector2 position, Color color, Glyph glyph, float italic, float outlineThickness = 0)
        {
            float left   = glyph.Bounds.Left;
            float top    = glyph.Bounds.Top;
            float right  = glyph.Bounds.Left + glyph.Bounds.Width;
            float bottom = glyph.Bounds.Top  + glyph.Bounds.Height;

            float u1 = (float)(glyph.TexCoords.Left);
            float v1 = (float)(glyph.TexCoords.Top);
            float u2 = (float)(glyph.TexCoords.Left + glyph.TexCoords.Width);
            float v2 = (float)(glyph.TexCoords.Top  + glyph.TexCoords.Height);

            vertices.Add(new Vertex(new Vector2(position.X + left  - italic * top    - outlineThickness, position.Y + top    - outlineThickness), new Vector2(u1, v1), color));
            vertices.Add(new Vertex(new Vector2(position.X + right - italic * top    - outlineThickness, position.Y + top    - outlineThickness), new Vector2(u2, v1), color));
            vertices.Add(new Vertex(new Vector2(position.X + left  - italic * bottom - outlineThickness, position.Y + bottom - outlineThickness), new Vector2(u1, v2), color));
            vertices.Add(new Vertex(new Vector2(position.X + left  - italic * bottom - outlineThickness, position.Y + bottom - outlineThickness), new Vector2(u1, v2), color));
            vertices.Add(new Vertex(new Vector2(position.X + right - italic * top    - outlineThickness, position.Y + top    - outlineThickness), new Vector2(u2, v1), color));
            vertices.Add(new Vertex(new Vector2(position.X + right - italic * bottom - outlineThickness, position.Y + bottom - outlineThickness), new Vector2(u2, v2), color));
        }
    }
}
