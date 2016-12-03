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
    public partial class Shader
    {
        private class UniformBinder : IDisposable
        {
            public int SavedProgram
            {
                get; set;
            }

            public int CurrentProgram
            {
                get; set;
            }

            public int Location
            {
                get; set;
            }

            public UniformBinder(Shader shader, string name)
            {
                SavedProgram = 0;
                CurrentProgram = shader._program;
                Location = -1;

                var uniform = this;
                if (CurrentProgram > 0)
                {
                    // Enable program object
                    GLChecker.Check(() => SavedProgram = GL.Arb.GetHandle(ArbShaderObjects.ProgramObjectArb));
                    if (CurrentProgram != SavedProgram)
                    {
                        GL.Arb.UseProgramObject(CurrentProgram);
                    }

                    // Store uniform location for further use outside constructor
                    Location = shader.GetUniformLocation(name);
                }
            }

            public void Dispose()
            {
                // Disable program object
                if (CurrentProgram > 0 && (CurrentProgram != SavedProgram))
                {
                    GLChecker.Check(() => GL.Arb.UseProgramObject(SavedProgram));
                }
            }
        }
    }
}
