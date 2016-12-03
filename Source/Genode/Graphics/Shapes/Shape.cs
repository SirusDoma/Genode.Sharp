using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Genode;
using Genode.Entities;

namespace Genode.Graphics
{
    /// <summary>
    /// Represents base class for textured shapes with outline.
    /// </summary>
    public abstract partial class Shape : Transformable, IRenderable, IColorable
    {
        private Texture     _texture;
        private System.Drawing.Rectangle   _texCoords;
        private Color       _fillColor;
        private Color       _outlineColor;
        private float       _outlineThickness;
        private VertexArray _vertices;
        private VertexArray _outlineVertices;
        private RectangleF  _insideBounds;
        private RectangleF  _bounds;

        /// <summary>
        /// Gets or sets a value indicating whether the current <see cref="Shape"/> object should be visible.
        /// </summary>
        public bool Visible
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets source <see cref="Graphics.Texture"/> of current <see cref="Shape"/> object.
        /// </summary>
        public Texture Texture
        {
            get
            {
                return _texture;
            }
            set
            {
                if (_texCoords == System.Drawing.Rectangle.Empty)
                {
                    TextureRect = new System.Drawing.Rectangle(Point.Empty, value.Size);
                }

                _texture = value;
            }
        }

        /// <summary>
        /// Gets or sets the sub <see cref="System.Drawing.Rectangle"/> of the <see cref="Texture"/> displayed by the current <see cref="Shape"/> object.
        /// </summary>
        public System.Drawing.Rectangle TextureRect
        {
            get
            {
                return _texCoords;
            }
            set
            {
                _texCoords = value;
                UpdateTexCoords();
            }
        }

        /// <summary>
        /// Gets or sets the fill color of current <see cref="Shape"/> object.
        /// </summary>
        public Color Color
        {
            get { return FillColor;  }
            set { FillColor = value; }
        }

        /// <summary>
        /// Gets or sets the fill color of current <see cref="Shape"/> object.
        /// </summary>
        public Color FillColor
        {
            get
            {
                return _fillColor;
            }
            set
            {
                _fillColor = value;
                UpdateFillColors();
            }
        }

        /// <summary>
        /// Gets or sets the outline color of current <see cref="Shape"/> object.
        /// </summary>
        public Color OutlineColor
        {
            get
            {
                return _outlineColor;
            }
            set
            {
                _outlineColor = value;
                UpdateOutlineColors();
            }
        }

        /// <summary>
        /// Gets or sets the outline thickness of current <see cref="Shape"/> object.
        /// </summary>
        public float OutlineThickness
        {
            get
            {
                return _outlineThickness;
            }
            set
            {
                _outlineThickness = value;
                Update();
            }
        }

        /// <summary>
        /// Gets the local bounding <see cref="RectangleF"/> of current <see cref="Shape"/> object.
        /// </summary>
        public RectangleF LocalBounds
        {
            get
            {
                return _bounds;
            }
        }

        /// <summary>
        /// Gets the global bounding <see cref="RectangleF"/> of current <see cref="Shape"/> object.
        /// </summary>
        public RectangleF GlobalBounds
        {
            get
            {
                return Transform.TransformRectangle(_bounds);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Shape"/> object.
        /// </summary>
        public Shape()
        {
            _texture          = null;
            _texCoords        = System.Drawing.Rectangle.Empty;
            _fillColor        = Color.White;
            _outlineColor     = Color.White;
            _outlineThickness = 0f;
            _vertices         = new VertexArray(PrimitiveType.TriangleFan);
            _outlineVertices  = new VertexArray(PrimitiveType.TriangleStrip);
            _insideBounds     = RectangleF.Empty;
            _bounds           = RectangleF.Empty;

            Visible           = true;
        }

        private void Update()
        {
            // Get the total number of points of the shape
            int count = GetPointCount();
            if (count < 3)
            {
                _vertices        = new VertexArray(_vertices.Type, 0);
                _outlineVertices = new VertexArray(_outlineVertices.Type, 0);
                return;
            }

            // + 2 for center and repeated first point
            _vertices = new VertexArray(_vertices.Type, count + 2);

            // Position
            for (int i = 0; i < count; ++i)
            {
                var vertex = _vertices[i + 1];
                vertex.Position = GetPoint(i);

                _vertices[i + 1] = vertex;
            }

            _vertices[count + 1] = new Vertex(_vertices[1].Position, 
                _vertices[count + 1].TexCoords, _vertices[count + 1].Color);

            // Update the bounding System.Drawing.Rectangle
            // so that the result of getBounds() is correct
            _vertices[0]  = _vertices[1]; 
            _insideBounds = _vertices.GetBounds();

            // Compute the center and make it the first vertex
            var first = _vertices[0];
            first.Position = new Vector2(_insideBounds.Left + _insideBounds.Width / 2,
                _insideBounds.Top + _insideBounds.Height / 2);

            _vertices[0] = first;

            // Color
            UpdateFillColors();

            // Texture coordinates
            UpdateTexCoords();

            // Outline
            UpdateOutline();
        }

        private void UpdateTexCoords()
        {
            for (int i = 0; i < _vertices.Count; ++i)
            {
                var vertex = _vertices[i];
                float xratio = _insideBounds.Width > 0 ? (_vertices[i].Position.X - _insideBounds.Left) / _insideBounds.Width : 0;
                float yratio = _insideBounds.Height > 0 ? (_vertices[i].Position.Y - _insideBounds.Top) / _insideBounds.Height : 0;

                vertex.TexCoords = new Vector2(_texCoords.Left + _texCoords.Width * xratio,
                    _texCoords.Top + _texCoords.Height * yratio);

                _vertices[i] = vertex;
            }
        }

        private void UpdateFillColors()
        {
            for (int i = 0; i < _vertices.Count; ++i)
            {
                var vertex   = _vertices[i];
                vertex.Color = _fillColor;

                _vertices[i] = vertex;
            }
        }

        private void UpdateOutline()
        {
            int count = _vertices.Count - 2;
            _outlineVertices = new VertexArray(_outlineVertices.Type, (count + 1) * 2);

            for (int i = 0; i < count; ++i)
            {
                int index = i + 1;

                // Get the two segments shared by the current point
                Vector2 p0 = (i == 0) ? _vertices[count].Position : _vertices[index - 1].Position;
                Vector2 p1 = _vertices[index].Position;
                Vector2 p2 = _vertices[index + 1].Position;

                // Compute their normal
                Vector2 n1 = ComputeNormal(p0, p1);
                Vector2 n2 = ComputeNormal(p1, p2);

                // Make sure that the normals point towards the outside of the shape
                // (this depends on the order in which the points were defined)
                if (DotProduct(n1, _vertices[0].Position - p1) > 0)
                    n1 = -n1;
                if (DotProduct(n2, _vertices[0].Position - p1) > 0)
                    n2 = -n2;

                // Combine them to get the extrusion direction
                float factor = 1f + (n1.X * n2.X + n1.Y * n2.Y);
                Vector2 normal = (n1 + n2) / factor;

                // Update the outline points
                var outline1 = _outlineVertices[i * 2 + 0];
                var outline2 = _outlineVertices[i * 2 + 1];
                outline1.Position = p1;
                outline2.Position = p1 + normal * _outlineThickness;

                _outlineVertices[i * 2 + 0] = outline1;
                _outlineVertices[i * 2 + 1] = outline2;
            }

            // Duplicate the first point at the end, to close the outline
            var end1      = _outlineVertices[count * 2 + 0];
            end1.Position = _outlineVertices[0].Position;

            var end2      = _outlineVertices[count * 2 + 1];
            end2.Position = _outlineVertices[1].position;

            _outlineVertices[count * 2 + 0] = end1;
            _outlineVertices[count * 2 + 1] = end2;

            // Update outline colors
            UpdateOutlineColors();

            // Update the shape's bounds
            _bounds = _outlineVertices.GetBounds();
        }

        private void UpdateOutlineColors()
        {
            for (int i = 0; i < _outlineVertices.Count; ++i)
            {
                var vertex   = _outlineVertices[i];
                vertex.Color = _outlineColor;

                _outlineVertices[i] = vertex;
            }
        }

        /// <summary>
        /// Get the total number of points of current <see cref="Shape"/> object.
        /// </summary>
        /// <returns>Number of points of current <see cref="Shape"/> object.</returns>
        protected abstract int GetPointCount();

        /// <summary>
        /// Get a specified point of current <see cref="Shape"/> object.
        /// </summary>
        /// <param name="index">Index of the point to get, in range [0 .. GetPointCount() - 1]</param>
        /// <returns>Index-th point of current <see cref="Shape"/> object.</returns>
        protected abstract Vector2 GetPoint(int index);

        /// <summary>
        /// Render the current <see cref="Shape"/> object to a <see cref="RenderTarget"/>.
        /// </summary>
        /// <param name="target">The <see cref="RenderTarget"/> to render to.</param>
        /// <param name="states">The current render states.</param>
        public virtual void Render(RenderTarget target, RenderStates states)
        {
            if (Visible)
            {
                states.Transform *= Transform;

                // Render the inside
                states.Texture = _texture;
                target.Render(_vertices, states);

                // Render the outline
                if (_outlineThickness != 0)
                {
                    states.Texture = null;
                    target.Render(_outlineVertices, states);
                }
            }
        }

        protected static Vector2 ComputeNormal(Vector2 p1, Vector2 p2)
        {
            var normal = new Vector2(p1.Y - p2.Y, p2.X - p1.X);
            float length = (float)Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y);
            if (length != 0f)
                normal /= length;
            return normal;
        }

        protected float DotProduct(Vector2 p1, Vector2 p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y;
        }
    }
}
