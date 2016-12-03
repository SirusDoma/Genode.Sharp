using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;

namespace Genode.Window
{
    /// <summary>
    /// Represents OpenGL Context.
    /// </summary>
    public class Context : IDisposable
    {
        private static Context _instance;

        /// <summary>
        /// Gets the <see cref="Context"/> that is current in the calling thread.
        /// </summary>
        public static Context Current
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Context(GraphicsContext.CurrentContext as GraphicsContext);
                }

                return _instance;
            }
        }

        private GraphicsContext _context;
        private object _windowInfo;

        /// <summary>
        /// Gets a value indicating whether the <see cref="Context"/> object is current in the calling thread.
        /// </summary>
        public bool IsCurrent
        {
            get { return _context.IsCurrent; }
        }

        /// <summary>
        /// Gets or sets whether the VSync should be enabled.
        /// </summary>
        public bool VSync
        {
            get { return _context.SwapInterval == 1; }
            set {
                if (value)
                    _context.SwapInterval = 1;
                else
                    _context.SwapInterval = 0;
            }
        }


        static Context()
        {
            GraphicsContext.ShareContexts = true;
        }

        private Context()
        {
        }

        internal Context(IntPtr handle, object windowInfo)
        {
            _windowInfo = windowInfo;
            _context = new GraphicsContext(new ContextHandle(handle), _windowInfo as IWindowInfo);
        }

        internal Context(GraphicsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Load all OpenGL entry points.
        /// </summary>
        public void Load()
        {
            _context.LoadAll();
        }

        /// <summary>
        /// Update the current <see cref="Context"/> object.
        /// It is necessary to call this method when the size of <see cref="RenderTarget"/> or <see cref="RenderWindow"/> is changed.
        /// </summary>
        public void Update()
        {
            _context.Update(_windowInfo as IWindowInfo);
        }

        /// <summary>
        /// Makes the <see cref="Context"/> to current rendering target.
        /// </summary>
        /// <returns><code>true</code> if the current <see cref="Context"/> object is current in calling thread, otherwise false.</returns>
        public bool Activate()
        {
            _context.MakeCurrent(_windowInfo as IWindowInfo);
            return _context.IsCurrent;
        }

        /// <summary>
        /// Swap the buffers, presenting rendered objects.
        /// </summary>
        public void Display()
        {
            _context.SwapBuffers();
        }

        /// <summary>
        /// Releases all resources used by <see cref="Context"/>.
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
