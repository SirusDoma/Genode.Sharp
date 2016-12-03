using System;
using System.Collections.Generic;
using System.Text;

namespace Genode.Graphics
{
    internal class Row
    {
        public int width;
        public int top;
        public int height;

        internal Row(int rowTop, int rowHeight)
        {
            width = 0;
            top = rowTop;
            height = rowHeight;
        }
    }
}
