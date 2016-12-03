using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Genode;
using Genode.Graphics;
using Genode.Window;

namespace Genode.Input
{
    /// <summary>
    /// Represents a mouse device.
    /// </summary>
    public partial class Mouse
    {
        private OpenTK.Input.MouseState _state;
        private RenderWindow _renderWindow;

        /// <summary>
        /// Gets a value indicating whether the current of <see cref="Mouse"/> device is connected.
        /// </summary>
        public bool IsConnected
        {
            get { return _state.IsConnected; }
        }

        /// <summary>
        /// Gets the mouse position.
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return _renderWindow != null ? 
                    new Vector2(_renderWindow.GameWindow.Mouse.X, _renderWindow.GameWindow.Mouse.Y) : 
                    new Vector2(_state.X, _state.Y);
            }
        }


        /// <summary>
        /// Gets a value of current state of the mouse scroll wheel.
        /// </summary>
        public Vector2 Scroll
        {
            get { return _state == null ? Vector2.Zero : new Vector2(_state.Scroll.X, _state.Scroll.Y); }
        }

        /// <summary>
        /// Gets the absolute wheel position in floating point units.
        /// </summary>
        public float Wheel
        {
            get
            {
                return _renderWindow != null ?
                    _renderWindow.GameWindow.Mouse.WheelPrecise :
                    _state.WheelPrecise;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the left button of the mouse is pressed.
        /// </summary>
        public bool LeftButton
        {
            get { return _state != null && _state.LeftButton == OpenTK.Input.ButtonState.Pressed; }
        }

        /// <summary>
        /// Gets a value indicating whether the right button of the mouse is pressed.
        /// </summary>
        public bool RightButton
        {
            get { return _state != null && _state.RightButton == OpenTK.Input.ButtonState.Pressed; }
        }

        /// <summary>
        /// Gets a value indicating whether the middle button of the mouse is pressed.
        /// </summary>
        public bool MiddleButton
        {
            get { return _state != null && _state.MiddleButton == OpenTK.Input.ButtonState.Pressed; }
        }

        /// <summary>
        /// Gets a value indicating whether the extra button 1 of the mouse is pressed.
        /// </summary>
        public bool XButton1
        {
            get { return _state != null && _state.XButton1 == OpenTK.Input.ButtonState.Pressed; }
        }

        /// <summary>
        /// Gets a value indicating whether the extra button 2 of the mouse is pressed.
        /// </summary>
        public bool XButton2
        {
            get { return _state != null && _state.XButton2 == OpenTK.Input.ButtonState.Pressed; }
        }

        /// <summary>
        /// Gets the current state of mouse device.
        /// </summary>
        /// <returns>The state of main mouse device.</returns>
        public static Mouse GetState()
        {
            return new Mouse(RenderWindow.Current);
        }

        /// <summary>
        /// Gets the state of specified mouse device.
        /// </summary>
        /// <param name="index">Index of the mouse device.</param>
        /// <returns>The state of the specified mouse.</returns>
        public static Mouse GetState(int index)
        {
            return new Mouse(OpenTK.Input.Mouse.GetState(index));
        }

        internal Mouse(OpenTK.Input.MouseState state)
        {
            _state = state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mouse"/> class.
        /// </summary>
        public Mouse()
            : this(RenderWindow.Current)
        {
            _renderWindow = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mouse"/> class
        /// that related to specified <see cref="RenderWindow"/>.
        /// </summary>
        /// <param name="window">The window.</param>
        public Mouse(RenderWindow window)
        {
            if (window != null)
            {
                _renderWindow = window;
                _state = window.GameWindow.Mouse.GetState();
            }
            else
            {
                _state = OpenTK.Input.Mouse.GetState();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mouse"/> class
        /// from specified mouse index.
        /// </summary>
        /// <param name="stateIndex">The index of the mouse.</param>
        public Mouse(int stateIndex)
        {
            _state = OpenTK.Input.Mouse.GetState(stateIndex);
        }


        /// <summary>
        /// Gets a value indicating whether the specified mouse button is pressed on the current frame..
        /// </summary>
        /// <param name="button">The mouse button to check.</param>
        /// <returns><code>true</code> if pressed, otherwise false.</returns>
        public bool IsButtonDown(Mouse.Button button)
        {
            return _state.IsButtonDown((OpenTK.Input.MouseButton)button);
        }

        /// <summary>
        /// Gets a value indicating whether the specified mouse button is released on the current frame..
        /// </summary>
        /// <param name="button">The mouse button to check.</param>
        /// <returns><code>true</code> if released, otherwise false.</returns>
        public bool IsButtonUp(Mouse.Button button)
        {
            return _state.IsButtonUp((OpenTK.Input.MouseButton)button);
        }

        /// <summary>
        /// Gets the position of the mouse.
        /// </summary>
        /// <param name="relativeTo">If specified, return mouse coordinate based on specified window.</param>
        /// <returns>The position of mouse.</returns>
        public static Vector2 GetPosition(RenderWindow relativeTo = null)
        {
            if (relativeTo != null)
            {
                return new Vector2(relativeTo.GameWindow.Mouse.X, relativeTo.GameWindow.Mouse.Y);
            }
            else
            {
                return new Vector2(OpenTK.Input.Mouse.GetCursorState().X, OpenTK.Input.Mouse.GetCursorState().Y);
            }
        }

        /// <summary>
        /// Sets the position of the mouse
        /// </summary>
        /// <param name="position">The new position of the mouse.</param>
        /// <param name="relativeTo">If specified, mouse coordinate format will be based on specified window.</param>
        public static void SetPosition(Vector2 position, RenderWindow relativeTo = null)
        {
            if (relativeTo != null)
            {
                var point = relativeTo.GameWindow.PointToScreen(new Point((int)position.X, (int)position.Y));
                OpenTK.Input.Mouse.SetPosition(point.X, point.Y);
            }
            else
            {
                OpenTK.Input.Mouse.SetPosition(position.X, position.Y);
            }
        }
    }
}
