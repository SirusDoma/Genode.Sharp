using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

using Genode;
using Genode.Internal.OpenAL;

namespace Genode.Audio
{
    /// <summary>
    /// Represents an object with sound properties.
    /// </summary>
    public abstract class SoundSource : IDisposable
    {
        private int _source;

        /// <summary>
        /// Gets the OpenAL native handle of current <see cref="SoundSource"/> object.
        /// </summary>
        public int Handle
        {
            get { return _source; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current <see cref="SoundSource"/> object is in loop mode.
        /// </summary>
        public abstract bool IsLooping
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current playing position of the current <see cref="Sound"/> object.
        /// </summary>
        public abstract TimeSpan PlayingOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets total duration of current <see cref="SoundSource"/> object.
        /// </summary>
        public abstract TimeSpan Duration
        {
            get;
        }

        /// <summary>
        /// Gets the current status of current <see cref="SoundSource"/> object.
        /// </summary>
        public virtual Status Status
        {
            get
            {
                ALSourceState state = 0;
                ALChecker.Check(() => state = AL.GetSourceState(_source));

                switch (state)
                {
                    case ALSourceState.Initial:
                    case ALSourceState.Stopped: return Status.Stopped;
                    case ALSourceState.Paused:  return Status.Paused;
                    case ALSourceState.Playing: return Status.Playing;
                }

                return Status.Stopped;
            }
        }

        /// <summary>
        /// Gets or sets the pitch of current <see cref="SoundSource"/> object.
        /// </summary>
        public float Pitch
        {
            get
            {
                float pitch = 0;
                ALChecker.Check(() => AL.GetSource(_source, ALSourcef.Pitch, out pitch));

                return pitch;
            }
            set
            {
                ALChecker.Check(() => AL.Source(_source, ALSourcef.Pitch, value));
            }
        }

        /// <summary>
        /// Gets or sets the volume of current <see cref="SoundSource"/> object.
        /// </summary>
        public float Volume
        {
            get
            {
                float gain = 0;
                ALChecker.Check(() => AL.GetSource(_source, ALSourcef.Gain, out gain));

                return gain * 100f;
            }
            set
            {
                ALChecker.Check(() => AL.Source(_source, ALSourcef.Gain, value * 0.01f));
            }
        }

        /// <summary>
        /// Gets or sets the 3D position of current <see cref="SoundSource"/> object in audio scene.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                float[] positions = new float[3];
                ALChecker.Check(() => AL.GetSource(_source, ALSource3f.Position, out positions[0], out positions[1], out positions[1]));

                return new Vector3(positions[0], positions[1], positions[2]);
            }
            set
            {
                ALChecker.Check(() => AL.Source(_source, ALSource3f.Position, value.X, value.Y, value.Z));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current <see cref="SoundSource"/> object position should relative to the listener.
        /// </summary>
        public bool IsRelativeListener
        {
            get
            {
                bool relative = false;
                ALChecker.Check(() => AL.GetSource(_source, ALSourceb.SourceRelative, out relative));

                return relative;
            }
            set
            {
                ALChecker.Check(() => AL.Source(_source, ALSourceb.SourceRelative, value));
            }
        }

        /// <summary>
        /// Gets or sets the minimum distance of current <see cref="SoundSource"/> object.
        /// </summary>
        public float MinDistance
        {
            get
            {
                float distance = 0;
                ALChecker.Check(() => AL.GetSource(_source, ALSourcef.ReferenceDistance, out distance));

                return distance;
            }
            set
            {
                ALChecker.Check(() => AL.Source(_source, ALSourcef.ReferenceDistance, value));
            }
        }

        /// <summary>
        /// Gets or sets the attenuation factor of current <see cref="SoundSource"/> object.
        /// </summary>
        public float Attenuation
        {
            get
            {
                float attenuation = 0f;
                ALChecker.Check(() => AL.GetSource(_source, ALSourcef.RolloffFactor, out attenuation));

                return attenuation;
            }
            set
            {
                ALChecker.Check(() => AL.Source(_source, ALSourcef.RolloffFactor, value));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SoundSource"/> class.
        /// </summary>
        public SoundSource()
        {
            // Ensure that AudioDevice is initialized
            AudioDevice.Initialize();

            ALChecker.Check(() => _source = AL.GenSource());
            ALChecker.Check(() => AL.Source(_source, ALSourcei.Buffer, 0));
        }


        /// <summary>
        /// Start or resume playing the current <see cref="SoundSource"/> object.
        /// </summary>
        public abstract void Play();

        /// <summary>
        /// Pause the current <see cref="SoundSource"/> object.
        /// </summary>
        public abstract void Pause();

        /// <summary>
        /// Stop playing the current <see cref="SoundSource"/> object.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Releases all resources used by the <see cref="SoundSource"/>.
        /// </summary>
        public virtual void Dispose()
        {
            ALChecker.Check(() => AL.Source(_source, ALSourcei.Buffer, 0));
            ALChecker.Check(() => AL.DeleteSource(_source));
        }
    }
}
