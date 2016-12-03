using System;
using System.Collections.Generic;
using System.Text;

namespace Genode.Graphics
{
    /// <summary>
    /// Represents an implementation to render an array of <see cref="Vertex"/>.
    /// </summary>
    public interface IRendererImplementation : IDisposable
    {
        /// <summary>
        /// Update the implementation with specified vertices.
        /// </summary>
        /// <param name="vertices">The vertices that will be uploaded.</param>
        void Update(Vertex[] vertices);

        /// <summary>
        /// Render the vertices using current implementation.
        /// </summary>
        /// <param name="type">Primitive type of the specified vertices.</param>
        void Render(PrimitiveType type);
    }
}
