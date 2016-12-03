namespace Genode.Graphics
{
    public partial struct BlendMode
    {
        /// <summary>
        /// Represents blending equations.
        /// The equations are mapped directly to their OpenGL equivalents,
        /// specified by glBlendEquation() or glBlendEquationSeparate().
        /// </summary>
        public enum Equation
        {
            /// <summary>
            /// Pixel = Src * SrcFactor + Dst * DstFactor
            /// </summary>
            Add,
            /// <summary>
            /// Pixel = Src * SrcFactor - Dst * DstFactor
            /// </summary>
            Subtract,
            /// <summary>
            /// Pixel = Dst * DstFactor - Src * SrcFactor
            /// </summary>
            ReverseSubtract
        };
    }
}
