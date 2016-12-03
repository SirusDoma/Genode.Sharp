using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Genode;
using Genode.Graphics;
using Genode.Internal.OpenGL;

namespace Genode.Graphics
{
    /// <summary>
    /// Represents a rendering implementation that uses Vertex Buffer Object.
    /// </summary>
    public sealed class VertexBufferRenderer : IRendererImplementation
    {
        private static bool isChecked = false;
        private static bool isAvailable = true;

        /// <summary>
        /// Gets a value indicating whether the system support Vertex Buffer Object rendering.
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
                if (!isChecked)
                {
                    isAvailable = new Version(GL.GetString(StringName.Version).Substring(0, 3)) >= new Version(1, 5) ? true : false;
                    isChecked = true;
                }

                return isAvailable;
            }
        }

        private int _vertexBufferId = -1, _count = 0;

        /// <summary>
        /// Gets the <see cref="VertexBufferRenderer"/> Handle name.
        /// </summary>
        public int Handle
        {
            get { return _vertexBufferId; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexBufferRenderer"/> class.
        /// </summary>
        public VertexBufferRenderer()
            : this(new VertexArray(PrimitiveType.Triangles, 3))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexBufferRenderer"/> class
        /// with specified vertices.
        /// </summary>
        /// <param name="vertices">The <see cref="VertexArray"/> to be upload into buffer.</param>
        public VertexBufferRenderer(VertexArray vertices)
        {
            if (!IsAvailable)
                throw new NotSupportedException("The System doesn't support Vertex Buffer Object.");

            Create();
            Update(vertices);
        }

        /// <summary>
        /// Generate Implementation Handle.
        /// This function will do nothing if VertexBufferObject is NOT supported or not preferred.
        /// </summary>
        public void Create()
        {
            // Check whether the system support VBO
            if (!IsAvailable)
                throw new NotSupportedException("The System doesn't support Vertex Buffer Object.");

            // Engine preference
            bool overrideHandle = true;

            // Check whether the handle is exist, and delete it if asked to do so.
            if (_vertexBufferId > 0 && overrideHandle)
                GLChecker.Check(() => GL.DeleteBuffer(_vertexBufferId));
            // .. and throw the exception if its not asked to delete existing buffer.
            else if (_vertexBufferId > 0 && !overrideHandle)
                throw new InvalidOperationException("Vertex Buffer Handle is already exist.");

            // Generate the handle
            GLChecker.Check(() => GL.GenBuffers(1, out _vertexBufferId));
        }


        /// <summary>
        /// Upload the Vertices into Buffer Data.
        /// </summary>
        public void Update(Vertex[] vertices)
        {
            // Check whether the system support VBO
            if (!IsAvailable)
                throw new NotSupportedException("The System doesn't support Vertex Buffer Object.");

            // Check whether the handles are valid.
            if (_vertexBufferId <= 0)
                Create();

            // Setup vertices information
            _count = vertices.Length;

            // Bind and Upload Vertices
            GLChecker.Check(() => GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferId));
            GLChecker.Check(() =>
                GL.BufferData(BufferTarget.ArrayBuffer,
                    new IntPtr(vertices.Length * Vertex.Stride),
                    vertices,
                    BufferUsageHint.StaticDraw)
            );

            // Set the Vertices Pointers
            GLChecker.Check(() => GL.VertexPointer(2, VertexPointerType.Float, Vertex.Stride, 0));
            GLChecker.Check(() => GL.TexCoordPointer(2, TexCoordPointerType.Float, Vertex.Stride, Vector2.Stride));
            GLChecker.Check(() => GL.ColorPointer(4, ColorPointerType.UnsignedByte, Vertex.Stride, Vector2.Stride + Vector2.Stride));
        }

        /// <summary>
        /// Render the vertices using current implementation.
        /// </summary>
        public void Render(PrimitiveType type)
        {
            // Check whether the system support VBO
            if (!IsAvailable)
                throw new NotSupportedException("The System doesn't support Vertex Buffer Object.");

            // Check whether the handles are valid.
            if (_vertexBufferId <= 0)
                throw new AccessViolationException("The Implementation Handle is empty or invalid.");

            // Bind the Vertex Buffer handle.
            GLChecker.Check(() => GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferId));

            // Draw the Vertices
            GLChecker.Check(() =>
                GL.DrawArrays((OpenTK.Graphics.OpenGL.PrimitiveType)type, 0, _count)
            );
        }

        /// <summary>
        /// Releases all resources used by the <see cref="VertexBufferRenderer"/>.
        /// </summary>
        public void Dispose()
        {
            if (_vertexBufferId > 0)
                GLChecker.Check(() => GL.DeleteBuffer(_vertexBufferId));
        }
    }
}
