#region

using System;
using Gondola.Draw;
using Gondola.Logic;
using Gondola.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Gondola.GameState{
    internal class PlayerState : IGameState{
        Angle3 _playerLookDir;
        Vector3 _playerPosition;
        readonly Text2D _x;
        readonly Text2D _y;
        readonly Text2D _z;
        readonly Text2D _pitch;
        readonly Text2D _yaw;
        readonly Point _viewportSize;
        bool _skipNextMouseUpdate;

        public PlayerState(Point viewportSize) {
            _viewportSize = viewportSize;
            _playerPosition = new Vector3(-31, 1043, -50);
            _playerLookDir = new Angle3(-0.49f, 0, -11.7f);//xxxxx this value gets ~2 added to it somehow
            GamestateManager.AddSharedData(SharedStateData.PlayerPosition, _playerPosition);
            GamestateManager.AddSharedData(SharedStateData.PlayerLook, _playerLookDir);
            _skipNextMouseUpdate = true;

            _x = new Text2D(0, 0, "hi");
            _y = new Text2D(0, 10, "hi");
            _z = new Text2D(0, 20, "hi");
            _pitch = new Text2D(0, 30, "hi");
            _yaw = new Text2D(0, 40, "hi");
            _x.Color = Color.Wheat;
            _y.Color = Color.Wheat;
            _z.Color = Color.Wheat;
            _pitch.Color = Color.Wheat;
            _yaw.Color = Color.Wheat;
        }

        #region IGameState Members

        public void Dispose(){
            GamestateManager.DeleteSharedData(SharedStateData.PlayerPosition);
            GamestateManager.DeleteSharedData(SharedStateData.PlayerLook);
        }

        public void Update(InputState state, double timeDelta) {
            #region update player position
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
            #endregion

            #region update player viewpos
            var pos = state.MousePos;
            var prevPos = state.PrevState.MousePos;
            int dx = pos.X - prevPos.X;
            int dy = pos.Y - prevPos.Y;
            if (dx != 0 || dy != 0){
                const double tolerance = 0.2f;

                //now apply viewport changes
                if (!_skipNextMouseUpdate) {
                    _playerLookDir.Yaw -= dx * 0.005f;
                    if (
                        (_playerLookDir.Pitch - dy * 0.005f) < 1.55 &&
                        (_playerLookDir.Pitch - dy * 0.005f) > -1.55) {
                        _playerLookDir.Pitch -= dy * 0.005f;
                    }
                }
                else{
                    _skipNextMouseUpdate = false;
                }

                //check to see if mouse is outside of permitted area
                if (
                    pos.X > _viewportSize.X*(1 - tolerance) ||
                    pos.X < _viewportSize.X*tolerance ||
                    pos.Y > _viewportSize.Y*(1 - tolerance) ||
                    pos.Y < _viewportSize.X*tolerance
                    ){
                    //move mouse to center of screen
                    Mouse.SetPosition(_viewportSize.X/2, _viewportSize.Y/2);
                    pos.X = _viewportSize.X/2;
                    pos.Y = _viewportSize.Y/2;
                    _skipNextMouseUpdate = true;
                }

                _pitch.Str = "pitch: " + _playerLookDir.Pitch;
                _yaw.Str = "yaw: " + _playerLookDir.Yaw;
            }

            #endregion

            GamestateManager.ModifySharedData(SharedStateData.PlayerPosition, _playerPosition);
            GamestateManager.ModifySharedData(SharedStateData.PlayerLook, _playerLookDir);
        }

        public void Draw(){
        }

        #endregion
    }
}