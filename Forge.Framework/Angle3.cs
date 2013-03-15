#region

using Microsoft.Xna.Framework;

#endregion

namespace Forge.Framework{
    public struct Angle3{
        public float Pitch;
        public float Roll;
        public float Yaw;

        public Angle3(float pitch, float roll, float yaw){
            Pitch = pitch;
            Roll = roll;
            Yaw = yaw;
        }

        public Vector3 NormalizeToVec(){
            float sum = Pitch + Roll + Yaw;
            return new Vector3(Pitch/sum, Roll/sum, Yaw/sum);
        }

        public Vector3 ToVec(){
            //all object start facing down x axis
            return new Vector3(Roll, Yaw, Pitch);
        }
    }
}