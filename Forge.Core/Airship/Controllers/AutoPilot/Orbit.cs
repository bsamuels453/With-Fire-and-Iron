#region

using System;
using Forge.Framework;
using Forge.Framework.Draw;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship.Controllers.AutoPilot{
    internal class Orbit : AirshipAutoPilot{
        readonly AirshipIndexer _airships;
        readonly float _preferredRange;
        readonly Airship _selfShip;
        readonly Airship _targetAirship;

        /// <summary>
        /// This value defines by how many time constants we should be leading the target.
        /// </summary>
        readonly float _targetLeadMultiplier;

        Vector3 _prevTargetVec;

        public Orbit(Airship selfShip, AirshipIndexer airships, int targetUid, float range){
            _preferredRange = range;
            _airships = airships;
            _targetAirship = _airships[targetUid];
            _selfShip = selfShip;
            _targetLeadMultiplier = 1;
            DebugText.CreateText("TargPos", 0, 40);
        }

        public override Pathfinder.RetAttributes CalculateNextPosition(double timeDelta){
            float timeDeltaSeconds = (float) timeDelta/1000f;
            var attributes = _selfShip.BuffedModelAttributes;
            var stateData = _selfShip.StateData;

            var curPosition = stateData.Position;

            var unitVec = Common.GetComponentFromAngle(_targetAirship.StateData.Angle.Y - (float) Math.PI/2, 1);
            var futureTargPosition = _targetAirship.StateData.Position;
            float posMult = _targetLeadMultiplier*(0.25f/timeDeltaSeconds);
            futureTargPosition.X += unitVec.X*_targetAirship.StateData.Velocity*posMult;
            futureTargPosition.Z += -unitVec.Y*_targetAirship.StateData.Velocity*posMult;
            futureTargPosition.Y += _targetAirship.StateData.AscentRate*posMult;

            var target = futureTargPosition;

            curPosition = new Vector3(curPosition.Z, curPosition.Y, curPosition.X);
            target = new Vector3(target.Z, target.Y, target.X);

            var diff = curPosition - target;
            diff.Normalize();

            var targetVec = Vector3.Cross(diff, Vector3.Down);
            _prevTargetVec = targetVec;


            var targetPos = _prevTargetVec*_preferredRange + target;
            targetPos.Y = target.Y;

            DebugText.SetText("TargPos", "(" + (curPosition - _targetAirship.StateData.Position).Length() + ")");

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