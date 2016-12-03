using System;
using System.Collections.Generic;
using System.Text;

namespace Genode.Graphics
{
    public abstract partial class Shape
    {
        /// <summary>
        /// Represent a rectangle <see cref="Shape"/>.
        /// </summary>
        public class Rectangle : Shape
        {
            private System.Drawing.SizeF _size;

            /// <summary>
            /// Gets or sets the size of current <see cref="Rectangle"/> object.
            /// </summary>
            public System.Drawing.SizeF Size
            {
                get
                {
                    return _size;
                }
                set
                {
                    _size = value;
                    Update();
                }
            }

            /// <summary>
            /// Gets or sets the width of current <see cref="Rectangle"/> object.
            /// </summary>
            public float Width
            {
                get
                {
                    return Size.Width;
                }
                set
                {
                    Size = new System.Drawing.SizeF(value, Size.Height);
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Rectangle"/> class.
            /// </summary>
            public Rectangle()
                : base()
            {
                Size = System.Drawing.SizeF.Empty;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Rectangle"/> class
            /// with specified size.
            /// </summary>
            /// <param name="size">The size of rectangle.</param>
            public Rectangle(System.Drawing.SizeF size)
                : base()
            {
                Size = size;
            }

            /// <summary>
            /// Get the total number of points of current <see cref="Rectangle"/> object.
            /// </summary>
            /// <returns>Number of points of current <see cref="Rectangle"/> object.</returns>
            protected override int GetPointCount()
            {
                return 4;
            }

            /// <summary>
            /// Get a specified point of current <see cref="Rectangle"/> object.
            /// </summary>
            /// <param name="index">Index of the point to get, in range [0 .. GetPointCount() - 1]</param>
            /// <returns>Index-th point of current <see cref="Rectangle"/> object.</returns>
            protected override Vector2 GetPoint(int index)
            {
                switch (index)
                {
                    default:
                    case 0: return new Vector2(0f, 0f);
                    case 1: return new Vector2(_size.Width, 0f);
                    case 2: return new Vector2(_size.Width, _size.Height);
                    case 3: return new Vector2(0f, _size.Height);
                }
            }
        }
    }
}
