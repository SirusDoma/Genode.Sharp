using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Genode
{
    public struct Vector4<T> : IEquatable<Vector4<T>>
    {
        public static readonly int Stride;

        public static Vector4<T> Zero
        {
            get
            {
                return new Vector4<T>(default(T));
            }
        }

        public T X;
        public T Y;
        public T Z;
        public T W;

        static Vector4()
        {
            Stride = Marshal.SizeOf(new Vector4());
        }

        public Vector4(T value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }

        public Vector4(T x, T y, T z, T w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static bool operator ==(Vector4<T> left, Vector4<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4<T> left, Vector4<T> right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return ((this.X.GetHashCode() ^ this.Y.GetHashCode()) ^ this.Z.GetHashCode() ^ this.W.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return ((obj is Vector4) && this.Equals((Vector4)obj));
        }

        public bool Equals(Vector4<T> other)
        {
            return EqualityComparer<Vector4<T>>.Default.Equals(this, other);
        }

        public override string ToString()
        {
            return string.Format("({0}{4} {1}{4} {2}{4} {3})", new object[] { this.X, this.Y, this.Z, this.W, CultureInfo.CurrentCulture.TextInfo.ListSeparator });
        }
    }

    public struct Vector4 : IEquatable<Vector4>
    {
        public static readonly int Stride;

        public static Vector4 Zero
        {
            get
            {
                return new Vector4(0);
            }
        }

        public float X;
        public float Y;
        public float Z;
        public float W;

        static Vector4()
        {
            Stride = Marshal.SizeOf(new Vector4());
        }

        public Vector4(float value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }

        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static Vector4 operator +(Vector4 left, Vector4 right)
        {
            left.X += right.X;
            left.Y += right.Y;
            left.Z += right.Z;
            left.W += right.W;
            return left;
        }

        public static Vector4 operator -(Vector4 left, Vector4 right)
        {
            left.X -= right.X;
            left.Y -= right.Y;
            left.Z -= right.Z;
            left.W -= right.W;
            return left;
        }

        public static Vector4 operator -(Vector4 vec)
        {
            vec.X = -vec.X;
            vec.Y = -vec.Y;
            vec.Z = -vec.Z;
            vec.W = -vec.W;
            return vec;
        }

        public static Vector4 operator *(Vector4 vec, float scale)
        {
            vec.X *= scale;
            vec.Y *= scale;
            vec.Z *= scale;
            vec.W *= scale;
            return vec;
        }

        public static Vector4 operator *(float scale, Vector4 vec)
        {
            vec.X *= scale;
            vec.Y *= scale;
            vec.Z *= scale;
            vec.W *= scale;
            return vec;
        }

        public static Vector4 operator *(Vector4 vec, Vector4 scale)
        {
            vec.X *= scale.X;
            vec.Y *= scale.Y;
            vec.Z *= scale.Z;
            vec.W *= scale.W;
            return vec;
        }

        public static Vector4 operator /(Vector4 vec, float scale)
        {
            float num = 1f / scale;
            vec.X *= num;
            vec.Y *= num;
            vec.Z *= num;
            vec.W *= num;
            return vec;
        }

        public static bool operator ==(Vector4 left, Vector4 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4 left, Vector4 right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return ((this.X.GetHashCode() ^ this.Y.GetHashCode()) ^ this.Z.GetHashCode() ^ this.W.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return ((obj is Vector4) && this.Equals((Vector4)obj));
        }

        public bool Equals(Vector4 other)
        {
            return (((this.X == other.X) && (this.Y == other.Y)) && (this.Z == other.Z) && (this.W == other.W));
        }

        public override string ToString()
        {
            return string.Format("({0}{4} {1}{4} {2}{4} {3})", new object[] { this.X, this.Y, this.Z, this.W, CultureInfo.CurrentCulture.TextInfo.ListSeparator });
        }
    }

    public struct Vector4i : IEquatable<Vector4i>
    {
        public static readonly int Stride;

        public static Vector4i Zero
        {
            get
            {
                return new Vector4i(0);
            }
        }

        public int X;
        public int Y;
        public int Z;
        public int W;

        static Vector4i()
        {
            Stride = Marshal.SizeOf(new Vector4i());
        }

        public Vector4i(int value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }

        public Vector4i(int x, int y, int z, int w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static Vector4i operator +(Vector4i left, Vector4i right)
        {
            left.X += right.X;
            left.Y += right.Y;
            left.Z += right.Z;
            left.W += right.W;
            return left;
        }

        public static Vector4i operator -(Vector4i left, Vector4i right)
        {
            left.X -= right.X;
            left.Y -= right.Y;
            left.Z -= right.Z;
            left.W -= right.W;
            return left;
        }

        public static Vector4i operator -(Vector4i vec)
        {
            vec.X = -vec.X;
            vec.Y = -vec.Y;
            vec.Z = -vec.Z;
            vec.W = -vec.W;
            return vec;
        }

        public static Vector4i operator *(Vector4i vec, int scale)
        {
            vec.X *= scale;
            vec.Y *= scale;
            vec.Z *= scale;
            vec.W *= scale;
            return vec;
        }

        public static Vector4i operator *(int scale, Vector4i vec)
        {
            vec.X *= scale;
            vec.Y *= scale;
            vec.Z *= scale;
            vec.W *= scale;
            return vec;
        }

        public static Vector4i operator *(Vector4i vec, Vector4i scale)
        {
            vec.X *= scale.X;
            vec.Y *= scale.Y;
            vec.Z *= scale.Z;
            vec.W *= scale.W;
            return vec;
        }

        public static Vector4i operator /(Vector4i vec, int scale)
        {
            int num = (int)(((int)1) / scale);
            vec.X *= num;
            vec.Y *= num;
            vec.Z *= num;
            vec.W *= num;
            return vec;
        }

        public static bool operator ==(Vector4i left, Vector4i right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4i left, Vector4i right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return ((this.X.GetHashCode() ^ this.Y.GetHashCode()) ^ this.Z.GetHashCode() ^ this.W.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return ((obj is Vector4i) && this.Equals((Vector4i)obj));
        }

        public bool Equals(Vector4i other)
        {
            return (((this.X == other.X) && (this.Y == other.Y)) && (this.Z == other.Z) && (this.W == other.W));
        }

        public override string ToString()
        {
            return string.Format("({0}{4} {1}{4} {2}{4} {3})", new object[] { this.X, this.Y, this.Z, this.W, CultureInfo.CurrentCulture.TextInfo.ListSeparator });
        }
    }

    public struct Vector4b : IEquatable<Vector4b>
    {
        public static readonly int Stride;

        public static Vector4b Zero
        {
            get
            {
                return new Vector4b(0);
            }
        }

        public byte X;
        public byte Y;
        public byte Z;
        public byte W;

        static Vector4b()
        {
            Stride = Marshal.SizeOf(new Vector4b());
        }

        public Vector4b(byte value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }

        public Vector4b(byte x, byte y, byte z, byte w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static Vector4b operator +(Vector4b left, Vector4b right)
        {
            left.X += right.X;
            left.Y += right.Y;
            left.Z += right.Z;
            left.W += right.W;
            return left;
        }

        public static Vector4b operator -(Vector4b left, Vector4b right)
        {
            left.X -= right.X;
            left.Y -= right.Y;
            left.Z -= right.Z;
            left.W -= right.W;
            return left;
        }

        public static Vector4b operator -(Vector4b vec)
        {
            vec.X = (byte)-vec.X;
            vec.Y = (byte)-vec.Y;
            vec.Z = (byte)-vec.Z;
            vec.W = (byte)-vec.W;
            return vec;
        }

        public static Vector4b operator *(Vector4b vec, byte scale)
        {
            vec.X *= scale;
            vec.Y *= scale;
            vec.Z *= scale;
            vec.W *= scale;
            return vec;
        }

        public static Vector4b operator *(byte scale, Vector4b vec)
        {
            vec.X *= scale;
            vec.Y *= scale;
            vec.Z *= scale;
            vec.W *= scale;
            return vec;
        }

        public static Vector4b operator *(Vector4b vec, Vector4b scale)
        {
            vec.X *= scale.X;
            vec.Y *= scale.Y;
            vec.Z *= scale.Z;
            vec.W *= scale.W;
            return vec;
        }

        public static Vector4b operator /(Vector4b vec, byte scale)
        {
            byte num = (byte)(1f / scale);
            vec.X *= num;
            vec.Y *= num;
            vec.Z *= num;
            vec.W *= num;
            return vec;
        }

        public static bool operator ==(Vector4b left, Vector4b right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4b left, Vector4b right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return ((this.X.GetHashCode() ^ this.Y.GetHashCode()) ^ this.Z.GetHashCode() ^ this.W.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return ((obj is Vector4b) && this.Equals((Vector4b)obj));
        }

        public bool Equals(Vector4b other)
        {
            return (((this.X == other.X) && (this.Y == other.Y)) && (this.Z == other.Z) && (this.W == other.W));
        }

        public override string ToString()
        {
            return string.Format("({0}{4} {1}{4} {2}{4} {3})", new object[] { this.X, this.Y, this.Z, this.W, CultureInfo.CurrentCulture.TextInfo.ListSeparator });
        }
    }
}
