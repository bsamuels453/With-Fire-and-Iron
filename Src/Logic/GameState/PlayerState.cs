#region

using System;
using Gondola.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Gondola.Logic.GameState{
    internal class PlayerState : IGameState{
        readonly GamestateManager _manager;
        Angle3 _playerLookDir;
        Vector3 _playerPosition;

        public PlayerState(GamestateManager mgr){
            _manager = mgr;
            _playerPosition = new Vector3(0, 0, 0);
            _playerLookDir = new Angle3(0, 0, 0);
            _manager.AddSharedData(SharedStateData.PlayerPosition, _playerPosition);
            _manager.AddSharedData(SharedStateData.PlayerLook, _playerLookDir);
        }

        #region IGameState Members

        public void Dispose(){
            _manager.DeleteSharedData(SharedStateData.PlayerPosition);
            _manager.DeleteSharedData(SharedStateData.PlayerLook);
        }

        public void Update(InputState state, double timeDelta){
            var keyState = state.KeyboardState;

            float movementspeed = 5.5f;
            if (keyState.IsKeyDown(Keys.LeftShift)){
                movementspeed = 50.0f;
            }
            if (keyState.IsKeyDown(Keys.LeftControl)){
                movementspeed = 0.25f;
            }
            if (keyState.IsKeyDown(Keys.W)){
                _playerPosition.X = _playerPosition.X + (float) Math.Sin(_playerLookDir.Yaw)*(float) Math.Cos(_playerLookDir.Pitch)*movementspeed;
                _playerPosition.Y = _playerPosition.Y + (float) Math.Sin(_playerLookDir.Pitch)*movementspeed;
                _playerPosition.Z = _playerPosition.Z + (float) Math.Cos(_playerLookDir.Yaw)*(float) Math.Cos(_playerLookDir.Pitch)*movementspeed;
            }
            if (keyState.IsKeyDown(Keys.S)){
                _playerPosition.X = _playerPosition.X - (float) Math.Sin(_playerLookDir.Yaw)*(float) Math.Cos(_playerLookDir.Pitch)*movementspeed;
                _playerPosition.Y = _playerPosition.Y - (float) Math.Sin(_playerLookDir.Pitch)*movementspeed;
                _playerPosition.Z = _playerPosition.Z - (float) Math.Cos(_playerLookDir.Yaw)*(float) Math.Cos(_playerLookDir.Pitch)*movementspeed;
            }
            if (keyState.IsKeyDown(Keys.A)){
                _playerPosition.X = _playerPosition.X + (float) Math.Sin(_playerLookDir.Yaw + 3.14159f/2)*movementspeed;
                _playerPosition.Z = _playerPosition.Z + (float) Math.Cos(_playerLookDir.Yaw + 3.14159f/2)*movementspeed;
            }

            if (keyState.IsKeyDown(Keys.D)){
                _playerPosition.X = _playerPosition.X - (float) Math.Sin(_playerLookDir.Yaw + 3.14159f/2)*movementspeed;
                _playerPosition.Z = _playerPosition.Z - (float) Math.Cos(_playerLookDir.Yaw + 3.14159f/2)*movementspeed;
            }

            //xx block wasd from further interpretation?
        }

        public void Draw(){
        }

        #endregion
    }
}