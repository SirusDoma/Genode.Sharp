using System;
using System.Collections.Generic;
using System.Text;

namespace Genode.Graphics
{
    public interface IRenderTextureImplementation : IDisposable
    {
        void Activate();

        void Deactivate();

        void UpdateTexture(int textureId);
    }
}
