using System;
using System.Collections.Generic;
using System.Text;

namespace Genode.Audio
{
    /// <summary>
    /// Represents a storage holding the sample properties of a sound.
    /// </summary>
    public struct SampleInfo
    {
        /// <summary>
        /// Gets total number of samples of the sound.
        /// </summary>
        public long SampleCount
        {
            get; private set;
        }

        /// <summary>
        /// Gets the number of channels of the sound.
        /// </summary>
        public int ChannelCount
        {
            get; private set;
        }

        /// <summary>
        /// Gets the samples rate of the sound, in samples per second.
        /// </summary>
        public int SampleRate
        {
            get; private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleInfo"/> struct.
        /// </summary>
        /// <param name="sampleCount">The number of samples.</param>
        /// <param name="channelCount">The number of channels.</param>
        /// <param name="sampleRate">The sample rate, in sample per second.</param>
        public SampleInfo(int sampleCount, int channelCount, int sampleRate)
        {
            SampleCount  = sampleCount;
            ChannelCount = channelCount;
            SampleRate   = sampleRate;
        }
    }
}
