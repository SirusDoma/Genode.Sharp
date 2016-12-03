using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

using Genode;
using Genode.Entities;

namespace Genode.Graphics
{
    public struct VTest
    {
        public int a;
        public int b;
        public float c;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex : IColorable
    {
        public static readonly int Stride;

        // Internal Members
        // This ensure the fields are hidden from the user hand
        // But accessible to the entire engine
        internal Vector2  position;
        internal Vector2  texCoords;
        internal Vector4b color;

        public Vector2 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        public Vector2 TexCoords
        {
            get
            {
                return texCoords;
            }
            set
            {
                texCoords = value;
            }
        }

        public Color Color
        {
            get
            {
                return Color.FromArgb(color.W, color.X, color.Y, color.Z);
            }
            set
            {
                color = new Vector4b(value.R, value.G, value.B, value.A);
            }
        }

        public Vertex(float value)
        {
            var color = Color.White;

            this.position = new Vector2(value);
            this.texCoords = new Vector2(value);
            this.color = new Vector4b(color.R, color.G, color.B, color.A);
        }

        public Vertex(Vertex v)
        {
            this.position = v.position;
            this.texCoords = v.texCoords;
            this.color = v.color;
        }

        public Vertex(float value, Color color)
        {
            this.position = new Vector2(value);
            this.texCoords = new Vector2(value);
            this.color = new Vector4b(color.R, color.G, color.B, color.A);
        }

        public Vertex(Vector2 position, Vector2 texCoord)
        {
            var color = Color.White;

            this.position = position;
            this.texCoords = texCoord;
            this.color = new Vector4b(color.R, color.G, color.B, color.A);
        }

        public Vertex(Vector2 position, Vector2 texCoord, Color color)
        {
            this.position = position;
            this.texCoords = texCoord;
            this.color = new Vector4b(color.R, color.G, color.B, color.A);
        }

        static Vertex()
        {
            Stride = Marshal.SizeOf(new Vertex());
        }

        public static bool operator ==(Vertex v1, Vertex v2)
        {
            return v1.Equals(v2);
        }

        public static bool operator !=(Vertex v1, Vertex v2)
        {
            return !v1.Equals(v2);
        }

        public override bool Equals(object obj)
        {
            Vertex right = (Vertex)obj;
            return this.position == right.position && this.texCoords == right.texCoords && this.color == right.color;
        }

        public override int GetHashCode()
        {
            return position.GetHashCode() ^ texCoords.GetHashCode() ^ color.GetHashCode();
        }

        public override string ToString()
        {
            return "{Position: [" + position.ToString() + "], " +
                   "TexCoords: [" + texCoords.ToString() + "], " +
                   "Color: [" + Color.A.ToString() + "," + 
                                Color.R.ToString() + "," + 
                                Color.G.ToString() + "," + 
                                Color.B.ToString() + 
                   "]}";
        }
    }
}
