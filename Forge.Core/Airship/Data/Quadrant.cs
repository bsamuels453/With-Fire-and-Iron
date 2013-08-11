#region

using System;

#endregion

namespace Forge.Core.Airship.Data{
    public static class Quadrant{
        #region Direction enum

        public enum Direction{
            Bow,
            Port,
            Stern,
            Starboard
        }

        #endregion

        #region Rotation enum

        public enum Rotation{
            Left90,
            Right90
        }

        #endregion

        #region Side enum

        public enum Side{
            Starboard,
            Port
        }

        #endregion

        public static Side PointToSide(float z){
            return z < 0 ? Side.Port : Side.Starboard;
        }

        public static Direction RotateDir(Direction original, Rotation rotation){
            if (rotation == Rotation.Left90){
                var iDir = (int) original + 1;
                if (iDir == 4){
                    iDir = 0;
                }
                return (Direction) iDir;
            }
            if (rotation == Rotation.Right90){
                var iDir = (int) original - 1;
                if (iDir == -1){
                    iDir = 3;
                }
                return (Direction) iDir;
            }
            throw new Exception("how the hell did that happen");
        }
    }
}