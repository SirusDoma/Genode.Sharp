using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

using Genode;
using Genode.Internal.OpenAL;

namespace Genode.Audio
{
    /// <summary>
    /// Represents a storage for audio samples defining a <see cref="Sound"/>.
    /// </summary>
    public class SoundBuffer : IDisposable
    {
        private int         _buffer;
        private short[]     _samples;
        private TimeSpan    _duration;
        private List<Sound> _sounds;

        /// <summary>
        /// Gets OpenAL Native Handle of current <see cref="SoundBuffer"/> object.
        /// </summary>
        public int Handle
        {
            get { return _buffer; }
        }

        /// <summary>
        /// Gets the number of samples stored in current <see cref="SoundBuffer"/> object.
        /// </summary>
        public int SampleCount
        {
            get { return _samples.Length; }
        }

        /// <summary>
        /// Gets the sample rate of current <see cref="SoundBuffer"/> object.
        /// </summary>
        public int SampleRate
        {
            get
            {
                int rate = 0;
                ALChecker.Check(() => AL.GetBuffer(_buffer, ALGetBufferi.Frequency, out rate));

                return rate;
            }
        }

        /// <summary>
        /// Gets the number of channels used by current <see cref="SoundBuffer"/> object.
        /// </summary>
        public int ChannelCount
        {
            get
            {
                int count = 0;
                ALChecker.Check(() => AL.GetBuffer(_buffer, ALGetBufferi.Channels, out count));

                return count;
            }
        }

        /// <summary>
        /// Gets the total duration of current <see cref="SoundBuffer"/> object.
        /// </summary>
        public TimeSpan Duration
        {
            get { return _duration; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SoundBuffer"/> class.
        /// </summary>
        public SoundBuffer()
        {
            // Ensure that AudioDevice is initialized
            AudioDevice.Initialize();

            _samples = new short[0];
            _sounds = new List<Sound>();
            _duration = TimeSpan.Zero;

            // Create the buffer
            ALChecker.Check(() => _buffer = AL.GenBuffer());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SoundBuffer"/> class
        /// from specified file.
        /// </summary>
        /// <param name="filename">The path of the sound file to load.</param>
        public SoundBuffer(string filename)
            : this(new MemoryStream(File.ReadAllBytes(filename)))
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SoundBuffer"/> class
        /// from specified an array of byte containing sound data.
        /// </summary>
        /// <param name="data">An array of byte contains sound data to load.</param>
        public SoundBuffer(byte[] data)
            : this(new MemoryStream(data))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SoundBuffer"/> class
        /// from specified <see cref="Stream"/> containing sound data.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> contains sound data to load.</param>
        public SoundBuffer(Stream stream)
            : this()
        {
            var reader = Decoders.CreateReader(stream);
            if (reader == null)
            {
                throw new NotSupportedException("The specified sound is not supported.");
            }

            var info = reader.Open(stream);
            Initialize(reader, info);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SoundBuffer"/> class
        /// from specified sound sample and sound parameters.
        /// </summary>
        /// <param name="samples">An array of audio samples.</param>
        /// <param name="channelCount">The number of channels (1 = mono, 2 = stereo, ...)</param>
        /// <param name="sampleRate">The sample rate (number of samples to play per second)</param>
        public SoundBuffer(short[] samples, int channelCount, int sampleRate)
            : this()
        {
            _samples = samples;
            Update(channelCount, sampleRate);
        }

        private void Initialize(SoundReader reader, SampleInfo info)
        {
            // Retrieve the sound parameters
            long sampleCount  = info.SampleCount;
            int  channelCount = info.ChannelCount;
            int  sampleRate   = info.SampleRate;

            // Read the samples from the provided file
            using (reader)
            {
                _samples = new short[sampleCount];
                if (reader.Read(_samples, sampleCount) == sampleCount)
                {
                    // Update the internal buffer with the new samples
                    Update(channelCount, sampleRate);
                }
                else
                {
                    throw new Exception("Failed to decode the sound data.");
                }
            }
        }

        private void Update(int channelCount, int sampleRate)
        {
            // Check parameters
            if (channelCount <= 0 || sampleRate <= 0)
            {
                throw new ArgumentException();
            }

            // Find the best format according to the number of channels
            var format = AudioDevice.GetFormat(channelCount);

            if (format == 0)
            {
                throw new Exception("Failed to load sound buffer (unsupported number of channels: " + channelCount.ToString() + ")");
            }

            // First make a copy of the list of sounds so we can reattach later
            var sounds = new List<Sound>(_sounds);

            // Detach the buffer from the sounds that use it (to avoid OpenAL errors)
            foreach (var sound in sounds)
                sound.ResetBuffer();

            // fill the buffer
            int size = _samples.Length * sizeof(short);
            ALChecker.Check(() => AL.BufferData(_buffer, format, _samples, size, sampleRate));

            // Compute the duration
            _duration = TimeSpan.FromSeconds((float)_samples.Length / sampleRate / channelCount);

            // Now reattach the buffer to the sounds that use it
            foreach (var sound in sounds)
                sound.Buffer = this;
        }

        /// <summary>
        /// Gets the samples of current <see cref="SoundBuffer"/> object.
        /// </summary>
        /// <returns>an array of sound samples.</returns>
        public short[] GetSamples()
        {
            return _samples;
        }

        /// <summary>
        /// Releases all resources used by <see cref="SoundBuffer"/>.
        /// </summary>
        public void Dispose()
        {
            // To prevent the iterator from becoming invalid, move the entire buffer to another
            // container. Otherwise calling resetBuffer would result in detachSound being
            // called which removes the sound from the internal list.
            var sounds = new List<Sound>(_sounds);

            // Detach the buffer from the sounds that use it (to avoid OpenAL errors)
            foreach (var sound in sounds)
                sound.ResetBuffer();

            // Destroy the buffer
            if (_buffer > 0)
                ALChecker.Check(() => AL.DeleteBuffer(_buffer));
        }

        internal void AttachSound(Sound sound)
        {
            _sounds.Add(sound);
        }

        internal void DetachSound(Sound sound)
        {
            _sounds.Remove(sound);
        }
    }
}
