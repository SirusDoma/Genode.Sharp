using System;
using System.Collections.Generic;
using System.Text;

using Genode;
using Genode.Entities;
using Genode.Graphics;

namespace Genode.Input
{
    /// <summary>
    /// Provides data for <see cref="IInputable"/> MouseWheel event.
    /// </summary>
    public class MouseWheelEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the value of the wheel in floating point units.
        /// </summary>
        public float Value
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the change in value of the wheel in floating point units.
        /// </summary>
        public float Delta
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the position of the mouse.
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

        internal MouseWheelEventArgs(OpenTK.Input.MouseWheelEventArgs e)
        {
            Value = e.ValuePrecise;
            Delta = e.DeltaPrecise;
            Position = new Vector2(e.X, e.Y);
            Mouse = new Mouse(e.Mouse);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseWheelEventArgs"/>
        /// with specified wheel value, delta wheel value and position of the mouse.
        /// </summary>
        /// <param name="value">The value of the wheel.</param>
        /// <param name="delta">The change in value of the wheel.</param>
        /// <param name="position">The position of the mouse.</param>
        public MouseWheelEventArgs(float value, float delta, Vector2 position)
        {
            Value = value;
            Delta = delta;
            Position = position;
            Mouse = Mouse.GetState();
        }
    }
}
