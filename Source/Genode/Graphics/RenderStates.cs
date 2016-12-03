using System;
using System.Collections.Generic;
using System.Text;

using Genode;

namespace Genode.Graphics
{
    /// <summary>
    /// Represents the states used for drawing to a <see cref="RenderTarget"/>.
    /// </summary>
    public struct RenderStates
    {
        /// <summary>
        /// Gets default <see cref="RenderStates"/>.
        /// </summary>
        public static RenderStates Default
        {
            get
            {
                return new RenderStates()
                {
                    Time      = 0,
                    Texture   = null,
                    Transform = Transform.Identity,
                    BlendMode = BlendMode.Alpha,
                    Shader    = null
                };
            }
        }

        private Transform _transform;

        /// <summary>
        /// Gets the delta time of current frame.
        /// </summary>
        public double Time
        {
            get; private set;
        }

        /// <summary>
        /// Gets or sets <see cref="Graphics.Texture"/> to use.
        /// </summary>
        public Texture Texture
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets <see cref="Graphics.BlendMode"/> to use.
        /// </summary>
        public BlendMode BlendMode
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets <see cref="Graphics.Shader"/> to use.
        /// </summary>
        public Shader Shader
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets <see cref="Graphics.Transform"/> to use,
        /// <code>null</code> equivalent to <see cref="Transform.Identity"/>.
        /// </summary>
        public Transform Transform
        {
            get
            {
                if (_transform == null)
                    return _transform = Transform.Identity;
                return _transform;
            }
            set
            {
                _transform = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderStates"/> struct
        /// with specified delta time.
        /// </summary>
        /// <param name="time">The delta time of current frame.</param>
        public RenderStates(double time)
            : this(Default)
        {
            Time = time;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderStates"/> struct
        /// with existing <see cref="RenderStates"/>.
        /// </summary>
        /// <param name="states">The existing <see cref="RenderStates"/>.</param>
        public RenderStates(RenderStates states)
        {
            _transform = states.Transform;

            Time = states.Time;
            Texture = states.Texture;
            BlendMode = states.BlendMode;
            Shader = states.Shader;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderStates"/> struct
        /// with specified delta time of current frame and existing <see cref="RenderStates"/>.
        /// </summary>
        /// <param name="states">The existing <see cref="RenderStates"/>.</param>
        public RenderStates(double time, RenderStates states)
        {
            _transform = states.Transform;

            Time = time;
            Texture = states.Texture;
            BlendMode = states.BlendMode;
            Shader = states.Shader;
        }

        public static implicit operator RenderStates(Texture texture)
        {
            return new RenderStates()
            {
                Time = 0,
                Texture = texture,
                Transform = Transform.Identity,
                BlendMode = BlendMode.Alpha,
                Shader = null,
            };
        }

        public static implicit operator RenderStates(Transform transform)
        {
            return new RenderStates()
            {
                Time = 0,
                Texture = null,
                Transform = transform,
                BlendMode = BlendMode.Alpha,
                Shader = null,
            };
        }

        public static implicit operator RenderStates(BlendMode blendMode)
        {
            return new RenderStates()
            {
                Time = 0,
                Texture = null,
                Transform = Transform.Identity,
                BlendMode = blendMode,
                Shader = null,
            };
        }

        public static implicit operator RenderStates(Shader shader)
        {
            return new RenderStates()
            {
                Time = 0,
                Texture = null,
                Transform = Transform.Identity,
                BlendMode = BlendMode.Alpha,
                Shader = shader
            };
        }
    }
}
