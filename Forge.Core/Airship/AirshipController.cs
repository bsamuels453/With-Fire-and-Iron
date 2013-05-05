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

        Vector3 _position;
        Vector3 _angle;

        #region properties
        /// <summary>
        /// Sets angular velocity of the ship on the XZ plane. Scales from -MaxTurnSpeed to MaxTurnSpeed, where negative indicates a turn to port.
        /// Measured in degrees/second.
        /// </summary>
        protected float AngularVelocity{
            get { return _angleVel; }
            set { 
                _angleVel = value;
                _noAngleTarget = true;
            }
        }

        /// <summary>
        /// Sets the angle target of the airship. Scales from 0 to 360. xx (make sure this is enforced)
        /// Measured in degrees.
        /// </summary>
        protected float AngleTarget{
            get{
                if (_noAngleTarget)
                    throw new Exception("No angle target set");
                return _angleTarget;
            }
            set{
                _angleTarget = value;
                _noAltitudeTarget = false;
            }
        }

        /// <summary>
        /// Sets ascent velocity of the ship on the XZ plane. Scales from -MaxAscentSpeed to MaxAscentSpeed, where negative indicates moving down.
        /// Measured in meters per second.
        /// </summary>
        protected float AscentVelocity {
            get { return _ascentVel; }
            set {
                _ascentVel = value;
                _noAltitudeTarget = true;
            }
        }

        /// <summary>
        /// Sets the altitude target of the airship.
        /// Measured in meters.
        /// </summary>
        protected float AltitudeTarget {
            get {
                if (_noAltitudeTarget)
                    throw new Exception("No altitude target set");
                return _altitudeTarget;
            }
            set {
                _altitudeTarget = value;
                _noAltitudeTarget = false;
            }
        }


        /// <summary>
        /// Sets airship velocity target scalar. Scales from -MaxReverseSpeed to MaxForwardSpeed
        /// Measured in meters per second.
        /// </summary>
        protected float VelocityTarget {
            get { return _velocityTarget; }
            set { _velocityTarget = value; }
        }


        //todo: depreciate this
        /// <summary>
        /// Sets airship velocity scalar. Scales from -MaxReverseSpeed to MaxForwardSpeed
        /// Measured in meters per second.
        /// </summary>
        protected float Velocity {
            get { return _velocity; }
            set { _velocity = value; }
        }
        #endregion

        protected void Fire(){
            throw new NotImplementedException();
        }


        protected AirshipController(Action<Matrix> setWorldMatrix, ModelAttributes modelData, AirshipMovementState movementState){
            _setAirshipWMatrix = setWorldMatrix;
            AirshipModelData = modelData;

            _position = movementState.CurPosition;
            _angle = movementState.Angle;
            _velocity = movementState.CurVelocity;
            _ascentVel = movementState.CurAltitudeVelocity;
            _angleTarget = movementState.AngleTarget;
            _velocityTarget = movementState.VelocityTarget;
            _altitudeTarget = movementState.AltitudeTarget;

        }

        public void Update(ref InputState state, double timeDelta){
            UpdateController(ref state, timeDelta);
            float timeDeltaSeconds = (float)timeDelta*1000;

            _angle.Y += _angleVel * _degreesPerRadian * timeDeltaSeconds;
            var unitVec = Common.GetComponentFromAngle(_angle.Y, 1);

            _position.X += unitVec.X * _velocity * timeDeltaSeconds;
            _position.Z += -unitVec.Y * _velocity * timeDeltaSeconds;
            _position.Y += _ascentVel * timeDeltaSeconds;;
            var worldMatrix = Common.GetWorldTranslation(_position, _angle, AirshipModelData.Length);
            _setAirshipWMatrix.Invoke(worldMatrix);
        }

        protected abstract void UpdateController(ref InputState state, double timeDelta);

        public struct ModelAttributes{
            public float Length;
            public float MaxAscentSpeed;
            public float MaxForwardSpeed;
            public float MaxReverseSpeed;
            public float MaxTurnSpeed;

            //public float AscentAcceleration;
            //public float TurnAcceleration;
            //public float EngineAcceleration;

        }

        public struct AirshipMovementState{
            public Vector3 CurPosition;
            public Vector3 Angle;

            public float CurVelocity;
            public float CurAltitudeVelocity;

            public float AngleTarget;
            public float VelocityTarget;
            public float AltitudeTarget;

        }
    }
}
