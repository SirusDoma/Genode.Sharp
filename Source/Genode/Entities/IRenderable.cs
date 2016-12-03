using System;

using Genode;
using Genode.Graphics;

namespace Genode.Entities
{
    /// <summary>
    /// Represents a Renderable object.
    /// </summary>
    public interface IRenderable
    {
        /// <summary>
        /// Render the current of <see cref="IRenderable"/> object with given render parameters.
        /// </summary>
        void Render(RenderTarget target, RenderStates states);
    }
}
