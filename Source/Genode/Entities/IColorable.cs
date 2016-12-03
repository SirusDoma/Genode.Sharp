using System;
using System.Drawing;

namespace Genode.Entities
{
    /// <summary>
    /// Represents an object that has <see cref="System.Drawing.Color"/>.
    /// </summary>
    public interface IColorable
    {
        /// <summary>
        /// Gets or sets the <see cref="System.Drawing.Color"/> of the current <see cref="IColorable"/> object.
        /// </summary>
        Color Color { get; set; }
    }
}
