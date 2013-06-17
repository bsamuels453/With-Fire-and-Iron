#region

using System;
using System.Collections.Generic;
using System.Linq;
using Forge.Core.Airship.Controllers.AutoPilot;
using Forge.Core.Airship.Data;
using Forge.Framework;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship.Controllers{
    /// <summary>
    /// An airship's controller is used to handle and modify its position, orientation, and rotation.
    /// The controller contains the pos/orient/rot information which is referenced by the airship class.
    /// </summary>
    public abstract class AirshipController{
        readonly AirshipIndexer _airships;
        readonly List<Hardpoint> _hardPoints;
        protected bool AutoPilotActive;
        ModelAttributes _airshipModelData;
        AirshipAutoPilot _autoPilot;
        float _velocityTarget;

        protected AirshipController(ModelAttributes modelData, AirshipStateData stateData, AirshipIndexer airships, List<Hardpoint> hardPoints){
            _airshipModelData = modelData;
            _hardPoints = hardPoints;
            _airships = airships;
            StateData = stateData;

            Position = stateData.Position;
            Angle = stateData.Angle;
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

        #region statistical properties

        /// <summary>
        ///   Position of the airship.
        /// </summary>
        public Vector3 Position{
            get { return StateData.Position; }
            private set { StateData.Position = value; }
        }

        /// <summary>
        ///   3D angle of the airship in radians.
        /// </summary>
        public Vector3 Angle{
            get { return StateData.Angle; }
            private set { StateData.Angle = value; }
        }

        /// <summary>
        ///   The worldmatrix used to translate the position of the airship into model space.
        /// </summary>
        public Matrix WorldTransform { get; private set; }

        public List<AirshipBuff> ActiveBuffs { get; private set; }

        public float MaxVelocityMod { get; private set; }
        public float MaxTurnRateMod { get; private set; }
        public float MaxAscentRateMod { get; private set; }
        public float MaxAccelerationMod { get; private set; }
        public float MaxTurnAccelerationMod { get; private set; }
        public float MaxAscentAccelerationMod { get; private set; }

        /// <summary>
        ///   Meters per second
        /// </summary>
        public float MaxVelocity{
            get { return _airshipModelData.MaxForwardVelocity*MaxVelocityMod; }
        }

        /// <summary>
        ///   Meters per second
        /// </summary>
        public float MaxReverseVelocity{
            get { return _airshipModelData.MaxReverseVelocity*MaxVelocityMod; }
        }

        /// <summary>
        ///   Radians per second
        /// </summary>
        public float MaxTurnRate{
            get { return _airshipModelData.MaxTurnSpeed*MaxTurnRateMod; }
        }

        /// <summary>
        ///   Meters per second
        /// </summary>
        public float MaxAscentRate{
            get { return _airshipModelData.MaxAscentRate*MaxAscentRateMod; }
        }

        /// <summary>
        ///   Meters per second squared
        /// </summary>
        public float MaxAcceleration{
            get { return _airshipModelData.MaxAcceleration*MaxAccelerationMod; }
        }

        /// <summary>
        ///   Radians per second squared
        /// </summary>
        public float MaxTurnAcceleration{
            get { return _airshipModelData.MaxTurnAcceleration*MaxTurnAccelerationMod; }
        }

        /// <summary>
        ///   Meters per second squared
        /// </summary>
        public float MaxAscentAcceleration{
            get { return _airshipModelData.MaxAscentAcceleration*MaxAscentAccelerationMod; }
        }

        #endregion

        #region Movement Properties

        /// <summary>
        ///   Sets angular velocity of the ship on the XZ plane. Scales from -MaxTurnSpeed to MaxTurnSpeed, where negative indicates a turn to port. Measured in degrees/second.
        /// </summary>
        public float TurnVelocity{
            get { return StateData.TurnRate; }
            protected set{
                float turnSpeed = value;
                if (value > _airshipModelData.MaxTurnSpeed*MaxTurnRateMod)
                    turnSpeed = _airshipModelData.MaxTurnSpeed*MaxTurnRateMod;
                if (value < -_airshipModelData.MaxTurnSpeed*MaxTurnRateMod)
                    turnSpeed = -_airshipModelData.MaxTurnSpeed*MaxTurnRateMod;
                StateData.TurnRate = turnSpeed;
            }
        }

        /// <summary>
        ///   Sets ascent velocity of the ship on the XZ plane. Scales from -MaxAscentRate to MaxAscentRate, where negative indicates moving down. Measured in meters per second.
        /// </summary>
        public float AscentRate{
            get { return StateData.AscentRate; }
            protected set{
                float ascentRate = value;
                if (value > _airshipModelData.MaxAscentRate*MaxAscentRateMod)
                    ascentRate = _airshipModelData.MaxAscentRate*MaxAscentRateMod;
                if (value < -_airshipModelData.MaxAscentRate*MaxAscentRateMod)
                    ascentRate = -_airshipModelData.MaxAscentRate*MaxAscentRateMod;
                StateData.AscentRate = ascentRate;
            }
        }

        /// <summary>
        ///   Sets airship velocity target scalar. Scales from -MaxReverseVelocity to MaxForwardVelocity Measured in meters per second.
        /// </summary>
        public float VelocityTarget{
            get { return _velocityTarget; }
            protected set{
                float velocityTarget = value;
                if (value > _airshipModelData.MaxForwardVelocity*MaxVelocityMod)
                    velocityTarget = _airshipModelData.MaxForwardVelocity*MaxVelocityMod;
                if (value < -_airshipModelData.MaxReverseVelocity*MaxVelocityMod)
                    velocityTarget = -_airshipModelData.MaxReverseVelocity*MaxVelocityMod;
                _velocityTarget = velocityTarget;
            }
        }


        //todo: depreciate this
        /// <summary>
        ///   Sets airship velocity scalar. Scales from -MaxReverseVelocity to MaxForwardSpeed Measured in meters per second.
        /// </summary>
        public float Velocity{
            get { return StateData.Velocity; }
            protected set { StateData.Velocity = value; }
        }

        #endregion

        public AirshipStateData StateData { get; private set; }

        public void SetAutoPilot(AirshipAutoPilot autoPilot){
            _autoPilot = autoPilot;
            AutoPilotActive = true;
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

        public ModelAttributes GetBuffedAttributes(){
            var ret = new ModelAttributes();
            ret.Length = _airshipModelData.Length;
            ret.NumDecks = _airshipModelData.NumDecks;
            ret.MaxAcceleration = MaxAcceleration;
            ret.MaxAscentAcceleration = MaxAscentAcceleration;
            ret.MaxAscentRate = MaxAscentRate;
            ret.MaxTurnAcceleration = MaxTurnAcceleration;
            ret.MaxTurnSpeed = MaxTurnRate;
            ret.MaxForwardVelocity = MaxVelocity;
            ret.MaxReverseVelocity = MaxReverseVelocity;
            return ret;
        }

        public void Update(ref InputState state, double timeDelta){
            UpdateController(ref state, timeDelta);

            if (AutoPilotActive){
                var ret = _autoPilot.CalculateNextPosition(timeDelta);
                AscentRate = ret.AscentRate;
                Velocity = ret.ForwardVelocity;
                TurnVelocity = ret.TurnVelocity;
            }

            float timeDeltaSeconds = (float) timeDelta/1000;

            var ang = Angle;
            ang.Y += StateData.TurnRate*timeDeltaSeconds;
            var unitVec = Common.GetComponentFromAngle(ang.Y - (float) Math.PI/2, 1);
            Angle = ang;

            var position = Position;
            position.X += unitVec.X*StateData.Velocity*timeDeltaSeconds;
            position.Z += -unitVec.Y*StateData.Velocity*timeDeltaSeconds;
            position.Y += StateData.AscentRate*timeDeltaSeconds;
            Position = position;

            WorldTransform = Common.GetWorldTranslation(Position, Angle + new Vector3(0, -(float) Math.PI/2, 0), _airshipModelData.Length);
        }

        protected abstract void UpdateController(ref InputState state, double timeDelta);
    }
}