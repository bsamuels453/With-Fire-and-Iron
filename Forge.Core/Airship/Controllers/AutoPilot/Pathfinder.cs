#region

using System;
using System.Diagnostics;
using Forge.Framework;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship.Controllers.AutoPilot{
    internal static class Pathfinder{
        const float _simulationSecondsPerTick = 1.0f;

        static int SimulatePath(Vector3 target, AirshipController controller, bool useReverse){
            //this constant is used to convert the units of input data from X/sec to X/tick

            var sw = new Stopwatch();
            sw.Start();

            int numTicks = 0;

            float maxVelocity = controller.MaxVelocity*_simulationSecondsPerTick;
            float maxTurnRate = controller.MaxTurnRate*_simulationSecondsPerTick;
            float maxAscentRate = controller.MaxAscentRate*_simulationSecondsPerTick;
            float maxAcceleration = controller.MaxAcceleration*_simulationSecondsPerTick;
            float maxTurnAcceleration = controller.MaxTurnAcceleration*_simulationSecondsPerTick;
            float maxAscentAcceleration = controller.MaxAscentAcceleration*_simulationSecondsPerTick;

            Vector3 curPosition = controller.Position;
            float curAscentRate = controller.AscentRate*_simulationSecondsPerTick;
            Vector3 curAngle;
            float curTurnVel, curVelocity;
            if (useReverse){
                curAngle = controller.Angle + new Vector3(0, (float) Math.PI, 0);
                curTurnVel = -controller.TurnVelocity*_simulationSecondsPerTick;
                curVelocity = -controller.Velocity*_simulationSecondsPerTick;
            }
            else{
                curAngle = controller.Angle;
                curTurnVel = controller.TurnVelocity*_simulationSecondsPerTick;
                curVelocity = controller.Velocity*_simulationSecondsPerTick;
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
                numTicks++;
                float destXZAngle, distToTarget;
                Vector3 diff = target - curPosition;
                Common.GetAngleFromComponents(out destXZAngle, out distToTarget, diff.X, diff.Z);
                CalculateNewScalar
                    (
                        curAngle.Y,
                        destXZAngle,
                        maxTurnAcceleration,
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
                        destXZAngle,
                        maxAcceleration,
                        curVelocity,
                        clampVelocity,
                        out newPos,
                        out curVelocity
                    );

                curPosition.X = newPos.X;
                curPosition.Z = newPos.Y;

                if (Vector3.Distance(target, curPosition) < maxVelocity){
                    break;
                }
            }


            sw.Stop();
            double d = sw.ElapsedMilliseconds;

            return numTicks;
        }

        /// <summary>
        /// Very lazy algorithm for figuring out whether a target should be approached in reverse, or if a turn
        /// should be executed and the approach made in forewards direction.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="target"></param>
        /// <param name="curAngle"></param>
        /// <param name="maxAngularSpeed"></param>
        /// <param name="maxAngularAcceleration">not used yet</param>
        /// <param name="maxVelocity"></param>
        /// <param name="maxReverseVelocity"></param>
        /// <param name="maxAcceleration">not used yet</param>
        /// <returns></returns>
        public static bool ShouldReverseBeUsed(
            Vector3 pos,
            Vector3 target,
            float curAngle,
            float maxAngularSpeed,
            float maxAngularAcceleration,
            float maxVelocity,
            float maxReverseVelocity,
            float maxAcceleration){
            float destXZAngle, distToTarget;
            Vector3 diff = target - pos;
            Common.GetAngleFromComponents(out destXZAngle, out distToTarget, diff.X, diff.Z);
            var tarAngle = destXZAngle - curAngle;

            //if the angle terminates behind us...
            if (tarAngle > Math.PI || tarAngle < -Math.PI){
                float timeToTurn = tarAngle/maxAngularSpeed;
                float timeToTravel = diff.Length()/maxVelocity;

                float timeToTravelReverse = diff.Length()/maxReverseVelocity;

                if (timeToTurn + timeToTravel > timeToTravelReverse){
                    return true;
                }
                return false;
            }
            return false;
        }

        static void CalculateNewScalar(float pos, float target, float maxAcceleration, float curVelocity, Func<float, float> clamp,
            out float newPos, out float newVel){
            float diff = target - pos;

            float sign;
            if (diff > 0)
                sign = 1;
            else
                sign = -1;

            var newVelocity = curVelocity + sign*maxAcceleration;
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

        static void CalculateNewVector(Vector2 pos, Vector2 target, float angle, float angleTarget, float maxAcceleration, float curVelocity,
            Func<float, float> clamp,
            out Vector2 newPos, out float newVel){
            Vector2 diff = target - pos;

            var newVelocity = curVelocity + maxAcceleration;
            newVelocity = clamp.Invoke(newVelocity);

            var breakoffDist = GetCoveredDistanceByAccel(Math.Abs(newVelocity), maxAcceleration);

            var posDiff = curVelocity + 0.5f*maxAcceleration;
            if (diff.Length() <= breakoffDist){
                posDiff = curVelocity + -0.5f*maxAcceleration;
                newVelocity = curVelocity + -maxAcceleration;

                newVelocity = clamp.Invoke(newVelocity);
            }

            float theta = Math.Abs(angleTarget - angle);

            Vector2 change = new Vector2();
            var unitVec = Common.GetComponentFromAngle(-angle, 1);
            change.X += unitVec.X*posDiff*theta;
            change.Y += -unitVec.Y*posDiff*theta;


            newPos = (pos + change);
            newVel = newVelocity*theta;
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