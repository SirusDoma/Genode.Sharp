using System;
using System.Collections.Generic;
using System.Text;

namespace Genode.Graphics
{
    public abstract partial class Shape
    {
        /// <summary>
        /// Represents a circle <see cref="Shape"/>.
        /// </summary>
        public class Circle : Shape
        {
            private float _radius;
            private int _pointCount;

            /// <summary>
            /// Gets or sets the number of points composing the circle.
            /// </summary>
            public int PointCount
            {
                get
                {
                    return _pointCount;
                }
                set
                {
                    _pointCount = value;
                    Update();
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Circle"/> class.
            /// </summary>
            public Circle()
                : base()
            {
                _radius = 0f;
                _pointCount = 30;

                Update();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Circle"/> class
            /// with specified radius.
            /// </summary>
            /// <param name="radius">The radius of the circle.</param>
            public Circle(float radius)
            {
                _radius = radius;
                _pointCount = 30;

                Update();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Circle"/> class
            /// with specified radius and the number of points.
            /// </summary>
            /// <param name="radius">The radius of the circle.</param>
            /// <param name="pointCount">The number of points composing the circle.</param>
            public Circle(float radius, int pointCount)
            {
                _radius = radius;
                _pointCount = pointCount;
                Update();
            }
            /// <summary>
            /// Get the total number of points of current <see cref="Circle"/> object.
            /// </summary>
            /// <returns>Number of points of current <see cref="Circle"/> object.</returns>
            protected override int GetPointCount()
            {
                return _pointCount;
            }

            /// <summary>
            /// Get a specified point of current <see cref="Circle"/> object.
            /// </summary>
            /// <param name="index">Index of the point to get, in range [0 .. GetPointCount() - 1]</param>
            /// <returns>Index-th point of current <see cref="Circle"/> object.</returns>
            protected override Vector2 GetPoint(int index)
            {
                float pi = 3.141592654f;

                float angle = index * 2 * pi / _pointCount - pi / 2;
                float x = (float)Math.Cos(angle) * _radius;
                float y = (float)Math.Sin(angle) * _radius;

                return new Vector2(_radius + x, _radius + y);
            }
        }
    }
}
