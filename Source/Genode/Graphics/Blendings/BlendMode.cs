namespace Genode.Graphics
{
    /// <summary>
    /// Represents a Blend Mode that determines how the colors of an object drawn are
    /// mixed with the colors that are already in the buffer.
    /// </summary>
    public partial struct BlendMode
    {
        /// <summary>
        /// Gets the default <see cref="BlendMode"/>.
        /// </summary>
        public static BlendMode Default
        {
            get
            {
                return new BlendMode()
                {
                    ColorSrcFactor = Factor.SrcAlpha,
                    ColorDstFactor = Factor.OneMinusSrcAlpha,
                    ColorEquation = Equation.Add,
                    AlphaSrcFactor = Factor.One,
                    AlphaDstFactor = Factor.OneMinusSrcAlpha,
                    AlphaEquation = Equation.Add
                };
            }
        }

        /// <summary>
        /// Gets <see cref="BlendMode"/> that equals to apply no <see cref="BlendMode"/>:
        /// Overwrite dest with source.
        /// </summary>
        public static BlendMode None
        {
            get
            {
                return new BlendMode(Factor.One, Factor.Zero, Equation.Add);
            }
        }

        /// <summary>
        /// Gets <see cref="BlendMode"/> that blend source and dest according to dest alpha.
        /// </summary>
        public static BlendMode Alpha
        {
            get
            {
                return new BlendMode(Factor.SrcAlpha, Factor.OneMinusSrcAlpha, Equation.Add,
                                     Factor.One, Factor.OneMinusSrcAlpha, Equation.Add);
            }
        }

        /// <summary>
        /// Gets <see cref="BlendMode"/> that add source to dest.
        /// </summary>
        public static BlendMode Add
        {
            get
            {
                return new BlendMode(Factor.SrcAlpha, Factor.One, Equation.Add,
                                     Factor.One, Factor.One, Equation.Add);
            }
        }

        /// <summary>
        /// Gets <see cref="BlendMode"/> that multiply source and dest.
        /// </summary>
        public static BlendMode Multiply
        {
            get
            {
                return new BlendMode(Factor.DstColor, Factor.Zero, Equation.Add);
            }
        }

        /// <summary>
        /// Gets or sets Source blending factor for the color channels.
        /// </summary>
        public Factor ColorSrcFactor { get; set; }

        /// <summary>
        /// Gets or sets Destination blending factor for the color channels.
        /// </summary>
        public Factor ColorDstFactor { get; set; }

        /// <summary>
        /// Gets or sets Blending equation for the color channels.
        /// </summary>
        public Equation ColorEquation { get; set; }

        /// <summary>
        /// Gets or sets Source blending factor for the alpha channel.
        /// </summary>
        public Factor AlphaSrcFactor { get; set; }

        /// <summary>
        /// Gets or sets Destination blending factor for the alpha channel.
        /// </summary>
        public Factor AlphaDstFactor { get; set; }

        /// <summary>
        /// Gets or sets Blending equation for the alpha channel.
        /// </summary>
        public Equation AlphaEquation { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="BlendMode"/> struct
        /// from SrcFactor, DstFactor and Blend Equation.
        /// </summary>
        /// <param name="sourceFactor"></param>
        /// <param name="destinationFactor"></param>
        /// <param name="blendEquation"></param>
        public BlendMode(Factor sourceFactor, Factor destinationFactor, Equation blendEquation)
        {
            ColorSrcFactor = sourceFactor;
            ColorDstFactor = destinationFactor;
            ColorEquation = blendEquation;
            AlphaSrcFactor = sourceFactor;
            AlphaDstFactor = destinationFactor;
            AlphaEquation = blendEquation;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BlendMode"/> struct
        /// with specified complete blending mode parameters.
        /// </summary>
        /// <param name="colorSourceFactor"></param>
        /// <param name="colorDestinationFactor"></param>
        /// <param name="colorBlendEquation"></param>
        /// <param name="alphaSourceFactor"></param>
        /// <param name="alphaDestinationFactor"></param>
        /// <param name="alphaBlendEquation"></param>
        public BlendMode(Factor colorSourceFactor, Factor colorDestinationFactor,
                         Equation colorBlendEquation, Factor alphaSourceFactor,
                         Factor alphaDestinationFactor, Equation alphaBlendEquation)
        {
            ColorSrcFactor = colorSourceFactor;
            ColorDstFactor = colorDestinationFactor;
            ColorEquation = colorBlendEquation;
            AlphaSrcFactor = alphaSourceFactor;
            AlphaDstFactor = alphaDestinationFactor;
            AlphaEquation = alphaBlendEquation;
        }

        /// <summary>
        /// Overload of the == operator.
        /// </summary>
        /// <param name="a">Left operand.</param>
        /// <param name="b">Right operand.</param>
        /// <returns>True if <see cref="BlendMode"/> are equal, false if they are different.</returns>
        public static bool operator ==(BlendMode a, BlendMode b)
        {
            return a.ColorSrcFactor == b.ColorSrcFactor &&
                   a.ColorDstFactor == b.ColorDstFactor &&
                   a.ColorEquation == b.ColorEquation &&
                   a.AlphaSrcFactor == b.AlphaSrcFactor &&
                   a.AlphaDstFactor == b.AlphaDstFactor &&
                   a.AlphaEquation == b.AlphaEquation;
        }

        /// <summary>
        /// Overload of the != operator.
        /// </summary>
        /// <param name="a">Left operand.</param>
        /// <param name="b">Right operand.</param>
        /// <returns>True if <see cref="BlendMode"/> are different, false if they are equal.</returns>
        public static bool operator !=(BlendMode a, BlendMode b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj.GetType() == typeof(BlendMode) && ((BlendMode)obj) == this;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code of this instance.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}