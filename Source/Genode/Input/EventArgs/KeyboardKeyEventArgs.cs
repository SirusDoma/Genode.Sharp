using System;
using System.Collections.Generic;
using System.Text;

using Genode;
using Genode.Entities;
using Genode.Graphics;

namespace Genode.Input
{
    /// <summary>
    /// Provides data for the <see cref="IInputable"/>  KeyUp and KeyDown events.
    /// </summary>
    public class KeyboardKeyEventArgs : EventArgs
    {
        private OpenTK.Input.KeyboardState _state;
        private bool _isRepeat;

        /// <summary>
        /// Gets the keyboard key associated with the event.
        /// </summary>
        public Keyboard.Key KeyCode { get; private set; }

        /// <summary>
        /// Gets a value that indicates whether the keyboard key referenced by the event is a repeated key. 
        /// </summary>
        public bool IsRepeat
        {
            get { return _isRepeat; }
        }

        /// <summary>
        /// Gets a value that indicates whether the Alt key referenced by the event is hold.
        /// </summary>
        public bool Alt
        {
            get
            {
                return (!_state[OpenTK.Input.Key.AltLeft] || _state[OpenTK.Input.Key.AltRight]);
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the Control key referenced by the event is hold.
        /// </summary>
        public bool Control
        {
            get
            {
                return (!_state[OpenTK.Input.Key.ControlLeft] || _state[OpenTK.Input.Key.ControlRight]);
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the Shift key referenced by the event is hold.
        /// </summary>
        public bool Shift
        {
            get
            {
                return (!_state[OpenTK.Input.Key.ShiftLeft] || _state[OpenTK.Input.Key.ShiftRight]);
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the System key referenced by the event is hold.
        /// </summary>
        public bool System
        {
            get
            {
                return (!_state[OpenTK.Input.Key.WinLeft] || _state[OpenTK.Input.Key.WinRight]);
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the keyboard key modifier referenced by the event is a hold. 
        /// </summary>
        public Keyboard.Modifiers Modifiers
        {
            get
            {
                Keyboard.Modifiers mods = 0;
                mods |= Alt ? Keyboard.Modifiers.Alt : 0;
                mods |= Control ? Keyboard.Modifiers.Control : 0;
                mods |= System ? Keyboard.Modifiers.System : 0;
                mods |= Shift ? Keyboard.Modifiers.Shift : 0;
                return mods;
            }
        }

        /// <summary>
        /// Gets the Scan Code of the keyboard key associated with the event.
        /// </summary>
        public uint ScanCode
        {
            get
            {
                return (uint)KeyCode;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="KeyboardKeyEventArgs"/> class.
        /// </summary>
        /// <param name="keyCode">The <see cref="Keyboard.Key"/> corresponding to the key the user input.</param>
        public KeyboardKeyEventArgs(Keyboard.Key keyCode)
            : base()
        {
            KeyCode = keyCode;
            _state = OpenTK.Input.Keyboard.GetState();
            _isRepeat = false;
        }

        internal KeyboardKeyEventArgs(Keyboard.Key keyCode, OpenTK.Input.KeyboardState state, bool isRepeat)
        {
            KeyCode = keyCode;
            _state = state;
            _isRepeat = isRepeat;
        }
    }
}
