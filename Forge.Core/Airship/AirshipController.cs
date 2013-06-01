#region

using System;
using System.Collections.Generic;
using Forge.Core.Airship.Data;
using Forge.Framework;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship{
    internal abstract class AirshipController{
        const float _degreesPerRadian = 0.0174532925f;
        readonly Action<Matrix> _setAirshipWMatrix;
        float _altitudeTarget;
        float _angleTarget;
        float _angleVel;

        float _ascentVel;
        bool _noAltitudeTarget;
        bool _noAngleTarget;
        float _velocity;

        List<Hardpoint> _hardPoints;

        protected AirshipController(Action<Matrix> setWorldMatrix, ModelAttributes modelData, AirshipMovementData movementData, List<Hardpoint> hardPoints){
            _setAirshipWMatrix = setWorldMatrix;
            AirshipModelData = modelData;

            Position = movementData.CurPosition;
            Angle = movementData.Angle;
            _velocity = movementData.CurVelocity;
            _ascentVel = movementData.CurAltitudeVelocity;
            _angleTarget = movementData.AngleTarget;
            VelocityTarget = movementData.VelocityTarget;
            _altitudeTarget = movementData.AltitudeTarget;

            _hardPoints = hardPoints;
        }

        protected ModelAttributes AirshipModelData { get; private set; }

        public Vector3 Position { get; private set; }
        public Vector3 Angle { get; private set; }

        #region properties

        /// <summary>
        ///   Sets angular velocity of the ship on the XZ plane. Scales from -MaxTurnSpeed to MaxTurnSpeed, where negative indicates a turn to port. Measured in degrees/second.
        /// </summary>
        public float AngularVelocity{
            get { return _angleVel; }
            protected set{
                float turnSpeed = value;
                if (value > AirshipModelData.MaxTurnSpeed*100)
                    turnSpeed = AirshipModelData.MaxTurnSpeed * 100;
                if (value < -AirshipModelData.MaxTurnSpeed * 100)
                    turnSpeed = -AirshipModelData.MaxTurnSpeed * 100;
                _angleVel = turnSpeed;
                _noAngleTarget = true;
            }
        }

        /// <summary>
        ///   Sets the angle target of the airship. Scales from 0 to 360. xx (make sure this is enforced) Measured in degrees.
        /// </summary>
        public float AngleTarget{
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
        ///   Sets ascent velocity of the ship on the XZ plane. Scales from -MaxAscentSpeed to MaxAscentSpeed, where negative indicates moving down. Measured in meters per second.
        /// </summary>
        public float AscentVelocity{
            get { return _ascentVel; }
            protected set{
                _ascentVel = value;
                _noAltitudeTarget = true;
            }
        }

        /// <summary>
        ///   Sets the altitude target of the airship. Measured in meters.
        /// </summary>
        public float AltitudeTarget{
            get{
                if (_noAltitudeTarget)
                    throw new Exception("No altitude target set");
                return _altitudeTarget;
            }
            protected set{
                _altitudeTarget = value;
                _noAltitudeTarget = false;
            }
        }


        /// <summary>
        ///   Sets airship velocity target scalar. Scales from -MaxReverseSpeed to MaxForwardSpeed Measured in meters per second.
        /// </summary>
        public float VelocityTarget { get; protected set; }


        //todo: depreciate this
        /// <summary>
        ///   Sets airship velocity scalar. Scales from -MaxReverseSpeed to MaxForwardSpeed Measured in meters per second.
        /// </summary>
        public float Velocity{
            get { return _velocity; }
            protected set { _velocity = value; }
        }

        #endregion

        protected void Fire(){
            foreach (var hardPoint in _hardPoints) {
                hardPoint.Fire();
            }
        }

        public void Update(ref InputState state, double timeDelta){
            UpdateController(ref state, timeDelta);
            float timeDeltaSeconds = (float) timeDelta/1000;

            var ang = Angle;
            ang.Y += _angleVel*_degreesPerRadian*timeDeltaSeconds;
            var unitVec = Common.GetComponentFromAngle(ang.Y, 1);
            Angle = ang;

            var position = Position;
            position.X += unitVec.X*_velocity*timeDeltaSeconds;
            position.Z += -unitVec.Y*_velocity*timeDeltaSeconds;
            position.Y += _ascentVel*timeDeltaSeconds;
            Position = position;

            var worldMatrix = Common.GetWorldTranslation(Position, Angle, AirshipModelData.Length);
            _setAirshipWMatrix.Invoke(worldMatrix);
        }

        protected abstract void UpdateController(ref InputState state, double timeDelta);
    }
}