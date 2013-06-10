#region

using System.Collections.Generic;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship.Data{
    /// <summary>
    ///   Contains information relevant to the current "state" of the airship. Includes things like position, orientation, controller, damage, etc.
    /// </summary>
    public class AirshipStateData{
        public string Model;
        public int AirshipId;

        public int FactionId;

        public Vector3 Position;
        public Vector3 Angle;

        public float AscentRate;
        public float TurnRate;
        public float Velocity;

        public List<AirshipBuff> ActiveBuffs;
        public AirshipControllerType ControllerType;
        public ManeuverTypeEnum CurrentManeuver;
        public object[] ManeuverParameters;

        public AirshipStateData(){
            AirshipId = -1;
            FactionId = -1;
            ActiveBuffs = new List<AirshipBuff>();
            ManeuverParameters = new object[0];
            ControllerType = AirshipControllerType.None; 
        }
    }
}