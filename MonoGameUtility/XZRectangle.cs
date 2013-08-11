using System;

namespace MonoGameUtility {
    public struct XZRectangle : IEquatable<XZRectangle>{
        #region Private Fields

        private static XZRectangle emptyRectangle = new XZRectangle();

        #endregion Private Fields


        #region Public Fields

        public int X;

        public int Z;

        public int Width;

        public int Length;

        #endregion Public Fields


        #region Public Properties

        public static XZRectangle Empty {
            get { return emptyRectangle; }
        }

        public int Left {
            get { return this.X; }
        }

        public int Right {
            get { return (this.X + this.Width); }
        }

        public int Top {
            get { return this.Z; }
        }

        public int Bottom {
            get { return (this.Z + this.Length); }
        }

        #endregion Public Properties


        #region Constructors

        public XZRectangle(int x, int z, int width, int length) {
            this.X = x;
            this.Z = z;
            this.Width = width;
            this.Length = length;
        }

        #endregion Constructors


        #region Public Methods

        public static bool operator ==(XZRectangle a, XZRectangle b) {
            return ((a.X == b.X) && (a.Z == b.Z) && (a.Width == b.Width) && (a.Length == b.Length));
        }

        public bool Contains(int x, int y) {
            return ((((this.X <= x) && (x < (this.X + this.Width))) && (this.Z <= y)) && (y < (this.Z + this.Length)));
        }

        public bool Contains(XZPoint value) {
            return ((((this.X <= value.X) && (value.X < (this.X + this.Width))) && (this.Z <= value.Z)) && (value.Z < (this.Z + this.Length)));
        }

        public bool Contains(XZRectangle value) {
            return ((((this.X <= value.X) && ((value.X + value.Width) <= (this.X + this.Width))) && (this.Z <= value.Z)) && ((value.Z + value.Length) <= (this.Z + this.Length)));
        }

        public static bool operator !=(XZRectangle a, XZRectangle b) {
            return !(a == b);
        }

        public void Offset(XZPoint offset) {
            X += offset.X;
            Z += offset.Z;
        }

        public void Offset(int offsetX, int offsetY) {
            X += offsetX;
            Z += offsetY;
        }

        public XZPoint Location {
            get {
                return new XZPoint(this.X, this.Z);
            }
            set {
                X = value.X;
                Z = value.Z;
            }
        }

        public XZPoint Center {
            get {
                return new XZPoint(this.X + (this.Width / 2), this.Z + (this.Length / 2));
            }
        }




        public void Inflate(int horizontalValue, int verticalValue) {
            X -= horizontalValue;
            Z -= verticalValue;
            Width += horizontalValue * 2;
            Length += verticalValue * 2;
        }

        public bool IsEmpty {
            get {
                return ((((this.Width == 0) && (this.Length == 0)) && (this.X == 0)) && (this.Z == 0));
            }
        }

        public bool Equals(XZRectangle other) {
            return this == other;
        }

        public override bool Equals(object obj) {
            return (obj is XZRectangle) ? this == ((XZRectangle)obj) : false;
        }

        public override string ToString() {
            return string.Format("{{X:{0} Z:{1} Width:{2} Length:{3}}}", X, Z, Width, Length);
        }

        public override int GetHashCode() {
            return (this.X ^ this.Z ^ this.Width ^ this.Length);
        }

        public bool Intersects(XZRectangle value) {
            return value.Left < Right &&
                   Left < value.Right &&
                   value.Top < Bottom &&
                   Top < value.Bottom;
        }


        public void Intersects(ref XZRectangle value, out bool result) {
            result = value.Left < Right &&
                     Left < value.Right &&
                     value.Top < Bottom &&
                     Top < value.Bottom;
        }

        public static XZRectangle Intersect(XZRectangle value1, XZRectangle value2) {
            XZRectangle rectangle;
            Intersect(ref value1, ref value2, out rectangle);
            return rectangle;
        }


        public static void Intersect(ref XZRectangle value1, ref XZRectangle value2, out XZRectangle result) {
            if (value1.Intersects(value2)) {
                int right_side = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
                int left_side = Math.Max(value1.X, value2.X);
                int top_side = Math.Max(value1.Z, value2.Z);
                int bottom_side = Math.Min(value1.Z + value1.Length, value2.Z + value2.Length);
                result = new XZRectangle(left_side, top_side, right_side - left_side, bottom_side - top_side);
            }
            else {
                result = new XZRectangle(0, 0, 0, 0);
            }
        }

        public static XZRectangle Union(XZRectangle value1, XZRectangle value2) {
            int x = Math.Min(value1.X, value2.X);
            int y = Math.Min(value1.Z, value2.Z);
            return new XZRectangle(x, y,
                                 Math.Max(value1.Right, value2.Right) - x,
                                     Math.Max(value1.Bottom, value2.Bottom) - y);
        }

        public static void Union(ref XZRectangle value1, ref XZRectangle value2, out XZRectangle result) {
            result.X = Math.Min(value1.X, value2.X);
            result.Z = Math.Min(value1.Z, value2.Z);
            result.Width = Math.Max(value1.Right, value2.Right) - result.X;
            result.Length = Math.Max(value1.Bottom, value2.Bottom) - result.Z;
        }

        #endregion Public Methods
    }
}
