using System;
using System.Collections.Generic;
using System.Text;

namespace Genode.Audio
{
    public static class Listener
    {
        public static float Volume
        {
            get
            {
                return AudioDevice.Volume;
            }
            set
            {
                AudioDevice.Volume = value;
            }
        }

        public static Vector3 Position
        {
            get
            {
                return AudioDevice.Position;
            }
            set
            {
                AudioDevice.Position = value;
            }
        }

        public static Vector3 Direction
        {
            get
            {
                return AudioDevice.Direction;
            }
            set
            {
                AudioDevice.Direction = value;
            }
        }

        public static Vector3 UpVector
        {
            get
            {
                return AudioDevice.UpVector;
            }
            set
            {
                AudioDevice.UpVector = value;
            }
        }
    }
}
