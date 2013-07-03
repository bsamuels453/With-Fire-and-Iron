#region

using System;
using Forge.Core.Util;
using Forge.Framework;
using MonoGameUtility;

#endregion

namespace Forge.Core.Camera{
    public class SubmarineCamera : ICamera{
        public Angle3 LookAng;
        public Vector3 Position;

        public SubmarineCamera(Vector3 position, Angle3 lookAng){
            throw new Exception();
            Position = position;
            LookAng = lookAng;
        }

        #region ICamera Members

        public Matrix ViewMatrix{
            get { return RenderHelper.CalculateViewMatrix(Position, LookAng); }
        }

        #endregion

        public void MoveForward(float dist){
            Position.X = Position.X + (float) Math.Sin(LookAng.Yaw)*(float) Math.Cos(LookAng.Pitch)*dist;
            Position.Y = Position.Y + (float) Math.Sin(LookAng.Pitch)*dist;
            Position.Z = Position.Z + (float) Math.Cos(LookAng.Yaw)*(float) Math.Cos(LookAng.Pitch)*dist;
        }

        public void MoveBackward(float dist){
            Position.X = Position.X - (float) Math.Sin(LookAng.Yaw)*(float) Math.Cos(LookAng.Pitch)*dist;
            Position.Y = Position.Y - (float) Math.Sin(LookAng.Pitch)*dist;
            Position.Z = Position.Z - (float) Math.Cos(LookAng.Yaw)*(float) Math.Cos(LookAng.Pitch)*dist;
        }

        public void MoveLeft(float dist){
            Position.X = Position.X + (float) Math.Sin(LookAng.Yaw + 3.14159f/2)*dist;
            Position.Z = Position.Z + (float) Math.Cos(LookAng.Yaw + 3.14159f/2)*dist;
        }

        public void MoveRight(float dist){
            Position.X = Position.X - (float) Math.Sin(LookAng.Yaw + 3.14159f/2)*dist;
            Position.Z = Position.Z - (float) Math.Cos(LookAng.Yaw + 3.14159f/2)*dist;
        }
    }
}