using System;
using System.Collections.Generic;
using System.Text;

using Genode;
using Genode.Entities;
using Genode.Graphics;

namespace Genode.Input
{
    /// <summary>
    /// Provides data for <see cref="IInputable"/> MouseMove event.
    /// </summary>
    public class MouseMoveEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the previous mouse position.
        /// </summary>
        public Vector2 Delta
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the mouse position.
        /// </summary>
        public Vector2 Position
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current state of the <see cref="Genode.Input.Mouse"/> input.
        /// </summary>
        public Mouse Mouse
        {
            get;
            private set;
        }

        internal MouseMoveEventArgs(OpenTK.Input.MouseMoveEventArgs e)
        {
            Position = new Vector2(e.X, e.Y);
            Delta = new Vector2(e.XDelta, e.YDelta);
            Mouse = new Input.Mouse(e.Mouse);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseMoveEventArgs"/>
        /// with specified position and delta position.
        /// </summary>
        /// <param name="position">The position of mouse.</param>
        /// <param name="delta">The delta position of mouse.</param>
        public MouseMoveEventArgs(Vector2 position, Vector2 delta)
        {
            Position = position;
            Delta = delta;
            Mouse = Mouse.GetState();
        }
    }
}
