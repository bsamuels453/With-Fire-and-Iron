#region

using System.Collections.Generic;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship.Data{
    /// <summary>
    ///   Contains information relevant to the current "state" of the airship. Includes things like position, orientation, controller, damage, etc.
    /// </summary>
    internal struct AirshipStateData{
        public Vector3 Position;
        public Vector3 Angle;

        public float AscentRate;
        public float TurnRate;
        public float Velocity;

        public List<AirshipBuff> ActiveBuffs;
        public AirshipControllerType ControllerType;
        public ManeuverTypeEnum CurrentManeuver;
        public object[] ManeuverParameters;

        public int FactionId;
    }
}