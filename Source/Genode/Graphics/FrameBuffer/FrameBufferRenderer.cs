using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Genode;
using Genode.Internal.OpenGL;

namespace Genode.Graphics
{
    public sealed class FrameBufferRenderer : IRenderTextureImplementation
    {
        private static bool isCompatibilityChecked, isSupported;

        public static bool IsAvailable
        {
            get
            {
                if (!isCompatibilityChecked)
                {
                    isSupported = GLExtensions.IsAvailable("GL_framebuffer_object") ||
                                  GLExtensions.IsAvailable("GL_EXT_framebuffer_object");
                    isCompatibilityChecked = true;
                }

                return isSupported;
            }
        }

        private int _frameBuffer, _depthBuffer;

        public FrameBufferRenderer(int width, int height, int textureId, bool useDepthBuffer)
        {
            // Create the framebuffer object
            GLChecker.Check(() => _frameBuffer = GL.Ext.GenFramebuffer());
            if (_frameBuffer <= 0)
                throw new InvalidOperationException("Failed to create Frame Buffer handle.");
            GLChecker.Check(() => GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, _frameBuffer));

            // Create the depth buffer if requested
            if (useDepthBuffer)
            {
                _depthBuffer = GL.Ext.GenFramebuffer();

                if (_depthBuffer <= 0)
                    throw new InvalidOperationException("Failed to create Frame Buffer handle.");

                GLChecker.Check(() => GL.Ext.BindRenderbuffer(RenderbufferTarget.RenderbufferExt, _depthBuffer));
                GLChecker.Check(() => GL.Ext.RenderbufferStorage(RenderbufferTarget.RenderbufferExt, RenderbufferStorage.DepthComponent, width, height));
                GLChecker.Check(() => GL.Ext.FramebufferRenderbuffer(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, RenderbufferTarget.RenderbufferExt, _depthBuffer));
            }

            // Link the texture to the frame buffer
            GLChecker.Check(() => GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, textureId, 0));

            // A final check, just to be sure...
            var status = GL.Ext.CheckFramebufferStatus(FramebufferTarget.FramebufferExt);
            if (status != FramebufferErrorCode.FramebufferCompleteExt)
            {
                GLChecker.Check(() => GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0));
                throw new InvalidOperationException("Failed to create Frame Buffer handle.");
            }
        }

        public void Activate()
        {
            GLChecker.Check(() => GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, _frameBuffer));
        }

        public void Deactivate()
        {
            GLChecker.Check(() => GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0));
            GLChecker.Check(() => GL.DrawBuffer(DrawBufferMode.Back));
        }

        public void UpdateTexture(int textureId)
        {
            GLChecker.Check(() => GL.Flush());
        }

        public void Dispose()
        {
            // Return to visible frame buffer
            GLChecker.Check(() => GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0));
            GLChecker.Check(() => GL.DrawBuffer(DrawBufferMode.Back));

            if (_frameBuffer > 0)
            {
                GLChecker.Check(() => GL.Ext.DeleteFramebuffer(_frameBuffer));
            }

            if (_depthBuffer > 0)
            {
                GLChecker.Check(() => GL.Ext.DeleteFramebuffer(_depthBuffer));
            }
        }
    }
}
