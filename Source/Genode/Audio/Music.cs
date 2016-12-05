using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Genode.Audio
{
    /// <summary>
    /// Represents a streamed music.
    /// </summary>
    public class Music : SoundStream
    {
        private readonly object _mutex = new object();

        private SoundReader _reader;
        private TimeSpan    _duration;
        private short[]     _samples;
        private SampleInfo  _info;

        /// <summary>
        /// Gets total duration of current <see cref="Music"/> object.
        /// </summary>
        public override TimeSpan Duration
        {
            get
            {
                return _duration;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Music"/> class.
        /// </summary>
        public Music()
            : base()
        {
            _reader   = null;
            _samples  = new short[0];
            _duration = TimeSpan.Zero;

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Music"/> class
        /// from specified file.
        /// </summary>
        /// <param name="filename">The path of the sound file to load.</param>
        public Music(string filename)
            : this(new MemoryStream(File.ReadAllBytes(filename)))
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Music"/> class
        /// from specified an array of byte containing sound data.
        /// </summary>
        /// <param name="data">An array of byte contains sound data to load.</param>
        public Music(byte[] data)
            : this(new MemoryStream(data))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Music"/> class
        /// from specified <see cref="Stream"/> containing sound data.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> contains sound data to load.</param>
        public Music(Stream stream)
            : this()
        {
            _reader = Decoders.CreateReader(stream);
            if (_reader == null)
            {
                throw new NotSupportedException("The specified sound is not supported.");
            }

            _info = _reader.Open(stream);
            Initialize(_info.ChannelCount, _info.SampleRate);
        }

        /// <summary>
        /// Request a new chunk of audio samples from the stream source.
        /// </summary>
        /// <param name="samples">The audio chunk that contains audio samples.</param>
        /// <returns><code>true</code> if reach the end of stream, otherwise false.</returns>
        protected override bool OnGetData(out short[] samples)
        {
            lock (_mutex)
            {
                // Fill the chunk parameters
                long count = _reader.Read(_samples, _samples.Length);
                samples    = _samples;
                
                // Check if we have reached the end of the audio file
                return count == samples.Length;
            }
        }

        /// <summary>
        /// Change the current playing position in the stream source.
        /// </summary>
        /// <param name="time">Seek to specified time.</param>
        protected override void OnSeek(TimeSpan time)
        {
            lock (_mutex)
                _reader.Seek((long)time.TotalSeconds * SampleRate * ChannelCount);
        }

        /// <summary>
        /// Performs initialization steps by providing the audio stream parameters.
        /// </summary>
        /// <param name="channelCount">The number of channels of current <see cref="SoundStream"/> object.</param>
        /// <param name="sampleRate">The sample rate, in samples per second.</param>
        protected override void Initialize(int channelCount, int sampleRate)
        {
            // Compute the music duration
            _duration = TimeSpan.FromSeconds(_info.SampleCount / channelCount / sampleRate);

            // Resize the internal buffer so that it can contain 1 second of audio samples
            _samples = new short[sampleRate * channelCount];

            // Initialize the stream
            base.Initialize(channelCount, sampleRate);
        }

        /// <summary>
        /// Releases all resources used by the <see cref="Music"/>.
        /// </summary>
        public override void Dispose()
        {
            _reader?.Dispose();
            base.Dispose();
        }
    }
}
