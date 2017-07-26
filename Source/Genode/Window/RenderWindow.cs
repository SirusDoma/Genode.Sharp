using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;

using Genode;
using Genode.Graphics;
using Genode.Entities;
using Genode.Input;
using Genode.Internal.OpenGL;

namespace Genode.Window
{
    /// <summary>
    /// Represents a window that can serve as a target for 2D drawing.
    /// </summary>
    public class RenderWindow : RenderTarget, IDisposable
    {
        private static List<RenderWindow> instances = new List<RenderWindow>();

        /// <summary>
        /// Gets the an array of <see cref="RenderWindow"/> instances.
        /// </summary>
        public static RenderWindow[] Instances
        {
            get
            {
                return instances.ToArray();
            }
        }

        /// <summary>
        /// Gets the <see cref="RenderWindow"/> that has active OpenGL Context in calling thread.
        /// </summary>
        public static RenderWindow Current
        {
            get
            {
                if (instances.Count > 1)
                {
                    foreach (RenderWindow window in instances)
                    {
                        if (window.GameWindow.Context.IsCurrent)
                        {
                            return window;
                        }
                    }

                    return null;
                }
                else if (instances.Count == 1)
                {
                    return instances[0];
                }
                else
                {
                    return null;
                }
            }
        }

        private Stopwatch _fpsTimer = new Stopwatch(), _deltaPollTimer = new Stopwatch();
        private bool _showFPS = true;
        private bool _isInitialized = false;
        private bool _isStarted = false;
        private bool _isEventPolled = false;
        private int _fps = 0;
        private long _frameLimit = 0;
        private TimeSpan _frameLimitSpan = TimeSpan.Zero;

        protected string _title = "Genode Game";
        protected bool _useVsync = true;

        internal GameWindow GameWindow;

        public event EventHandler<EventArgs> Load, Resize;
        public event TargetRenderEventHandler RenderFrame;
        public event TargetUpdateEventHandler UpdateFrame;

        public event KeyboardEventHandler KeyDown, KeyUp;
        public event KeyPressEventHandler KeyPress;

        public event MouseMoveEventHandler MouseMove;
        public event MouseButtonEventHandler MouseDown, MouseUp;
        public event MouseWheelEventHandler MouseWheel;

        public event EventHandler<EventArgs> TitleChanged;
        public event EventHandler<EventArgs> VisibleChanged;
        public event EventHandler<EventArgs> FocusChanged;
        public event EventHandler<EventArgs> IconChanged;
        public event EventHandler<EventArgs> Move;
        public event EventHandler<EventArgs> WindowBorderChanged;
        public event EventHandler<EventArgs> WindowStateChanged;
        public event EventHandler<System.ComponentModel.CancelEventArgs> Closing;
        public event EventHandler<EventArgs> Closed;
        public event EventHandler<EventArgs> Disposed;

        internal IWindowInfo iWnd
        {
            get
            {
                return GameWindow.WindowInfo;
            }
        }

        /// <summary>
        /// Gets the delta time of the current frame.
        /// </summary>
        public double Time
        {
            get; private set;
        }

        /// <summary>
        /// Gets the <see cref="RenderWindow"/> native handle.
        /// </summary>
        public IntPtr hWnd
        {
            get
            {
                return GameWindow.WindowInfo.Handle;
            }
        }

        /// <summary>
        /// Gets or sets the title of current <see cref="RenderWindow"/> object.
        /// </summary>
        public virtual string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                GameWindow.Title = _title;

                CheckWindowFPS(true);
            }
        }

        /// <summary>
        /// Gets or sets an Icon for current <see cref="RenderWindow"/> object.
        /// </summary>
        [Obsolete("Gets or sets the Icon property may cause crash on Linux / OSX.", false)]
        public Icon Icon
        {
            get
            {
                return GameWindow.Icon;
            }
            set
            {
                GameWindow.Icon = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the VSync should be enabled.
        /// </summary>
        public bool UseVSync
        {
            get
            {
                return _useVsync;
            }
            set
            {
                if (_useVsync == value)
                    return;

                _useVsync = value;
                GameWindow.VSync = (_useVsync ? VSyncMode.On : VSyncMode.Off);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="RenderWindow"/> has input focus.
        /// </summary>
        public bool Focused
        {
            get
            {
                return GameWindow.Focused;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="RenderWindow"/> is open and safe to use.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return GameWindow.Exists && !GameWindow.IsExiting;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the shutdown sequence has been initiated.
        /// </summary>
        public bool IsRequestClose
        {
            get
            {
                return GameWindow.IsExiting;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="RenderWindow"/> object is exist.
        /// </summary>
        public bool Exists
        {
            get
            {
                return GameWindow.Exists;
            }
        }

        /// <summary>
        /// Gets or sets the target render frequency.
        /// </summary>
        public long FrameLimit
        {
            get
            {
                return _frameLimit;
            }
            set
            {
                _frameLimit = value;
                _frameLimitSpan = TimeSpan.FromSeconds(1f / _frameLimit);

                GameWindow.TargetRenderFrequency = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of current <see cref="RenderWindow"/> object.
        /// </summary>
        public override Size Size
        {
            get
            {
                return GameWindow.ClientSize;
            }
        }

        /// <summary>
        /// Gets or sets width of current <see cref="RenderWindow"/> object.
        /// </summary>
        public int Width
        {
            get
            {
                return GameWindow.Width;
            }
            set
            {
                GameWindow.Width = value;
            }
        }

        /// <summary>
        /// Gets or sets height of current <see cref="RenderWindow"/> object.
        /// </summary>
        public int Height
        {
            get
            {
                return GameWindow.Height;
            }
            set
            {
                GameWindow.Height = value;
            }
        }

        /// <summary>
        /// Gets or sets the location of this window on the desktop.
        /// </summary>
        public Point Location
        {
            get
            {
                return GameWindow.Location;
            }
            set
            {
                GameWindow.Location = value;
            }
        }

        /// <summary>
        /// Gets or sets the horizontal location of this window in the screen coordinates.
        /// </summary>
        public int X
        {
            get
            {
                return GameWindow.X;
            }
            set
            {
                GameWindow.X = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertical location of this window in the screen coordinates.
        /// </summary>
        public int Y
        {
            get
            {
                return GameWindow.Y;
            }
            set
            {
                GameWindow.Y = value;
            }
        }

        /// <summary>
        /// Gets or sets the bounds of the OpenGL surface, in window coordinates.
        /// </summary>
        public Rectangle ClientRectangle
        {
            get
            {
                var rect = GameWindow.ClientRectangle;
                return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
            }
            set
            {
                GameWindow.ClientRectangle = new Rectangle(value.X, value.Y,
                    value.Width, value.Height);
            }
        }

        /// <summary>
        /// Gets or sets the border of the current <see cref="RenderWindow"/> object.
        /// </summary>
        public BorderStyle Border
        {
            get
            {
                return (BorderStyle)GameWindow.WindowBorder;
            }
            set
            {
                GameWindow.WindowBorder = (WindowBorder)value;
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether the FPS counter should be displayed in title bar.
        /// </summary>
        public bool ShowFPS
        {
            get
            {
                return _showFPS;
            }
            set
            {
                if (_showFPS == value)
                    return;

                _showFPS = value;
                Title = _title;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the system mouse cursor should be visible.
        /// </summary>
        public bool ShowCursor
        {
            get
            {
                return GameWindow.CursorVisible;
            }
            set
            {
                GameWindow.CursorVisible = value;
            }
        }

        /// <summary>
        /// Gets the frame per second that achieved by the current <see cref="RenderWindow"/> object.
        /// </summary>
        public int FPS
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderWindow"/> class.
        /// </summary>
        public RenderWindow()
            : this(800, 600, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderWindow"/> class
        /// with specified size and window state.
        /// </summary>
        /// <param name="width">The width of the window.</param>
        /// <param name="height">The height of the window.</param>
        /// <param name="fullScreen">A value indicating whether the window should be fullscreen mode.</param>
        public RenderWindow(int width, int height, bool fullScreen = false)
            : this(width, height, "Game", fullScreen)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderWindow"/> class
        /// with specified size, title and window state.
        /// </summary>
        /// <param name="width">The width of the window.</param>
        /// <param name="height">The height of the window.</param>
        /// <param name="title">The title of the window.</param>
        /// <param name="fullScreen">A value indicating whether the window should be fullscreen mode.</param>
        public RenderWindow(int width, int height, string title, bool fullScreen = false)
        {
            // Resolve DPI Scaling on Windows
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
                {
                    // Check whether the DPI Scaling is greater than 100%
                    // 96f = 100% DPI Scaling
                    if (graphics.DpiX > 96f || graphics.DpiY > 96f)
                    {
                        Logger.Warning("DPI Scaling detected.\n" +
                            "Engine will automatically normalizing the DPI scaling so it wont affect the size of window.");

                        width  = (int)((width / DpiCalc.FromDPI(graphics.DpiX)) * 100f);
                        height = (int)((height / DpiCalc.FromDPI(graphics.DpiY)) * 100f);
                    }
                }
            }

            _title = title;
            GameWindow = new GameWindow(
                width,
                height,
                GraphicsMode.Default,
                _title,
                (fullScreen ? GameWindowFlags.Fullscreen : GameWindowFlags.FixedWindow)
            );

            instances.Add(this);
        }

        /// <summary>
        /// Performs the common initialization steps.
        /// </summary>
        protected override void Initialize()
        {
            UseVSync = false;
            base.Initialize();
        }

        /// <summary>
        /// Activate the current <see cref="RenderWindow"/> as target for rendering.
        /// This will makes the corresponding OpenGL Context being current in calling thread.
        /// </summary>
        /// <returns><code>true</code> if the context is activated, otherwise false.</returns>
        protected override bool Activate()
        {
            if (!GameWindow.Context.IsCurrent)
                GameWindow.MakeCurrent();
            return GameWindow.Context.IsCurrent;
        }

        private void CheckWindowFPS(bool forceRefresh = false)
        {
            if (forceRefresh || _fpsTimer.ElapsedMilliseconds >= 1000)
            {
                
                if (_showFPS && _fps > 0)
                {
                    GameWindow.Title = _title;
                    GameWindow.Title += " [FPS: " + _fps.ToString() + "]";
                }
            }

            if (_fpsTimer.ElapsedMilliseconds >= 1000)
            {
                FPS = _fps;

                _fps = 0;
                _fpsTimer.Reset();
            }
        }

        private void WireEvents()
        {
            GameWindow.TitleChanged            += OnInternalTitleChanged;
            GameWindow.VisibleChanged          += OnInternalVisibleChanged;
            GameWindow.FocusedChanged          += OnInternalFocusChanged;
            GameWindow.IconChanged             += OnInternalIconChanged;
            GameWindow.Move                    += OnInternalMove;
            GameWindow.WindowBorderChanged     += OnInternalWindowBorderChanged;
            GameWindow.WindowStateChanged      += OnInternalWindowStateChanged;

            GameWindow.MouseMove               += OnInternalMouseMove;
            GameWindow.MouseDown               += OnInternalMouseDown;
            GameWindow.MouseUp                 += OnInternalMouseUp;
            GameWindow.MouseWheel              += OnInternalMouseWheel;

            GameWindow.KeyPress                += OnInternalKeyPress;
            GameWindow.KeyDown                 += OnInternalKeyDown;
            GameWindow.KeyUp                   += OnInternalKeyUp;
            
            GameWindow.Load                    += OnInternalLoad;
            GameWindow.Resize                  += OnInternalResize;
            GameWindow.RenderFrame             += OnInternalRender;
            GameWindow.UpdateFrame             += OnInternalUpdate;

            GameWindow.Closing                 += OnInternalClosing;
            GameWindow.Closed                  += OnInternalClosed;
            GameWindow.Disposed                += OnInternalDisposed;
        }

        private void OnInternalLoad(object sender, EventArgs e)
        {
            Initialize();

            OnLoad(this, e);
            Load?.Invoke(this, e);

            _isInitialized = true;
        }

        private void OnInternalResize(object sender, EventArgs e)
        {
            // Refresh current view
            View = View;

            if (!_isStarted)
            {
                GameWindow.Context.Update(GameWindow.WindowInfo);
            }

            OnResize(this, e);
            Resize?.Invoke(this, e);
        }

        private void OnInternalRender(object sender, OpenTK.FrameEventArgs e)
        {
            if (!_fpsTimer.IsRunning)
            {
                _fpsTimer.Start();
            }

            RenderFrameEventArgs args = new RenderFrameEventArgs(e.Time * 1000D);

            OnRenderFrame(this, args);
            RenderFrame?.Invoke(this, args);

            if (!_isEventPolled && _isStarted)
            {
                _fps++;
                CheckWindowFPS();
            }
        }

        private void OnInternalUpdate(object sender, OpenTK.FrameEventArgs e)
        {
            UpdateFrameEventArgs args = new UpdateFrameEventArgs(e.Time * 1000D);
            Time = args.Time;

            OnUpdateFrame(this, args);
            UpdateFrame?.Invoke(this, args);
        }

        private void OnInternalKeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            var args = new KeyboardKeyEventArgs((Keyboard.Key)e.Key, GameWindow.Keyboard.GetState(), e.IsRepeat);

            if ((e.Keyboard.IsKeyDown(OpenTK.Input.Key.AltLeft) || e.Keyboard.IsKeyDown(OpenTK.Input.Key.AltRight)) && e.Keyboard.IsKeyDown(OpenTK.Input.Key.F4))
            {
                Close();
            }

            OnKeyDown(this, args);
            KeyDown?.Invoke(this, args);
        }

        private void OnInternalKeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            var args = new KeyboardKeyEventArgs((Keyboard.Key)e.Key, GameWindow.Keyboard.GetState(), e.IsRepeat);

            OnKeyUp(this, args);
            KeyUp?.Invoke(this, args);
        }

        private void OnInternalKeyPress(object sender, OpenTK.KeyPressEventArgs e)
        {
            var args = new KeyboardPressEventArgs(e.KeyChar);

            OnKeyPress(this, args);
            KeyPress?.Invoke(this, args);
        }

        private void OnInternalMouseMove(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            var args = new MouseMoveEventArgs(e);

            OnMouseMove(this, args);
            MouseMove?.Invoke(this, args);
        }

        private void OnInternalMouseDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            var args = new MouseButtonEventArgs(e);

            OnMouseDown(this, args);
            MouseDown?.Invoke(this, args);
        }

        private void OnInternalMouseUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            var args = new MouseButtonEventArgs(e);

            OnMouseUp(this, args);
            MouseUp?.Invoke(this, args);
        }

        private void OnInternalMouseWheel(object sender, OpenTK.Input.MouseWheelEventArgs e)
        {
            var args = new MouseWheelEventArgs(e);

            OnMouseWheel(this, args);
            MouseWheel?.Invoke(this, args);
        }

        private void OnInternalTitleChanged(object sender, EventArgs e)
        {
            OnTitleChanged(this, e);
            TitleChanged?.Invoke(this, e);
        }

        private void OnInternalVisibleChanged(object sender, EventArgs e)
        {
            OnVisibleChanged(this, e);
            VisibleChanged?.Invoke(this, e);
        }

        private void OnInternalFocusChanged(object sender, EventArgs e)
        {
            OnFocusChanged(this, e);
            FocusChanged?.Invoke(this, e);
        }

        private void OnInternalIconChanged(object sender, EventArgs e)
        {
            OnIconChanged(this, e);
            IconChanged?.Invoke(this, e);
        }

        private void OnInternalMove(object sender, EventArgs e)
        {
            OnMove(this, e);
            Move?.Invoke(this, e);
        }

        private void OnInternalWindowBorderChanged(object sender, EventArgs e)
        {
            OnWindowBorderChanged(this, e);
            WindowBorderChanged?.Invoke(this, e);
        }

        private void OnInternalWindowStateChanged(object sender, EventArgs e)
        {
            OnWindowStateChanged(this, e);
            WindowStateChanged?.Invoke(this, e);
        }

        private void OnInternalClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OnClosing(this, e);
            Closing?.Invoke(this, e);
        }

        private void OnInternalClosed(object sender, EventArgs e)
        {
            OnClosed(this, e);
            Closed?.Invoke(this, e);
        }

        private void OnInternalDisposed(object sender, EventArgs e)
        {
            OnDisposed(this, e);
            Disposed?.Invoke(this, e);
        }

        /// <summary>
        /// Called before the window is displayed for the first time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnLoad(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Called whenever the window is resized.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnResize(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Called when it is time to render the frame.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        protected virtual void OnRenderFrame(RenderTarget target, RenderFrameEventArgs e)
        {
        }

        /// <summary>
        /// Called when it is time to update the frame.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnUpdateFrame(object sender, UpdateFrameEventArgs e)
        {
        }

        /// <summary>
        /// Called whenever the keyboard key is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
        }

        /// <summary>
        /// Called whenever the keyboard key is released.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
        }

        /// <summary>
        /// Called whenever character is typed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnKeyPress(object sender, KeyboardPressEventArgs e)
        {
        }

        /// <summary>
        /// Called whenever the mouse is moved.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
        }

        /// <summary>
        /// Called whenever the mouse button is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// Called whenever the mouse button is released.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// Called whenever the mouse wheel is scrolled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
        }

        /// <summary>
        /// Called whenever the title of the window is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnTitleChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Called whenever the visibility of the window is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnVisibleChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Called whenever the window focus state is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnFocusChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Called whenever the icon of window is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnIconChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Called whenever the window location is moved.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnMove(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Called whenever the window border is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnWindowBorderChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Called whenever the window state is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnWindowStateChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Called when the window about to close.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        /// <summary>
        /// Called when the window is closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnClosed(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Called when the window is disposed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnDisposed(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Start the current <see cref="RenderWindow"/> object.
        /// </summary>
        public virtual void Start()
        {
            Start(0, 0);
        }

        /// <summary>
        /// Start the current <see cref="RenderWindow"/> object.
        /// with limited update rate.
        /// </summary>
        /// <param name="updateRate">The update frequency.</param>
        public virtual void Start(double updateRate)
        {
            Start(updateRate, 0);
        }

        /// <summary>
        /// Start the current <see cref="RenderWindow"/> object.
        /// with limited update rate and frame per second.
        /// </summary>
        /// <param name="updateRate">The update frequency.</param>
        /// <param name="framePerSecond">The limited value of frame per second.</param>
        public virtual void Start(double updateRate, double framePerSecond)
        {
            WireEvents();

            Title = _title;
            _isStarted = true;
            GameWindow.Run(updateRate, framePerSecond);
        }

        /// <summary>
        /// Process pending event of the current <see cref="RenderWindow"/> object,
        /// such as input system and rendering process.
        /// </summary>
        public void ProcessEvents()
        {
            if (!_isInitialized)
            {
                OnInternalLoad(this, EventArgs.Empty);
                GameWindow.Visible = true;
            }

            if (!_fpsTimer.IsRunning)
            {
                _fpsTimer.Start();
            }

            if (!_deltaPollTimer.IsRunning)
            {
                _deltaPollTimer.Start();
            }

            GameWindow.ProcessEvents();
            _isEventPolled = true;
        }

        /// <summary>
        /// Swap the buffer of the <see cref="RenderWindow"/>.
        /// Presenting all rendered objects to the surface of current <see cref="RenderWindow"/> object.
        /// </summary>
        public override void Display()
        {
            if (!_isEventPolled && !_isStarted)
            {
                throw new InvalidOperationException("Failed to swap the front and back buffer. " +
                    "The window is not yet running or window event is not polled yet in current frame.");
            }

            if (!IsOpen)
            {
                return;
            }

            GameWindow.SwapBuffers();
            if (_isEventPolled && _deltaPollTimer.IsRunning)
            {
                Time = _deltaPollTimer.ElapsedMilliseconds;

                _deltaPollTimer.Reset();
                _deltaPollTimer.Start();

                if (FrameLimit > 0)
                {
                    Thread.Sleep(((int)_frameLimitSpan.TotalMilliseconds / 2) - (int)_deltaPollTimer.ElapsedMilliseconds);
                }
            }

            if (_isEventPolled && !_isStarted)
            {
                _fps++;
                CheckWindowFPS();
            }

            _isEventPolled = false;
        }

        /// <summary>
        /// Makes the OpenGL Context that associated with current <see cref="RenderWindow"/> object current on calling thread.
        /// </summary>
        public void MakeCurrent()
        {
            Activate();
        }

        /// <summary>
        /// Closes the current <see cref="RenderWindow"/> object.
        /// </summary>
        public void Close()
        {
            GameWindow.Close();
        }

        /// <summary>
        /// Releases all resources used by the <see cref="RenderWindow"/>.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            GameWindow.Dispose();
            instances.Remove(this);

            // Dispose audio device in case it is not disposed yet
            if (instances.Count == 0)
            {
                Genode.Audio.AudioDevice.Dispose();
            }
        }
    }
}
