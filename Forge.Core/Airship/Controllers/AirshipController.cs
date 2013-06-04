#region

using System.Collections.Generic;
using System.Linq;
using Forge.Core.Airship.Data;
using Forge.Framework;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship.Controllers{
    internal abstract class AirshipController{
        const float _degreesPerRadian = 0.0174532925f;
        readonly List<Hardpoint> _hardPoints;
        float _angleVel;
        float _ascentRate;
        float _velocity;

        protected AirshipController(ModelAttributes modelData, AirshipStateData stateData, List<Hardpoint> hardPoints){
            AirshipModelData = modelData;
            _hardPoints = hardPoints;

            Position = stateData.Position;
            Angle = stateData.Angle;
            _velocity = stateData.Velocity;
            _ascentRate = stateData.AscentRate;
            VelocityTarget = stateData.Velocity;

            MaxVelocityMod = 1;
            MaxTurnRateMod = 1;
            MaxAscentRateMod = 1;
            MaxAccelerationMod = 1;
            MaxTurnAccelerationMod = 1;
            MaxAscentAccelerationMod = 1;

            ActiveBuffs = stateData.ActiveBuffs;
            RecalculateBuffs();
        }

        protected ModelAttributes AirshipModelData { get; private set; }

        public Vector3 Position { get; private set; }
        public Vector3 Angle { get; private set; }
        public Matrix WorldMatrix { get; private set; }
        public List<AirshipBuff> ActiveBuffs { get; private set; }

        public float MaxVelocityMod { get; private set; }
        public float MaxTurnRateMod { get; private set; }
        public float MaxAscentRateMod { get; private set; }
        public float MaxAccelerationMod { get; private set; }
        public float MaxTurnAccelerationMod { get; private set; }
        public float MaxAscentAccelerationMod { get; private set; }

        #region Movement Properties

        /// <summary>
        ///   Sets angular velocity of the ship on the XZ plane. Scales from -MaxTurnSpeed to MaxTurnSpeed, where negative indicates a turn to port. Measured in degrees/second.
        /// </summary>
        public float TurnVelocity{
            get { return _angleVel; }
            protected set{
                float turnSpeed = value;
                if (value > AirshipModelData.MaxTurnSpeed)
                    turnSpeed = AirshipModelData.MaxTurnSpeed;
                if (value < -AirshipModelData.MaxTurnSpeed)
                    turnSpeed = -AirshipModelData.MaxTurnSpeed;
                _angleVel = turnSpeed;
            }
        }

        /// <summary>
        ///   Sets ascent velocity of the ship on the XZ plane. Scales from -MaxAscentSpeed to MaxAscentSpeed, where negative indicates moving down. Measured in meters per second.
        /// </summary>
        public float AscentVelocity{
            get { return _ascentRate; }
            protected set { _ascentRate = value; }
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

        public void SetAutoPilot(){
        }

        /// <summary>
        ///   Applies a specified buff to the airship to modify how it moves.
        /// </summary>
        public void ApplyBuff(AirshipBuff newBuff){
            ActiveBuffs.Add(newBuff);
            RecalculateBuffs();
        }

        /// <summary>
        ///   Recalculates the attribute modifiers based off of the buffs in ActiveBuffs.
        /// </summary>
        void RecalculateBuffs(){
            var activeBuffsGrouped =
                from buff in ActiveBuffs
                group buff by buff.Type;

            foreach (var buffType in activeBuffsGrouped){
                float statModifier = 1;

                foreach (var buff in buffType){
                    statModifier = statModifier*buff.Modifier;
                }
                switch (buffType.Key){
                    case AirshipBuff.BuffType.MaxAscentAcceleration:
                        MaxAscentAccelerationMod = statModifier;
                        break;
                    case AirshipBuff.BuffType.MaxTurnAcceleration:
                        MaxTurnAccelerationMod = statModifier;
                        break;
                    case AirshipBuff.BuffType.MaxAcceleration:
                        MaxAccelerationMod = statModifier;
                        break;
                    case AirshipBuff.BuffType.MaxAscentRate:
                        MaxAscentRateMod = statModifier;
                        break;
                    case AirshipBuff.BuffType.MaxTurnRate:
                        MaxTurnRateMod = statModifier;
                        break;
                    case AirshipBuff.BuffType.MaxVelocity:
                        MaxVelocityMod = statModifier;
                        break;
                    default:
                        DebugConsole.WriteLine("WARNING: Unhandled buff type detected: " + buffType);
                        break;
                }
            }
        }

        protected void Fire(){
            foreach (var hardPoint in _hardPoints){
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
            position.Y += _ascentRate*timeDeltaSeconds;
            Position = position;

            WorldMatrix = Common.GetWorldTranslation(Position, Angle, AirshipModelData.Length);
        }

        protected abstract void UpdateController(ref InputState state, double timeDelta);
    }
}