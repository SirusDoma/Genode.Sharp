using System;
using System.Collections.Generic;
using System.Text;

using Genode;
using Genode.Entities;
using Genode.Graphics;

namespace Genode.Input
{
    /// <summary>
    /// Provides data for <see cref="IInputable"/> MouseDown and MouseUp event.
    /// </summary>
    public class MouseButtonEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a value indicating whether the corresponding mouse button is pressed.
        /// </summary>
        public bool IsPressed
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets which mouse button was pressed.
        /// </summary>
        public Mouse.Button Button
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

        /// <summary>
        /// Gets the mouse position.
        /// </summary>
        public Vector2 Position
        {
            get;
            private set;
        }

        internal MouseButtonEventArgs(OpenTK.Input.MouseButtonEventArgs e)
        {
            Button = (Mouse.Button)e.Button;
            IsPressed = e.IsPressed;
            Position = new Vector2(e.X, e.Y);
            Mouse = new Mouse(e.Mouse);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseButtonEventArgs"/>
        /// with specified button, position and press state.
        /// </summary>
        /// <param name="button">The mouse button was pressed.</param>
        /// <param name="position">The position of mouse.</param>
        /// <param name="isPressed">The value indicating whether the corresponding mouse button is pressed.</param>
        public MouseButtonEventArgs(Mouse.Button button, Vector2 position, bool isPressed)
        {
            Button = button;
            IsPressed = isPressed;
            Position = position;
            Mouse = Mouse.GetState();
        }
    }
}
