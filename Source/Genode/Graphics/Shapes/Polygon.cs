using System;
using System.Collections.Generic;
using System.Text;

namespace Genode.Graphics
{
    public abstract partial class Shape
    {
        /// <summary>
        /// Represents a convex polygon <see cref="Shape"/>.
        /// </summary>
        public class Polygon : Shape
        {
            private Vector2[] _points;

            public int PointCount
            {
                get
                {
                    return _points.Length;
                }
                set
                {
                    Array.Resize(ref _points, value);
                    Update();
                }
            }

            /// <summary>
            /// Gets or sets a specified point of current <see cref="Polygon"/> object.
            /// </summary>
            /// <param name="index">Index of the point to get, in range [0 .. GetPointCount() - 1]</param>
            public Vector2 this[int index]
            {
                get
                {
                    return GetPoint(index);
                }
                set
                {
                    SetPoint(index, value);
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Polygon"/> class.
            /// </summary>
            public Polygon()
                : base()
            {
                PointCount = 0;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Polygon"/> class
            /// with specified number of points.
            /// </summary>
            /// <param name="pointCount">The number of points of the polygon.</param>
            public Polygon(int pointCount)
                : base()
            {
                PointCount = pointCount;
            }

            /// <summary>
            /// Get the total number of points of current <see cref="Polygon"/> object.
            /// </summary>
            /// <returns>Number of points of current <see cref="Polygon"/> object.</returns>
            protected override int GetPointCount()
            {
                return _points.Length;
            }

            /// <summary>
            /// Get a specified point of current <see cref="Polygon"/> object.
            /// </summary>
            /// <param name="index">Index of the point to get, in range [0 .. GetPointCount() - 1]</param>
            /// <returns>Index-th point of current <see cref="Polygon"/> object.</returns>
            protected override Vector2 GetPoint(int index)
            {
                return _points[index];
            }

            /// <summary>
            /// Set a specified point of current <see cref="Polygon"/> object.
            /// </summary>
            /// <param name="index">Index of the point to get, in range [0 .. GetPointCount() - 1]</param>
            /// <param name="point">The new point.</param>
            public void SetPoint(int index, Vector2 point)
            {
                _points[index] = point;
                Update();
            }
        }
    }
}
