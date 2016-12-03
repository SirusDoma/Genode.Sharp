using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Genode.Graphics
{
    internal class Page
    {
        public Dictionary<long, Glyph> glyphs;
        public Texture texture;
        public int nextRow;
        public List<Row> rows;

        internal Page()
        {
            glyphs = new Dictionary<long, Graphics.Glyph>();
            rows = new List<Row>();
            nextRow = 3;

            var image = new Bitmap(128, 128);
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if (x < 2 && y < 2)
                    {
                        image.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        image.SetPixel(x, y, Color.FromArgb(0, 255, 255, 255));
                    }
                }
            }

            texture = new Texture(image);
            texture.IsSmooth = true;
        }
    }
}
