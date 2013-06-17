#define PROFILE_PATHS

#region

using System;
using Forge.Core.Airship.Data;
using Forge.Framework;
using MonoGameUtility;

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
            float maxAcceleration = attributes.MaxAcceleration*timeFrac;
            float maxTurnAcceleration = attributes.MaxTurnAcceleration*timeFrac;
            float maxAscentAcceleration = attributes.MaxAscentAcceleration*timeFrac;

            var curPosition = new Vector3(selfStateData.Position.Z, selfStateData.Position.Y, selfStateData.Position.X);
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

            float turnDiff = GetAngularDistance(curAngle.Y, destXZAngle);
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

            float altDiff = (target.Y - curPosition.Y);
            CalculateNewScalar
                (
                    curPosition.Y,
                    altDiff,
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

            if (useReverse){
                curVelocity = -curVelocity;
                curTurnVel = -curTurnVel;
            }

            return new RetAttributes(curAscentRate, curTurnVel, curVelocity);
        }

        /// <summary>
        /// Calculates the angle of the airship at the next tick.
        /// </summary>
        /// <param name="target">The target position the airship should be approaching.</param>
        /// <param name="selfStateData"> </param>
        /// <param name="timeDelta">The amount of delta time that should be taken into consideration for velocities, in milliseconds. </param>
        /// <param name="attributes"> </param>
        /// <returns></returns>
        public static RetAttributes CalculateAirshipAngle(
            float target,
            ModelAttributes attributes,
            AirshipStateData selfStateData,
            float timeDelta){
            float timeFrac = timeDelta/1000f;
            float maxTurnRate = attributes.MaxTurnSpeed;
            float maxTurnAcceleration = attributes.MaxTurnAcceleration*timeFrac;

            var curAngle = selfStateData.Angle;
            var curTurnVel = selfStateData.TurnRate;

            Func<float, float> clampTurnRate = v =>{
                                                   if (v > maxTurnRate){
                                                       v = maxTurnRate;
                                                   }
                                                   if (v < -maxTurnRate){
                                                       v = -maxTurnRate;
                                                   }
                                                   return v;
                                               };

            float turnDiff = GetAngularDistance(curAngle.Y, target);
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

            return new RetAttributes(0, curTurnVel, 0);
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
            var target2 = new Vector2(target.X, target.Z);
            var pos2 = new Vector2(pos.X, pos.Z);
            Vector2 diff = target2 - pos2;
            Common.GetAngleFromComponents(out destXZAngle, out distToTarget, diff.X, diff.Y);
            var tarAngle = GetAngularDistance(curAngle, destXZAngle);

            //if the angle terminates behind us...
            if (tarAngle > Math.PI/2 || tarAngle < -Math.PI/2){
                float timeToTurn = Math.Abs(tarAngle/maxAngularSpeed);
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
            if (diff == 0){
                newPos = pos;
                newVel = curVelocity;
                return;
            }
            float sign;
            if (diff > 0)
                sign = 1;
            else
                sign = -1;

            var newVelocity = curVelocity + sign*maxAcceleration;
            newVelocity = clamp.Invoke(newVelocity);

            var breakoffDist = GetCoveredDistanceByAccel(Math.Abs(newVelocity), maxAcceleration);

            var posDiff = curVelocity + 0.5f*sign*maxAcceleration;
            //slow down if necessary
            if (sign*diff <= breakoffDist){
                posDiff = curVelocity + -sign*0.5f*maxAcceleration;
                newVelocity = curVelocity + -sign*maxAcceleration;

                newVelocity = clamp.Invoke(newVelocity);
            }
            //apply partial acceleration if necessary
            if (newVelocity > Math.Abs(diff)){
                newVelocity = 0;
                posDiff = diff;
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

            float theta = (float) Math.Cos(Math.Abs(angleTarget - angle)) + 0.5f;
            //if (theta > 1)
            theta = 1;


            var unitVec = Common.GetComponentFromAngle(angle, 1);

            Vector2 change;
            if (Math.Abs(posDiff) > diff.Length()){
                change = diff;
                newVel = 0;
            }
            else{
                change = new Vector2(unitVec.X*posDiff*theta, -unitVec.Y*posDiff*theta);
                newVel = newVelocity*theta;
            }

            newPos = (pos + change);
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
        static float GetAngularDistance(float a1, float a2){
            while (a2 >= Math.PI*2)
                a2 -= (float) Math.PI*2;
            while (a2 < 0)
                a2 += (float) Math.PI*2;
            while (a1 >= Math.PI*2)
                a1 -= (float) Math.PI*2;
            while (a1 < 0)
                a1 += (float) Math.PI*2;

            float d = a2 - a1;
            if (Math.Abs(d) > Math.PI){
                if (d > 0){
                    float targ = (float) (Math.PI*2) - d;
                    return -targ;
                }
                else{
                    float targ = (float) (Math.PI*2) + d;
                    return targ;
                }
            }
            else{
                return d;
            }
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