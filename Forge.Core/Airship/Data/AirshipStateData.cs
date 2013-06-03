#region

using System;
using System.Collections.Generic;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship.Data{
    /// <summary>
    ///   Contains information relevant to the current "state" of the airship. Includes things like position, orientation, controller, damage, etc.
    /// </summary>
    internal struct AirshipStateData{
        public List<AirshipBuff> ActiveBuffs;
        public Vector3 Angle;
        public float AscentRate;
        public AirshipControllerType ControllerType;
        public ManeuverTypeEnum CurrentManeuver;
        public int FactionId;
        public object[] ManeuverParameters;
        public Vector3 Position;
        public float TurnRate;
        public float Velocity;
    }
}