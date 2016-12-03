using System;
using System.Collections.Generic;
using System.Text;

namespace Genode.Input
{
    public delegate void KeyboardEventHandler(object sender, KeyboardKeyEventArgs e);
    public delegate void KeyPressEventHandler(object sender, KeyboardPressEventArgs e);

    public delegate void MouseMoveEventHandler(object sender, MouseMoveEventArgs e);
    public delegate void MouseButtonEventHandler(object sender, MouseButtonEventArgs e);
    public delegate void MouseWheelEventHandler(object sender, MouseWheelEventArgs e);
}
