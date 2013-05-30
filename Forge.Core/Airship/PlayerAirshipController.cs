using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forge.Core.Airship.Data;
using Forge.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Forge.Core.Airship {
    class PlayerAirshipController : AirshipController{
        public PlayerAirshipController(Action<Matrix> setWorldMatrix, ModelAttributes modelData, AirshipMovementData movementData) : 
            base(setWorldMatrix, modelData, movementData){
        }

        int _engineSpeed;
        public TurnState CurTurnState;
        public int EngineSpeed {
            get { return _engineSpeed; }
            set {
                if (value <= 3 && value >= -2) {
                    _engineSpeed = value;
                }
            }
        }

        public enum TurnState {
            TurningLeft,
            TurningRight,
            Stable
        }

        protected override void UpdateController(ref InputState state, double timeDelta){

            var keyState = state.KeyboardState;
            var prevKeyState = state.PrevState.KeyboardState;

            if (keyState.IsKeyUp(Keys.W) && prevKeyState.IsKeyDown(Keys.W)) {
                EngineSpeed++;
            }
            if (keyState.IsKeyUp(Keys.S) && prevKeyState.IsKeyDown(Keys.S)) {
                if (EngineSpeed > 0) {
                    EngineSpeed = 0;
                }
                else {
                    EngineSpeed--;
                }
            }

            int altitudeSpeed = 0;
            if (keyState.IsKeyDown(Keys.LeftShift)) {
                altitudeSpeed = 1;
            }
            if (keyState.IsKeyDown(Keys.LeftControl)) {
                altitudeSpeed = -1;
            }
            bool isTurning = false;
            if (keyState.IsKeyDown(Keys.A)) {
                CurTurnState = TurnState.TurningLeft;
                isTurning = true;
            }
            if (keyState.IsKeyDown(Keys.D)) {
                CurTurnState = TurnState.TurningRight;
                isTurning = true;
            }
            if (!isTurning) {
                CurTurnState = TurnState.Stable;
            }

            float engineDutyCycle = (float)_engineSpeed / 3;
            float altitudeDutyCycle = altitudeSpeed;

            int turnValue = 0;
            switch (CurTurnState) {
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

            base.AngularVelocity = turnValue * base.AirshipModelData.MaxTurnSpeed;
            base.Velocity = engineDutyCycle * base.AirshipModelData.MaxForwardSpeed;
            base.AscentVelocity = altitudeDutyCycle * base.AirshipModelData.MaxAscentSpeed;

        }
    }
}
