#region



#endregion

namespace Forge.Core.Airship.Controllers.AutoPilot{
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
        }

        public override Pathfinder.RetAttributes CalculateNextPosition(double timeDelta){
            var attributes = _selfShip.BuffedModelAttributes;
            var stateData = _selfShip.StateData;

            //get target position

            var diff = stateData.Position - _targetAirship.StateData.Position;
            diff.Normalize();
            var targetPos = diff*_preferredRange + _targetAirship.StateData.Position;

            //figure out if we should go forwards or backwards

            bool useReverse = Pathfinder.ShouldReverseBeUsed
                (
                    stateData.Position,
                    targetPos,
                    stateData.Angle.Y,
                    attributes.MaxTurnSpeed,
                    attributes.MaxTurnAcceleration,
                    attributes.MaxForwardVelocity,
                    attributes.MaxReverseVelocity,
                    attributes.MaxAcceleration
                );

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
    }
}