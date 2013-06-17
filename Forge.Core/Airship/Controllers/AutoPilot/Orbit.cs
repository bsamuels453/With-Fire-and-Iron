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
        Vector3 _prevTargetVec;

        public Orbit(Airship selfShip, AirshipIndexer airships, int targetUid, float range){
            _preferredRange = range;
            _airships = airships;
            _targetAirship = _airships[targetUid];
            _selfShip = selfShip;
            DebugText.CreateText("TargPos", 0, 40);
        }

        public override Pathfinder.RetAttributes CalculateNextPosition(double timeDelta){
            float timeDeltaSeconds = (float) timeDelta/1000f;
            var attributes = _selfShip.BuffedModelAttributes;
            var stateData = _selfShip.StateData;

            //get target position

            var curPosition = stateData.Position;

            var ang = _targetAirship.StateData.Angle;
            ang.Y += _targetAirship.StateData.TurnRate*timeDeltaSeconds;
            var unitVec = Common.GetComponentFromAngle(ang.Y - (float) Math.PI/2, 1);
            var futureTargPosition = _targetAirship.StateData.Position;
            futureTargPosition.X += unitVec.X*_targetAirship.StateData.Velocity*timeDeltaSeconds*20;
            futureTargPosition.Z += -unitVec.Y*_targetAirship.StateData.Velocity*timeDeltaSeconds*20;
            futureTargPosition.Y += _targetAirship.StateData.AscentRate*timeDeltaSeconds*20;

            var target = futureTargPosition;
            //if (Vector3.Distance(curPosition, target) >= _preferredRange * 0.9f){
            //apply hack to fix the weird axis
            curPosition = new Vector3(curPosition.Z, curPosition.Y, curPosition.X);
            target = new Vector3(target.Z, target.Y, target.X);

            var diff = curPosition - target;
            diff.Normalize();

            var targetVec = Vector3.Cross(diff, Vector3.Down);
            _prevTargetVec = targetVec;
            //}

            var targetPos = _prevTargetVec*_preferredRange + target;
            targetPos.Y = target.Y;


            DebugText.SetText("TargPos", "(" + _prevTargetVec.X*_preferredRange + "," + _prevTargetVec.Z*_preferredRange + ")");

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