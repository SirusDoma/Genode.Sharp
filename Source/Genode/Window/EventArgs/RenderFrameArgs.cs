using System;
using System.Collections.Generic;
using System.Text;

using Genode;
using Genode.Graphics;

namespace Genode.Window
{
    /// <summary>
    /// Provides data for the <see cref="Genode.Window.RenderWindow"/> Render events.
    /// </summary>
    public class RenderFrameEventArgs : FrameEventArgs
    {
        private RenderStates _states;

        /// <summary>
        /// Gets the rendering states of current frame.
        /// </summary>
        public RenderStates States
        {
            get { return _states; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderFrameEventArgs"/> class
        /// with specified delta time.
        /// </summary>
        /// <param name="time">The delta time of current frame.</param>
        public RenderFrameEventArgs(double time)
            : base(time)
        {
            _states = new RenderStates(time);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderFrameEventArgs"/> class
        /// with specified delta time and render states.
        /// </summary>
        /// <param name="time">The delta time of current frame.</param>
        /// <param name="states">The <see cref="RenderStates"/> of current frame.</param>
        public RenderFrameEventArgs(double time, RenderStates states)
            : base(time)
        {
            _states = new RenderStates(time, states);
        }
    }
}
