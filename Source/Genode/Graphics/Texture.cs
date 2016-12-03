using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Genode;
using Genode.Internal.OpenGL;


namespace Genode.Graphics
{
    /// <summary>
    /// Represents a storage pixels that can be rendered.
    /// </summary>
    public class Texture : IDisposable
    {
        /// <summary>
        /// Re[resents <see cref="CoordinateType"/> that used by <see cref="Texture"/> when binding.
        /// </summary>
        public enum CoordinateType
        {
            /// <summary>
            /// Default OpenGL Texture Coordinate [0..1].
            /// </summary>
            Normalized = 0,

            /// <summary>
            /// Un-normalized Texture Coordinate [0..size].
            /// </summary>
            Pixel = 1
        }

        private static long _uId = 1;
        private static int _maxSize = 0;
        private static bool _maxSizeChecked = false;
        private static bool _textureSrgbChecked = false;
        private static bool _textureSrgbWarned = false;
        private static bool _textureSrgb = false;
        private static bool _textureEdgeClampChecked = false;
        private static bool _textureEdgeClamp = false;
        private static object _idMutex = new object();

        private long _cacheId;
        private int _textureId;
        private Size _size;
        private Size _actualSize;
        private bool _isSmooth;
        private bool _isRepeated;
        private bool _sRgb;
        private bool _pixelsFlipped;
        private bool _isFboAttachment;
        private bool _hasMipmap;

        /// <summary>
        /// Gets unique identifier of current <see cref="Texture"/> object.
        /// </summary>
        internal long CacheId
        {
            get { return _cacheId; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current <see cref="Texture"/> object is used as RenderTexture attachment.
        /// </summary>
        internal bool IsFboAttachment
        {
            get { return _isFboAttachment; }
            set { _isFboAttachment = value; }
        }

        internal bool IsPixelsFlipped
        {
            get { return _pixelsFlipped; }
            set { _pixelsFlipped = value; }
        }

        /// <summary>
        /// Gets the OpenGL <see cref="Texture"/> Handle.
        /// </summary>
        public int Handle
        {
            get { return _textureId; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Texture"/> smooth filter should be enabled.
        /// </summary>
        public bool IsSmooth
        {
            get { return _isSmooth; }
            set
            {
                if (_isSmooth == value)
                    return;

                _isSmooth = value;
                if (_textureId > 0)
                {
                    GLChecker.Check(() => GL.BindTexture(TextureTarget.Texture2D, _textureId));
                    GLChecker.Check(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                        (int)(_isSmooth ? TextureMagFilter.Linear : TextureMagFilter.Nearest)));

                    if (_hasMipmap)
                    {
                        GLChecker.Check(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                            (int)(_isSmooth ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.NearestMipmapLinear)));
                    }
                    else
                    {
                        GLChecker.Check(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                            (int)(_isSmooth ? TextureMinFilter.Linear : TextureMinFilter.Nearest)));
                    }
                }
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Texture"/> repeated filter should be enabled.
        /// </summary>
        public bool IsRepeated
        {
            get { return _isRepeated; }
            set
            {
                if (_isRepeated == value)
                    return;

                _isRepeated = value;
                if (_textureId > 0)
                {
                    if (!_textureEdgeClampChecked)
                    {
                        _textureEdgeClamp = GLExtensions.IsAvailable("GL_EXT_texture_edge_clamp") || GLExtensions.IsAvailable("GL_texture_edge_clamp");
                        _textureEdgeClampChecked = true;

                        if (!_isRepeated && !_textureEdgeClamp)
                        {
                            Logger.Warning("OpenGL extension SGIS_texture_edge_clamp unavailable\n" +
                                           "Artifacts may occur along texture edges\n" +
                                           "Ensure that hardware acceleration is enabled if available.");
                        }
                    }

                    GLChecker.Check(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                        (int)(_isRepeated ? TextureWrapMode.Repeat : (_textureEdgeClamp ? TextureWrapMode.ClampToEdge : TextureWrapMode.Clamp))));
                    GLChecker.Check(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                        (int)(_isRepeated ? TextureWrapMode.Repeat : (_textureEdgeClamp ? TextureWrapMode.ClampToEdge : TextureWrapMode.Clamp))));

                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether sRGB Conversion should be enabled.
        /// After enabling or disabling sRGB conversion,
        /// <see cref="Texture"/> data need to be reloaded in order for the setting to take effect.
        /// </summary>
        public bool sRGB
        {
            get { return _sRgb; }
            set { _sRgb = value; }
        }

        /// <summary>
        /// Gets the size of current <see cref="Texture"/> object.
        /// </summary>
        public Size Size
        {
            get { return _size; }
        }

        /// <summary>
        /// Gets the width of current <see cref="Texture"/> object.
        /// </summary>
        public int Width
        {
            get { return _size.Width; }
        }

        /// <summary>
        /// Gets the height of current <see cref="Texture"/> object.
        /// </summary>
        public int Height
        {
            get { return _size.Height; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture"/> class.
        /// </summary>
        public Texture()
        {
            _cacheId = GetUniqueId();
            _size = _actualSize = Size.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture"/>
        /// with specified width and height.
        /// </summary>
        /// <param name="width">The width of the <see cref="Texture"/>.</param>
        /// <param name="height">The height of the <see cref="Texture"/>.</param>
        /// <param name="srgb"><code>true</code> to convert the <see cref="Texture"/> source from sRGB, otherwise <code>false</code>.</param>
        public Texture(int width, int height, bool srgb = false)
        {
            Create(width, height, srgb);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture"/> class
        /// from an existing <see cref="Texture"/> instance.
        /// </summary>
        /// <param name="texture">The existing <see cref="Texture"/> instance.</param>
        public Texture(Texture texture)
            : this(texture.ToImage())
        {
            _isSmooth = texture._isSmooth;
            _isRepeated = texture._isRepeated;
            _sRgb = texture._sRgb;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture"/> class
        /// from a file on disk.
        /// </summary>
        /// <param name="filename">Path of the image file to load.</param>
        public Texture(string filename)
            : this(Image.FromFile(filename))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture"/> class
        /// from a <see cref="Stream"/> that contain <see cref="Image"/> data.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> that contain <see cref="Image"/> data.</param>
        public Texture(Stream stream)
            : this(Image.FromStream(stream))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture"/> class
        /// from an existing <see cref="Image"/> instance.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> instance.</param>
        public Texture(Image image)
            : this()
        {
            Create(image.Width, image.Height, false);
            Update(image);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture"/> class
        /// from an existing <see cref="Image"/> instance with specified region.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> instance.</param>
        /// <param name="region">The area of the <see cref="Image"/> to load.</param>
        public Texture(Image image, Rectangle region)
            : this()
        {
            // Retrieve the image size
            int areaWidth = image.Width;
            int areaHeight = image.Height;

            // Load the entire image if the source area is either empty or contains the whole image
            if (region.Width == 0 || (region.Height == 0) ||
               ((region.Left <= 0) && (region.Top <= 0) && (region.Width >= areaWidth) && (region.Height >= areaHeight)))
            {
                // Load the entire image
                Create(image.Width, image.Height, false);
                Update(image);
            }
            else
            {
                // Clamp the region size to valid value
                int left   = (region.Left < 0) ? 0 : region.Left;
                int top    = (region.Top  < 0) ? 0 : region.Top;
                int width  = (region.Left + region.Width > areaWidth)  ? areaWidth  - region.Left : region.Width;
                int height = (region.Top + region.Height > areaHeight) ? areaHeight - region.Top : region.Height;

                Create(width, height, false);

                // Copy bitmap to specified region
                using (Bitmap bmp = new Bitmap(image))
                {
                    BitmapData data = bmp.LockBits(new Rectangle(left, top, width, height), ImageLockMode.ReadOnly,
                                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    GLChecker.Check(() => GL.BindTexture(TextureTarget.Texture2D, _textureId));
                    GLChecker.Check(() => GL.TexImage2D(TextureTarget.Texture2D, 0,
                        0, 0, width, height,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0));

                    GLChecker.Check(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                        (int)(_isSmooth ? TextureMinFilter.Linear : TextureMinFilter.Nearest)));

                    bmp.UnlockBits(data);

                    // Force an OpenGL flush, so that the texture data will appear updated
                    // in all contexts immediately (solves problems in multi-threaded apps)
                    GLChecker.Check(() => GL.Flush());
                }
            }
        }

        private void Create(int width, int height, bool srgb)
        {
            // Check whether the specified size is valid
            if (width == 0 || height == 0)
            {
                throw new ArgumentException("Failed to create texture." +
                                            "invalid size (" + width.ToString() + "x" + height.ToString() + ")");
            }

            // Compute the internal texture dimensions depending on NPOT textures support
            var actualSize = new Size(GetValidSize(width), GetValidSize(height));

            // Check the maximum texture size
            int maxSize = GetMaximumSize();
            if (actualSize.Width > maxSize || actualSize.Height >= maxSize)
            {
                throw new ArgumentException("Failed to create texture.\n" +
                    "Texture size (" + width.ToString() + "x" + height.ToString() + ") " +
                    "is exceeding the allowed size (" + maxSize.ToString() + "x" + maxSize.ToString() + ").");
            }

            // All the validity checks passed, store the new texture settings
            _size = new Size(width, height);
            _actualSize = actualSize;
            _sRgb = srgb;
            _pixelsFlipped = false;
            _isFboAttachment = false;

            // Generate Texture Name (if not exist)
            if (_textureId <= 0)
            {
                GLChecker.Check(() => GL.GenTextures(1, out _textureId));

                // Check whether the texture name has been generated properly.
                if (_textureId <= 0)
                {
                    throw new AccessViolationException("Failed to create texture.\n" +
                                                       "Check whether the graphic context is valid.");
                }
            }

            if (!_textureEdgeClampChecked)
            {
                _textureEdgeClamp = GLExtensions.IsAvailable("GL_EXT_texture_edge_clamp") || GLExtensions.IsAvailable("GL_texture_edge_clamp");
                _textureEdgeClampChecked = true;

                if (!_isRepeated && !_textureEdgeClamp)
                {
                    Logger.Warning("OpenGL extension SGIS_texture_edge_clamp unavailable\n" +
                                   "Artifacts may occur along texture edges\n" +
                                   "Ensure that hardware acceleration is enabled if available.");
                }
            }

            if (!_textureSrgbChecked)
            {
                _textureSrgb = GLExtensions.IsAvailable("GL_EXT_texture_sRGB");
            }

            if (_sRgb && !_textureSrgb)
            {
                if (!_textureSrgbWarned)
                {
                    _textureSrgbWarned = true;
                    Logger.Warning("OpenGL extension EXT_texture_sRGB unavailable\n" +
                                   "Automatic sRGB to linear conversion disable.");
                }

                _sRgb = false;
            }

            // Initialize the texture
            GLChecker.Check(() => GL.BindTexture(TextureTarget.Texture2D, _textureId));
            GLChecker.Check(() => GL.TexImage2D(TextureTarget.Texture2D, 0, (_sRgb ? PixelInternalFormat.Srgb8Alpha8 : PixelInternalFormat.Rgba),
                                                _actualSize.Width, _actualSize.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero));

            GLChecker.Check(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                (int)(_isRepeated ? TextureWrapMode.Repeat : (_textureEdgeClamp ? TextureWrapMode.ClampToEdge : TextureWrapMode.Clamp))));
            GLChecker.Check(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                (int)(_isRepeated ? TextureWrapMode.Repeat : (_textureEdgeClamp ? TextureWrapMode.ClampToEdge : TextureWrapMode.Clamp))));

            GLChecker.Check(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)(_isSmooth ? TextureMagFilter.Linear : TextureMagFilter.Nearest)));
            GLChecker.Check(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)(_isSmooth ? TextureMinFilter.Linear : TextureMinFilter.Nearest)));

            _cacheId = GetUniqueId();
            _hasMipmap = false;
        }

        internal void InvalidateMipMap()
        {
            GLChecker.Check(() => GL.BindTexture(TextureTarget.Texture2D, _textureId));
            GLChecker.Check(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                            (int)(_isSmooth ? TextureMinFilter.Linear : TextureMinFilter.Nearest)));

            _hasMipmap = false;
        }

        /// <summary>
        /// Update the texture from an <see cref="Image"/>.
        /// </summary>
        /// <param name="image"><see cref="Image"/> to copy to the texture.</param>
        public void Update(Image image)
        {
            Update(image, 0, 0, image.Width, image.Height);
        }


        /// <summary>
        /// Update the texture from an <see cref="Image"/>.
        /// </summary>
        /// <param name="image"><see cref="Image"/> to copy to the texture.</param>
        /// <param name="x">X offset in the texture where to copy the source image</param>
        /// <param name="y">Y offset in the texture where to copy the source image</param>
        /// <param name="width">Width of the pixel region contained in image.</param>
        /// <param name="height">Height of the pixel region contained in image.</param>
        public void Update(Image image, int x, int y, int width, int height)
        {
            // Check if texture is previously created
            if (_textureId <= 0)
            {
                throw new AccessViolationException("Invalid Texture Handle.");
            }

            // Copy bitmap to specified region
            using (Bitmap bmp = new Bitmap(image))
            {
                BitmapData data = bmp.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly,
                                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GLChecker.Check(() => GL.BindTexture(TextureTarget.Texture2D, _textureId));
                GLChecker.Check(() => GL.TexSubImage2D(TextureTarget.Texture2D, 0,
                    x, y, width, height,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0));

                GLChecker.Check(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int)(_isSmooth ? TextureMinFilter.Linear : TextureMinFilter.Nearest)));

                bmp.UnlockBits(data);

                // Force an OpenGL flush, so that the texture data will appear updated
                // in all contexts immediately (solves problems in multi-threaded apps)
                GLChecker.Check(() => GL.Flush());
            }
        }

        /// <summary>
        /// Update the texture from an array of byte
        /// that contains <see cref="Image"/> pixels.
        /// </summary>
        /// <param name="pixels">An array of bytes containing <see cref="Image"/> pixels.</param>
        public void Update(byte[] pixels)
        {
            Update(pixels, 0, 0, Size.Width, Size.Height);
        }

        /// <summary>
        /// Update the texture from an array of byte
        /// that contains <see cref="Image"/> pixels
        /// with specified coordinate and size.
        /// </summary>
        /// <param name="pixels">An array of bytes containing <see cref="Image"/> pixels.</param>
        /// <param name="x">X offset in the texture where to copy the source pixels.</param>
        /// <param name="y">Y offset in the texture where to copy the source pixels.</param>
        /// <param name="width">Width of the pixel region contained in pixels.</param>
        /// <param name="height">Height of the pixel region contained in pixels.</param>
        public void Update(byte[] pixels, int x, int y, int width, int height)
        {
            // Check if texture is previously created
            if (_textureId <= 0)
            {
                throw new AccessViolationException("Invalid Texture Handle.");
            }

            // Check if specified location isn't exceed current bounds of texture
            if (x + width >= Size.Width || y + height >= Size.Height)
            {
                return;
            }

            // Check whether the pixels isn't empty
            if (pixels == null || pixels.Length == 0)
            {
                return;
            }

            // Copy pixels from the given array to the texture
            GLChecker.Check(() => GL.BindTexture(TextureTarget.Texture2D, _textureId));
            GLChecker.Check(() => GL.TexSubImage2D(TextureTarget.Texture2D, 0,
                x, y, width, height,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte,
                pixels));

            GLChecker.Check(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (_isSmooth ? (int)TextureMinFilter.Linear : (int)TextureMinFilter.Nearest)));

            _hasMipmap     = false;
            _pixelsFlipped = false;
            _cacheId       = GetUniqueId();

            // Force an OpenGL flush, so that the texture data will appear updated
            // in all contexts immediately (solves problems in multi-threaded apps)
            GLChecker.Check(() => GL.Flush());
        }

        /// <summary>
        /// Copy the <see cref="Texture"/> pixels to an <see cref="Image"/> instance.
        /// </summary>
        /// <returns><see cref="Image"/> containing the texture's pixels.</returns>
        public Image ToImage()
        {
            // Check for invalid / empty texture, just return empty bitmap
            if (_textureId <= 0 || (Size.Width == 0 && Size.Height == 0))
            {
                return new Bitmap(Size.Width, Size.Height);
            }

            // Setup Bitmap data and use lockbit method to copy the pixels later
            Bitmap bmp = new Bitmap(Size.Width, Size.Height);
            BitmapData data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Copy the texture pixels into bitmap data
            GLChecker.Check(() => GL.BindTexture(TextureTarget.Texture2D, _textureId));
            GLChecker.Check(() => GL.GetTexImage(TextureTarget.Texture2D,
                0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0));

            // Pixels copied, Unlock it
            bmp.UnlockBits(data);

            if (_pixelsFlipped)
            {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }

            return bmp;
        }


        /// <summary>
        /// Generates a MipMap using the current <see cref="Texture"/> data.
        /// </summary>
        /// <returns></returns>
        public void GenerateMipMap()
        {
            if (!GLExtensions.IsAvailable("GL_EXT_framebuffer_object"))
            {
                return;
            }

            GLChecker.Check(() => GL.BindTexture(TextureTarget.Texture2D, _textureId));
            GLChecker.Check(() => GL.GenerateMipmap(GenerateMipmapTarget.Texture2D));
            GLChecker.Check(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)(_isSmooth ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.NearestMipmapLinear)));

            _hasMipmap = true;
            return;
        }

        

        /// <summary>
        /// Releases all resources used by the <see cref="Texture"/>.
        /// </summary>
        public void Dispose()
        {
            if (_textureId > 0)
            {
                GLChecker.Check(() => GL.DeleteTexture(_textureId));
            }
        }

        /// <summary>
        /// Bind the existing <see cref="Texture"/> to texturing target.
        /// If the <see cref="Texture"/> is null, it will unbind any <see cref="Texture"/> from texturing target. 
        /// </summary>
        /// <param name="texture">Texture to bind.</param>
        public static void Bind(Texture texture, CoordinateType type = CoordinateType.Pixel)
        {
            if (texture != null && texture._textureId > 0)
            {
                Matrix4 matrix = new Matrix4
                                 (1f, 0f, 0f, 0f,
                                  0f, 1f, 0f, 0f,
                                  0f, 0f, 1f, 0f,
                                  0f, 0f, 0f, 1f);

                // Convert to normalized one ([0..1])
                if (type == CoordinateType.Pixel)
                {
                    matrix[0, 0] = 1f / texture.Size.Width;
                    matrix[1, 1] = 1f / texture.Size.Height;
                }

                if (texture._pixelsFlipped)
                {
                    matrix[1, 1] = -matrix[1, 1];
                    matrix[3, 1] = texture.Size.Height / texture._actualSize.Height;
                }

                // Bind the texture
                GLChecker.Check(() => GL.BindTexture(TextureTarget.Texture2D, texture._textureId));

                // Apply the texture matrix
                GLChecker.Check(() => GL.MatrixMode(MatrixMode.Texture));
                GLChecker.Check(() => GL.LoadMatrix(ref matrix));

                // Back to ModelView to prevent problems with the RenderTarget
                GLChecker.Check(() => GL.MatrixMode(MatrixMode.Modelview));
            }
            else
            {
                // Bind no texture (Unbind)
                GLChecker.Check(() => GL.BindTexture(TextureTarget.Texture2D, 0));

                // Reset Texture Matrix
                GLChecker.Check(() => GL.MatrixMode(MatrixMode.Texture));
                GLChecker.Check(() => GL.LoadIdentity());

                // RenderTarget always expect ModelView mode, so let's change it back
                GLChecker.Check(() => GL.MatrixMode(MatrixMode.Modelview));
            }
        }

        /// <summary>
        /// Get the maximum texture size allowed.
        /// </summary>
        /// <returns>Maximum size allowed for textures, in pixels.</returns>
        internal static int GetMaximumSize()
        {
            if (!_maxSizeChecked)
            {
                _maxSizeChecked = true;
                GLChecker.Check(() => GL.GetInteger(GetPName.MaxTextureSize, out _maxSize));
            }

            return _maxSize;
        }

        /// <summary>
        /// Get a valid image size according to hardware support.
        /// </summary>
        /// <param name="size">The size to convert.</param>
        /// <returns>Valid nearest size (greater than or equal to specified size).</returns>
        internal static int GetValidSize(int size)
        {
            if (GLExtensions.IsAvailable("GL_ARB_texture_non_power_of_two"))
            {
                return size;
            }
            else
            {
                // If hardware doesn't support NPOT textures, we calculate the nearest power of two
                // However, we just need to return the size if the size is POT to prevent performance issue

                if (size % 2 == 0) return size;

                int powerOfTwo = 1;
                while (powerOfTwo < size)
                {
                    powerOfTwo *= 2;
                }

                return powerOfTwo;
            }
        }

        private static long GetUniqueId()
        {
            lock (_idMutex)
            {
                // start at 1, zero is "no texture"
                return _uId++;
            }
        }
    }
}
