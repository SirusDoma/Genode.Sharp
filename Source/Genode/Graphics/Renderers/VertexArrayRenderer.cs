using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Genode;
using Genode.Internal.OpenGL;

namespace Genode.Graphics
{
    /// <summary>
    /// Represents a rendering implementation that uses Vertex Array Object.
    /// </summary>
    public sealed class VertexArrayRenderer : IRendererImplementation
    {
        private static bool isChecked = false;
        private static bool isAvailable = true;

        /// <summary>
        /// Gets a value indicating whether the system support Vertex Arrays rendering.
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
                if (!isChecked)
                {
                    isAvailable = new Version(GL.GetString(StringName.Version).Substring(0, 3)) >= new Version(1, 1) ? true : false;
                    isChecked = true;
                }

                return isAvailable;
            }
        }

        private Vertex[] _vertices;
        private int _length;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexArrayRenderer"/> class.
        /// </summary>
        public VertexArrayRenderer()
            : this(new VertexArray(PrimitiveType.TriangleStrip, 4))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexArrayRenderer"/> class
        /// with specified vertices.
        /// </summary>
        /// <param name="vertices">The <see cref="VertexArray"/> to be uploaded into buffer.</param>
        public VertexArrayRenderer(VertexArray vertices)
        {
            Update(vertices);
        }

        /// <summary>
        /// Upload the Vertices into Buffer Data.
        /// </summary>
        public void Update(Vertex[] vertices)
        {
            // Assign vertices information
            _vertices = vertices;
            _length   = vertices.Length;

            // Pin the vertices, prevent GC wipe this pointer
            using (var memory = new MemoryLock(_vertices))
            {
                var pointer = memory.Address;

                // Calculate the stride and upload the vertices
                var stride = Vertex.Stride;
                GL.VertexPointer(2, VertexPointerType.Float, stride,
                    pointer.Increment(0));
                GLChecker.CheckError();

                GL.TexCoordPointer(2, TexCoordPointerType.Float, stride,
                    pointer.Increment(8));
                GLChecker.CheckError();

                GL.ColorPointer(4, ColorPointerType.UnsignedByte, stride,
                    pointer.Increment(16));
                GLChecker.CheckError();
            }
        }

        /// <summary>
        /// Render the vertices using current implementation.
        /// </summary>
        public void Render(PrimitiveType type)
        {
            // Draw the Vertices
            GLChecker.Check(() =>
                GL.DrawArrays((OpenTK.Graphics.OpenGL.PrimitiveType)type, 0, _length)
            );
        }

        /// <summary>
        /// Releases all resources used by the <see cref="VertexArrayRenderer"/>.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
