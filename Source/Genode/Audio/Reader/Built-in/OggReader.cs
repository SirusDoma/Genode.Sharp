using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

using NVorbis;
using System.IO;

namespace Genode.Audio
{
    /// <summary>
    /// Represents an implementation of <see cref="SoundReader"/> that handles ogg sound format.
    /// </summary>
    public sealed class OggReader : SoundReader
    {
        private NVorbis.VorbisReader _reader;
        private int                  _channelCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="OggReader"/> class.
        /// </summary>
        public OggReader()
        {
            _reader       = null;
            _channelCount = 0;
        }

        /// <summary>
        /// Check if current <see cref="OggReader"/> object can handle a give data from specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to check.</param>
        /// <returns><code>true</code> if supported, otherwise false.</returns>
        public override bool Check(Stream stream)
        {
            byte[] header = new byte[4];
            stream.Read(header, 0, 4);

            return Encoding.UTF8.GetString(header) == "OggS";
        }

        /// <summary>
        /// Open a <see cref="Stream"/> of sound for reading.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to open.</param>
        /// <param name="ownStream">Specify whether the <see cref="SoundReader"/> should close the source <see cref="Stream"/> upon disposing the reader.</param>
        /// <returns>A <see cref="SampleInfo"/> containing sample information.</returns>
        public override SampleInfo Open(Stream stream, bool ownStream = false)
        {
            _reader = new NVorbis.VorbisReader(stream, ownStream);
            _channelCount = _reader.Channels;

            return new SampleInfo((int)(_reader.TotalSamples * _reader.Channels), 
                _reader.Channels, _reader.SampleRate);
        }

        /// <summary>
        /// Sets the current read position of the sample offset.
        /// </summary>
        /// <param name="sampleOffset">The index of the sample to jump to, relative to the beginning.</param>
        public override void Seek(long sampleOffset)
        {
            _reader.DecodedPosition = sampleOffset / _channelCount;
        }

        /// <summary>
        /// Reads a block of audio samples from the current stream and writes the data to given sample.
        /// </summary>
        /// <param name="samples">The sample array to fill.</param>
        /// <param name="count">The maximum number of samples to read.</param>
        /// <returns>The number of samples actually read.</returns>
        public override long Read(short[] samples, long count)
        {
            float[] buffer = new float[count];
            int read = _reader.ReadSamples(buffer, 0, (int)count);
            CastBuffer(buffer, samples, read);

            return read;
        }

        /// <summary>
        /// Releases all resources used by the <see cref="OggReader"/>.
        /// </summary>
        public override void Dispose()
        {
            _reader.Dispose();
        }

        private static void CastBuffer(float[] inBuffer, short[] outBuffer, int length)
        {
            for (int i = 0; i < length; i++)
            {
                var temp = (int)(32767f * inBuffer[i]);
                if (temp > short.MaxValue)
                    temp = short.MaxValue;
                else if (temp < short.MinValue)
                    temp = short.MinValue;

                outBuffer[i] = (short)temp;
            }
        }
    }
}
