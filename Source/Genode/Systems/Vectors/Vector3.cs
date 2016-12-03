using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Genode
{ 
    public struct Vector3<T> : IEquatable<Vector3<T>>
    {
        public static readonly int Stride;

        public static Vector3<T> Zero
        {
            get
            {
                return new Vector3<T>(default(T));
            }
        }

        public T X;
        public T Y;
        public T Z;

        static Vector3()
        {
            Stride = Marshal.SizeOf(new Vector3());
        }

        public Vector3(T value)
        {

            X = value;
            Y = value;
            Z = value;
        }

        public Vector3(T x, T y, T z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static bool operator ==(Vector3<T> left, Vector3<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3<T> left, Vector3<T> right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return ((this.X.GetHashCode() ^ this.Y.GetHashCode()) ^ this.Z.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return ((obj is Vector3) && this.Equals((Vector3)obj));
        }

        public bool Equals(Vector3<T> other)
        {
            return EqualityComparer<Vector3<T>>.Default.Equals(this, other);
            //return (((this.X == other.X) && (this.Y == other.Y)) && (this.Z == other.Z));
        }

        public override string ToString()
        {
            return string.Format("({0}{3} {1}{3} {2})", new object[] { this.X, this.Y, this.Z, CultureInfo.CurrentCulture.TextInfo.ListSeparator });
        }
    }

    public struct Vector3 : IEquatable<Vector3>
    {
        public static readonly int Stride;

        public static Vector3 Zero
        {
            get
            {
                return new Vector3(0);
            }
        }

        public float X;
        public float Y;
        public float Z;

        static Vector3()
        {
            Stride = Marshal.SizeOf(new Vector3());
        }

        public Vector3(float value)
        {
            
            X = value;
            Y = value;
            Z = value;
        }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 operator +(Vector3 left, Vector3 right)
        {
            left.X += right.X;
            left.Y += right.Y;
            left.Z += right.Z;
            return left;
        }

        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            left.X -= right.X;
            left.Y -= right.Y;
            left.Z -= right.Z;
            return left;
        }

        public static Vector3 operator -(Vector3 vec)
        {
            vec.X = -vec.X;
            vec.Y = -vec.Y;
            vec.Z = -vec.Z;
            return vec;
        }

        public static Vector3 operator *(Vector3 vec, float scale)
        {
            vec.X *= scale;
            vec.Y *= scale;
            vec.Z *= scale;
            return vec;
        }

        public static Vector3 operator *(float scale, Vector3 vec)
        {
            vec.X *= scale;
            vec.Y *= scale;
            vec.Z *= scale;
            return vec;
        }

        public static Vector3 operator *(Vector3 vec, Vector3 scale)
        {
            vec.X *= scale.X;
            vec.Y *= scale.Y;
            vec.Z *= scale.Z;
            return vec;
        }

        public static Vector3 operator /(Vector3 vec, float scale)
        {
            float num = 1f / scale;
            vec.X *= num;
            vec.Y *= num;
            vec.Z *= num;
            return vec;
        }

        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return ((this.X.GetHashCode() ^ this.Y.GetHashCode()) ^ this.Z.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return ((obj is Vector3) && this.Equals((Vector3)obj));
        }

        public bool Equals(Vector3 other)
        {
            return (((this.X == other.X) && (this.Y == other.Y)) && (this.Z == other.Z));
        }

        public override string ToString()
        {
            return string.Format("({0}{3} {1}{3} {2})", new object[] { this.X, this.Y, this.Z, CultureInfo.CurrentCulture.TextInfo.ListSeparator });
        }
    }

    public struct Vector3i : IEquatable<Vector3i>
    {
        public static readonly int Stride;

        public static Vector3i Zero
        {
            get
            {
                return new Vector3i(0);
            }
        }

        public int X;
        public int Y;
        public int Z;

        static Vector3i()
        {
            Stride = Marshal.SizeOf(new Vector3i());
        }

        public Vector3i(int value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        public Vector3i(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3i operator +(Vector3i left, Vector3i right)
        {
            left.X += right.X;
            left.Y += right.Y;
            left.Z += right.Z;
            return left;
        }

        public static Vector3i operator -(Vector3i left, Vector3i right)
        {
            left.X -= right.X;
            left.Y -= right.Y;
            left.Z -= right.Z;
            return left;
        }

        public static Vector3i operator -(Vector3i vec)
        {
            vec.X = -vec.X;
            vec.Y = -vec.Y;
            vec.Z = -vec.Z;
            return vec;
        }

        public static Vector3i operator *(Vector3i vec, int scale)
        {
            vec.X *= scale;
            vec.Y *= scale;
            vec.Z *= scale;
            return vec;
        }

        public static Vector3i operator *(int scale, Vector3i vec)
        {
            vec.X *= scale;
            vec.Y *= scale;
            vec.Z *= scale;
            return vec;
        }

        public static Vector3i operator *(Vector3i vec, Vector3i scale)
        {
            vec.X *= scale.X;
            vec.Y *= scale.Y;
            vec.Z *= scale.Z;
            return vec;
        }

        public static Vector3i operator /(Vector3i vec, int scale)
        {
            int num = 1 / scale;
            vec.X *= num;
            vec.Y *= num;
            vec.Z *= num;
            return vec;
        }

        public static bool operator ==(Vector3i left, Vector3i right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3i left, Vector3i right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return ((this.X.GetHashCode() ^ this.Y.GetHashCode()) ^ this.Z.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return ((obj is Vector3i) && this.Equals((Vector3i)obj));
        }

        public bool Equals(Vector3i other)
        {
            return (((this.X == other.X) && (this.Y == other.Y)) && (this.Z == other.Z));
        }

        public override string ToString()
        {
            return string.Format("({0}{3} {1}{3} {2})", new object[] { this.X, this.Y, this.Z, CultureInfo.CurrentCulture.TextInfo.ListSeparator });
        }
    }

    public struct Vector3b : IEquatable<Vector3b>
    {
        public static readonly int Stride;

        public static Vector3b Zero
        {
            get
            {
                return new Vector3b(0);
            }
        }

        public byte X;
        public byte Y;
        public byte Z;

        static Vector3b()
        {
            Stride = Marshal.SizeOf(new Vector3());
        }

        public Vector3b(byte value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        public Vector3b(byte x, byte y, byte z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3b operator +(Vector3b left, Vector3b right)
        {
            left.X += right.X;
            left.Y += right.Y;
            left.Z += right.Z;
            return left;
        }

        public static Vector3b operator -(Vector3b left, Vector3b right)
        {
            left.X -= right.X;
            left.Y -= right.Y;
            left.Z -= right.Z;
            return left;
        }

        public static Vector3b operator -(Vector3b vec)
        {
            vec.X = (byte)-vec.X;
            vec.Y = (byte)-vec.Y;
            vec.Z = (byte)-vec.Z;
            return vec;
        }

        public static Vector3b operator *(Vector3b vec, byte scale)
        {
            vec.X *= scale;
            vec.Y *= scale;
            vec.Z *= scale;
            return vec;
        }

        public static Vector3b operator *(byte scale, Vector3b vec)
        {
            vec.X *= scale;
            vec.Y *= scale;
            vec.Z *= scale;
            return vec;
        }

        public static Vector3b operator *(Vector3b vec, Vector3b scale)
        {
            vec.X *= scale.X;
            vec.Y *= scale.Y;
            vec.Z *= scale.Z;
            return vec;
        }

        public static Vector3b operator /(Vector3b vec, byte scale)
        {
            byte num = (byte)(((byte)1) / scale);
            vec.X *= num;
            vec.Y *= num;
            vec.Z *= num;
            return vec;
        }

        public static bool operator ==(Vector3b left, Vector3b right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3b left, Vector3b right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return ((this.X.GetHashCode() ^ this.Y.GetHashCode()) ^ this.Z.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return ((obj is Vector3b) && this.Equals((Vector3b)obj));
        }

        public bool Equals(Vector3b other)
        {
            return (((this.X == other.X) && (this.Y == other.Y)) && (this.Z == other.Z));
        }

        public override string ToString()
        {
            return string.Format("({0}{3} {1}{3} {2})", new object[] { this.X, this.Y, this.Z, CultureInfo.CurrentCulture.TextInfo.ListSeparator });
        }
    }
}
