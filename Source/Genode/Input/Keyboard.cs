using System;
using System.Collections.Generic;
using System.Text;

namespace Genode.Input
{
    /// <summary>
    /// Represents a keyboard device.
    /// </summary>
    public partial class Keyboard
    {
        private OpenTK.Input.KeyboardState _state;

        /// <summary>
        /// Gets a value indicating whether the current of <see cref="Keyboard"/> device is connected.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _state.IsConnected;
            }
        }

        /// <summary>
        /// Gets the main keyboard device.
        /// </summary>
        /// <returns>The main keyboard device.</returns>
        public static Keyboard GetState()
        {
            return new Keyboard();
        }

        /// <summary>
        /// Gets the state of specified keyboard device.
        /// </summary>
        /// <param name="index">Index of the keyboard device.</param>
        /// <returns>The state of the specified keyboard.</returns>
        public static Keyboard GetState(int index)
        {
            return new Keyboard(OpenTK.Input.Keyboard.GetState(index));
        }

        internal Keyboard(OpenTK.Input.KeyboardState device)
        {
            _state = device;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Keyboard"/> class.
        /// </summary>
        public Keyboard()
            : this(OpenTK.Input.Keyboard.GetState())
        {
        }

        /// <summary>
        /// Gets a value indicating whether the specified keyboard key is pressed on current frame.
        /// </summary>
        /// <param name="key">The keyboard key to check.</param>
        /// <returns><code>true</code> if pressed, otherwise false.</returns>
        public bool IsKeyDown(Key key)
        {
            return _state.IsKeyDown((OpenTK.Input.Key)key);
        }

        /// <summary>
        /// Gets a value indicating whether the specified keyboard key is released on current frame.
        /// </summary>
        /// <param name="key">The keyboard key to check.</param>
        /// <returns><code>true</code> if released, otherwise false.</returns>
        public bool IsKeyUp(Key key)
        {
            return _state.IsKeyUp((OpenTK.Input.Key)key);
        }
    }
}
