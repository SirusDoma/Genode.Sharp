using System;
using System.Collections.Generic;
using System.Text;

namespace Genode.Graphics
{
    /// <summary>
    /// Represents event data for <see cref="Shader"/> events.
    /// </summary>
    public class ShaderEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="Shader"/> operation is executed successfully.
        /// </summary>
        public bool IsSuccess
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the status code of shader compilation.
        /// </summary>
        public int StatusCode
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the message of the corresponding event.
        /// </summary>
        public string Message
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderEventArgs"/> class.
        /// </summary>
        public ShaderEventArgs()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderEventArgs"/> class
        /// with specified compilation information.
        /// </summary>
        /// <param name="message">Compilation Log.</param>
        /// <param name="statusCode">Compilation Status Code.</param>
        /// <param name="success"><code>true</code>, if compilation successful, otherwise false.</param>
        public ShaderEventArgs(string message, int statusCode, bool success)
        {
            Message = message;
            StatusCode = statusCode;
            IsSuccess = success;
        }
    }
}
