using System;
using System.Collections.Generic;
using System.Text;

namespace Genode.Audio
{
    /// <summary>
    /// Represents <see cref="SoundSource"/> states.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// Sound is not playing.
        /// </summary>
        Stopped,
        
        /// <summary>
        /// Sound is paused.
        /// </summary>
        Paused,

        /// <summary>
        /// Sound is playing.
        /// </summary>
        Playing
    }
}
