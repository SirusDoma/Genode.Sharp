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
    /// <summary>
    /// Represents a program that  executed directly by the graphics card.
    /// This is allow the rendered entities to apply real-time operations.
    /// </summary>
    public partial class Shader
    {
        private static bool isChecked = false;
        private static bool isAvailable = false;
        private static bool isGeometryChecked = false;
        private static bool isGeometryAvailable = false;

        /// <summary>
        /// Gets a value indicating whether the <see cref="Shader"/> is available for this system.
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
                // Check the compatibility once, then return the cached one
                if (!isChecked)
                {
                    isChecked = true;
                    isAvailable = (GLExtensions.IsAvailable("GL_ARB_multitexture") || GLExtensions.IsAvailable("GL_EXT_multitexture")) &&
                                  (GLExtensions.IsAvailable("GL_ARB_shading_language_100") || GLExtensions.IsAvailable("GL_EXT_shading_language_100")) &&
                                  (GLExtensions.IsAvailable("GL_ARB_vertex_shader") || GLExtensions.IsAvailable("GL_EXT_vertex_shader")) &&
                                  (GLExtensions.IsAvailable("GL_ARB_fragment_shader") || GLExtensions.IsAvailable("GL_EXT_fragment_shader"));

                    //isAvailable = new Version(GL.GetString(StringName.Version).Substring(0, 3)) >= new Version(2, 0) ? true : false;
                }

                return isAvailable;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Geometry <see cref="Shader"/> is available for this system.
        /// </summary>
        public static bool IsGeometryAvailable
        {
            get
            {
                // Check the compatibility once, then return the cached one
                if (!isGeometryChecked)
                {
                    isGeometryChecked = true;
                    isGeometryAvailable = IsAvailable && 
                            (GLExtensions.IsAvailable("GL_ARB_geometry_shader4") || GLExtensions.IsAvailable("GL_EXT_geometry_shader4"));
                }

                return isGeometryAvailable;
            }
        }

        private int _program;
        private int _currentTexture;
        private Dictionary<int, Texture> _textures;
        private Dictionary<string, int>  _uniforms;

        /// <summary>
        /// Gets the OpenGL <see cref="Shader"/> Handle.
        /// </summary>
        public int Handle
        {
            get { return _program; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Shader"/> class
        /// from specified source.
        /// </summary>
        public Shader(string source, ShaderType type)
        {
            switch(type)
            {
                case ShaderType.Vertex   : Compile(source, string.Empty, string.Empty); break;
                case ShaderType.Geometry : Compile(string.Empty, source, string.Empty); break;
                case ShaderType.Fragment : Compile(string.Empty, string.Empty, source); break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Shader"/> class
        /// from Vertex and Fragment source.
        /// </summary>
        public Shader(string vertexSource, string fragmentSource)
        {
            Compile(vertexSource, string.Empty, fragmentSource);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Shader"/> class
        /// from Vertex, Geometry and Fragment source.
        /// </summary>
        public Shader(string vertexSource, string geometrySource, string fragmentSource)
        {
            Compile(vertexSource, geometrySource, fragmentSource);
        }

        /// <summary>
        /// Specify value for one component uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="x">Value of the scalar.</param>
        public void SetUniform(string name, int x)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                    GLChecker.Check(() => GL.Uniform1(binder.Location, x));
            }
        }

        /// <summary>
        /// Specify value for one component uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="x">Value of the scalar.</param>
        public void SetUniform(string name, float x)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                    GLChecker.Check(() => GL.Uniform1(binder.Location, x));
            }
        }

        /// <summary>
        /// Specify value for one component uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="x">Value of the scalar.</param>
        public void SetUniform(string name, double x)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                    GLChecker.Check(() => GL.Uniform1(binder.Location, x));
            }
        }

        /// <summary>
        /// Specify value for two component uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="vector">Value of the scalar.</param>
        public void SetUniform(string name, Vector2 vector)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                    GLChecker.Check(() => GL.Uniform2(binder.Location, vector.X, vector.Y));
            }
        }

        /// <summary>
        /// Specify value for three component uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="vector">Value of the scalar.</param>
        public void SetUniform(string name, Vector3 vector)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                    GLChecker.Check(() => GL.Uniform3(binder.Location, vector.X, vector.Y, vector.Z));
            }
        }

        /// <summary>
        /// Specify value for four component uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="vector">Value of the scalar.</param>
        public void SetUniform(string name, Vector4 vector)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                    GLChecker.Check(() => GL.Uniform4(binder.Location, vector.X, vector.Y, vector.Z, vector.W));
            }
        }

        /// <summary>
        /// Specify value for two component uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="vector">Value of the scalar.</param>
        public void SetUniform(string name, Vector2i vector)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                    GLChecker.Check(() => GL.Uniform2(binder.Location, vector.X, vector.Y));
            }
        }

        /// <summary>
        /// Specify value for two component uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="vector">Value of the scalar.</param>
        public void SetUniform(string name, Vector2<double> vector)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                    GLChecker.Check(() => GL.Uniform2(binder.Location, vector.X, vector.Y));
            }
        }

        /// <summary>
        /// Specify value for three component uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="vector">Value of the scalar.</param>
        public void SetUniform(string name, Vector3i vector)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                    GLChecker.Check(() => GL.Uniform3(binder.Location, vector.X, vector.Y, vector.Z));
            }
        }

        /// <summary>
        /// Specify value for three component uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="vector">Value of the scalar.</param>
        public void SetUniform(string name, Vector3<double> vector)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                    GLChecker.Check(() => GL.Uniform3(binder.Location, vector.X, vector.Y, vector.Z));
            }
        }

        /// <summary>
        /// Specify value for four component uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="vector">Value of the scalar.</param>
        public void SetUniform(string name, Vector4i vector)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                    GLChecker.Check(() => GL.Uniform4(binder.Location, vector.X, vector.Y, vector.Z, vector.W));
            }
        }

        /// <summary>
        /// Specify value for four component uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="vector">Value of the scalar.</param>
        public void SetUniform(string name, Vector4<double> vector)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                    GLChecker.Check(() => GL.Uniform4(binder.Location, vector.X, vector.Y, vector.Z, vector.W));
            }
        }

        /// <summary>
        /// Specify value for matrix uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="matrix">Value of the scalar.</param>
        public void SetUniform(string name, float[] matrix)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                {
                    if (matrix.Length == 9)
                    {
                        GLChecker.Check(() => GL.UniformMatrix3(binder.Location, 1, false, matrix));
                    }
                    else if (matrix.Length == 16)
                    {
                        GLChecker.Check(() => GL.UniformMatrix4(binder.Location, 1, true, matrix));
                    }
                    else
                    {
                        throw new ArgumentException("Matrix data must either 3x3 (9 elements) or 4x4 (16 elements).");
                    }
                }
            }
        }

        /// <summary>
        /// Specify a texture as Sampler2D uniform.
        /// </summary>
        /// <param name="name">The name of the <see cref="Texture"/> in the shader.</param>
        /// <param name="texture">The <see cref="Texture"/> to assign.</param>
        public void SetUniform(string name, Texture texture)
        {
            if (_program > 0)
            {
                // Find the location of the variable in the shader
                int location = GetUniformLocation(name);
                if (location != -1)
                {
                    // Store the location -> texture mapping
                    if (!_textures.ContainsKey(location))
                    {
                        // New entry, make sure there are enough texture units
                        int maxUnits = GetMaxTextureUnits();
                        if (_textures.Count + 1 >= maxUnits)
                        {
                            Logger.Warning("Impossible to use texture \"{0}\" for shader.\n" +
                                "all available texture units are used", name);
                            return;
                        }

                        _textures[location] = texture;
                    }
                    else
                    {
                        // Location already used, just replace the texture
                        _textures[location] = texture;
                    }
                }
            }
        }

        /// <summary>
        /// Specify values for array uniform.
        /// </summary>
        /// <param name="name">The name of the uniform variable in GLSL.</param>
        /// <param name="scalarArray">The array of float values.</param>
        public void SetUniformArray(string name, float[] scalarArray)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                    GLChecker.Check(() => GL.Uniform1(binder.Location, scalarArray.Length, scalarArray));
            }
        }


        /// <summary>
        /// Specify values for array uniform.
        /// </summary>
        /// <param name="name">The name of the uniform variable in GLSL.</param>
        /// <param name="vector">The array of vector values.</param>
        public void SetUniformArray(string name, Vector2[] vector)
        {
            var contiguous = vector.Flatten();
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                    GLChecker.Check(() => GL.Uniform2(binder.Location, contiguous.Length, contiguous));
            }
        }

        /// <summary>
        /// Specify values for array uniform.
        /// </summary>
        /// <param name="name">The name of the uniform variable in GLSL.</param>
        /// <param name="vector">The array of vector values.</param>
        public void SetUniformArray(string name, Vector3[] vector)
        {
            var contiguous = vector.Flatten();
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                    GLChecker.Check(() => GL.Uniform3(binder.Location, contiguous.Length, contiguous));
            }
        }

        /// <summary>
        /// Specify values for array uniform.
        /// </summary>
        /// <param name="name">The name of the uniform variable in GLSL.</param>
        /// <param name="vector">The array of vector values.</param>
        public void SetUniformArray(string name, Vector4[] vector)
        {
            var contiguous = vector.Flatten();
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                    GLChecker.Check(() => GL.Uniform4(binder.Location, contiguous.Length, contiguous));
            }
        }

        /// <summary>
        /// Specify values for array uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="vector">Value of the scalar.</param>
        public void SetUniformArray(string name, Vector2i[] vector)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                {
                    int[] contiguous = vector.Flatten();
                    GLChecker.Check(() => GL.Uniform2(binder.Location, contiguous.Length, contiguous));
                }
            }
        }

        /// <summary>
        /// Specify values for array uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="vector">Value of the scalar.</param>
        public void SetUniformArray(string name, Vector2<double>[] vector)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                {
                    double[] contiguous = vector.Flatten();
                    GLChecker.Check(() => GL.Uniform2(binder.Location, contiguous.Length, contiguous));
                }
            }
        }

        /// <summary>
        /// Specify values for array uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="vector">Value of the scalar.</param>
        public void SetUniformArray(string name, Vector3i[] vector)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                {
                    int[] contiguous = vector.Flatten();
                    GLChecker.Check(() => GL.Uniform2(binder.Location, contiguous.Length, contiguous));
                }
            }
        }

        /// <summary>
        /// Specify values for array uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="vector">Value of the scalar.</param>
        public void SetUniformArray(string name, Vector3<double>[] vector)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                {
                    double[] contiguous = vector.Flatten();
                    GLChecker.Check(() => GL.Uniform2(binder.Location, contiguous.Length, contiguous));
                }
            }
        }

        /// <summary>
        /// Specify values for array uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="vector">Value of the scalar.</param>
        public void SetUniformArray(string name, Vector4i[] vector)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                {
                    int[] contiguous = vector.Flatten();
                    GLChecker.Check(() => GL.Uniform2(binder.Location, contiguous.Length, contiguous));
                }
            }
        }

        /// <summary>
        /// Specify values for array uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="vector">Value of the scalar.</param>
        public void SetUniformArray(string name, Vector4<double>[] vector)
        {
            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                {
                    double[] contiguous = vector.Flatten();
                    GLChecker.Check(() => GL.Uniform2(binder.Location, contiguous.Length, contiguous));
                }
            }
        }

        /// <summary>
        /// Specify values for matrix array uniform.
        /// </summary>
        /// <param name="name">The name of uniform variable in GLSL.</param>
        /// <param name="matrix">The array of matrix values.</param>
        public void SetUniformArray(string name, float[][] matrix)
        {
            int matrixSize = 0;
            if (matrix.Length == 3 * 3)
            {
                matrixSize = 3 * 3;
            }
            else if (matrix.Length == 4 * 4)
            {
                matrixSize = 4 * 4;
            }
            else
            {
                throw new ArgumentException("Matrix data must either 3x3 (9 elements) or 4x4 (16 elements).");
            }

            float[] contiguous = new float[matrixSize * matrix.Length];
            for (int i = 0; i < matrix.Length; ++i)
                Array.Copy(matrix[i], 0, contiguous, matrixSize * i, matrixSize);

            using (var binder = new UniformBinder(this, name))
            {
                if (binder.Location != -1)
                    GLChecker.Check(() => GL.Uniform2(binder.Location, contiguous.Length, contiguous));
            }
        }

        /// <summary>
        /// Specify current texture as Sampler2D uniform.
        /// </summary>
        /// <param name="name">The name of the texture in the shader</param>
        public void SetUniform(string name)
        {
            _currentTexture = GetUniformLocation(name);
        }

        private void Compile(string vertexSource, string geometrySource, string fragmentSource)
        {
            // First make sure that we can use shaders
            if (!IsAvailable)
            {
                throw new NotSupportedException("Failed to create a shader. The system doesn't support shaders.\n" +
                                                "Shader.IsAvailable must return true in order to use Shader class.");
            }

            // Make sure that we can make geometry shader if requested
            if (!string.IsNullOrEmpty(geometrySource) && !IsGeometryAvailable)
            {
                throw new NotSupportedException("Failed to create a shader. The system doesn't support geometry shaders.\n" +
                                                "Shader.IsGeometryAvailable must return true in order to use Geometry Shader.");
            }

            // Reset internal state of shader
            _currentTexture = -1;
            _textures = new Dictionary<int, Texture>();
            _uniforms = new Dictionary<string, int>();

            // Create the program
            int shaderProgram = 0;
            GLChecker.Check(() => shaderProgram = GL.CreateProgram());

            // Initialize compilation log info variables
            int statusCode = -1;
            string info = string.Empty;

            // Create vertex shader if requested
            if (!string.IsNullOrEmpty(vertexSource))
            {
                // Create and compile the shader
                int vertexShader = 0;
                GLChecker.Check(() => vertexShader = GL.CreateShader(OpenTK.Graphics.OpenGL.ShaderType.VertexShader));
                GLChecker.Check(() => GL.ShaderSource(vertexShader, vertexSource));
                GLChecker.Check(() => GL.CompileShader(vertexShader));

                // Check the compile log
                GLChecker.Check(() => GL.GetShaderInfoLog(vertexShader, out info));
                GLChecker.Check(() => GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out statusCode));

                // Check the compile log
                if (statusCode != 1)
                {
                    // Delete every handles when compilation failed
                    GLChecker.Check(() => GL.DeleteShader(vertexShader));
                    GLChecker.Check(() => GL.DeleteProgram(shaderProgram));

                    throw new InvalidOperationException("Failed to Compile Vertex Shader Source.\n" +
                        info + "\n\n" +
                        "Status Code: " + statusCode.ToString());
                }

                // Attach the shader to the program, and delete it (not needed anymore)
                GLChecker.Check(() => GL.AttachShader(shaderProgram, vertexShader));
                GLChecker.Check(() => GL.DeleteShader(vertexShader));
            }

            // Create geometry shader if requested
            if (!string.IsNullOrEmpty(geometrySource))
            {
                // Create and compile the shader
                int geometryShader = 0;
                GLChecker.Check(() => geometryShader = GL.CreateShader(OpenTK.Graphics.OpenGL.ShaderType.GeometryShader));
                GLChecker.Check(() => GL.ShaderSource(geometryShader, geometrySource));
                GLChecker.Check(() => GL.CompileShader(geometryShader));

                // Check the compile log
                GLChecker.Check(() => GL.GetShaderInfoLog(geometryShader, out info));
                GLChecker.Check(() => GL.GetShader(geometryShader, ShaderParameter.CompileStatus, out statusCode));

                // Check the compile log
                if (statusCode != 1)
                {
                    // Delete every handles when compilation failed
                    GLChecker.Check(() => GL.DeleteShader(geometryShader));
                    GLChecker.Check(() => GL.DeleteProgram(shaderProgram));

                    throw new InvalidOperationException("Failed to Compile Geometry Shader Source.\n" +
                        info + "\n\n" +
                        "Status Code: " + statusCode.ToString());
                }

                // Attach the shader to the program, and delete it (not needed anymore)
                GLChecker.Check(() => GL.AttachShader(shaderProgram, geometryShader));
                GLChecker.Check(() => GL.DeleteShader(geometryShader));
            }

            // Create fragment shader if requested
            if (!string.IsNullOrEmpty(geometrySource))
            {
                // Create and compile the shader
                int fragmentShader = 0;
                GLChecker.Check(() => fragmentShader = GL.CreateShader(OpenTK.Graphics.OpenGL.ShaderType.FragmentShader));
                GLChecker.Check(() => GL.ShaderSource(fragmentShader, fragmentSource));
                GLChecker.Check(() => GL.CompileShader(fragmentShader));

                // Check the compile log
                GLChecker.Check(() => GL.GetShaderInfoLog(fragmentShader, out info));
                GLChecker.Check(() => GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out statusCode));

                // Check the compile log
                if (statusCode != 1)
                {
                    // Delete every handles when compilation failed
                    GLChecker.Check(() => GL.DeleteShader(fragmentShader));
                    GLChecker.Check(() => GL.DeleteProgram(shaderProgram));

                    throw new InvalidOperationException("Failed to Compile Fragment Shader Source.\n" +
                        info + "\n\n" +
                        "Status Code: " + statusCode.ToString());
                }

                // Attach the shader to the program, and delete it (not needed anymore)
                GLChecker.Check(() => GL.AttachShader(shaderProgram, fragmentShader));
                GLChecker.Check(() => GL.DeleteShader(fragmentShader));
            }

            // Link the compiled program
            GLChecker.Check(() => GL.LinkProgram(shaderProgram));

            // Check for link status
            GLChecker.Check(() => GL.GetProgramInfoLog(shaderProgram, out info));
            GLChecker.Check(() => GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out statusCode));
            if (statusCode != 1)
            {
                // Delete the handles when failed to link the program
                GLChecker.Check(() => GL.DeleteProgram(shaderProgram));

                throw new InvalidOperationException("Failed to Link Shader Program.\n" +
                        info + "\n\n" +
                        "Status Code: " + statusCode.ToString());
            }

            // Program is compiled successfully
            _program = shaderProgram;

            // Force an OpenGL flush, so that the shader will appear updated
            // in all contexts immediately (solves problems in multi-threaded apps)
            GLChecker.Check(() => GL.Flush());
        }

        private void BindTextures()
        {
            int index = 1;
            foreach (KeyValuePair<int, Texture> texture in _textures)
            {
                GLChecker.Check(() => GL.Uniform1(texture.Key, index));
                GLChecker.Check(() => GL.ActiveTexture(TextureUnit.Texture0 + index));

                Texture.Bind(texture.Value);
                index++;
            }

            // Make sure that the texture unit which is left active is the number 0
            GLChecker.Check(() => GL.ActiveTexture(TextureUnit.Texture0));
        }

        private int GetUniformLocation(string name)
        {
            if (_uniforms.ContainsKey(name))
            {
                // Already in cache, return it
                return _uniforms[name];
            }
            else
            {
                // Not in cache, request the location from OpenGL
                int location = -1;
                GLChecker.Check(() => location = GL.GetUniformLocation(_program, name));

                _uniforms.Add(name, location);
                if (location == -1)
                {
                    Logger.Warning("Uniform {0} not found in shader.", name);
                }

                return location;
            }
        }

        public static void Bind(Shader shader)
        {
            // Make sure that we can use shaders
            if (!IsAvailable)
            {
                throw new NotSupportedException("Failed to bind or unbind shader. The system doesn't support shaders.\n" +
                                                "Shader.IsAvailable must return true in order to use Shader class.");
            }

            if (shader != null && shader._program > 0)
            {
                // Enable the program
                GLChecker.Check(() => GL.UseProgram(shader._program));

                // Bind the textures
                shader.BindTextures();

                // Bind the current texture
                if (shader._currentTexture != -1)
                    GLChecker.Check(() => GL.Uniform1(shader._currentTexture, 0));
            }
            else
            {
                // Bind no shader
                GLChecker.Check(() => GL.UseProgram(0));
            }
        }

        internal static int GetMaxTextureUnits()
        {
            int maxUnits = 0;
            GLChecker.Check(() => 
                GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out maxUnits)
            );

            return maxUnits;
        }
    }
}
