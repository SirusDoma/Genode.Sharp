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
        public unsafe void Update(Vertex[] vertices)
        {
            // Assign vertices information
            _vertices = vertices;
            _length   = vertices.Length;

            // Pin the vertices, prevent GC wipe this pointer
            var handle     = GCHandle.Alloc(_vertices, GCHandleType.Pinned);
            var pointer    = handle.AddrOfPinnedObject();

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
            handle.Free();

            /* Upload the vertices
             * This version is pinning the vertices temporary
             * GC will wipe the pointer few milliseconds later
             * Resulting the rendered object disappear immediately after being drawn
             * Use following codes only for future references
            fixed (Vertex* data = _vertices)
            {
                // Calculate the stride and upload the vertices
                var stride = Vertex.Stride;
                GL.VertexPointer(2, VertexPointerType.Float, stride,
                    new IntPtr(data).Increment(0));

                GL.TexCoordPointer(2, TexCoordPointerType.Float, stride,
                    new IntPtr(data).Increment(8));
                GLChecker.CheckError();

                GL.ColorPointer(4, ColorPointerType.UnsignedByte, stride,
                    new IntPtr(data).Increment(16));
                GLChecker.CheckError();
                //}
            }
            */
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
