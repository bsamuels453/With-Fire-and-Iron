#region

using Forge.Framework;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship.Controllers.AutoPilot{
    /// <summary>
    /// Autopilot configuration used for keeping a target airship at a certain range.
    /// </summary>
    public class KeepAtRange : AirshipAutoPilot{
        readonly AirshipIndexer _airships;
        readonly float _preferredRange;
        readonly Airship _selfShip;
        readonly Airship _targetAirship;

        public KeepAtRange(Airship selfShip, AirshipIndexer airships, int targetUid, float range){
            _preferredRange = range;
            _airships = airships;
            _targetAirship = _airships[targetUid];
            _selfShip = selfShip;
            DebugStateShift.AddNewSet("ShipReverse", true);
        }

        public override Pathfinder.RetAttributes CalculateNextPosition(double timeDelta){
            var attributes = _selfShip.BuffedModelAttributes;
            var stateData = _selfShip.StateData;

            //get target position

            var curPosition = stateData.Position;
            var target = _targetAirship.StateData.Position;

            //apply hack to fix the weird axis
            curPosition = new Vector3(curPosition.Z, curPosition.Y, curPosition.X);
            target = new Vector3(target.Z, target.Y, target.X);

            var diff = curPosition - target;
            if (diff.Length() > 0.1f + _preferredRange){
                diff.Normalize();
                var targetPos = diff*_preferredRange + target;
                targetPos.Y = target.Y;
                //figure out if we should go forwards or backwards

                bool useReverse = Pathfinder.ShouldReverseBeUsed
                    (
                        curPosition,
                        targetPos,
                        stateData.Angle.Y,
                        attributes.MaxTurnSpeed,
                        attributes.MaxTurnAcceleration,
                        attributes.MaxForwardVelocity,
                        attributes.MaxReverseVelocity,
                        attributes.MaxAcceleration
                    );

                //DebugStateShift.UpdateSet("ShipReverse", useReverse, "Ship is in reverse: " + useReverse);

                var ret = Pathfinder.CalculateAirshipPath
                    (
                        targetPos,
                        attributes,
                        stateData,
                        (float) timeDelta,
                        useReverse
                    );
                return ret;
            }
            else{
                //we're already at the preferred keep@range point, so align ourselves to the target airship's angle
                //so can maintain keep@range easier
                var ret = Pathfinder.CalculateAirshipAngle
                    (
                        _targetAirship.StateData.Angle.Y,
                        attributes,
                        stateData,
                        (float) timeDelta
                    );
                return ret;
            }
        }
    }
}