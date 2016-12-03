using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Genode;
using Genode.Graphics;
using Genode.Internal.OpenGL;

namespace Genode.Graphics
{
    /// <summary>
    /// Represents an Implementation that uses Immediate Mode.
    /// </summary>
    public sealed class LegacyRenderer : IRendererImplementation
    {
        private Vertex[] _vertices;

        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyRenderer"/> class.
        /// </summary>
        public LegacyRenderer()
            : this(new VertexArray(PrimitiveType.Triangles, 3))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyRenderer"/> class
        /// with specified vertices.
        /// </summary>
        /// <param name="vertices"><see cref="VertexArray"/> to upload into buffer.</param>
        public LegacyRenderer(VertexArray vertices)
        {
            _vertices = vertices.ToArray();
        }

        /// <summary>
        /// Create the Implementation.
        /// This does nothing for this implementation.
        /// </summary>
        public void Create()
        {
        }

        /// <summary>
        /// Upload the Vertices into Buffer Data.
        /// </summary>
        public void Update(Vertex[] vertices)
        {
            _vertices = vertices;
            // Then nothing to do.
            // Immediate mode doesn't use buffer data.
        }

        /// <summary>
        /// Render the vertices using current implementation.
        /// </summary>
        public void Render(PrimitiveType type)
        {
            // Notes:
            // GL.GetError() cannot be called upon render the immediate mode implementation.
            // There is no way to check automatically whether our data is valid or not.
            // Just make sure the specified colors, texcoords and positions are valid and correct.

            // Check the error before begin(?)
            GLChecker.CheckError();

            // Specify primitive mode.
            GL.Begin((OpenTK.Graphics.OpenGL.PrimitiveType)((int)type));

            // Set the Color, Texture Coordinate and Positions.
            for (int i = 0; i < _vertices.Length; i++)
            {
                // Color.
                GL.Color4(_vertices[i].Color.R, _vertices[i].Color.G, _vertices[i].Color.B, _vertices[i].Color.A);

                // TexCoord.
                GL.TexCoord2(_vertices[i].TexCoords.X, _vertices[i].TexCoords.Y);

                // Position.
                GL.Vertex2(_vertices[i].Position.X, _vertices[i].TexCoords.Y);
            }

            // Finished.
            GL.End();

            // Check error again(?)
            GLChecker.CheckError();
        }

        /// <summary>
        /// Dispose the Implementation.
        /// <see cref="LegacyRenderer"/> doesn't use unmanaged / OpenGL resource.
        /// So basically this functions does nothing.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
