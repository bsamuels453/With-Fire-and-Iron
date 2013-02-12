#region

using System;
using Gondola.Common;
using Gondola.Draw;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Gondola.Logic.GameState{
    internal class PlayerState : IGameState{
        readonly GamestateManager _manager;
        readonly RenderTarget _renderTarget;
        Angle3 _playerLookDir;
        Vector3 _playerPosition;
        Text2D _x;
        Text2D _y;
        Text2D _z;

        public PlayerState(GamestateManager mgr){
            _renderTarget = new RenderTarget(0f);
            _manager = mgr;
            _playerPosition = new Vector3(0, 0, 0);
            _playerLookDir = new Angle3(0, 0, 0);
            _manager.AddSharedData(SharedStateData.PlayerPosition, _playerPosition);
            _manager.AddSharedData(SharedStateData.PlayerLook, _playerLookDir);

            _x = new Text2D(_renderTarget, 0, 0, "hi");
            _y = new Text2D(_renderTarget, 0, 10, "hi");
            _z = new Text2D(_renderTarget, 0, 20, "hi");
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

            _x.Str = "x: "+_playerPosition.X;
            _y.Str = "y: " + _playerPosition.Y;
            _z.Str = "z: " + _playerPosition.Z;
            //xx block wasd from further interpretation?
        }

        public void Draw(){
            _renderTarget.Bind();
            _x.Draw();
            _y.Draw();
            _z.Draw();
            _renderTarget.Unbind();
        }

        #endregion
    }
}