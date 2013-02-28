using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Gondola.Logic {
    /// <summary>
    ///   this abstract class creates a camera that rotates around a point
    /// </summary>
    internal class BodyCenteredCamera : IInputUpdates {
        Rectangle _boundingBox;
        float _cameraDistance;
        float _cameraPhi;
        float _cameraTheta;
        public Vector3 CameraPosition;
        public Vector3 CameraTarget;

        /// <summary>
        ///   default constructor makes it recieve from entire screen
        /// </summary>
        /// <param name="boundingBox"> </param>
        public BodyCenteredCamera(Rectangle? boundingBox = null) {
            _cameraPhi = 1.2f;
            _cameraTheta = 1.93f;
            _cameraDistance = 60;
            if (boundingBox != null) {
                _boundingBox = (Rectangle)boundingBox;
            }
            else {
                _boundingBox = new Rectangle(0, 0, Gbl.ScreenSize.X, Gbl.ScreenSize.Y);
            }
            GamestateManager.AddSharedData(SharedStateData.CameraTarget, CameraTarget);
            GamestateManager.AddSharedData(SharedStateData.PlayerPosition, CameraPosition);
        }

        public void SetCameraTarget(Vector3 target) {
            CameraTarget = target;

            CameraPosition.X = (float)(_cameraDistance * Math.Sin(_cameraPhi) * Math.Cos(_cameraTheta)) + CameraTarget.X;
            CameraPosition.Z = (float)(_cameraDistance * Math.Sin(_cameraPhi) * Math.Sin(_cameraTheta)) + CameraTarget.Z;
            CameraPosition.Y = (float)(_cameraDistance * Math.Cos(_cameraPhi)) + CameraTarget.Y;

            GamestateManager.ModifySharedData(SharedStateData.PlayerPosition, CameraPosition);
        }

        public void UpdateInput(ref InputState state) {
            if (_boundingBox.Contains(state.MousePos.X, state.MousePos.Y)) {
                if (state.RightButtonState == ButtonState.Pressed) {
                    if (!state.KeyboardState.IsKeyDown(Keys.LeftControl)) {
                        int dx = state.MousePos.X - state.PrevState.MousePos.X;
                        int dy = state.MousePos.Y - state.PrevState.MousePos.Y;

                        if (state.RightButtonState == ButtonState.Pressed) {
                            _cameraPhi -= dy * 0.01f;
                            _cameraTheta += dx * 0.01f;

                            if (_cameraPhi > (float)Math.PI - 0.01f) {
                                _cameraPhi = (float)Math.PI - 0.01f;
                            }
                            if (_cameraPhi < 0.01f) {
                                _cameraPhi = 0.01f;
                            }

                            CameraPosition.X = (float)(_cameraDistance * Math.Sin(_cameraPhi) * Math.Cos(_cameraTheta)) + CameraTarget.X;
                            CameraPosition.Z = (float)(_cameraDistance * Math.Sin(_cameraPhi) * Math.Sin(_cameraTheta)) + CameraTarget.Z;
                            CameraPosition.Y = (float)(_cameraDistance * Math.Cos(_cameraPhi)) + CameraTarget.Y;
                        }

                        state.AllowMouseMovementInterpretation = false;
                    }
                    else {
                        int dx = state.MousePos.X - state.PrevState.MousePos.X;
                        int dy = state.MousePos.Y - state.PrevState.MousePos.Y;

                        _cameraPhi -= dy * 0.005f;
                        _cameraTheta += dx * 0.005f;

                        if (_cameraPhi > (float)Math.PI - 0.01f) {
                            _cameraPhi = (float)Math.PI - 0.01f;
                        }
                        if (_cameraPhi < 0.01f) {
                            _cameraPhi = 0.01f;
                        }

                        CameraTarget.X = ((float)(_cameraDistance * Math.Sin(_cameraPhi + Math.PI) * Math.Cos(_cameraTheta + Math.PI)) - CameraPosition.X) * -1;
                        CameraTarget.Z = ((float)(_cameraDistance * Math.Sin(_cameraPhi + Math.PI) * Math.Sin(_cameraTheta + Math.PI)) - CameraPosition.Z) * -1;
                        CameraTarget.Y = ((float)(_cameraDistance * Math.Cos(_cameraPhi + Math.PI)) + CameraPosition.Y) * 1;

                        int f = 4;
                    }
                }
                GamestateManager.ModifySharedData(SharedStateData.PlayerPosition, CameraPosition);
                GamestateManager.ModifySharedData(SharedStateData.CameraTarget, CameraTarget);
            }


            if (state.AllowMouseScrollInterpretation) {
                if (_boundingBox.Contains(state.MousePos.X, state.MousePos.Y)) {
                    _cameraDistance += -state.MouseScrollChange / 20f;
                    if (_cameraDistance < 5) {
                        _cameraDistance = 5;
                    }

                    CameraPosition.X = (float)(_cameraDistance * Math.Sin(_cameraPhi) * Math.Cos(_cameraTheta)) + CameraTarget.X;
                    CameraPosition.Z = (float)(_cameraDistance * Math.Sin(_cameraPhi) * Math.Sin(_cameraTheta)) + CameraTarget.Z;
                    CameraPosition.Y = (float)(_cameraDistance * Math.Cos(_cameraPhi)) + CameraTarget.Y;
                }
            }
        }
    }
}
