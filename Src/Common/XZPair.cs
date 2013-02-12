#region

using System;
using System.Runtime.Serialization;

#endregion

namespace Gondola.Common{
    /// <summary>
    ///   each XZpair cooresponds to an x and z value. SERIALIZABLE CLONEABLE
    /// </summary>
    struct XZPair : ICloneable{
        public int X;
        public int Z;

        public XZPair(int x, int z){
            X = x;
            Z = z;
        }

        public XZPair(XZPair xzpair){
            X = xzpair.X;
            Z = xzpair.Z;
        }

        public XZPair(SerializationInfo info, StreamingContext ctxt){
            X = (int) info.GetValue("X", typeof (int));
            Z = (int) info.GetValue("Z", typeof (int));
        }

        #region ICloneable Members

        public object Clone(){
            return new XZPair(X, Z);
        }

        #endregion

        public static XZPair Zero(){
            return new XZPair(0, 0);
        }

        /*
        public static bool operator ==(Pair var1, XZpair var2) {
            if ((var1.X == var2.X) && (var1.Y == var2.Z)) {
                return true;
            }
            return false;
        }

        public static bool operator !=(Pair var1, XZpair var2) {
            if ((var1.X == var2.X) && (var1.Y == var2.Z)) {
                return false;
            }
            return true;
        }
         * */

        public static bool operator ==(XZPair var1, XZPair var2){
            if ((var1.X == var2.X) && (var1.Z == var2.Z)){
                return true;
            }
            return false;
        }

        public static bool operator !=(XZPair var1, XZPair var2){
            if ((var1.X == var2.X) && (var1.Z == var2.Z)){
                return false;
            }
            return true;
        }

        public bool Equals(XZPair other){
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.X == X && other.Z == Z;
        }

        public override bool Equals(object obj){
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (XZPair)) return false;
            return Equals((XZPair) obj);
        }

        public override int GetHashCode(){
            unchecked{
                return (X*397) ^ Z;
            }
        }
    }
}