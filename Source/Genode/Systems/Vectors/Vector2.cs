using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Genode
{
    public struct Vector2<T> : IEquatable<Vector2<T>>
    {
        public static readonly int Stride;

        public static Vector2<T> Zero
        {
            get
            {
                return new Vector2<T>(default(T));
            }
        }

        public T X;
        public T Y;

        static Vector2()
        {
            Stride = Marshal.SizeOf(new Vector2());
        }

        public Vector2(T value)
        {
            X = value;
            Y = value;
        }

        public Vector2(T x, T y, T z, T w)
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(Vector2<T> left, Vector2<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2<T> left, Vector2<T> right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return ((this.X.GetHashCode() ^ this.Y.GetHashCode()));
        }

        public override bool Equals(object obj)
        {
            return ((obj is Vector2) && this.Equals((Vector2)obj));
        }

        public bool Equals(Vector2<T> other)
        {
            return EqualityComparer<Vector2<T>>.Default.Equals(this, other);
        }

        public override string ToString()
        {
            return string.Format("({0}{2} {1})", new object[] { this.X, this.Y, CultureInfo.CurrentCulture.TextInfo.ListSeparator });
        }
    }

    public struct Vector2 : IEquatable<Vector2>
    {
        public static readonly int Stride;

        public static Vector2 Zero
        {
            get
            {
                return new Vector2(0);
            }
        }

        public float X;
        public float Y;

        static Vector2()
        {
            Stride = Marshal.SizeOf(new Vector2());
        }

        public Vector2(float value)
        {
            X = value;
            Y = value;
        }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 operator +(Vector2 left, Vector2 right)
        {
            left.X += right.X;
            left.Y += right.Y;
            return left;
        }

        public static Vector2 operator -(Vector2 left, Vector2 right)
        {
            left.X -= right.X;
            left.Y -= right.Y;
            return left;
        }

        public static Vector2 operator -(Vector2 vec)
        {
            vec.X = -vec.X;
            vec.Y = -vec.Y;
            return vec;
        }

        public static Vector2 operator *(Vector2 vec, float scale)
        {
            vec.X *= scale;
            vec.Y *= scale;
            return vec;
        }

        public static Vector2 operator *(float scale, Vector2 vec)
        {
            vec.X *= scale;
            vec.Y *= scale;
            return vec;
        }

        public static Vector2 operator *(Vector2 vec, Vector2 scale)
        {
            vec.X *= scale.X;
            vec.Y *= scale.Y;
            return vec;
        }

        public static Vector2 operator /(Vector2 vec, float scale)
        {
            float num = 1f / scale;
            vec.X *= num;
            vec.Y *= num;
            return vec;
        }

        public static bool operator ==(Vector2 left, Vector2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2 left, Vector2 right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return ((obj is Vector2) && this.Equals((Vector2)obj));
        }

        public bool Equals(Vector2 other)
        {
            return (((this.X == other.X) && (this.Y == other.Y)));
        }

        public override int GetHashCode()
        {
            return ((this.X.GetHashCode() ^ this.Y.GetHashCode()));
        }

        public override string ToString()
        {
            return string.Format("({0}{2} {1})", new object[] { this.X, this.Y, CultureInfo.CurrentCulture.TextInfo.ListSeparator });
        }
    }

    public struct Vector2i : IEquatable<Vector2i>
    {
        public static readonly int Stride;

        public static Vector2i Zero
        {
            get
            {
                return new Vector2i(0);
            }
        }

        public int X;
        public int Y;

        static Vector2i()
        {
            Stride = Marshal.SizeOf(new Vector2());
        }

        public Vector2i(int value)
        {
            X = value;
            Y = value;
        }

        public Vector2i(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Vector2i operator +(Vector2i left, Vector2i right)
        {
            left.X += right.X;
            left.Y += right.Y;
            return left;
        }

        public static Vector2i operator -(Vector2i left, Vector2i right)
        {
            left.X -= right.X;
            left.Y -= right.Y;
            return left;
        }

        public static Vector2i operator -(Vector2i vec)
        {
            vec.X = -vec.X;
            vec.Y = -vec.Y;
            return vec;
        }

        public static Vector2i operator *(Vector2i vec, int scale)
        {
            vec.X *= scale;
            vec.Y *= scale;
            return vec;
        }

        public static Vector2i operator *(int scale, Vector2i vec)
        {
            vec.X *= scale;
            vec.Y *= scale;
            return vec;
        }

        public static Vector2i operator *(Vector2i vec, Vector2i scale)
        {
            vec.X *= scale.X;
            vec.Y *= scale.Y;
            return vec;
        }

        public static Vector2i operator /(Vector2i vec, int scale)
        {
            int num = (int)(1f / scale);
            vec.X *= num;
            vec.Y *= num;
            return vec;
        }

        public static bool operator ==(Vector2i left, Vector2i right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2i left, Vector2i right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return ((obj is Vector2i) && this.Equals((Vector2i)obj));
        }

        public bool Equals(Vector2i other)
        {
            return (((this.X == other.X) && (this.Y == other.Y)));
        }

        public override int GetHashCode()
        {
            return ((this.X.GetHashCode() ^ this.Y.GetHashCode()));
        }

        public override string ToString()
        {
            return string.Format("({0}{2} {1})", new object[] { this.X, this.Y, CultureInfo.CurrentCulture.TextInfo.ListSeparator });
        }
    }

    public struct Vector2b : IEquatable<Vector2b>
    {
        public static readonly int Stride;

        public static Vector2b Zero
        {
            get
            {
                return new Vector2b(0);
            }
        }

        public byte X;
        public byte Y;

        static Vector2b()
        {
            Stride = Marshal.SizeOf(new Vector2b());
        }

        public Vector2b(byte value)
        {
            X = value;
            Y = value;
        }

        public Vector2b(byte x, byte y)
        {
            X = x;
            Y = y;
        }

        public static Vector2b operator +(Vector2b left, Vector2b right)
        {
            left.X += right.X;
            left.Y += right.Y;
            return left;
        }

        public static Vector2b operator -(Vector2b left, Vector2b right)
        {
            left.X -= right.X;
            left.Y -= right.Y;
            return left;
        }

        public static Vector2b operator -(Vector2b vec)
        {
            vec.X = (byte)-vec.X;
            vec.Y = (byte)-vec.Y;
            return vec;
        }

        public static Vector2b operator *(Vector2b vec, byte scale)
        {
            vec.X *= scale;
            vec.Y *= scale;
            return vec;
        }

        public static Vector2b operator *(byte scale, Vector2b vec)
        {
            vec.X *= scale;
            vec.Y *= scale;
            return vec;
        }

        public static Vector2b operator *(Vector2b vec, Vector2b scale)
        {
            vec.X *= scale.X;
            vec.Y *= scale.Y;
            return vec;
        }

        public static Vector2b operator /(Vector2b vec, byte scale)
        {
            byte num = (byte)(((byte)1) / scale);
            vec.X *= num;
            vec.Y *= num;
            return vec;
        }

        public static bool operator ==(Vector2b left, Vector2b right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2b left, Vector2b right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return ((obj is Vector2b) && this.Equals((Vector2b)obj));
        }

        public bool Equals(Vector2b other)
        {
            return (((this.X == other.X) && (this.Y == other.Y)));
        }

        public override int GetHashCode()
        {
            return ((this.X.GetHashCode() ^ this.Y.GetHashCode()));
        }

        public override string ToString()
        {
            return string.Format("({0}{2} {1})", new object[] { this.X, this.Y, CultureInfo.CurrentCulture.TextInfo.ListSeparator });
        }
    }
}
