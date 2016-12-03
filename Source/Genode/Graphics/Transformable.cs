using System;
using System.Collections.Generic;
using System.Text;

using Genode;
using Genode.Entities;

namespace Genode.Graphics
{
    /// <summary>
    /// Represents a decomposed <see cref="Graphics.Transform"/> defined by a position, a rotation and a scale.
    /// </summary>
    public abstract class Transformable
    {
        private Vector2 _origin;
        private Vector2 _position;
        private float _rotation;
        private Vector2 _scale;
        private Transform _transform;
        private bool _isTransformUpdated;

        /// <summary>
        /// Gets or sets the X-Coordinate position of current <see cref="Transformable"/> object.
        /// </summary>
        public float X
        {
            get { return _position.X; }
            set
            {
                _position = new Vector2(value, _position.Y);
                _isTransformUpdated = false;
            }
        }

        /// <summary>
        /// Gets or sets the Y-Coordinate position of current <see cref="Transformable"/> object.
        /// </summary>
        public float Y
        {
            get { return _position.Y; }
            set
            {
                _position = new Vector2(_position.X, value);
                _isTransformUpdated = false;
            }
        }

        /// <summary>
        /// Gets or sets the position of current <see cref="Transformable"/> object.
        /// </summary>
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                _isTransformUpdated = false;
            }
        }

        /// <summary>
        /// Gets or sets the orientation of current <see cref="Transformable"/> object.
        /// </summary>
        public float Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value % 360f;
                if (_rotation < 0)
                    _rotation += 360f;

                _isTransformUpdated = false;
            }
        }

        /// <summary>
        /// Gets or sets the scale factors of current <see cref="Transformable"/> object.
        /// </summary>
        public Vector2 Scaling
        {
            get { return _scale; }
            set
            {
                _scale = value;
                _isTransformUpdated = false;
            }
        }

        /// <summary>
        /// Gets or sets the local origin of current <see cref="Transformable"/> object.
        /// </summary>
        public Vector2 Origin
        {
            get { return _origin; }
            set
            {
                _origin = value;

                // Fix some quality problem
                _origin.X = (float)Math.Round(_origin.X);
                _origin.Y = (float)Math.Round(_origin.Y);

                _isTransformUpdated = false;
            }
        }

        /// <summary>
        /// Gets the combined <see cref="Graphics.Transform"/> of current <see cref="Transformable"/> object.
        /// </summary>
        public Transform Transform
        {
            get
            {
                if (!_isTransformUpdated)
                {
                    float angle  = -_rotation * 3.141592654f / 180f;
                    float cosine = (float)(Math.Cos(angle));
                    float sine   = (float)(Math.Sin(angle));
                    float sxc    = _scale.X * cosine;
                    float syc    = _scale.Y * cosine;
                    float sxs    = _scale.X * sine;
                    float sys    = _scale.Y * sine;
                    float tx     = -_origin.X * sxc - _origin.Y * sys + _position.X;
                    float ty     =  _origin.X * sxs - _origin.Y * syc + _position.Y;

                    _transform  =  new Transform( sxc, sys, tx,
                                                 -sxs, syc, ty,
                                                  0f,  0f,  1f);
                    _isTransformUpdated = true;
                }

                return _transform;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transformable"/> class.
        /// </summary>
        public Transformable()
            : base()
        {
            _origin = Vector2.Zero;
            _position = Vector2.Zero;
            _rotation = 0;
            _scale = new Vector2(1f, 1f);
            _transform = Transform.Identity;
            _isTransformUpdated = false;
        }

        /// <summary>
        /// Move the current <see cref="Transformable"/> object by a given offset.
        /// </summary>
        /// <param name="offsetX">The X Offset.</param>
        /// <param name="offsetY">The Y Offset.</param>
        public void Move(float offsetX, float offsetY)
        {
            Move(new Vector2(offsetX, offsetY));
        }

        /// <summary>
        /// Move the current <see cref="Transformable"/> object by a given offset.
        /// </summary>
        /// <param name="offset">The Offset.</param>
        public void Move(Vector2 offset)
        {
            Position += offset;
        }

        /// <summary>
        /// Rotate the current <see cref="Transformable"/> object.
        /// </summary>
        /// <param name="angle">Angle of rotation, in degrees.</param>
        public void Rotate(float angle)
        {
            Rotation += angle;
        }

        /// <summary>
        /// Scale the current <see cref="Transformable"/> object.
        /// </summary>
        /// <param name="factorX">Horizontal scale factor.</param>
        /// <param name="factorY">Vertical scale factor.</param>
        public void Scale(float factorX, float factorY)
        {
            Scale(new Vector2(factorX, factorY));
        }

        /// <summary>
        /// Scale the current <see cref="Transformable"/> object.
        /// </summary>
        /// <param name="factor">Scale factors.</param>
        public void Scale(Vector2 factor)
        {
            Scaling += factor;
        }
    }
}
