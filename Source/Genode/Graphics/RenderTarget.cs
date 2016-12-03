using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Genode;
using Genode.Entities;
using Genode.Internal.OpenGL;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Genode.Graphics
{
    public abstract class RenderTarget : IDisposable
    {
        /// <summary>
        /// Represents Render States Cache.
        /// </summary>
        protected struct StatesCache
        {
            /// <summary>
            /// The maximum size of Vertex cache.
            /// </summary>
            public const int VertexCacheSize = 4;

            /// <summary>
            /// A value indicating that the internal GL states is set.
            /// </summary>
            public bool GlStatesSet;

            /// <summary>
            /// A value indicating whether the current view has changed since last draw.
            /// </summary>
            public bool ViewChanged;

            /// <summary>
            /// Cached blending mode.
            /// </summary>
            public BlendMode LastBlendMode;

            /// <summary>
            /// Cached Texture.
            /// </summary>
            public long LastTextureId;

            /// <summary>
            /// A value indicating whether the previous rendering uses vertex cache.
            /// </summary>
            public bool UseVertexCache;

            /// <summary>
            /// Pre-transformed vertices cache
            /// </summary>
            public Vertex[] VertexCache;
        };

        private static bool _isBlendingWarned = false;

        private View _defaultView, _view;
        protected StatesCache Cache;
        protected IRendererImplementation Implementation;

        /// <summary>
        /// Gets the size of the rendering region of current <see cref="RenderTarget"/> object.
        /// </summary>
        public abstract Size Size
        {
            get;
        }


        /// <summary>
        /// Gets or sets the view currently in use in current <see cref="RenderTarget"/> object.
        /// </summary>
        public View View
        {
            get { return _view; }
            set
            {
                _view = value;
                Cache.ViewChanged = true;
            }
        }

        /// <summary>
        /// Gets the default view for current <see cref="RenderTarget"/> object.
        /// </summary>
        public View DefaultView
        {
            get { return _defaultView; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTarget"/> class.
        /// </summary>
        public RenderTarget()
        {
            _defaultView = new View();
            _view = new View();

            Cache = new StatesCache();
            Cache.VertexCache = new Vertex[StatesCache.VertexCacheSize];
            Cache.GlStatesSet = false;
        }

        private void ApplyCurrentView()
        {
            // Check whether the view is null, use default in case it is null
            if (View == null)
            {
                View = DefaultView;
            }

            // Set the viewport
            Rectangle viewport = GetViewport(View);
            int top = Size.Height - (viewport.Y + viewport.Height);
            GLChecker.Check(() => GL.Viewport(viewport.X, top, viewport.Width, viewport.Height));

            // Set the projection matrix
            GLChecker.Check(() => GL.MatrixMode(MatrixMode.Projection));
            GLChecker.Check(() => GL.LoadMatrix(View.Transform.Matrix));

            // Go back to model-view mode
            GLChecker.Check(() => GL.MatrixMode(MatrixMode.Modelview));

            // View is applied, therefore set the flag
            Cache.ViewChanged = false;
        }

        private void ApplyBlendMode(BlendMode mode)
        {
            if (GLExtensions.IsAvailable("GL_EXT_blend_func_separate"))
            {
                GLChecker.Check(() =>
                    GL.BlendFuncSeparate((BlendingFactorSrc)FactorToGL(mode.ColorSrcFactor), (BlendingFactorDest)FactorToGL(mode.ColorDstFactor),
                                         (BlendingFactorSrc)FactorToGL(mode.AlphaSrcFactor), (BlendingFactorDest)FactorToGL(mode.AlphaDstFactor))
                );
            }
            else
            {
                var src = (BlendingFactorSrc)FactorToGL(mode.ColorSrcFactor);
                var dst = (BlendingFactorDest)FactorToGL(mode.ColorDstFactor);
                GLChecker.Check(() =>
                    GL.BlendFunc(src, dst)
                );
            }

            if (GLExtensions.IsAvailable("GL_EXT_blend_minmax") && GLExtensions.IsAvailable("GL_EXT_blend_subtract"))
            {
                if (GLExtensions.IsAvailable("GL_EXT_blend_equation_separate"))
                {
                    GLChecker.Check(() =>
                        GL.Ext.BlendEquationSeparate(
                            (BlendEquationModeExt)EquationToGL(mode.ColorEquation),
                            (BlendEquationModeExt)EquationToGL(mode.AlphaEquation))
                    );
                }
                else
                {
                    GLChecker.Check(() =>
                        GL.Ext.BlendEquation(EquationToGL(mode.ColorEquation))
                    );
                }
            }
            else if ((mode.ColorEquation != BlendMode.Equation.Add) || (mode.AlphaEquation != BlendMode.Equation.Add))
            {
                if (!_isBlendingWarned)
                {
                    Logger.Warning("OpenGL extension EXT_blend_minmax and / or EXT_blend_subtract unavailable.\n" +
                                   "Selecting a blend equation is not possible\n" +
                                   "Ensure that hardware acceleration is enabled if available.");
                    _isBlendingWarned = true;
                }
            }

            Cache.LastBlendMode = mode;
        }

        private void ApplyTransform(Transform transform)
        {
            // ModelView is always preferred in all cases of modules.
            // All operations that interact with the matrix will switch back the mode to ModelView.
            // So no need to change the matrix mode to ModelView.

            // Null transform should be replaced by identity transform
            if (transform == null)
            {
                transform = Transform.Identity;
            }

            // Apply Transformation Matrix
            GLChecker.Check(() => GL.LoadMatrix(transform.Matrix));
        }

        private void ApplyTexture(Texture texture)
        {
            Texture.Bind(texture, Texture.CoordinateType.Pixel);
            Cache.LastTextureId = (texture != null) ? texture.CacheId : 0;
        }

        private void ApplyShader(Shader shader)
        {
            Shader.Bind(shader);
        }

        /// <summary>
        /// Activate the target for rendering.
        /// When overidden by derived class,
        /// the corresponding OpenGL context is current in the calling thread.
        /// it is called by the base class everytime it's going to use OpenGL calls.
        /// </summary>
        /// <returns><code>true</code> if the context is activated, otherwise false.</returns>
        protected abstract bool Activate();

        /// <summary>
        /// Performs the common initialization steps.
        /// This function should be called when the derived class ready.
        /// </summary>
        protected virtual void Initialize()
        {
            // Setup the default and current views
            _defaultView.Reset(new RectangleF(0, 0, Size.Width, Size.Height));
            _view = _defaultView;

            // Set GL states only on first draw, so that we don't pollute user's states
            Cache.GlStatesSet = false;

            // TODO: Should we prefer to VAO instead of VBO?
            if (VertexArrayRenderer.IsAvailable)
                Implementation = new VertexArrayRenderer();
            else if (VertexBufferRenderer.IsAvailable)
                Implementation = new VertexBufferRenderer();
            else
                Implementation = new LegacyRenderer();
        }

        /// <summary>
        /// Reset current internal states of OpenGL to default.
        /// </summary>
        protected void Reset()
        {
            // Disable Unused features
            GLChecker.Check(() => GL.Disable(EnableCap.CullFace));
            GLChecker.Check(() => GL.Disable(EnableCap.Lighting));
            GLChecker.Check(() => GL.Disable(EnableCap.DepthTest));
            GLChecker.Check(() => GL.Disable(EnableCap.AlphaTest));

            // Enable required features
            GLChecker.Check(() => GL.Enable(EnableCap.Texture2D));
            GLChecker.Check(() => GL.Enable(EnableCap.Blend));

            // Enable Client State of Vertex Pointers
            GLChecker.Check(() => GL.EnableClientState(ArrayCap.VertexArray));
            GLChecker.Check(() => GL.EnableClientState(ArrayCap.TextureCoordArray));
            GLChecker.Check(() => GL.EnableClientState(ArrayCap.ColorArray));

            // Switch to default matrix mode and reset it
            GLChecker.Check(() => GL.MatrixMode(MatrixMode.Modelview));
            GLChecker.Check(() => GL.LoadIdentity());

            // OpenGL States Initialized
            Cache.GlStatesSet = true;

            // Apply default engine states
            ApplyBlendMode(BlendMode.Alpha);
            ApplyTexture(null);
            ApplyTransform(Transform.Identity);
            if (Shader.IsAvailable)
                ApplyShader(null);
            Cache.UseVertexCache = false;

            // Re-apply the view.
            View = View;
        }

        /// <summary>
        /// Swap the buffer of the <see cref="RenderTarget"/>.
        /// When overidden by derived class,
        /// rendered objects will be applied to the surface of current <see cref="RenderTarget"/> object.
        /// </summary>
        public abstract void Display();

        /// <summary>
        /// Push all current OpenGL States.
        /// </summary>
        public void PushGLStates()
        {
#if DEBUG
            var error = GLChecker.GetError();
            if (error != ErrorCode.NoError)
            {
                Logger.Warning("OpenGL Error ({0}) has detected in user code.\n" +
                               "Make sure to check the error when using custom OpenGL codes.", error);
            }
#endif
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.MatrixMode(MatrixMode.Texture);
            GL.PushMatrix();

            // Reset GL States
            Reset();
        }

        /// <summary>
        /// Pop all previous OpenGL States.
        /// </summary>
        public void PopGLStates()
        {
#if DEBUG
            var error = GLChecker.GetError();
            if (error != ErrorCode.NoError)
            {
                Logger.Warning("OpenGL Error ({1}) has detected in user code.\n" +
                               "Make sure to check the error when using custom OpenGL codes.", error);
            }
#endif
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Texture);
            GL.PopMatrix();

            // Reset GL States
            Reset();
        }

        /// <summary>
        /// Clear the entire target with a single color.
        /// </summary>
        /// <param name="color">Fill <see cref="Color"/> to use to clear the render target.</param>
        public void Clear(Color color)
        {
            if (Activate())
            {
                ApplyTexture(null);

                GLChecker.Check(() => GL.ClearColor(color));
                GLChecker.Check(() => GL.Clear(ClearBufferMask.ColorBufferBit));
            }
        }

        /// <summary>
        /// Get the viewport of a view, applied to current <see cref="RenderTarget"/> object.
        /// </summary>
        /// <param name="view">The view for which we want to compute the viewport.</param>
        /// <returns>Viewport rectangle, expressed in pixels.</returns>
        public Rectangle GetViewport(View view)
        {
            float width = (float)Size.Width;
            float height = (float)Size.Height;
            RectangleF viewport = view.Viewport;

            return new Rectangle((int)(0.5f + width * viewport.Left),
                                 (int)(0.5f + height * viewport.Top),
                                 (int)(0.5f + width * viewport.Width),
                                 (int)(0.5f + height * viewport.Height));
        }
        /// <summary>
        /// Convert a point from target coordinates to world coordinates using the current <see cref="Graphics.View"/>.
        /// </summary>
        /// <param name="point">The pixel to convert.</param>
        /// <returns>The converted point, in "world" coordinates.</returns>
        public Vector2 MapPixelToCoords(Vector2 point)
        {
            return MapPixelToCoords(point, View);
        }

        /// <summary>
        /// Convert a point from target coordinates to world coordinates
        /// with specified <see cref="Graphics.View"/>.
        /// </summary>
        /// <param name="point">The pixel to convert.</param>
        /// <param name="view">The view to use for converting the point.</param>
        /// <returns>The converted point, in "world" units.</returns>
        public Vector2 MapPixelToCoords(Vector2 point, View view)
        {
            Vector2 normalized;
            Rectangle viewport = GetViewport(view);
            normalized.X = -1f + 2f * (point.X - viewport.X) / viewport.Width;
            normalized.Y = 1f - 2f * (point.Y - viewport.Y) / viewport.Height;

            return view.Transform.GetInverse().TransformPoint(normalized);
        }

        /// <summary>
        /// Convert a point from world coordinates to target coordinates using the current <see cref="Graphics.View"/>.
        /// </summary>
        /// <param name="point">The point to convert.</param>
        /// <returns>The converted point, in target coordinates (pixels).</returns>
        public Vector2 MapCoordsToPixel(Vector2 point)
        {
            return MapCoordsToPixel(point, View);
        }

        /// <summary>
        /// Convert a point from world coordinates to target coordinates.
        /// </summary>
        /// <param name="point">The point to convert.</param>
        /// <param name="view">The view to use for converting the point.</param>
        /// <returns></returns>
        public Vector2 MapCoordsToPixel(Vector2 point, View view)
        {
            Vector2 normalized = view.Transform.TransformPoint(point);

            Vector2 pixel;
            Rectangle viewport = GetViewport(view);
            pixel.X = ((normalized.X + 1f) / 2f * viewport.Width + viewport.X);
            pixel.Y = ((-normalized.Y + 1f) / 2f * viewport.Height + viewport.Y);

            return pixel;
        }

        /// <summary>
        /// Render a <see cref="IRenderable"/> object to the current <see cref="RenderTarget"/> object.
        /// </summary>
        /// <param name="renderable">The <see cref="IRenderable"/> object to render.</param>
        public void Render(IRenderable renderable)
        {
            Render(renderable, RenderStates.Default);
        }

        /// <summary>
        /// Render a <see cref="IRenderable"/> object to the current <see cref="RenderTarget"/> object.
        /// </summary>
        /// <param name="renderable">The <see cref="IRenderable"/> object to render.</param>
        /// <param name="states">The <see cref="RenderStates"/> parameter to be used on rendering.</param>
        public void Render(IRenderable renderable, RenderStates states)
        {
            renderable.Render(this, states);
        }

        /// <summary>
        /// Render vertices to the current <see cref="RenderTarget"/> object.
        /// </summary>
        /// <param name="vertices">The vertices to render.</param>
        public void Render(VertexArray vertices)
        {
            Render(vertices, vertices.Type, RenderStates.Default);
        }

        /// <summary>
        /// Render vertices to the current <see cref="RenderTarget"/> object.
        /// </summary>
        /// <param name="vertices">The vertices to render.</param>
        /// <param name="states">The <see cref="RenderStates"/> parameter to be used on rendering.</param>
        public void Render(VertexArray vertices, RenderStates states)
        {
            Render(vertices, vertices.Type, states);
        }

        /// <summary>
        /// Render vertices to the current <see cref="RenderTarget"/> object.
        /// </summary>
        /// <param name="vertices">The vertices to render.</param>
        /// <param name="type">Primitive type of vertices.</param>
        public void Render(Vertex[] vertices, PrimitiveType type)
        {
            Render(vertices, type, RenderStates.Default);
        }

        /// <summary>
        /// Render vertices to the current <see cref="RenderTarget"/> object.
        /// </summary>
        /// <param name="vertices">The vertices to render.</param>
        /// <param name="type">Primitive type of vertices.</param>
        /// <param name="states">The <see cref="RenderStates"/> parameter to be used on rendering.</param>
        public void Render(Vertex[] vertices, PrimitiveType type, RenderStates states)
        {
            // Nothing to draw
            if (vertices == null || vertices.Length == 0)
            {
                return;
            }

            // Check whether the implementation is set by derived class
            if (Implementation == null)
            {
                throw new SystemException("Implementation is not defined.\n" +
                    "Derived class of RenderTarget must specify Implementation upon Initialize() function is called.");
            }

            if (Activate())
            {
                // First set the persistent OpenGL states if it's the very first call
                if (!Cache.GlStatesSet)
                    Reset();

                // Check if the vertex count is low enough so that we can pre-transform them
                bool useVertexCache = (vertices.Length <= StatesCache.VertexCacheSize);
                if (useVertexCache)
                {
                    // Pre-transform the vertices and store them into the vertex cache
                    for (int i = 0; i < vertices.Length; ++i)
                    {
                        Cache.VertexCache[i].position  = states.Transform * vertices[i].position;
                        Cache.VertexCache[i].texCoords = vertices[i].texCoords;
                        Cache.VertexCache[i].color     = vertices[i].color;
                    }

                    // Since vertices are transformed, we must use an identity transform to render them
                    if (!Cache.UseVertexCache)
                        ApplyTransform(Transform.Identity);
                }
                else
                {
                    ApplyTransform(states.Transform);
                }

                // Apply the view
                if (Cache.ViewChanged)
                    ApplyCurrentView();

                // Apply the blend mode
                if (states.BlendMode != Cache.LastBlendMode)
                    ApplyBlendMode(states.BlendMode);

                // Apply the texture
                long textureId = states.Texture != null ? states.Texture.CacheId : 0;
                if (textureId != Cache.LastTextureId)
                    ApplyTexture(states.Texture);

                // Apply the shader
                if (states.Shader != null)
                    ApplyShader(states.Shader);

                // If we pre-transform the vertices, we must use our internal vertex cache
                if (useVertexCache)
                {
                    // ... and if we already used it previously, we don't need to set the pointers again
                    if (!Cache.UseVertexCache)
                        vertices = Cache.VertexCache;
                    else
                        vertices = null;
                }

                // Setup the pointers to the vertices' components
                if (vertices != null)
                {
                    Implementation.Update(vertices);
                }

                // Render the primitives
                Implementation.Render(type);

                // Unbind the shader, if any
                if (states.Shader != null)
                    ApplyShader(null);

                // If the texture we used to draw belonged to a RenderTexture, then forcibly unbind that texture.
                // This prevents a bug where some drivers do not clear RenderTextures properly.
                if (states.Texture != null && states.Texture.IsFboAttachment)
                    ApplyTexture(null);

                // Update the cache
                Cache.UseVertexCache = useVertexCache;
            }
        }

        /// <summary>
        /// Releases all resources used by the <see cref="RenderTarget"/>.
        /// </summary>
        public virtual void Dispose()
        {
            Implementation?.Dispose();
        }

        /// <summary>
        /// Convert <see cref="BlendMode.Factor"/> to the corresponding OpenGL constant.
        /// </summary>
        /// <param name="factor">The <see cref="BlendMode.Factor"/> to be converted.</param>
        /// <returns>OpenGL constant.</returns>
        protected static int FactorToGL(BlendMode.Factor factor)
        {
            switch (factor)
            {
                case BlendMode.Factor.Zero:             return 0;
                case BlendMode.Factor.One:              return 1;
                case BlendMode.Factor.SrcColor:         return (int)BlendingFactorSrc.SrcColor;
                case BlendMode.Factor.OneMinusSrcColor: return (int)BlendingFactorSrc.OneMinusSrcColor;
                case BlendMode.Factor.DstColor:         return (int)BlendingFactorDest.DstColor;
                case BlendMode.Factor.OneMinusDstColor: return (int)BlendingFactorDest.OneMinusDstColor;
                case BlendMode.Factor.SrcAlpha:         return (int)BlendingFactorSrc.SrcAlpha;
                case BlendMode.Factor.OneMinusSrcAlpha: return (int)BlendingFactorSrc.OneMinusSrcAlpha;
                case BlendMode.Factor.DstAlpha:         return (int)BlendingFactorDest.DstAlpha;
                case BlendMode.Factor.OneMinusDstAlpha: return (int)BlendingFactorDest.OneMinusDstAlpha;
            }

            Logger.Warning("Invalid value for BlendMode.Factor. Fallback to BlendMode.Factor.Zero.");
            return 0;
        }

        /// <summary>
        /// Convert <see cref="BlendMode.Equation"/> to the corresponding OpenGL constant.
        /// </summary>
        /// <param name="equation">The <see cref="BlendMode.Factor"/> to be converted.</param>
        /// <returns>OpenGL constant.</returns>
        protected BlendEquationMode EquationToGL(BlendMode.Equation equation)
        {
            switch (equation)
            {
                case BlendMode.Equation.Add:                return BlendEquationMode.FuncAdd;
                case BlendMode.Equation.Subtract:           return BlendEquationMode.FuncSubtract;
                case BlendMode.Equation.ReverseSubtract:    return BlendEquationMode.FuncReverseSubtract;
            }

            Logger.Warning("Invalid value for BlendMode.Equation. Fallback to BlendMode.Equation.Add.");
            return BlendEquationMode.FuncAdd;
        }
    }
}
