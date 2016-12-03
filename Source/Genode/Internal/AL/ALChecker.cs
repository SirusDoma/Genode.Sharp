using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

using Genode;

namespace Genode.Internal.OpenAL
{
    /// <summary>
    /// Provides OpenGL checking functions.
    /// </summary>
    public static class ALChecker
    {
        private static bool _isManuallyChecked = true;

        /// <summary>
        /// Represents Verbose level of <see cref="GLChecker"/>.
        /// </summary>
        public enum VerboseFlags
        {
            /// <summary>
            /// Display all messages.
            /// </summary>
#if !DEBUG
            [Obsolete("Changing VerboseLevel to All will lead performance issue under Release Build Configurations.")]
#endif
            All = 0,

            /// <summary>
            /// Display Warning and Error messages only.
            /// </summary>
            Error = 1
        }

        /// <summary>
        /// Gets the last OpenGL Error that occurred.
        /// Any OpenGL call should be called through <see cref="ALChecker.Check(Action)"/> to make this property working properly.
        /// </summary>
        internal static ALError LastError
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets whether the Error Checking should be performed and printed under <see cref="Trace"/> / <see cref="Debug"/> Listeners
        /// Regardless to Build Configurations.
        /// </summary>
        public static bool Verbose
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Verbose Level of if <see cref="Verbose"/> property return true.
        /// </summary>
        public static VerboseFlags VerboseLevel
        {
            get;
            set;
        }

        static ALChecker()
        {
            Verbose = false;
            VerboseLevel = VerboseFlags.Error;
        }

        /// <summary>
        /// Call specific function and check for OpenGL Error.
        /// </summary>
        /// <param name="function">Function to call.</param>
        public static void Check(Action function)
        {
            function();
            _isManuallyChecked = false;

            if (Verbose)
            {
                CheckError();
                return;
            }
#if DEBUG
            CheckError();
#endif
        }

        /// <summary>
        /// Gets for the latest OpenGL Error.
        /// </summary>
        /// <returns>Latest OpenGL Error.</returns>
        internal static ALError GetError()
        {
            ALError errorCode = AL.GetError();
            LastError = errorCode;

            return errorCode;
        }

        /// <summary>
        /// Check for the OpenGL Error.
        /// </summary>
        public static void CheckError()
        {
            ALError errorCode = GetError();
            if (errorCode == ALError.NoError)
            {
                if (VerboseLevel == VerboseFlags.All)
                {
                    if (_isManuallyChecked)
                        Logger.StackFrame = 2;
                    else
                        Logger.StackFrame = 3;

                    Logger.Log("NoError: GL Operation Success", Logger.Level.Information);
                    Logger.StackFrame = 1;
                }

                _isManuallyChecked = true;
                return;
            }

            string error = "Unknown Error.";
            string description = "No Description available.";

            // Decode the error code
            switch (errorCode)
            {
                case ALError.InvalidName:
                {
                    error = "AL_INVALID_NAME";
                    description = "A bad name (ID) has been specified.";
                    break;
                }

                case ALError.InvalidEnum:
                {
                    error = "AL_INVALID_ENUM";
                    description = "An unacceptable value has been specified for an enumerated argument.";
                    break;
                }

                case ALError.InvalidValue:
                {
                    error = "AL_INVALID_VALUE";
                    description = "A numeric argument is out of range.";
                    break;
                }

                case ALError.InvalidOperation:
                {
                    error = "AL_INVALID_OPERATION";
                    description = "The specified operation is not allowed in the current state.";
                    break;
                }

                case ALError.OutOfMemory:
                {
                    error = "AL_OUT_OF_MEMORY";
                    description = "There is not enough memory left to execute the command.";
                    break;
                }
                default:
                {
                    error = errorCode.ToString();
                    break;
                }
            }

            if (_isManuallyChecked)
            {
                Logger.StackFrame = 2;
            }
            else
            {
                Logger.StackFrame = 3;
            }

            _isManuallyChecked = true;
            Logger.Log(error + ": " + description, Logger.Level.Error);
            Logger.StackFrame = 1;
        }
    }
}
