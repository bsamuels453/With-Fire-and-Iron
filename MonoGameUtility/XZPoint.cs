using System;
using ProtoBuf;

namespace MonoGameUtility {
    [ProtoContract]
    public struct XZPoint : IEquatable<XZPoint> {
        #region Private Fields

        private static XZPoint zeroPoint = new XZPoint();

        #endregion Private Fields


        #region Public Fields
        [ProtoMember(1)]
        public int X;
        [ProtoMember(2)]
        public int Z;

        #endregion Public Fields


        #region Properties

        public static XZPoint Zero {
            get { return zeroPoint; }
        }

        #endregion Properties


        #region Constructors

        public XZPoint(int x, int z) {
            this.X = x;
            this.Z = z;
        }

        #endregion Constructors


        #region Public methods

        public static bool operator ==(XZPoint a, XZPoint b) {
            return a.Equals(b);
        }

        public static bool operator !=(XZPoint a, XZPoint b) {
            return !a.Equals(b);
        }

        public bool Equals(XZPoint other) {
            return ((X == other.X) && (Z == other.Z));
        }

        public override bool Equals(object obj) {
            return (obj is XZPoint) ? Equals((XZPoint)obj) : false;
        }

        public override int GetHashCode() {
            return X ^ Z;
        }

        public override string ToString() {
            return string.Format("{{X:{0} Z:{1}}}", X, Z);
        }

        #endregion


        #region overloads

        public static XZPoint operator +(XZPoint value1, XZPoint value2) {
            value1.X += value2.X;
            value1.Z += value2.Z;
            return value1;
        }

        public static XZPoint operator -(XZPoint value1, XZPoint value2) {
            value1.X -= value2.X;
            value1.Z -= value2.Z;
            return value1;
        }

        #endregion
    }
}
