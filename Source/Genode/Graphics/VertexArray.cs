using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Genode.Graphics
{
    /// <summary>
    /// Represents a set of one or more 2D primitives. 
    /// </summary>
    public class VertexArray : IEnumerable<Vertex>
    {
        private List<Vertex> _vertices = new List<Vertex>();

        /// <summary>
        /// Get or Set Primitive Type of Vertices.
        /// </summary>
        public PrimitiveType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Get the <see cref="Vertex"/> based of index on Array.
        /// </summary>
        /// <param name="index">Index of <see cref="Vertex"/> in Array.</param>
        /// <returns>Selected <see cref="Vertex"/> based specified index.</returns>
        public Vertex this[int index]
        {
            get
            {
                if (index >= _vertices.Count || index < 0 || _vertices.Count == 0)
                {
                    throw new IndexOutOfRangeException();
                }

                return _vertices[index];
            }
            set
            {
                if (index >= _vertices.Count || index < 0 || _vertices.Count == 0)
                {
                    throw new IndexOutOfRangeException();
                }
                
                _vertices[index] = value;
            }
        }

        /// <summary>
        /// Gets the number of Vertex inside of the current instance of <see cref="VertexArray"/>.
        /// </summary>
        public int Count
        {
            get
            {
                return _vertices.Count;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexArray"/> class.
        /// </summary>
        public VertexArray()
            : this(PrimitiveType.Triangles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexArray"/> class.
        /// </summary>
        /// <param name="type">Type of primitive.</param>
        public VertexArray(PrimitiveType type)
        {
            Type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexArray"/> class.
        /// </summary>
        /// <param name="type">Type of primitive.</param>
        /// <param name="count">Initial number of vertex in this array.</param>
        public VertexArray(PrimitiveType type, int count)
        {
            Type = type;
            for (int i = 0; i < count; i++)
            {
                _vertices.Add(new Vertex(new Vector2(0f, 0f), new Vector2(0f, 0f), Color.White));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexArray"/> class.
        /// </summary>
        /// <param name="type">Type of primitive.</param>
        /// <param name="vertices">Existing vertices.</param>
        public VertexArray(PrimitiveType type, Vertex[] vertices)
        {
            this._vertices = new List<Vertex>(vertices);
            Type = type;
        }

        /// <summary>
        /// Add <see cref="Vertex"/> into current <see cref="VertexArray"/> object.
        /// </summary>
        /// <param name="vertex">The <see cref="Vertex"/> that to be added into <see cref="VertexArray"/></param>
        public void Add(Vertex vertex)
        {
            _vertices.Add(vertex);
        }

        /// <summary>
        /// Add <see cref="Vertex"/> into current <see cref="VertexArray"/> object.
        /// </summary>
        /// <param name="vertex">The <see cref="Vertex"/> that to be added.</param>
        public void Add(params Vertex[] vertex)
        {
            _vertices.AddRange(vertex);
        }

        /// <summary>
        /// Remove <see cref="Vertex"/> from the current <see cref="VertexArray"/> object.
        /// </summary>
        /// <param name="index">Index of the <see cref="Vertex"/> that will be removed.</param>
        /// <param name="count">The number of <see cref="Vertex"/> that will be removed.</param>
        public void Remove(int index, int count = 1)
        {
            if (index < 0 || index >= Count || count <= 0 || index + count >= Count)
            {
                return;
            }

            _vertices.RemoveRange(index, count);
        }

        /// <summary>
        /// Update Vertices inside current <see cref="VertexArray"/> object at specified index with specified array of <see cref="Vertex"/>.
        /// </summary>
        /// <param name="index">Start index of Vertices that will be updated.</param>
        /// <param name="vertices">Vertices that will be applied.</param>
        public void Update(int index, params Vertex[] vertices)
        {
            if (index < 0 || index >= Count || vertices.Length <= 0 || index + vertices.Length >= Count)
            {
                return;
            }

            for (int i = index; i < vertices.Length; i++)
            {
                _vertices[i] = vertices[i - index];
            }
        }

        /// <summary>
        /// Compute the bounding <see cref="RectangleF"/> of the current <see cref="VertexArray"/> object..
        /// </summary>
        /// <returns>Bounding <see cref="RectangleF"/>.</returns>
        public RectangleF GetBounds()
        {
            if (_vertices != null && _vertices.Count > 0)
            {
                float left   = _vertices[0].Position.X;
                float top    = _vertices[0].Position.Y;
                float right  = _vertices[0].Position.X;
                float bottom = _vertices[0].Position.Y;

                for (int i = 1; i < _vertices.Count; ++i)
                {
                    Vector2 position = _vertices[i].Position;

                    if (position.X < left)
                    {
                        left = position.X;
                    }
                    else if (position.X > right)
                    {
                        right = position.X;
                    }

                    if (position.Y < top)
                    {
                        top = position.Y;
                    }
                    else if (position.Y > bottom)
                    {
                        bottom = position.Y;
                    }
                }

                return new RectangleF(left, top, right - left, bottom - top);
            }
            else
            {
                return RectangleF.Empty;
            }
        }

        /// <summary>
        /// Clear the current <see cref="VertexArray"/> object.
        /// </summary>
        public void Clear()
        {
            _vertices.Clear();
        }

        /// <summary>
        /// Return Vertices as an Array of <see cref="Vertex"/>.
        /// </summary>
        /// <returns>An Array of <see cref="Vertex"/>.</returns>
        public Vertex[] ToArray()
        {
            return _vertices.ToArray();
        }

        /// <summary>
        /// Return Vertices that interates for Enumeration.
        /// </summary>
        /// <returns>The current <see cref="Vertex"/> in interation.</returns>
        public IEnumerator<Vertex> GetEnumerator()
        {
            for (int i = 0; i < _vertices.Count; i++)
            {
                yield return _vertices[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public static implicit operator Vertex[](VertexArray vertices)
        {
            return vertices.ToArray();
        }
    }
}
