using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using Genode;
using Genode.Window;

namespace Genode.Graphics
{
    public class RenderTexture : RenderTarget, IDisposable
    {
        private IRenderTextureImplementation _implementation;

        public Texture Texture
        {
            get;
            private set;
        }

        public bool IsSmooth
        {
            get
            {
                return Texture.IsSmooth;
            }
            set
            {
                Texture.IsSmooth = value;
            }
        }

        public bool IsRepeated
        {
            get
            {
                return Texture.IsRepeated;
            }
            set
            {
                Texture.IsRepeated = value;
            }
        }

        public override Size Size
        {
            get
            {
                return new Size(Texture.Width, Texture.Height);
            }
        }

        public RenderTexture(int width, int height, bool useDepthBuffer = false)
            : base()
        {
            Texture = new Texture(width, height);
            IsSmooth = false;

            if (FrameBufferRenderer.IsAvailable)
            {
                _implementation = new FrameBufferRenderer(width, height, Texture.Handle, useDepthBuffer);
            }
            else
            {
                _implementation = new LegacyFrameRenderer(width, height, Texture.Handle, useDepthBuffer);
            }

            Texture.IsFboAttachment = true;
            Initialize();
        }

        protected override bool Activate()
        {
            _implementation.Activate();
            return true;
        }

        public override void Display()
        {
            _implementation.Deactivate();

            _implementation.UpdateTexture(Texture.Handle);
            Texture.IsPixelsFlipped = true;
            Texture.InvalidateMipMap();
        }

        public void GenerateMipMap()
        {
            Texture.GenerateMipMap();
        }

        public override void Dispose()
        {
            base.Dispose();
            Dispose(false);
        }

        public void Dispose(bool disposeTexture)
        {
            if (disposeTexture)
            {
                Texture?.Dispose();
            }

            _implementation.Dispose();
        }
    }
}
