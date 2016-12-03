using System;
using System.Collections.Generic;
using System.Text;

using Genode;
using Genode.Entities;

namespace Genode.Input
{

    /// <summary>
    /// Provides data for the <see cref="IInputable"/> KeyPress event.
    /// </summary>
    public class KeyboardPressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the character corresponding to the key pressed.
        /// </summary>
        public char KeyChar { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardPressEventArgs"/> class.
        /// </summary>
        /// <param name="keyChar">The ASCII character corresponding to the key the user pressed.</param>
        public KeyboardPressEventArgs(char keyChar)
            : base()
        {
            KeyChar = keyChar;
        }

        internal KeyboardPressEventArgs(OpenTK.KeyPressEventArgs e)
        {
            KeyChar = e.KeyChar;
        }
    }
}
