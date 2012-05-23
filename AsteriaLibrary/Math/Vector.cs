using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace AsteriaLibrary.Math
{
    /// <summary>
    /// Defines a 2D vector.
    /// An explicit casting from string operator is provided so the following statement can be used: Vector v = (Vector)"0,1,0";
    /// </summary>
    //public struct Vector
    //{
    //    #region Fields
    //    public float X;
    //    public float Y;
    //    public float Z;

    //    private static Vector zero = new Vector(0);
    //    private static Vector one = new Vector(1);
    //    #endregion

    //    #region Properties
    //    public static Vector Zero { get { return zero; } }
    //    public static Vector One { get { return one; } }
    //    #endregion

    //    #region Constructors
    //    public Vector(float x, float y, float z)
    //    {
    //        this.X = x;
    //        this.Y = y;
    //        this.Z = z;
    //    }

    //    public Vector(float value)
    //    {
    //        this.X = this.Y = this.Z = value;
    //    }
    //    #endregion

    //    #region Methods
    //    public float Length()
    //    {
    //        float num = ((X * X) + (Y * Y)) + (Z * Z);
    //        return (float)System.Math.Sqrt((double)num);
    //    }

    //    public float LengthSquared()
    //    {
    //        return (((X * X) + (Y * Y)) + (Z * Z));
    //    }

    //    public static float Distance(Vector a, Vector b)
    //    {
    //        float xd = a.X - b.X;
    //        float yd = a.Y - b.Y;
    //        float zd = a.Z - b.Z;
    //        float dist = ((xd * xd) + (yd * yd)) + (zd * zd);
    //        return (float)System.Math.Sqrt((double)dist);
    //    }

    //    public static float DistanceSquared(Vector a, Vector b)
    //    {
    //        float xd = a.X - b.X;
    //        float yd = a.Y - b.Y;
    //        float zd = a.Z - b.Z;
    //        return ((xd * xd) + (yd * yd)) + (zd * zd);
    //    }

    //    public void Normalize()
    //    {
    //        float factor = 1f / ((float)System.Math.Sqrt((double)LengthSquared()));
    //        this.X *= factor;
    //        this.Y *= factor;
    //        this.Z *= factor;
    //    }

    //    public string ToOutput()
    //    {
    //        return string.Format("X: {0:00.00}\r\nY: {1:00.00}\r\nZ: {2:00.00}", this.X, this.Y, this.Z);
    //    }

    //    #region Operator Overloading
    //    public override bool Equals(object obj)
    //    {
    //        bool result = false;
    //        if (obj is Vector)
    //        {
    //            result = this.Equals((Vector)obj);
    //        }
    //        return result;
    //    }

    //    public static bool operator ==(Vector a, Vector b)
    //    {
    //        return (((a.X == b.X) && (a.Y == b.Y)) && (a.Z == b.Z));
    //    }
    //    public static bool operator !=(Vector a, Vector b)
    //    {
    //        return (a.X != b.X) || (a.Y != b.Y) || (a.Z != b.Z);
    //    }
    //    public override int GetHashCode()
    //    {
    //        return ((this.X.GetHashCode() + this.Y.GetHashCode()) + this.Z.GetHashCode());
    //    }

    //    public static Vector operator +(Vector a, Vector b)
    //    {
    //        return new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    //    }
    //    public static Vector operator +(Vector a, int b)
    //    {
    //        return new Vector(a.X + (float)b, a.Y + (float)b, a.Z + (float)b);
    //    }
    //    public static Vector operator +(Vector a, float b)
    //    {
    //        return new Vector(a.X + b, a.Y + b, a.Z + b);
    //    }

    //    public static Vector operator -(Vector a, Vector b)
    //    {
    //        return new Vector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    //    }
    //    public static Vector operator -(Vector a, int b)
    //    {
    //        return new Vector(a.X - (float)b, a.Y - (float)b, a.Z - (float)b);
    //    }
    //    public static Vector operator -(Vector a, float b)
    //    {
    //        return new Vector(a.X - b, a.Y - b, a.Z - b);
    //    }

    //    public static Vector operator *(Vector a, Vector b)
    //    {
    //        return new Vector(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    //    }
    //    public static Vector operator *(Vector a, int b)
    //    {
    //        return new Vector(a.X * (float)b, a.Y * (float)b, a.Z * (float)b);
    //    }
    //    public static Vector operator *(Vector a, float b)
    //    {
    //        return new Vector(a.X * b, a.Y * b, a.Z * b);
    //    }

    //    public static Vector operator /(Vector a, Vector b)
    //    {
    //        return new Vector(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
    //    }
    //    public static Vector operator /(Vector a, int b)
    //    {
    //        return new Vector(a.X / (float)b, a.Y / (float)b, a.Z / (float)b);
    //    }

    //    public static bool operator <(Vector a, Vector b)
    //    {
    //        return (a.X < b.X) && (a.Y < b.Y) && (a.Z < b.Z);
    //    }

    //    public static bool operator <=(Vector a, Vector b)
    //    {
    //        return (a.X <= b.X) && (a.Y <= b.Y) && (a.Z <= b.Z);
    //    }

    //    public static bool operator >(Vector a, Vector b)
    //    {
    //        return (a.X > b.X) && (a.Y > b.Y) && (a.Z > b.Z);
    //    }

    //    public static bool operator >=(Vector a, Vector b)
    //    {
    //        return (a.X >= b.X) && (a.Y >= b.Y) && (a.Z >= b.Z);
    //    }

    //    public static float Dot(Vector a, Vector b)
    //    {
    //        return (((a.X * b.X) + (a.Y * b.Y)) + (a.Z * b.Z));
    //    }

    //    public static Vector Cross(Vector a, Vector b)
    //    {
    //        Vector vector = new Vector((a.Y * b.Z) - (a.Z * b.Y), (a.Z * b.X) - (a.X * b.Z), (a.X * b.Y) - (a.Y * b.X));
    //        return vector;
    //    }
    //    #endregion

    //    #region ISaveable<Vector> Members
    //    public override string ToString()
    //    {
    //        return X.ToString(CultureInfo.InvariantCulture) + "," + Y.ToString(CultureInfo.InvariantCulture) + "," + Z.ToString(CultureInfo.InvariantCulture);
    //    }

    //    public static explicit operator Vector(string s)
    //    {
    //        Vector v = new Vector();
    //        string[] data = s.Split(new char[] { ',' });
    //        v.X = float.Parse(data[0], NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture);
    //        v.Y = float.Parse(data[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture);
    //        v.Z = float.Parse(data[2], NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture);
    //        return v;
    //    }

    //    /// <summary>
    //    /// Fills the buffer with 12 bytes representing X,Y and Z coordinates respectively.
    //    /// </summary>
    //    /// <param name="buffer"></param>
    //    public void SaveToBuffer(ref byte[] buffer)
    //    {
    //        byte[] floatBuffer;
    //        floatBuffer = BitConverter.GetBytes(X);
    //        floatBuffer.CopyTo(buffer, 0);

    //        floatBuffer = BitConverter.GetBytes(Y);
    //        floatBuffer.CopyTo(buffer, 4);

    //        floatBuffer = BitConverter.GetBytes(Z);
    //        floatBuffer.CopyTo(buffer, 8);
    //    }

    //    /// <summary>
    //    /// Fills the buffer with 12 bytes starting at the offset element and representing X,Y and Z coordinates respectively.
    //    /// </summary>
    //    /// <param name="buffer"></param>
    //    public void SaveToBuffer(ref byte[] buffer, int offset)
    //    {
    //        byte[] floatBuffer;
    //        floatBuffer = BitConverter.GetBytes(X);
    //        floatBuffer.CopyTo(buffer, offset);

    //        floatBuffer = BitConverter.GetBytes(Y);
    //        floatBuffer.CopyTo(buffer, offset + 4);

    //        floatBuffer = BitConverter.GetBytes(Z);
    //        floatBuffer.CopyTo(buffer, offset + 8);
    //    }

    //    /// <summary>
    //    /// Loads a Vector from buffer
    //    /// </summary>
    //    /// <param name="buffer"></param>
    //    /// <param name="offset"></param>
    //    /// <returns></returns>
    //    public static Vector FromBuffer(ref byte[] buffer, int offset)
    //    {
    //        Vector v = Vector.zero;
    //        FromBuffer(ref v, ref buffer, offset);
    //        return v;
    //    }

    //    /// <summary>
    //    /// Loads a Vector from buffer.
    //    /// </summary>
    //    /// <param name="v"></param>
    //    /// <param name="buffer"></param>
    //    /// <param name="offset"></param>
    //    public static void FromBuffer(ref Vector v, ref byte[] buffer, int offset)
    //    {
    //        v.X = System.BitConverter.ToSingle(buffer, offset);
    //        v.Y = System.BitConverter.ToSingle(buffer, offset + 4);
    //        v.Z = System.BitConverter.ToSingle(buffer, offset + 8);
    //    }

    //    /// <summary>
    //    /// Converts a byte array to a Vector. The length of the byte array must be at least 12, if larger only the first 12 elements are used.
    //    /// </summary>
    //    /// <param name="buffer"></param>
    //    /// <returns></returns>
    //    public static explicit operator Vector(byte[] buffer)
    //    {
    //        Vector v = new Vector();
    //        v.X = BitConverter.ToSingle(buffer, 0);
    //        v.Y = BitConverter.ToSingle(buffer, 4);
    //        v.Z = BitConverter.ToSingle(buffer, 8);
    //        return v;
    //    }
    //    #endregion

    //    #endregion
    //}
}
