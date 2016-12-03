using System;
using System.Collections.Generic;
using System.Text;

namespace Genode.Window
{
    /// <summary>
    /// Provides data for the <see cref="Genode.Window.RenderWindow"/> Update events.
    /// </summary>
    public class UpdateFrameEventArgs : FrameEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateFrameEventArgs"/>
        /// with specified delta time.
        /// </summary>
        /// <param name="time">The delta time of current frame.</param>
        public UpdateFrameEventArgs(double time)
            : base(time)
        {
        }
    }
}
