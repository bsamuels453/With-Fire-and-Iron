#region

using System.Collections.Generic;
using Forge.Core.Airship.Data;
using Forge.Framework.Control;
using Forge.Framework.Resources;

#endregion

namespace Forge.Core.Airship.Controllers{
    /// <summary>
    /// Controller used for input-based control of the airship.
    /// </summary>
    public class PlayerAirshipController : AirshipController{
        #region TurnState enum

        public enum TurnState{
            TurningLeft,
            TurningRight,
            Stable
        }

        #endregion

        int _altitudeSpeed;
        TurnState _curTurnState;
        int _engineSpeed;

        public PlayerAirshipController(ModelAttributes modelData, AirshipStateData stateData, List<Hardpoint> hardPoints, AirshipIndexer airships) :
            base(modelData, stateData, airships, hardPoints){
        }

        public int EngineSpeed{
            get { return _engineSpeed; }
            set{
                if (value <= 3 && value >= -2){
                    _engineSpeed = value;
                }
            }
        }

        public KeyboardController GenerateKeyboardBindings(){
            var controller = new KeyboardController();

            var bindingDefs = Resource.LoadConfig("Config/AirshipBindings.config");
            controller.LoadFromFile<AirshipBinds>(bindingDefs);

            #region lambda defs

            KeyboardController.OnKeyPress increaseSpeed =
                (o, i, arg3) => { EngineSpeed++; };

            KeyboardController.OnKeyPress decreaseSpeed =
                (o, i, arg3) => { EngineSpeed--; };

            KeyboardController.OnKeyPress turnPort =
                (o, i, arg3) =>{
                    //if both turn buttons held down, go straight
                    if (_curTurnState == TurnState.TurningRight){
                        _curTurnState = TurnState.Stable;
                    }
                    else{
                        _curTurnState = TurnState.TurningLeft;
                    }
                };

            KeyboardController.OnKeyPress turnStarboard =
                (o, i, arg3) =>{
                    if (_curTurnState == TurnState.TurningLeft){
                        _curTurnState = TurnState.Stable;
                    }
                    else{
                        _curTurnState = TurnState.TurningRight;
                    }
                };

            KeyboardController.OnKeyPress decreaseAltitude =
                (o, i, arg3) =>{
                    if (_altitudeSpeed > 0){
                        _altitudeSpeed = 0;
                    }
                    else{
                        _altitudeSpeed = -1;
                    }
                };

            KeyboardController.OnKeyPress increaseAltitude =
                (o, i, arg3) =>{
                    if (_altitudeSpeed < 0){
                        _altitudeSpeed = 0;
                    }
                    else{
                        _altitudeSpeed = 1;
                    }
                };

            KeyboardController.OnKeyPress fire =
                (o, i, arg3) => Fire();

            #endregion

            controller.AddBindCallback(AirshipBinds.IncreaseForwardSpeed, BindCondition.OnKeyDown, increaseSpeed);
            controller.AddBindCallback(AirshipBinds.DecreaseForwardSpeed, BindCondition.OnKeyDown, decreaseSpeed);
            controller.AddBindCallback(AirshipBinds.TurnPort, BindCondition.KeyHeldDown, turnPort);
            controller.AddBindCallback(AirshipBinds.TurnStarboard, BindCondition.KeyHeldDown, turnStarboard);
            controller.AddBindCallback(AirshipBinds.DecreaseAltitude, BindCondition.KeyHeldDown, decreaseAltitude);
            controller.AddBindCallback(AirshipBinds.IncreaseAltitude, BindCondition.KeyHeldDown, increaseAltitude);
            controller.AddBindCallback(AirshipBinds.Fire, BindCondition.OnKeyDown, fire);

            return controller;
        }

        protected override void UpdateController(double timeDelta){
            float engineDutyCycle = (float) _engineSpeed/3;
            float altitudeDutyCycle = _altitudeSpeed;

            int turnValue = 0;
            switch (_curTurnState){
                case TurnState.Stable:
                    turnValue = 0;
                    break;
                case TurnState.TurningLeft:
                    turnValue = 1;
                    break;
                case TurnState.TurningRight:
                    turnValue = -1;
                    break;
            }
            //reset
            _curTurnState = TurnState.Stable;
            _altitudeSpeed = 0;

            if (!AutoPilotActive){
                base.TurnVelocity = turnValue*base.MaxTurnRate;
                base.Velocity = engineDutyCycle*base.MaxVelocity;
                base.AscentRate = altitudeDutyCycle*base.MaxAscentRate;
            }
        }

        #region Nested type: AirshipBinds

        enum AirshipBinds{
            IncreaseForwardSpeed,
            DecreaseForwardSpeed,
            TurnPort,
            TurnStarboard,
            IncreaseAltitude,
            DecreaseAltitude,
            Fire
        }

        #endregion
    }
}