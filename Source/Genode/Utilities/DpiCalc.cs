using System;
using System.Collections.Generic;
using System.Text;

namespace Genode
{
    internal class DpiCalc
    {
        public static float FromDPI(float value)
        {
            switch ((int)value)
            {
                case 96:
                    return 100f;
                case 120:
                    return 125f;
                case 144:
                    return 150f;
                case 192:
                    return 200f;
                case 240:
                    return 250f;
                case 288:
                    return 300f;
                case 384:
                    return 400f;
                case 480:
                    return 500f;
                default:
                    return value;
            }
        }
    }
}
