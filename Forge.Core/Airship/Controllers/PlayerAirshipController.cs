#region

using System.Collections.Generic;
using Forge.Core.Airship.Data;
using Forge.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Forge.Core.Airship.Controllers{
    internal class PlayerAirshipController : AirshipController{
        #region TurnState enum

        public enum TurnState{
            TurningLeft,
            TurningRight,
            Stable
        }

        #endregion

        public TurnState CurTurnState;
        int _engineSpeed;

        public PlayerAirshipController(ModelAttributes modelData, AirshipStateData stateData, List<Hardpoint> hardPoints) :
            base(modelData, stateData, hardPoints){
        }

        public int EngineSpeed{
            get { return _engineSpeed; }
            set{
                if (value <= 3 && value >= -2){
                    _engineSpeed = value;
                }
            }
        }

        protected override void UpdateController(ref InputState state, double timeDelta){
            var keyState = state.KeyboardState;
            var prevKeyState = state.PrevState.KeyboardState;

            if (state.PrevState.KeyboardState.IsKeyDown(Keys.Space) && state.KeyboardState.IsKeyUp(Keys.Space)){
                Fire();
            }

            if (keyState.IsKeyUp(Keys.W) && prevKeyState.IsKeyDown(Keys.W)){
                EngineSpeed++;
            }
            if (keyState.IsKeyUp(Keys.S) && prevKeyState.IsKeyDown(Keys.S)){
                if (EngineSpeed > 0){
                    EngineSpeed = 0;
                }
                else{
                    EngineSpeed--;
                }
            }

            int altitudeSpeed = 0;
            if (keyState.IsKeyDown(Keys.LeftShift)){
                altitudeSpeed = 1;
            }
            if (keyState.IsKeyDown(Keys.LeftControl)){
                altitudeSpeed = -1;
            }
            bool isTurning = false;
            if (keyState.IsKeyDown(Keys.A)){
                CurTurnState = TurnState.TurningLeft;
                isTurning = true;
            }
            if (keyState.IsKeyDown(Keys.D)){
                CurTurnState = TurnState.TurningRight;
                isTurning = true;
            }
            if (!isTurning){
                CurTurnState = TurnState.Stable;
            }

            float engineDutyCycle = (float) _engineSpeed/3;
            float altitudeDutyCycle = altitudeSpeed;

            int turnValue = 0;
            switch (CurTurnState){
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

            base.TurnVelocity = turnValue*base.MaxTurnRate;
            base.Velocity = engineDutyCycle*base.MaxVelocity;
            base.AscentRate = altitudeDutyCycle*base.MaxAscentRate;
        }
    }
}