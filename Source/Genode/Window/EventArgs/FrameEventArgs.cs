using System;
using System.Collections.Generic;
using System.Text;

namespace Genode
{
    /// <summary>
    /// Represent the base class for frame event data.
    /// </summary>
    public class FrameEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the delta time of the current frame.
        /// </summary>
        public double Time
        {
            get; private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameEventArgs"/> class.
        /// </summary>
        /// <param name="time">The delta time of current frame.</param>
        public FrameEventArgs(double time)
        {
            Time = time;
        }
    }
}
