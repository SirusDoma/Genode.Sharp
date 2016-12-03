using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Genode.Graphics
{
    public class Glyph
    {
        public float Advance
        {
            get;
            set;
        }

        public RectangleF Bounds
        {
            get;
            set;
        }

        public Rectangle TexCoords
        {
            get;
            set;
        }

        public Glyph()
        {
            Advance = 0f;
            Bounds = RectangleF.Empty;
            TexCoords = Rectangle.Empty;
        }
    }
}
