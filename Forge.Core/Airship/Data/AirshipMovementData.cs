using Microsoft.Xna.Framework;

namespace Forge.Core.Airship.Data {
    public struct AirshipMovementData {
        public Vector3 CurPosition;
        public Vector3 Angle;

        public float CurVelocity;
        public float CurAltitudeVelocity;

        public float AngleTarget;
        public float VelocityTarget;
        public float AltitudeTarget;
    }
}
