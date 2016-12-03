using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using Genode;

namespace Genode.Graphics
{
    /// <summary>
    /// Represents a camera in the 2D scene.
    /// </summary>
    public class View
    {
        private Vector2 _center;
        private SizeF _size;
        private float _rotation;
        private RectangleF _viewport;
        private Transform _transform;
        private bool _isUpdated;

        /// <summary>
        /// Gets or sets the center point of the current <see cref="View"/> object.
        /// </summary>
        public Vector2 Center
        {
            get { return _center; } 
            set
            {
                _center = value;
                _isUpdated = false;
            }
        }

        /// <summary>
        /// Gets or sets the size of the current <see cref="View"/> object.
        /// </summary>
        public SizeF Size
        {
            get { return _size; }
            set
            {
                _size = value;
                _isUpdated = false;
            }
        }

        /// <summary>
        /// Gets or sets the rotation of the current <see cref="View"/> object.
        /// </summary>
        public float Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value % 360f;
                if (_rotation < 0)
                    _rotation += 360f;

                _isUpdated = false;
            }
        }

        /// <summary>
        /// Gets or sets the target viewport rectangle of the <see cref="View"/>.
        /// </summary>
        public RectangleF Viewport
        {
            get { return _viewport; }
            set { _viewport = value; }
        }

        /// <summary>
        /// Get the Projection <see cref="Graphics.Transform"/> of the <see cref="View"/>.
        /// </summary>
        public Transform Transform
        {
            get
            {
                if (!_isUpdated)
                {
                    // Rotation components
                    float angle  = _rotation * 3.141592654f / 180f;
                    float cosine = (float)Math.Cos(angle);
                    float sine   = (float)Math.Sin(angle);
                    float tx     = -_center.X * cosine - _center.Y * sine + _center.X;
                    float ty     =  _center.X * sine - _center.Y * cosine + _center.Y;

                    // Projection components
                    float a =  2f / _size.Width;
                    float b = -2f / _size.Height;
                    float c = -a * _center.X;
                    float d = -b * _center.Y;

                    // Rebuild the projection matrix
                    _transform = new Transform(a * cosine, a * sine  , a * tx + c,
                                              -b * sine  , b * cosine, b * ty + d,
                                               0f        , 0f        , 1f);
                    _isUpdated = true;
                }

                return _transform;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="View"/> class.
        /// </summary>
        public View()
        {
            _center = Vector2.Zero;
            _size = SizeF.Empty;
            _rotation = 0;
            _viewport = new RectangleF(0f, 0f, 1f, 1f);
            _isUpdated = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="View"/> class
        /// with specified viewport.
        /// </summary>
        /// <param name="viewport">The viewport of view.</param>
        public View(RectangleF viewport)
            : this()
        {
            Reset(viewport);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="View"/>
        /// with specified center point and size.
        /// </summary>
        /// <param name="center">The center point of the view.</param>
        /// <param name="size">The size of the view.</param>
        public View(Vector2 center, SizeF size)
        {
            _center = center;
            _size = size;
            _rotation = 0;
            _viewport = new RectangleF(0f, 0f, 1f, 1f);
            _isUpdated = false;
        }

        /// <summary>
        /// Reset the <see cref="View"/>.
        /// </summary>
        /// <param name="rectangle">Rectangle that defines an area to display.</param>
        public void Reset(RectangleF rectangle)
        {
            _center = new Vector2(rectangle.X + rectangle.Width / 2f, rectangle.Y + rectangle.Height / 2f);
            _size = rectangle.Size;
            _rotation = 0;

            _isUpdated = false;
        }

        /// <summary>
        /// Move the <see cref="View"/> by given offset.
        /// </summary>
        /// <param name="x">X-Coordinate Offset.</param>
        /// <param name="y">Y-Coordinate Offset.</param>
        public void Move(float x, float y)
        {
            Center = new Vector2(_center.X + x, _center.Y + y);
        }

        /// <summary>
        /// Move the <see cref="View"/> by given offset.
        /// </summary>
        /// <param name="offset">Move Offset.</param>
        public void Move(Vector2 offset)
        {
            Center = offset;
        }

        /// <summary>
        /// Rotate the <see cref="View"/> by given angle.
        /// </summary>
        /// <param name="angle">Angle of Rotation, in degrees.</param>
        public void Rotate(float angle)
        {
            Rotation += angle;
        }

        /// <summary>
        /// Zoom the <see cref="View"/> by given factor.
        /// </summary>
        /// <param name="factor">Zoom factor.</param>
        public void Zoom(float factor)
        {
            Size = new SizeF(_size.Width * factor, _size.Height * factor);
        }
    }
}
