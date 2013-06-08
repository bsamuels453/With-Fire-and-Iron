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

        /// <summary>
        ///   Simulates how long it would take for the airship to reach the target position using the supplied parameters.
        /// </summary>
        /// <param name="target"> The position of the target that you want the airship to be at. </param>
        /// <param name="useReverse"> Whether or not to approach the target in reverse. </param>
        /// <returns> The number of milliseconds required to reach target. </returns>
        int SimulatePath(Vector3 target, bool useReverse) {
            //this constant is used to convert the units of input data from X/sec to X/tick
            const float secondsPerTick = 0.025f;

            var sw = new Stopwatch();
            sw.Start();

            var startPos = Controller.Position;

            float maxVelocity = Controller.MaxVelocity * secondsPerTick;
            float maxTurnRate = Controller.MaxTurnRate * secondsPerTick;
            float maxAscentRate = Controller.MaxAscentRate * secondsPerTick;
            float maxAcceleration = Controller.MaxAcceleration * secondsPerTick;
            float maxTurnAcceleration = Controller.MaxTurnAcceleration * secondsPerTick;
            float maxAscentAcceleration = Controller.MaxAscentAcceleration * secondsPerTick;

            Vector3 curPosition = Controller.Position;
            float curAscentRate = Controller.AscentRate * secondsPerTick;
            Vector3 curAngle;
            float curTurnVel, curVelocity;
            if (useReverse) {
                curAngle = Controller.Angle + new Vector3(0, (float)Math.PI, 0);
                curTurnVel = -Controller.TurnVelocity * secondsPerTick;
                curVelocity = -Controller.Velocity * secondsPerTick;
            }
            else {
                curAngle = Controller.Angle;
                curTurnVel = Controller.TurnVelocity * secondsPerTick;
                curVelocity = Controller.Velocity * secondsPerTick;
            }

            Func<float, float> clampTurnRate = v => {
                if (v > maxTurnRate) {
                    v = maxTurnRate;
                }
                if (v < -maxTurnRate) {
                    v = -maxTurnRate;
                }
                return v;
            };


            while (true) {
                float destXZAngle, distToTarget;
                Vector3 diff = target - curPosition;
                Common.GetAngleFromComponents(out destXZAngle, out distToTarget, diff.X, diff.Z);
                float diffXZAngle = destXZAngle - curAngle.Y;


                float sign;
                if (diffXZAngle > 0)
                    sign = 1;
                else
                    sign = -1;

                var newVelocity = curTurnVel + sign * maxTurnAcceleration;
                newVelocity = clampTurnRate(newVelocity);

                var breakoffDist = GetCoveredDistanceByAccel(Math.Abs(newVelocity), maxTurnAcceleration);

                var angleDiff = curTurnVel + 0.5f * sign * maxTurnAcceleration;
                if (sign * diffXZAngle <= breakoffDist) {
                    angleDiff = curTurnVel + -sign * 0.5f * maxTurnAcceleration;
                    newVelocity = curTurnVel + -sign * maxTurnAcceleration;

                    //angleDiff = clampTurnRate(angleDiff);
                    newVelocity = clampTurnRate(newVelocity);
                }


                curAngle += new Vector3(0, angleDiff, 0);
                curTurnVel = newVelocity;

                if (diffXZAngle > 0.01f) {
                    break;
                }
            }

            sw.Stop();
            double d = sw.ElapsedMilliseconds;
            throw new Exception();
        }

        /// <summary>
        /// Returns the distance that is covered while the body accelerates from REST to vf.
        /// Derived from V_f = (V_o^2 + 2*a*dX)^(1/2)
        /// x = (V_f^2-V_o^2)/(2a)
        /// </summary>
        /// <param name="vf">The final speed to be accelerated to.</param>
        /// <param name="a">The acceleration.</param>
        /// <returns></returns>
        static float GetCoveredDistanceByAccel(float vf, float a) {
            float numerator = vf * vf - 0;
            float denominator = 2 * a;
            return numerator / denominator;
        }
    }
}