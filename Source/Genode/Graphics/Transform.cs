using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Genode;
using Genode.Entities;
using Genode.Graphics;

namespace Genode.Graphics
{
    /// <summary>
    /// Represents 3x3 transform matrix.
    /// </summary>
    public class Transform
    {
        /// <summary>
        /// Gets Identity <see cref="Transform"/>, which defines a <see cref="Transform"/> with initial state.
        /// </summary>
        public static Transform Identity
        {
            get
            {
                return new Transform();
            }
        }

        private bool _isTransformUpdated = false;
        private float _rotation = 0;
        private Vector2 _origin = Vector2.Zero, 
                        _position = Vector2.Zero, 
                        _scale = new Vector2(1f, 1f);

        private float[] _matrix = new float[16] {
            1f, 0f, 0f, 0f,
            0f, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f
        };

        /// <summary>
        /// Gets the array of 4x4 matrix elements.
        /// </summary>
        public float[] Matrix
        {
            get
            {
                if (!_isTransformUpdated)
                {
                    float angle = -_rotation * 3.141592654f / 180f;
                    float cosine = (float)Math.Cos(angle);
                    float sine = (float)Math.Sin(angle);

                    float sxc = _scale.X * cosine;
                    float syc = _scale.Y * cosine;
                    float sxs = _scale.X * sine;
                    float sys = _scale.Y * sine;
                    float tx = -_origin.X * sxc - _origin.Y * sys + _position.X;
                    float ty = _origin.X * sxs - _origin.Y * syc + _position.Y;
                    
                    _matrix = new float[16] {
                        sxc, -sxs, 0f, 0f,
                        sys,  syc, 0f, 0f,
                         0f,   0f, 1f, 0f,
                         tx,   ty, 0f, 1f
                    };

                    _isTransformUpdated = true;
                }

                return _matrix;
            }
            private set
            {
                _matrix = value;
                _isTransformUpdated = true;
            }
        }

        /// <summary>
        /// Gets or sets Coodinate Position of the object.
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                _isTransformUpdated = false;
            }
        }

        /// <summary>
        /// Gets or sets Rotation of the object, in degrees.
        /// </summary>
        public float Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value % 360f;
                if (_rotation < 0f)
                {
                    _rotation += 360f;
                }

                _isTransformUpdated = false;
            }
        }

        /// <summary>
        /// Gets or sets Scaling of the object.
        /// </summary>
        public Vector2 Scaling
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = new Vector2(value.X, value.Y);
                _isTransformUpdated = false;
            }
        }

        /// <summary>
        /// Gets or sets Local Origin of the object.
        /// </summary>
        public Vector2 Origin
        {
            get
            {
                return _origin;
            }
            set
            {
                _origin = new Vector2(value.X, value.Y);

                // Fix some quality problem
                _origin.X = (float)Math.Round(_origin.X);
                _origin.Y = (float)Math.Round(_origin.Y);

                _isTransformUpdated = false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transform"/> class.
        /// The initialized instance is equal to <see cref="Transform.Identity"/>.
        /// </summary>
        public Transform()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instace of the <see cref="Transform"/> class from existing <see cref="Transform"/> instance.
        /// </summary>
        /// <param name="transform">Existing <see cref="Transform"/></param>
        public Transform(Transform transform)
            : base()
        {
            Position = transform.Position;
            Origin   = transform.Origin;
            Rotation = transform.Rotation;
            Scaling  = transform.Scaling;
            Matrix   = transform.Matrix;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transform"/> class from an array of matrix elements.
        /// </summary>
        /// <param name="matrix">
        /// An existing an array of 3x3 matrix elements that will be used to construct <see cref="Transform"/>.
        /// This array of matrix length should be 16.
        /// </param>
        public Transform(float[] matrix)
            : base()
        {
            if (matrix.Length != 16)
            {
                throw new ArgumentException("Invalid array of matrix elements. The length is not 16.", "matrix");
            }

            Matrix = matrix;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transform"/> class from 3x3 matrix elements.
        /// </summary>
        /// <param name="m00">1st Matrix Element.</param>
        /// <param name="m01">2nd Matrix Element.</param>
        /// <param name="m02">3rd Matrix Element.</param>
        /// <param name="m10">4th Matrix Element.</param>
        /// <param name="m11">5th Matrix Element.</param>
        /// <param name="m12">6th Matrix Element.</param>
        /// <param name="m20">7th Matrix Element.</param>
        /// <param name="m21">8th Matrix Element.</param>
        /// <param name="m22">9th Matrix Element.</param>
        public Transform(float m00, float m01, float m02,
                         float m10, float m11, float m12,
                         float m20, float m21, float m22)
            : base()
        {
            float[] matrix = new float[16];
            matrix[0] = m00; matrix[4] = m01; matrix[8] = 0f; matrix[12] = m02;
            matrix[1] = m10; matrix[5] = m11; matrix[9] = 0f; matrix[13] = m12;
            matrix[2] = 0f;  matrix[6] = 0f;  matrix[10] = 1f; matrix[14] = 0f;
            matrix[3] = m20; matrix[7] = m21; matrix[11] = 0f; matrix[15] = m22;

            Matrix = matrix;
        }

        /// <summary>
        /// Translate the object from it's current position by given offset.
        /// </summary>
        /// <param name="x">The offfset to apply on X axis.</param>
        /// <param name="y">The offfset to apply on Y axis.</param>
        public Transform Translate(float x, float y)
        {
            var translation = new Transform(1, 0, x,
                                            0, 1, y,
                                            0, 0, 1);

            return Combine(translation);
        }

        /// <summary>
        /// Translate the object from it's current position by given offset.
        /// <param name="point">Offset.</param>
        /// </summary>
        public Transform Translate(Vector2 point)
        {
            return Translate(point.X, point.Y);
        }

        /// <summary>
        /// Rotate the object from it's current rotation by given angle.
        /// </summary>
        /// <param name="angle">Angle of rotation, in degrees.</param>
        public Transform Rotate(float angle)
        {
            float rad = angle * 3.141592654f / 180f;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            var rotation = new Transform(cos, -sin, 0,
                                         sin, cos,  0,
                                         0,   0,    1);

            return Combine(rotation);
        }

        /// <summary>
        /// Rotate the object from it's current rotation by given angle.
        /// </summary>
        /// <param name="angle">Angle of rotation, in degrees.</param>
        /// <param name="centerX">X coordinate of the center of rotation.</param>
        /// <param name="centerY">Y coordinate of the center of rotation.</param>
        public Transform Rotate(float angle, float centerX, float centerY)
        {
            float rad = angle * 3.141592654f / 180f;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            var rotation = new Transform(cos, -sin, centerX * (1 - cos) + centerY * sin,
                                         sin,  cos, centerY * (1 - cos) - centerX * sin,
                                         0,    0,   1);

            return Combine(rotation);
        }

        /// <summary>
        /// Rotate the object from it's current rotation by given angle.
        /// </summary>
        /// <param name="angle">Angle of rotation, in degrees.</param>
        /// <param name="center">The coordinate of the center of rotation.</param>
        public Transform Rotate(float angle, Vector2 center)
        {
            return Rotate(angle, center.X, center.Y);
        }

        /// <summary>
        /// Scale the object from it's current scaling by given factor.
        /// </summary>
        /// <param name="factorX">Scale factor for X Axis.</param>
        /// <param name="factorY">Scale factor for Y Axis.</param>
        public Transform Scale(float factorX, float factorY)
        {
            var scaling = new Transform(factorX, 0,       0,
                                        0,       factorY, 0,
                                        0,       0,       1);

            return Combine(scaling);
        }

        /// <summary>
        /// Scale the object from it's current scaling by given factor.
        /// </summary>
        /// <param name="factor">Scale Factor.</param>
        public Transform Scale(Vector2 factor)
        {
            return Scale(factor.X, factor.Y);
        }

        /// <summary>
        /// Scale the object from it's current scaling by given factor.
        /// </summary>
        /// <param name="factorX">Scale factor for X Axis.</param>
        /// <param name="factorY">Scale factor for Y Axis.</param>
        /// <param name="centerX">X coordinate of the center of scale.</param>
        /// <param name="centerY">Y coordinate of the center of scale.</param>
        public Transform Scale(float factorX, float factorY, float centerX, float centerY)
        {
            var scaling = new Transform(factorX, 0,       centerX * (1 - factorY),
                                        0,       factorY, centerY * (1 - factorY),
                                        0,       0,       1);

            return Combine(scaling);
        }

        /// <summary>
        /// Scale the object from it's current scaling by given factor.
        /// </summary>
        /// <param name="factor">Scale Factor.</param>
        /// <param name="center">The coordinate of the center of scale.</param>
        public Transform Scale(Vector2 factor, Vector2 center)
        {
            return Scale(factor.X, factor.Y, center.X, center.Y);
        }

        /// <summary>
        /// <see cref="Transform"/> a 2D Point.
        /// </summary>
        /// <param name="x">X-Coordinate Point to <see cref="Transform"/>.</param>
        /// <param name="y">Y-Coordinate Point to <see cref="Transform"/>.</param>
        /// <returns>Transformed Point.</returns>
        public Vector2 TransformPoint(float x, float y)
        {
            float[] matrix = Matrix;
            return new Vector2(matrix[0] * x + matrix[4] * y + matrix[12],
                              matrix[1] * x + matrix[5] * y + matrix[13]);
        }

        /// <summary>
        /// <see cref="Transform"/> a 2D Point.
        /// </summary>
        /// <param name="point">Point to <see cref="Transform"/>.</param>
        /// <returns>Transformed Point.</returns>
        public Vector2 TransformPoint(Vector2 point)
        {
            float[] matrix = Matrix;
            return new Vector2(matrix[0] * point.X + matrix[4] * point.Y + matrix[12],
                               matrix[1] * point.X + matrix[5] * point.Y + matrix[13]);
        }

        /// <summary>
        /// <see cref="Transform"/> a Rectangle.
        /// </summary>
        /// <param name="rectangle">Rectangle to <see cref="Transform"/>.</param>
        /// <returns>
        /// Transformed Rectangle in axis oriented <see cref="Rectangle"/>.
        /// In case rotation is exist, it will return bounding <see cref="Rectangle"/> of <see cref="Transform"/> object.
        /// </returns>
        public RectangleF TransformRectangle(RectangleF rectangle)
        {
            // Transform the 4 corners of the rectangle
            Vector2[] points = new Vector2[]
            {
                TransformPoint(rectangle.Left, rectangle.Top),
                TransformPoint(rectangle.Left, rectangle.Top + rectangle.Height),
                TransformPoint(rectangle.Left + rectangle.Width, rectangle.Top),
                TransformPoint(rectangle.Left + rectangle.Width, rectangle.Top + rectangle.Height)
            };

            // Compute the bounding rectangle of the transformed points
            float left = points[0].X;
            float top = points[0].Y;
            float right = points[0].X;
            float bottom = points[0].Y;
            for (int i = 1; i < 4; ++i)
            {
                if (points[i].X < left) left = points[i].X;
                else if (points[i].X > right) right = points[i].X;
                if (points[i].Y < top) top = points[i].Y;
                else if (points[i].Y > bottom) bottom = points[i].Y;
            }

            return new RectangleF(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// Combine the current <see cref="Transform"/> object Matrix with another <see cref="Transform"/>.
        /// </summary>
        /// <param name="transform"><see cref="Transform"/> that will be combined.</param>
        /// <returns>Return combined <see cref="Transform"/>.</returns>
        public Transform Combine(Transform transform)
        {
            float[] a = Matrix;
            float[] b = transform.Matrix;

            var newTransform = new Transform(a[0] * b[0]  + a[4] * b[1]  + a[12] * b[3],
                                             a[0] * b[4]  + a[4] * b[5]  + a[12] * b[7],
                                             a[0] * b[12] + a[4] * b[13] + a[12] * b[15],
                                             a[1] * b[0]  + a[5] * b[1]  + a[13] * b[3],
                                             a[1] * b[4]  + a[5] * b[5]  + a[13] * b[7],
                                             a[1] * b[12] + a[5] * b[13] + a[13] * b[15],
                                             a[3] * b[0]  + a[7] * b[1]  + a[15] * b[3],
                                             a[3] * b[4]  + a[7] * b[5]  + a[15] * b[7],
                                             a[3] * b[12] + a[7] * b[13] + a[15] * b[15]);

            Position += transform.Position;
            Origin   *= transform.Origin;
            Rotation += transform.Rotation;
            Scaling  *= transform.Scaling;

            Matrix = newTransform.Matrix;
            return this;
        }

        /// <summary>
        /// Get an inversed copy of <see cref="Transform"/>.
        /// </summary>
        public Transform GetInverse()
        {
            var matrix = Matrix;
            Array.Reverse(matrix);

            return new Transform(matrix);
        }

        /// <summary>
        /// Inverse the current instance of <see cref="Transform"/>.
        /// </summary>
        public void Inverse()
        {
            Array.Reverse(Matrix);
        }

        /// <summary>
        /// Reset <see cref="Transform"/> to Identity.
        /// </summary>
        public void Reset()
        {
            Position = Vector2.Zero;
            Origin   = Vector2.Zero;
            Scaling  = Vector2.Zero;
            Rotation = 0f;

            Matrix   = Identity.Matrix;
        }

        /// <summary>
        /// Multiply between two <see cref="Transform"/>.
        /// </summary>
        /// <param name="x">Left hand <see cref="Transform"/>.</param>
        /// <param name="y">Right hand <see cref="Transform"/>.</param>
        /// <returns>Combined <see cref="Transform"/></returns>
        public static Transform operator *(Transform x, Transform y)
        {
            if (y == null)
            {
                y = Transform.Identity;
            }

            return new Transform(x).Combine(y);
        }

        /// <summary>
        /// Multiply <see cref="Transform"/> with a 2D Point.
        /// </summary>
        /// <param name="x">Left Hand <see cref="Transform"/>.</param>
        /// <param name="y">Right Hand 2D Point.</param>
        /// <returns>Transformed Point./></returns>
        public static Vector2 operator *(Transform x, Vector2 y)
        {
            return new Transform(x).TransformPoint(y.X, y.Y);
        }
    }
}
