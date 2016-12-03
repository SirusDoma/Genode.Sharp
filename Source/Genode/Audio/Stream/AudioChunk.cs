using System;
using System.Collections.Generic;
using System.Text;

namespace Genode.Audio
{
    public class AudioChunk
    {
        public short[] Samples
        {
            get;
            private set;
        }

        public AudioChunk()
        {
            Samples = null;
        }

        public AudioChunk(short[] samples)
        {
            Samples = samples;
        }
    }
}
