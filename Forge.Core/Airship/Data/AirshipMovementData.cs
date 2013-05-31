#region

using MonoGameUtility;

#endregion

namespace Forge.Core.Airship.Data{
    public struct AirshipMovementData{
        public float AltitudeTarget;
        public Vector3 Angle;

        public float AngleTarget;
        public float CurAltitudeVelocity;
        public Vector3 CurPosition;
        public float CurVelocity;
        public float VelocityTarget;
    }
}