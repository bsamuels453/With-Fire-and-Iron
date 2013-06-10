#define PROFILE_PATHS

#region

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Forge.Core.Airship.Data;
using Forge.Framework;
using MonoGameUtility;
using Point = MonoGameUtility.Point;

#endregion

namespace Forge.Core.Airship.Controllers.AutoPilot{
    public static class Pathfinder{
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
            float maxAcceleration = attributes.MaxAcceleration * timeFrac;
            float maxTurnAcceleration = attributes.MaxTurnAcceleration * timeFrac;
            float maxAscentAcceleration = attributes.MaxAscentAcceleration * timeFrac;

            Vector3 curPosition = selfStateData.Position;
            float curAscentRate = selfStateData.AscentRate;
            Vector3 curAngle;
            float curTurnVel, curVelocity, maxVelocity;
            if (useReverse){
                curAngle = selfStateData.Angle + new Vector3(0, (float)Math.PI, 0);
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

            return new RetAttributes(curAscentRate, curTurnVel, curVelocity);
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
        /// <param name="target"></param>
        /// <param name="maxAcceleration"></param>
        /// <param name="curVelocity"></param>
        /// <param name="clamp"></param>
        /// <param name="newPos"></param>
        /// <param name="newVel"></param>
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
            var unitVec = Common.GetComponentFromAngle(-angle, 1);
            change.X += unitVec.X*posDiff*theta;
            change.Y += -unitVec.Y*posDiff*theta;


            newPos = (pos + change);
            newVel = newVelocity;//*theta;
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

        /// <summary>
        /// Gets the angular distance between the  two provided angles, in radians.
        /// </summary>
        /// <param name="a1">Reference angle</param>
        /// <param name="a2">Target angle</param>
        /// <returns></returns>
        static float GetAngularDistance(float a1, float a2) {
            while (a2 >= Math.PI * 2)
                a2 -= (float)Math.PI * 2;
            while (a2 < 0)
                a2 += (float)Math.PI * 2;
            while (a1 >= Math.PI * 2)
                a1 -= (float)Math.PI * 2;
            while (a1 < 0)
                a1 += (float)Math.PI * 2;

            //calculate the diff between the target angle and the current angle
            float d1 = a2 - a1;
            float shifter = 2 * (float)Math.PI - (a2 > a1 ? a2 : a1);
            float shifted = a2 < a1 ? a2 : a1;
            float d2 = shifter + shifted;
            return Math.Abs(d1) < Math.Abs(d2) ? d1 : d2;
        }

        #region Nested type: RetAttributes

        /// <summary>
        /// Represents the data returned by CalculateAirshipPath.
        /// </summary>
        public struct RetAttributes{
            public readonly float AscentRate;
            public readonly float ForwardVelocity;
            public readonly float TurnVelocity;

            public RetAttributes(float ascentRate, float turnVelocity, float forwardVelocity) : this(){
                AscentRate = ascentRate;
                TurnVelocity = turnVelocity;
                ForwardVelocity = forwardVelocity;
            }
        }

        #endregion
    }
}