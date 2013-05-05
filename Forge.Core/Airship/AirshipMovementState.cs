using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Forge.Core.Airship {
    public struct AirshipMovementState {
        public Vector3 CurPosition;
        public Vector3 Angle;

        public float CurVelocity;
        public float CurAltitudeVelocity;

        public float AngleTarget;
        public float VelocityTarget;
        public float AltitudeTarget;
    }
}
