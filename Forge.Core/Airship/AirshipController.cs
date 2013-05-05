using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forge.Framework;
using Microsoft.Xna.Framework;

namespace Forge.Core.Airship {
    abstract class AirshipController {
        const float _degreesPerRadian = 0.0174532925f;
        readonly Action<Matrix> _setAirshipWMatrix;
        protected ModelAttributes AirshipModelData { get; private set; }

        bool _noAngleTarget;
        float _angleVel;
        float _angleTarget;

        float _velocity;
        float _velocityTarget;

        bool _noAltitudeTarget;
        float _ascentVel;
        float _altitudeTarget;

        public Vector3 Position { get; private set; }
        public Vector3 Angle { get; private set; }

        #region properties
        /// <summary>
        /// Sets angular velocity of the ship on the XZ plane. Scales from -MaxTurnSpeed to MaxTurnSpeed, where negative indicates a turn to port.
        /// Measured in degrees/second.
        /// </summary>
        public float AngularVelocity{
            get { return _angleVel; }
            protected set {
                float turnSpeed = value;
                if (value > AirshipModelData.MaxTurnSpeed)
                    turnSpeed = AirshipModelData.MaxTurnSpeed;
                if (value < -AirshipModelData.MaxTurnSpeed)
                    turnSpeed = -AirshipModelData.MaxTurnSpeed;
                _angleVel = turnSpeed;
                _noAngleTarget = true;
            }
        }

        /// <summary>
        /// Sets the angle target of the airship. Scales from 0 to 360. xx (make sure this is enforced)
        /// Measured in degrees.
        /// </summary>
        public float AngleTarget {
            get{
                if (_noAngleTarget)
                    throw new Exception("No angle target set");
                return _angleTarget;
            }
            protected set{
                _angleTarget = value;
                _noAltitudeTarget = false;
            }
        }

        /// <summary>
        /// Sets ascent velocity of the ship on the XZ plane. Scales from -MaxAscentSpeed to MaxAscentSpeed, where negative indicates moving down.
        /// Measured in meters per second.
        /// </summary>
        public float AscentVelocity {
            get { return _ascentVel; }
            protected set {
                _ascentVel = value;
                _noAltitudeTarget = true;
            }
        }

        /// <summary>
        /// Sets the altitude target of the airship.
        /// Measured in meters.
        /// </summary>
        public float AltitudeTarget {
            get {
                if (_noAltitudeTarget)
                    throw new Exception("No altitude target set");
                return _altitudeTarget;
            }
            protected set {
                _altitudeTarget = value;
                _noAltitudeTarget = false;
            }
        }


        /// <summary>
        /// Sets airship velocity target scalar. Scales from -MaxReverseSpeed to MaxForwardSpeed
        /// Measured in meters per second.
        /// </summary>
        public float VelocityTarget {
            get { return _velocityTarget; }
            protected set { _velocityTarget = value; }
        }


        //todo: depreciate this
        /// <summary>
        /// Sets airship velocity scalar. Scales from -MaxReverseSpeed to MaxForwardSpeed
        /// Measured in meters per second.
        /// </summary>
        public float Velocity {
            get { return _velocity; }
            protected set { _velocity = value; }
        }
        #endregion

        protected void Fire(){
            throw new NotImplementedException();
        }

        protected AirshipController(Action<Matrix> setWorldMatrix, ModelAttributes modelData, AirshipMovementState movementState){
            _setAirshipWMatrix = setWorldMatrix;
            AirshipModelData = modelData;

            Position = movementState.CurPosition;
            Angle = movementState.Angle;
            _velocity = movementState.CurVelocity;
            _ascentVel = movementState.CurAltitudeVelocity;
            _angleTarget = movementState.AngleTarget;
            _velocityTarget = movementState.VelocityTarget;
            _altitudeTarget = movementState.AltitudeTarget;

        }

        public void Update(ref InputState state, double timeDelta){
            UpdateController(ref state, timeDelta);
            float timeDeltaSeconds = (float)timeDelta/1000;

            var ang = Angle;
            ang.Y += _angleVel * _degreesPerRadian * timeDeltaSeconds;
            var unitVec = Common.GetComponentFromAngle(ang.Y, 1);
            Angle = ang;

            var position = Position;
            position.X += unitVec.X * _velocity * timeDeltaSeconds;
            position.Z += -unitVec.Y * _velocity * timeDeltaSeconds;
            position.Y += _ascentVel * timeDeltaSeconds;
            Position = position;

            var worldMatrix = Common.GetWorldTranslation(Position, Angle, AirshipModelData.Length);
            _setAirshipWMatrix.Invoke(worldMatrix);
        }

        protected abstract void UpdateController(ref InputState state, double timeDelta);
    }
}
