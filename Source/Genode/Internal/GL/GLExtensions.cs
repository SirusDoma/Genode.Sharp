using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Genode.Internal.OpenGL
{
    /// <summary>
    /// Represents a list of OpenGL Extensions that could processed by the hardware.
    /// </summary>
    public sealed class GLExtensions
    {
        internal static List<string> _extensions = null;
        internal static List<string> _ext
        {
            get
            {
                if (_extensions == null)
                    _extensions = new List<string>(GL.GetString(StringName.Extensions).Split(' '));
                return _extensions;
            }
        }

        /// <summary>
        /// Check availability of the OpenGL extension in current hardware.
        /// </summary>
        /// <param name="ext">Extension name to check.</param>
        /// <returns>Return true if available, otherwise false.</returns>
        public static bool IsAvailable(string ext)
        {
            return _ext.Contains(ext);
        }

        /// <summary>
        /// Get all available extensions in current hardware.
        /// </summary>
        /// <returns>An array of string that contains available extensions.</returns>
        public static string[] GetExtensions()
        {
            return _ext.ToArray();
        }
    }
}
