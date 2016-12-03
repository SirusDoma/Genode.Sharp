using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Genode.Audio
{
    public static class Decoders
    {
        private static List<Type> _registered;

        static Decoders()
        {
            _registered = new List<Type>();

            // Register Built-in readers
            Register<WavReader>();
            Register<OggReader>();
        }

        /// <summary>
        /// Check whether the specified <see cref="SoundReader"/> is already registered.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="SoundReader"/> to check.</typeparam>
        /// <returns><code>true</code> if registered, otherwise false.</returns>
        public static bool IsRegistered<T>()
            where T : SoundReader, new()
        {
            return _registered.Contains(typeof(T));
        }

        /// <summary>
        /// Register specified <see cref="SoundReader"/>.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="SoundReader"/> to register.</typeparam>
        public static void Register<T>()
            where T : SoundReader, new()
        {
            if (IsRegistered<T>())
            {
                Logger.Warning("{0} is already registered.", typeof(T).Name);
                return;
            }

            _registered.Add(typeof(T));
        }

        /// <summary>
        /// Unregister specified <see cref="SoundReader"/>.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="SoundReader"/> to unregister.</typeparam>
        public static void Unregister<T>()
            where T : SoundReader, new()
        {
            if (!IsRegistered<T>())
            {
                Logger.Warning("{0} is not registered.", typeof(T).Name);
                return;
            }

            _registered.Remove(typeof(T));
        }

        /// <summary>
        /// Create a registered instance of <see cref="SoundReader"/> from specified sound <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> that contains sound.</param>
        /// <returns><see cref="SoundReader"/> that can handle the data, otherwise null.</returns>
        public static SoundReader CreateReader(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
            {
                throw new ArgumentException("The specified stream must be readable and seekable.");
            }

            foreach (var type in _registered)
            {
                stream.Position = 0;

                var reader = (SoundReader)Activator.CreateInstance(type);
                if (reader.Check(stream))
                    return reader;
            }

            return null;
        }
    }
}
