#region

using MonoGameUtility;

#endregion

namespace Forge.Core.Airship.Controllers.AutoPilot{
    internal class Orbit : AirshipAutoPilot{
        readonly AirshipIndexer _airships;
        readonly float _preferredRange;
        readonly Airship _selfShip;
        readonly Airship _targetAirship;

        public Orbit(Airship selfShip, AirshipIndexer airships, int targetUid, float range){
            _preferredRange = range;
            _airships = airships;
            _targetAirship = _airships[targetUid];
            _selfShip = selfShip;
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
            diff.Normalize();

            var targetVec = Vector3.Cross(diff, Vector3.Up);

            var targetPos = targetVec*_preferredRange + target;
            targetPos.Y = target.Y;


            var ret = Pathfinder.CalculateAirshipPath
                (
                    targetPos,
                    attributes,
                    stateData,
                    (float) timeDelta,
                    false
                );
            return ret;
        }
    }
}