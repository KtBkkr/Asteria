using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace AsteriaLibrary.Math
{
    public struct Size
    {
        #region Fields
        public int X;
        public int Y;

        private static Size zero = new Size(0);
        private static Size one = new Size(1);
        #endregion

        #region Properties
        public static Size Zero { get { return zero; } }
        public static Size One { get { return one; } }

        public int Width { get { return X; } }
        public int Height { get { return Y; } }
        #endregion

        #region Constructors
        public Size(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public Size(int value)
        {
            this.X = this.Y = value;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return X.ToString(CultureInfo.InvariantCulture) + "," + Y.ToString(CultureInfo.InvariantCulture);
        }

        public static explicit operator Size(string s)
        {
            Size size = new Size();
            string[] data = s.Split(',');
            size.X = int.Parse(data[0], NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture);
            size.Y = int.Parse(data[1], NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture);
            return size;
        }

        public static bool operator ==(Size a, Size b)
        {
            return (a.X == b.X) && (a.Y == b.Y);
        }

        public static bool operator !=(Size a, Size b)
        {
            return (a.Y != b.Y) || (a.X != b.X);
        }
        #endregion
    }

    public struct SizeF
    {
        #region Fields
        public float X;
        public float Y;

        private static SizeF zero = new SizeF(0f);
        private static SizeF one = new SizeF(1f);
        #endregion

        #region Properties
        public static SizeF Zero { get { return zero; } }
        public static SizeF One { get { return one; } }

        public float Width { get { return X; } }
        public float Height { get { return Y; } }
        #endregion

        #region Constructors
        public SizeF(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public SizeF(float value)
        {
            this.X = this.Y = value;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return X.ToString(CultureInfo.InvariantCulture) + "," + Y.ToString(CultureInfo.InvariantCulture);
        }

        public static explicit operator SizeF(string s)
        {
            SizeF size = new SizeF();
            string[] data = s.Split(',');
            size.X = float.Parse(data[0], NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture);
            size.Y = float.Parse(data[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture);
            return size;
        }

        public static bool operator ==(SizeF a, SizeF b)
        {
            return (a.X == b.X) && (a.Y == b.Y);
        }

        public static bool operator !=(SizeF a, SizeF b)
        {
            return (a.Y != b.Y) || (a.X != b.X);
        }
        #endregion
    }
}
