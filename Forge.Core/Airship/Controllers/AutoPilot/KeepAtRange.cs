#region

using System;
using System.Diagnostics;
using Forge.Framework;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship.Controllers.AutoPilot{
    internal class KeepAtRange : AirshipAutoPilot{
        readonly float _preferredRange;
        readonly Airship _targetAirship;

        public KeepAtRange(int targetUid, float range, AirshipController controller, AirshipIndexer airships) : base(controller, airships){
            _preferredRange = range;
            _targetAirship = airships[targetUid];
        }

        protected override void UpdateChild(double timeDelta){
            //get target position

            //figure out if we should go forwards or backwards

            //apply the change

            throw new NotImplementedException();
        }

        int SimulatePath(Vector3 target, bool useReverse){
            //this constant is used to convert the units of input data from X/sec to X/tick
            const float secondsPerTick = 0.1f;

            var sw = new Stopwatch();
            sw.Start();

            var startPos = Controller.Position;

            float maxVelocity = Controller.MaxVelocity*secondsPerTick;
            float maxTurnRate = Controller.MaxTurnRate*secondsPerTick;
            float maxAscentRate = Controller.MaxAscentRate*secondsPerTick;
            float maxAcceleration = Controller.MaxAcceleration*secondsPerTick;
            float maxTurnAcceleration = Controller.MaxTurnAcceleration*secondsPerTick;
            float maxAscentAcceleration = Controller.MaxAscentAcceleration*secondsPerTick;

            Vector3 curPosition = Controller.Position;
            float curAscentRate = Controller.AscentRate*secondsPerTick;
            Vector3 curAngle;
            float curTurnVel, curVelocity;
            if (useReverse){
                curAngle = Controller.Angle + new Vector3(0, (float) Math.PI, 0);
                curTurnVel = -Controller.TurnVelocity*secondsPerTick;
                curVelocity = -Controller.Velocity*secondsPerTick;
            }
            else{
                curAngle = Controller.Angle;
                curTurnVel = Controller.TurnVelocity*secondsPerTick;
                curVelocity = Controller.Velocity*secondsPerTick;
            }

            Func<float, float> clampTurnRate = v =>{
                                                   if (v > maxTurnRate){
                                                       v = maxTurnRate;
                                                   }
                                                   if (v < -maxTurnRate){
                                                       v = -maxTurnRate;
                                                   }
                                                   return v;
                                               };


            Func<float, float> clampAscentRate = v =>{
                                                     if (v > maxAscentRate){
                                                         v = maxAscentRate;
                                                     }
                                                     if (v < -maxAscentRate){
                                                         v = -maxAscentRate;
                                                     }
                                                     return v;
                                                 };

            Func<float, float> clampVelocity = v =>{
                                                   if (v > maxVelocity){
                                                       v = maxVelocity;
                                                   }
                                                   if (v < -maxVelocity){
                                                       v = -maxVelocity;
                                                   }
                                                   return v;
                                               };


            while (true){
                float destXZAngle, distToTarget;
                Vector3 diff = target - curPosition;
                Common.GetAngleFromComponents(out destXZAngle, out distToTarget, diff.X, diff.Z);
                CalculateNewScalar
                    (
                        curAngle.Y,
                        destXZAngle,
                        maxTurnAcceleration,
                        maxTurnRate,
                        curTurnVel,
                        clampTurnRate,
                        out curAngle.Y,
                        out curTurnVel
                    );

                CalculateNewScalar
                    (
                        curPosition.Y,
                        target.Y,
                        maxAscentAcceleration,
                        maxAscentRate,
                        curAscentRate,
                        clampAscentRate,
                        out curPosition.Y,
                        out curAscentRate
                    );

                Vector2 newPos;

                CalculateNewVector
                    (
                        new Vector2(curPosition.X, curPosition.Z),
                        new Vector2(target.X, target.Z),
                        curAngle.Y,
                        maxAcceleration,
                        maxVelocity,
                        curVelocity,
                        clampVelocity,
                        out newPos,
                        out curVelocity
                    );

                curPosition.X = newPos.X;
                curPosition.Z = newPos.Y;
                if (Vector3.Distance(target, curPosition) < 10){
                    break;
                }
            }


            sw.Stop();
            double d = sw.ElapsedMilliseconds;

            throw new Exception();
        }

        void CalculateNewScalar(float pos, float target, float maxAcceleration, float maxVelocity, float curVelocity, Func<float, float> clamp,
            out float newPos, out float newVel){
            float diff = target - pos;

            float sign;
            if (diff > 0)
                sign = 1;
            else
                sign = -1;

            var newVelocity = curVelocity + sign*maxVelocity;
            newVelocity = clamp.Invoke(newVelocity);

            var breakoffDist = GetCoveredDistanceByAccel(Math.Abs(newVelocity), maxAcceleration);

            var posDiff = curVelocity + 0.5f*sign*maxAcceleration;
            if (sign*diff <= breakoffDist){
                posDiff = curVelocity + -sign*0.5f*maxAcceleration;
                newVelocity = curVelocity + -sign*maxAcceleration;

                newVelocity = clamp.Invoke(newVelocity);
            }
            newPos = pos + posDiff;
            newVel = newVelocity;
        }

        void CalculateNewVector(Vector2 pos, Vector2 target, float angle, float maxAcceleration, float maxVelocity, float curVelocity, Func<float, float> clamp,
            out Vector2 newPos, out float newVel){
            Vector2 diff = target - pos;

            var newVelocity = curVelocity + maxVelocity;
            newVelocity = clamp.Invoke(newVelocity);

            var breakoffDist = GetCoveredDistanceByAccel(Math.Abs(newVelocity), maxAcceleration);

            var posDiff = curVelocity + 0.5f*maxAcceleration;
            if (diff.Length() <= breakoffDist){
                posDiff = curVelocity + -0.5f*maxAcceleration;
                newVelocity = curVelocity + -maxAcceleration;

                newVelocity = clamp.Invoke(newVelocity);
            }

            Vector2 change = new Vector2();
            var unitVec = Common.GetComponentFromAngle(-angle, 1);
            change.X += unitVec.X*posDiff;
            change.Y += -unitVec.Y*posDiff;


            newPos = (pos + change);
            newVel = newVelocity;
        }


        /// <summary>
        /// Returns the distance that is covered while the body accelerates from REST to vf.
        /// Derived from V_f = (V_o^2 + 2*a*dX)^(1/2)
        /// x = (V_f^2-V_o^2)/(2a)
        /// </summary>
        /// <param name="vf">The final speed to be accelerated to.</param>
        /// <param name="a">The acceleration.</param>
        /// <returns></returns>
        static float GetCoveredDistanceByAccel(float vf, float a){
            float numerator = vf*vf - 0;
            float denominator = 2*a;
            return numerator/denominator;
        }
    }
}