namespace Forge.Core.Airship.Controllers.AutoPilot{
    internal class KeepAtRange : AirshipAutoPilot{
        readonly AirshipIndexer _airships;
        readonly AirshipController _controller;
        readonly float _preferredRange;
        readonly Airship _targetAirship;

        public KeepAtRange(int targetUid, float range, AirshipController controller, AirshipIndexer airships){
            _preferredRange = range;
            _targetAirship = _airships[targetUid];
            _controller = controller;
            _airships = airships;
        }

        public override Pathfinder.RetAttributes CalculateNextPosition(double timeDelta){
            //get target position
            var diff = _controller.Position - _targetAirship.Position;
            diff.Normalize();
            var targetPos = diff*_preferredRange;

            //figure out if we should go forwards or backwards
            bool useReverse = Pathfinder.ShouldReverseBeUsed
                (
                    _controller.Position,
                    targetPos,
                    _controller.Angle.Y,
                    _controller.MaxTurnRate,
                    _controller.MaxTurnAcceleration,
                    _controller.MaxVelocity,
                    _controller.MaxReverseVelocity,
                    _controller.MaxAcceleration
                );

            var ret = Pathfinder.CalculateAirshipPath
                (
                    targetPos,
                    _controller,
                    (float) timeDelta,
                    useReverse
                );
            return ret;
        }
    }
}