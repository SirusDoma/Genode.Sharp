using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Genode.Audio
{
    /// <summary>
    /// Represents an implementation of <see cref="SoundReader"/> that handles wav sound format.
    /// </summary>
    public sealed class WavReader : SoundReader
    {
        enum WavFormat
        {
            PCM = 1,
            Float = 3
        }


        private Stream    _stream;
        private int       _bytesPerSample;
        private long      _offsetStart;
        private long      _offsetEnd;
        private WavFormat _format;
        private bool      _ownStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="WavReader"/> class.
        /// </summary>
        public WavReader()
        {
            _stream         = null;
            _bytesPerSample = 0;
            _offsetStart    = 0;
            _offsetEnd      = 0;
            _format         = WavFormat.PCM;
            _ownStream      = false;
        }

        private SampleInfo ParseHeader()
        {
            // If we are here, it means that the first part of the header
            // (the format) has already been checked
            _stream.Seek(12, SeekOrigin.Begin);

            int sampleCount = 0;
            short channelCount = 0;
            int sampleRate = 0;

            // Parse all the sub-chunks
            bool dataChunkFound = false;
            var reader = new BinaryReader(_stream);
            while (!dataChunkFound)
            {
                // Parse the sub-chunk id and size
                byte[] subChunkId = new byte[4];
                if (_stream.Read(subChunkId, 0, 4) != 4)
                    throw new Exception("Failed to open WAV sound file (invalid or unsupported file)");
                int subChunkSize = reader.ReadInt32();

                // Check which chunk it is
                var signature = Encoding.UTF8.GetString(subChunkId);
                if (signature == "fmt ")
                {
                    // "fmt" chunk

                    // Audio format
                    short format = reader.ReadInt16();
                    _format = (WavFormat)format;

                    //if (format != 1) // PCM
                    //throw new Exception("Failed to open WAV sound file (invalid or unsupported file)");

                    // Channel count
                    channelCount = reader.ReadInt16();

                    // Sample rate
                    sampleRate = reader.ReadInt32();

                    // Byte rate
                    int byteRate = reader.ReadInt32();

                    // Block align
                    short blockAlign = reader.ReadInt16();

                    // Bits per sample
                    short bitsPerSample = reader.ReadInt16();
                    if (bitsPerSample != 8 && bitsPerSample != 16 && bitsPerSample != 24 && bitsPerSample != 32)
                    {
                        throw new FormatException(
                                    "Unsupported sample size: " + bitsPerSample.ToString() + " bit (Supported sample sizes are 8/16/24/32 bit)"
                                  );
                    }
                    _bytesPerSample = bitsPerSample / 8;

                    // Skip potential extra information (should not exist for PCM)
                    if (subChunkSize > 16)
                    {
                        if (_stream.Seek(subChunkSize - 16, SeekOrigin.Current) == -1)
                            throw new Exception("Failed to open WAV sound file (invalid or unsupported file)");
                    }
                }
                else if (signature == "data")
                {
                    // "data" chunk

                    // Compute the total number of samples
                    sampleCount = subChunkSize / _bytesPerSample;

                    // Store the start and end position of samples in the file
                    _offsetStart = _stream.Position;
                    _offsetEnd = _offsetStart + sampleCount * _bytesPerSample;

                    dataChunkFound = true;
                }
                else
                {
                    // unknown chunk, skip it
                    if (_stream.Seek(subChunkSize, SeekOrigin.Current) == -1)
                        throw new Exception("Failed to open WAV sound file (invalid or unsupported file)");
                }
            }

            return new SampleInfo(sampleCount, channelCount, sampleRate);
        }

        /// <summary>
        /// Check if current <see cref="WavReader"/> object can handle a give data from specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to check.</param>
        /// <returns><code>true</code> if supported, otherwise false.</returns>
        public override bool Check(Stream stream)
        {
            int size = 12;
            byte[] header = new byte[size];
            if (stream.Read(header, 0, size) < size)
            {
                return false;
            }

            return Encoding.UTF8.GetString(header, 0, 4) == "RIFF" &&
                   Encoding.UTF8.GetString(header, 8, 4) == "WAVE";
        }

        /// <summary>
        /// Open a <see cref="Stream"/> of sound for reading.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to open.</param>
        /// <param name="ownStream">Specify whether the <see cref="SoundReader"/> should close the source <see cref="Stream"/> upon disposing the reader.</param>
        /// <returns>A <see cref="SampleInfo"/> containing sample information.</returns>
        public override SampleInfo Open(Stream stream, bool ownStream = false)
        {
            _stream    = stream;
            _ownStream = ownStream;
            return ParseHeader();
        }

        /// <summary>
        /// Sets the current read position of the sample offset.
        /// </summary>
        /// <param name="sampleOffset">The index of the sample to jump to, relative to the beginning.</param>
        public override void Seek(long sampleOffset)
        {
            _stream.Seek(_offsetStart + sampleOffset * _bytesPerSample, SeekOrigin.Begin);
        }

        /// <summary>
        /// Reads a block of audio samples from the current stream and writes the data to given sample.
        /// </summary>
        /// <param name="samples">The sample array to fill.</param>
        /// <param name="count">The maximum number of samples to read.</param>
        /// <returns>The number of samples actually read.</returns>
        public override long Read(short[] samples, long count)
        {
            long read = 0;
            int index = 0;
            try
            {
                var reader = new BinaryReader(_stream);
                while ((read < count) && (_stream.Position < _offsetEnd))
                {
                    switch (_bytesPerSample)
                    {
                        case 1:
                            {
                                byte sample = reader.ReadByte();
                                samples[index] = (short)((sample - 128) << 8);
                                break;
                            }

                        case 2:
                            {
                                short sample = reader.ReadInt16();
                                samples[index] = sample;
                                break;
                            }

                        case 3:
                            {
                                byte[] bytes = reader.ReadBytes(3);
                                int sample = bytes[0] | (bytes[1] << 8) | (bytes[2] << 16);
                                samples[index] = (short)(sample >> 8);
                                break;
                            }
                        case 4:
                            {
                                if (_format == WavFormat.PCM)
                                {
                                    int sample = reader.ReadInt32();
                                    samples[index] = (short)(sample >> 16);
                                }
                                else
                                {
                                    float sample = reader.ReadSingle();
                                    samples[index] = (short)(sample * 32767.0f);
                                }
                                break;
                            }
                    }

                    ++index;
                    ++read;
                }
            }
            catch (EndOfStreamException)
            {
                return read;
            }

            
            return read;
        }

        /// <summary>
        /// Release all resources by the <see cref="WavReader"/>.
        /// </summary>
        public override void Dispose()
        {
            if (_ownStream)
                _stream?.Dispose();
        }
    }
}
