using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Genode;
using Genode.Entities;

namespace Genode.Graphics
{
    /// <summary>
    /// Represents a renderable <see cref="Texture"/> 
    /// with its own Transformations, Color and other properties
    /// </summary>
    public class Sprite : Transformable, IRenderable, IColorable, IDisposable
    {
        private Vertex[]  _vertices;
        private Texture   _texture;
        private Rectangle _texCoords;

        /// <summary>
        /// Gets or sets a value indicating whether the current <see cref="Sprite"/> object should be visible.
        /// </summary>
        public bool Visible
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the sub <see cref="Rectangle"/> of the <see cref="Texture"/> displayed by the current <see cref="Sprite"/> object.
        /// </summary>
        public virtual Rectangle TextureRect
        {
            get
            {
                return _texCoords;
            }
            set
            {
                if (_texCoords != value)
                {
                    _texCoords = value;
                    UpdatePositions();
                    UpdateTexCoords();
                }
            }
        }

        /// <summary>
        /// Gets or sets the source <see cref="Graphics.Texture"/> of current <see cref="Sprite"/> object.
        /// </summary>
        public virtual Texture Texture
        {
            get
            {
                return _texture;
            }
            set
            {
                if (_texCoords == Rectangle.Empty)
                {
                    TextureRect = new Rectangle(Point.Empty, value.Size);
                }

                _texture = value;
            }
        }

        /// <summary>
        /// Gets or sets the global color of current <see cref="Sprite"/> object.
        /// </summary>
        public Color Color
        {
            get
            {
                return _vertices[0].Color;
            }
            set
            {
                for (int i = 0; i < _vertices.Length; i++)
                {
                    _vertices[i].Color = value;
                }
            }
        }

        /// <summary>
        /// Gets the local bounding <see cref="RectangleF"/> of current <see cref="Sprite"/> object.
        /// </summary>
        public RectangleF LocalBounds
        {
            get
            {
                float width  = (float)Math.Abs(_texCoords.Width);
                float height = (float)Math.Abs(_texCoords.Height);

                return new RectangleF(0f, 0f, width, height);
            }
        }

        /// <summary>
        /// Gets the local bounding <see cref="RectangleF"/> of current <see cref="Sprite"/> object.
        /// </summary>
        public RectangleF GlobalBounds
        {
            get
            {
                return Transform.TransformRectangle(LocalBounds);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sprite"/> class.
        /// </summary>
        public Sprite()
            : base()
        {
            _vertices  = new Vertex[4];
            _texture   = null;
            _texCoords = Rectangle.Empty;

            Color      = Color.White;
            Visible    = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sprite"/> class
        /// with specified <see cref="Graphics.Texture"/> source.
        /// </summary>
        /// <param name="texture">The <see cref="Graphics.Texture"/> source.</param>
        public Sprite(Texture texture)
            : this()
        {
            Texture = texture;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sprite"/> class
        /// with specified <see cref="Graphics.Texture"/> source and texture rectangle.
        /// </summary>
        /// <param name="texture">The <see cref="Graphics.Texture"/> source.</param>
        /// <param name="texcoords">The sub <see cref="Rectangle"/> of the <see cref="Texture"/> displayed.</param>
        public Sprite(Texture texture, Rectangle texcoords)
            : this()
        {
            Texture = texture;
        }

        private void UpdatePositions()
        {
            RectangleF bounds = LocalBounds;

            _vertices[0].Position = new Vector2(0, 0);
            _vertices[1].Position = new Vector2(0, bounds.Height);
            _vertices[2].Position = new Vector2(bounds.Width, 0);
            _vertices[3].Position = new Vector2(bounds.Width, bounds.Height);
        }

        private void UpdateTexCoords()
        {
            float left   = (float)_texCoords.Left;
            float right  = left + _texCoords.Width;
            float top    = (float)_texCoords.Top;
            float bottom = top + _texCoords.Height;

            _vertices[0].TexCoords = new Vector2(left, top);
            _vertices[1].TexCoords = new Vector2(left, bottom);
            _vertices[2].TexCoords = new Vector2(right, top);
            _vertices[3].TexCoords = new Vector2(right, bottom);
        }

        internal Vertex[] GetVertices()
        {
            return _vertices;
        }

        /// <summary>
        /// Render the current <see cref="Sprite"/> object to a <see cref="RenderTarget"/>.
        /// </summary>
        /// <param name="target">The <see cref="RenderTarget"/> to render to.</param>
        /// <param name="states">The current render states.</param>
        public virtual void Render(RenderTarget target, RenderStates states)
        {
            if (Visible && _texture != null)
            {
                states.Transform *= Transform;
                states.Texture = Texture;

                target.Render(_vertices, PrimitiveType.TriangleStrip, states);
            }
        }

        /// <summary>
        /// Releases all resources used by the <see cref="Sprite"/>.
        /// </summary>
        public void Dispose()
        {
            Texture?.Dispose();
        }
    }
}
