using System;
using System.Collections.Generic;
using System.Text;

using Genode;
using Genode.Graphics;

namespace Genode.Window
{
    public delegate void TargetRenderEventHandler(RenderTarget target, RenderFrameEventArgs e);
    public delegate void TargetUpdateEventHandler(object sender, UpdateFrameEventArgs e);

    public delegate void RenderEventHandler(object sender, RenderFrameEventArgs e);
    public delegate void UpdateEventHandler(object sender, UpdateFrameEventArgs e);
}
