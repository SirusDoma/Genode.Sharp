using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Genode;
using Genode.Internal;
using Genode.Internal.OpenGL;

namespace Genode.Graphics
{
    public class LegacyFrameRenderer : IRenderTextureImplementation
    {
        private int _width, _height;

        public LegacyFrameRenderer(int width, int height, int textureId, bool useDepthBuffer)
        {
            _width = width;
            _height = height;
        }

        public void Activate()
        {
            GLChecker.Check(() => GL.Flush());   
        }

        public void Deactivate()
        {
        }

        public void UpdateTexture(int textureId)
        {
            GLChecker.Check(() => GL.BindTexture(TextureTarget.Texture2D, textureId));
            GLChecker.Check(() => GL.CopyTexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 0, 0, _width, _height));
        }

        public void Dispose()
        {
        }
    }
}
