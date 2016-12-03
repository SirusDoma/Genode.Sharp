using System;
using System.Collections.Generic;
using System.Text;

namespace Genode.Graphics
{
    /// <summary>
    /// Represents type of <see cref="Shader"/>.
    /// </summary>
    public enum ShaderType
    {
        /// <summary>
        /// Vertex Shader.
        /// </summary>
        Vertex = 1,

        /// <summary>
        /// Geometry Shader.
        /// </summary>
        Geometry = 2,

        /// <summary>
        /// Fragment (Pixel) Shader Source.
        /// </summary>
        Fragment = 3
    }
}
