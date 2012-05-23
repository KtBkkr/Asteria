using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace AsteriaLibrary.Math
{
    public struct Point
    {
        #region Fields
        public int X;
        public int Y;

        private static Point zero = new Point(0);
        private static Point one = new Point(1);
        #endregion

        #region Properties
        public static Point Zero { get { return zero; } }
        public static Point One { get { return one; } }
        #endregion

        #region Constructors
        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public Point(int value)
        {
            this.X = this.Y = value;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return X.ToString(CultureInfo.InvariantCulture) + "," + Y.ToString(CultureInfo.InvariantCulture);
        }

        public static explicit operator Point(string s)
        {
            Point p = new Point();
            string[] data = s.Split(',');
            p.X = int.Parse(data[0], NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture);
            p.Y = int.Parse(data[1], NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture);
            return p;
        }

        public float Length()
        {
            float num = ((this.X * this.X) + (this.Y * this.Y));
            return (float)System.Math.Sqrt((double)num);
        }

        public string ToOutput()
        {
            return String.Format("X: {0:00.00}\r\nY: {1:00.00}", X, Y);
        }

        /// <summary>
        /// Fills the buffer with 8 bytes representing X and Y coordinates respectively.
        /// </summary>
        /// <param name="buffer"></param>
        public void SaveToBuffer(ref byte[] buffer)
        {
            byte[] floatBuffer;
            floatBuffer = BitConverter.GetBytes(X);
            floatBuffer.CopyTo(buffer, 0);

            floatBuffer = BitConverter.GetBytes(Y);
            floatBuffer.CopyTo(buffer, 4);
        }

        /// <summary>
        /// Fills the buffer with 8 bytes starting at the offset element and representing X and Y coordinates respectively.
        /// </summary>
        /// <param name="buffer"></param>
        public void SaveToBuffer(ref byte[] buffer, int offset)
        {
            byte[] floatBuffer;
            floatBuffer = BitConverter.GetBytes(X);
            floatBuffer.CopyTo(buffer, offset);

            floatBuffer = BitConverter.GetBytes(Y);
            floatBuffer.CopyTo(buffer, offset + 4);
        }

        /// <summary>
        /// Loads a Point from buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Point FromBuffer(ref byte[] buffer, int offset)
        {
            Point v = Point.zero;
            FromBuffer(ref v, ref buffer, offset);
            return v;
        }

        /// <summary>
        /// Loads a Point from buffer.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        public static void FromBuffer(ref Point v, ref byte[] buffer, int offset)
        {
            v.X = BitConverter.ToInt32(buffer, offset);
            v.Y = BitConverter.ToInt32(buffer, offset + 4);
        }

        #region Operator Overloading
        public static bool operator ==(Point a, Point b)
        {
            return ((a.X == b.X) && (a.Y == b.Y));
        }

        public static bool operator !=(Point a, Point b)
        {
            return (a.Y != b.Y) || (a.X != b.X);
        }

        public static bool operator <=(Point a, Point b)
        {
            return (a.X <= b.X) && (a.Y <= b.Y);
        }

        public static bool operator >=(Point a, Point b)
        {
            return (a.X >= b.X) && (a.Y >= b.Y);
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }
        #endregion

        #endregion
    }
}
