#region

using System;
using System.Diagnostics;
using System.Drawing;
using Forge.Core.Airship.Controllers;
using Forge.Core.Airship.Data;
using Forge.Framework;
using MonoGameUtility;

#endregion

namespace Pathfinding{
    internal class Pathfinder{
        readonly ModelAttributes _attributes;
        readonly Bitmap _bmp;
        readonly AirshipStateData _stateData;
        readonly Vector3 _targPos;
        readonly float _timeDelta;

        public Pathfinder(float timeDelta){
            _timeDelta = timeDelta;
            _bmp = new Bitmap(500, 500);
            for (int x = 0; x < 500; x++){
                for (int y = 0; y < 500; y++){
                    _bmp.SetPixel(x, y, Color.White);
                }
            }


            _attributes = new ModelAttributes();
            _attributes.MaxAcceleration = 1;
            _attributes.MaxAscentAcceleration = 1;
            _attributes.MaxAscentRate = 1;
            _attributes.MaxForwardVelocity = 20;
            _attributes.MaxReverseVelocity = 1;
            _attributes.MaxTurnAcceleration =0.3f;
            _attributes.MaxTurnSpeed =0.3f;

            _stateData = new AirshipStateData();
            _stateData.Angle = new Vector3(0,-0.5f,0);
            _stateData.AscentRate = 0;
            _stateData.Position = new Vector3(200, 0, 50);
            _stateData.TurnRate = 0;
            _stateData.Velocity = 0;

            _targPos = new Vector3(200, 0, 200);

            Bmp.Dot(_bmp, 200, 50, Color.Green);
            Bmp.Dot(_bmp, 200, 200, Color.Red);

        }

        public Bitmap Tick(out float velocity, out float turnVel, out float angleDiff, out float curAngle){
            var ret = CalculateAirshipPath
                (
                    _targPos,
                    _attributes,
                    _stateData,
                    _timeDelta,
                    false);

            _stateData.TurnRate = ret.TurnVelocity;
            _stateData.Velocity = ret.ForwardVelocity;


            float timeDeltaSeconds = _timeDelta/1000;

            var ang = _stateData.Angle;
            ang.Y += _stateData.TurnRate*timeDeltaSeconds;
            var unitVec = Common.GetComponentFromAngle(ang.Y, 1);
            _stateData.Angle = ang;

            var position = _stateData.Position;
            position.X += unitVec.X*_stateData.Velocity*timeDeltaSeconds;
            position.Z += unitVec.Y*_stateData.Velocity*timeDeltaSeconds;
            position.Y += _stateData.AscentRate*timeDeltaSeconds;
            _stateData.Position = position;

            try {
                _bmp.SetPixel((int)_stateData.Position.X, (int)_stateData.Position.Z, Color.Black);
            }
            catch{
            }

            velocity = _stateData.Velocity;
            turnVel = _stateData.TurnRate;
            angleDiff = ret.AngleDiff;
            curAngle = _stateData.Angle.Y;
            return _bmp;
        }


        /// <summary>
        /// Calculates the path of the airship at the next tick.
        /// </summary>
        /// <param name="target">The target position the airship should be approaching.</param>
        /// <param name="selfStateData"> </param>
        /// <param name="timeDelta">The amount of delta time that should be taken into consideration for velocities, in milliseconds. </param>
        /// <param name="useReverse">Whether or not the approach should be made in reverse.</param>
        /// <param name="attributes"> </param>
        public static RetAttributes CalculateAirshipPath(
            Vector3 target,
            ModelAttributes attributes,
            AirshipStateData selfStateData,
            float timeDelta,
            bool useReverse){
            float timeFrac = timeDelta/1000f;
            float maxTurnRate = attributes.MaxTurnSpeed;
            float maxAscentRate = attributes.MaxAscentRate;
            float maxAcceleration = attributes.MaxAcceleration*timeFrac;
            float maxTurnAcceleration = attributes.MaxTurnAcceleration*timeFrac;
            float maxAscentAcceleration = attributes.MaxAscentAcceleration*timeFrac;

            Vector3 curPosition = selfStateData.Position;
            float curAscentRate = selfStateData.AscentRate;
            Vector3 curAngle;
            float curTurnVel, curVelocity, maxVelocity;
            if (useReverse){
                curAngle = selfStateData.Angle + new Vector3(0, (float) Math.PI, 0);
                curTurnVel = -selfStateData.TurnRate;
                curVelocity = -selfStateData.Velocity;
                maxVelocity = attributes.MaxReverseVelocity;
            }
            else{
                curAngle = selfStateData.Angle;
                curTurnVel = selfStateData.TurnRate;
                curVelocity = selfStateData.Velocity;
                maxVelocity = attributes.MaxForwardVelocity;
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
            float destXZAngle, distToTarget;
            Vector3 diff = target - curPosition;
            Common.GetAngleFromComponents(out destXZAngle, out distToTarget, diff.X, diff.Z);

            float targAngle1 = curAngle.Y - destXZAngle;

            float targAngle2 = destXZAngle - curAngle.Y;

            float targAngle = targAngle1 < targAngle2 ? targAngle1 : targAngle2;

            //restrict both angles to between 0 and 2pi
            while (destXZAngle >= Math.PI * 2)
                destXZAngle -= (float)Math.PI * 2;
            while (destXZAngle < 0)
                destXZAngle += (float)Math.PI * 2;
            while (curAngle.Y >= Math.PI * 2)
                curAngle.Y -= (float)Math.PI * 2;
            while (curAngle.Y < 0)
                curAngle.Y += (float)Math.PI * 2;

            //calculate the diff between the target angle and the current angle
            float d1 = destXZAngle - curAngle.Y;
            float shifter = 2 * (float)Math.PI - (destXZAngle > curAngle.Y ? destXZAngle : curAngle.Y);
            float shifted = destXZAngle < curAngle.Y ? destXZAngle : curAngle.Y;
            float d2 = shifter + shifted;
            float turnDiff = Math.Abs(d1) < Math.Abs(d2) ? d1 : d2;

            CalculateNewScalar
                (
                    curAngle.Y,
                    turnDiff,
                    maxTurnAcceleration,
                    curTurnVel,
                    clampTurnRate,
                    out curAngle.Y,
                    out curTurnVel
                );
            /*
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
             */

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

            if (useReverse){
                curVelocity = -curVelocity;
                curTurnVel = -curTurnVel;
            }

            return new RetAttributes(curAscentRate, curTurnVel, curVelocity, targAngle);
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

        /// <summary>
        /// Calculates the next position and velocity of the provided scalar.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="diff"></param>
        /// <param name="maxAcceleration"></param>
        /// <param name="curVelocity"></param>
        /// <param name="clamp"></param>
        /// <param name="newPos"></param>
        /// <param name="newVel"></param>
        static void CalculateNewScalar(float pos, float diff, float maxAcceleration, float curVelocity, Func<float, float> clamp,
            out float newPos, out float newVel){



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

        /// <summary>
        /// Calculates the next position and velocity of the provided vector. A modifier is applied that takes
        /// into consideration the angle between the movement vector and the target vector in order to prevent
        /// the airship from moving full steam ahead in the tangential direction.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="target"></param>
        /// <param name="angle"></param>
        /// <param name="angleTarget"></param>
        /// <param name="maxAcceleration"></param>
        /// <param name="curVelocity"></param>
        /// <param name="clamp"></param>
        /// <param name="newPos"></param>
        /// <param name="newVel"></param>
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
            var unitVec = Common.GetComponentFromAngle(angle, 1);


            posDiff = 5;
            newVelocity = 5;


            change.X += unitVec.X*posDiff*theta;
            change.Y += -unitVec.Y*posDiff*theta;


            newPos = (pos + change);
            newVel = newVelocity; //*theta;
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

        static int SimulatePath(Vector3 target, AirshipController controller, bool useReverse){
            //this constant is used to convert the units of input data from X/sec to X/tick
            const float simulationSecondsPerTick = 1.0f;
#if PROFILE_PATHS
            var difff = target - controller.Position;
            var bmp = new Bitmap((int) difff.X*2, (int) difff.Z*2);
            var offset = new Point((int) difff.X, (int) difff.Y);
#endif

            var sw = new Stopwatch();
            sw.Start();

            int numTicks = 0;

            float maxVelocity = controller.MaxVelocity*simulationSecondsPerTick;
            float maxTurnRate = controller.MaxTurnRate*simulationSecondsPerTick;
            float maxAscentRate = controller.MaxAscentRate*simulationSecondsPerTick;
            float maxAcceleration = controller.MaxAcceleration*simulationSecondsPerTick;
            float maxTurnAcceleration = controller.MaxTurnAcceleration*simulationSecondsPerTick;
            float maxAscentAcceleration = controller.MaxAscentAcceleration*simulationSecondsPerTick;

            Vector3 curPosition = controller.Position;
            float curAscentRate = controller.AscentRate*simulationSecondsPerTick;
            Vector3 curAngle;
            float curTurnVel, curVelocity;
            if (useReverse){
                curAngle = controller.Angle + new Vector3(0, (float) Math.PI, 0);
                curTurnVel = -controller.TurnVelocity*simulationSecondsPerTick;
                curVelocity = -controller.Velocity*simulationSecondsPerTick;
            }
            else{
                curAngle = controller.Angle;
                curTurnVel = controller.TurnVelocity*simulationSecondsPerTick;
                curVelocity = controller.Velocity*simulationSecondsPerTick;
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
#if PROFILE_PATHS
                var color = Color.FromArgb(255, 255 - (int) (curVelocity/maxVelocity*255), 255);
                try{
                    bmp.SetPixel((int) curPosition.X + offset.X, (int) curPosition.Z + offset.Y, color);
                }
                catch{
                    break;
                }
#endif
                if (Vector3.Distance(target, curPosition) < maxVelocity){
                    break;
                }
            }
#if PROFILE_PATHS
            bmp.SetPixel((int) controller.Position.X + offset.X, (int) controller.Position.Z + offset.Y, Color.Red);
            bmp.SetPixel((int) target.X + offset.X, (int) target.Z + offset.Y, Color.Red);

            bmp.Save("Test.png", ImageFormat.Png);
            bmp.Dispose();
#endif

            sw.Stop();

            double d = sw.ElapsedMilliseconds;

            return numTicks;
        }

        #region Nested type: RetAttributes

        /// <summary>
        /// Represents the data returned by CalculateAirshipPath.
        /// </summary>
        public struct RetAttributes{
            public readonly float AscentRate;
            public readonly float ForwardVelocity;
            public readonly float TurnVelocity;
            public readonly float AngleDiff;

            public RetAttributes(float ascentRate, float turnVelocity, float forwardVelocity, float angleDiff)
                : this(){
                AscentRate = ascentRate;
                TurnVelocity = turnVelocity;
                ForwardVelocity = forwardVelocity;
                AngleDiff = angleDiff;
            }
        }

        #endregion
    }
}