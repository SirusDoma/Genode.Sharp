using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Genode.Audio
{
    /// <summary>
    /// Represents a decoder to decode specific audio format.
    /// </summary>
    public abstract class SoundReader : IDisposable
    {
        /// <summary>
        /// Check if current <see cref="SoundReader"/> object can handle a give data from specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to check.</param>
        /// <returns><code>true</code> if supported, otherwise false.</returns>
        public abstract bool Check(Stream stream);

        /// <summary>
        /// Open a <see cref="Stream"/> of sound for reading.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to open.</param>
        /// <param name="ownStream">Specify whether the <see cref="SoundReader"/> should close the source <see cref="Stream"/> upon disposing the reader.</param>
        /// <returns>A <see cref="SampleInfo"/> containing sample information.</returns>
        public abstract SampleInfo Open(Stream stream, bool ownStream = false);

        /// <summary>
        /// Sets the current read position of the sample offset.
        /// </summary>
        /// <param name="sampleOffset">The index of the sample to jump to, relative to the beginning.</param>
        public abstract void Seek(long sampleOffset);

        /// <summary>
        /// Reads a block of audio samples from the current stream and writes the data to given sample.
        /// </summary>
        /// <param name="samples">The sample array to fill.</param>
        /// <param name="count">The maximum number of samples to read.</param>
        /// <returns>The number of samples actually read.</returns>
        public abstract long Read(short[] samples, long count);

        /// <summary>
        /// Release all resources used by the <see cref="SoundReader"/>.
        /// </summary>
        public abstract void Dispose();
    }
}
