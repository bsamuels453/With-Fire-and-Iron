#region

using System;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor{
    public struct WallSegmentIdentifier : IEquatable<WallSegmentIdentifier>{
        public readonly Vector3 RefPoint1;
        public readonly Vector3 RefPoint2;

        public WallSegmentIdentifier(Vector3 refPoint2, Vector3 refPoint1){
            RefPoint2 = refPoint2;
            RefPoint1 = refPoint1;
        }

        #region equality operators

        public bool Equals(WallSegmentIdentifier other){
            if (RefPoint2 == other.RefPoint2 && RefPoint1 == other.RefPoint1)
                return true;
            if (RefPoint2 == other.RefPoint1 && other.RefPoint2 == RefPoint1)
                return true;
            return false;
        }

        public static bool operator ==(WallSegmentIdentifier wallid1, WallSegmentIdentifier wallid2){
            if (wallid1.RefPoint2 == wallid2.RefPoint2 && wallid1.RefPoint1 == wallid2.RefPoint1)
                return true;
            if (wallid1.RefPoint2 == wallid2.RefPoint1 && wallid2.RefPoint1 == wallid1.RefPoint2)
                return true;
            return false;
        }

        public static bool operator !=(WallSegmentIdentifier wallid1, WallSegmentIdentifier wallid2){
            if (wallid1.RefPoint2 == wallid2.RefPoint2 && wallid1.RefPoint1 == wallid2.RefPoint1)
                return false;
            if (wallid1.RefPoint2 == wallid2.RefPoint1 && wallid2.RefPoint1 == wallid1.RefPoint2)
                return false;
            return true;
        }

        public override bool Equals(object obj){
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof (WallSegmentIdentifier)) return false;
            return Equals((WallSegmentIdentifier) obj);
        }

        #endregion

        public override int GetHashCode(){
            unchecked{
                // ReSharper disable NonReadonlyFieldInGetHashCode
                return (RefPoint2.GetHashCode()*397) ^ RefPoint1.GetHashCode();
                // ReSharper restore NonReadonlyFieldInGetHashCode
            }
        }
    }
}