#region

using System;
using Forge.Framework;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework.Input;
using MonoGameUtility;

#endregion

namespace Forge.Core.Camera{
    /// <summary>
    ///   this abstract class creates a camera that rotates around a point
    /// </summary>
    internal sealed class BodyCenteredCamera : ICamera{
        public Vector3 CameraPosition;
        public Vector3 CameraTarget;
        Rectangle _boundingBox;
        float _cameraDistance;
        float _cameraPhi;
        float _cameraTheta;
        const float _camAngularSpeed = 0.005f;//0.01f
        const float _camScrollSpeedDivisor = 100f;
        /// <summary>
        ///   default constructor makes it recieve from entire screen
        /// </summary>
        /// <param name="boundingBox"> </param>
        public BodyCenteredCamera(Rectangle? boundingBox = null){
            _cameraPhi = 1.19f;
            _cameraTheta = -3.65f;
            _cameraDistance = 29.4f;
            if (boundingBox != null){
                _boundingBox = (Rectangle) boundingBox;
            }
            else{
                _boundingBox = new Rectangle(0, 0, Resource.ScreenSize.X, Resource.ScreenSize.Y);
            }
        }

        #region ICamera Members

        public void Update(ref InputState state, double timeDelta){
            if (_boundingBox.Contains(state.MousePos.X, state.MousePos.Y)){
                if (state.RightButtonState == ButtonState.Pressed){
                    if (!state.KeyboardState.IsKeyDown(Keys.LeftControl)){
                        int dx = state.MousePos.X - state.PrevState.MousePos.X;
                        int dy = state.MousePos.Y - state.PrevState.MousePos.Y;
                        
                        if (state.RightButtonState == ButtonState.Pressed){
                            _cameraPhi -= dy * _camAngularSpeed;
                            _cameraTheta += dx * _camAngularSpeed;

                            if (_cameraPhi > (float) Math.PI - 0.01f){
                                _cameraPhi = (float) Math.PI - 0.01f;
                            }
                            if (_cameraPhi < 0.01f){
                                _cameraPhi = 0.01f;
                            }

                            CameraPosition.X = (float) (_cameraDistance*Math.Sin(_cameraPhi)*Math.Cos(_cameraTheta)) + CameraTarget.X;
                            CameraPosition.Z = (float) (_cameraDistance*Math.Sin(_cameraPhi)*Math.Sin(_cameraTheta)) + CameraTarget.Z;
                            CameraPosition.Y = (float) (_cameraDistance*Math.Cos(_cameraPhi)) + CameraTarget.Y;
                        }

                        state.AllowMouseMovementInterpretation = false;
                    }
                    else{
                        int dx = state.MousePos.X - state.PrevState.MousePos.X;
                        int dy = state.MousePos.Y - state.PrevState.MousePos.Y;

                        _cameraPhi -= dy*0.005f;
                        _cameraTheta += dx*0.005f;

                        if (_cameraPhi > (float) Math.PI - 0.01f){
                            _cameraPhi = (float) Math.PI - 0.01f;
                        }
                        if (_cameraPhi < 0.01f){
                            _cameraPhi = 0.01f;
                        }

                        CameraTarget.X = ((float) (_cameraDistance*Math.Sin(_cameraPhi + Math.PI)*Math.Cos(_cameraTheta + Math.PI)) - CameraPosition.X)*-1;
                        CameraTarget.Z = ((float) (_cameraDistance*Math.Sin(_cameraPhi + Math.PI)*Math.Sin(_cameraTheta + Math.PI)) - CameraPosition.Z)*-1;
                        CameraTarget.Y = ((float) (_cameraDistance*Math.Cos(_cameraPhi + Math.PI)) + CameraPosition.Y)*1;
                    }
                }
            }


            if (state.AllowMouseScrollInterpretation){
                if (_boundingBox.Contains(state.MousePos.X, state.MousePos.Y)){
                    _cameraDistance += -state.MouseScrollChange / _camScrollSpeedDivisor;
                    if (_cameraDistance < 5){
                        _cameraDistance = 5;
                    }

                    CameraPosition.X = (float) (_cameraDistance*Math.Sin(_cameraPhi)*Math.Cos(_cameraTheta)) + CameraTarget.X;
                    CameraPosition.Z = (float) (_cameraDistance*Math.Sin(_cameraPhi)*Math.Sin(_cameraTheta)) + CameraTarget.Z;
                    CameraPosition.Y = (float) (_cameraDistance*Math.Cos(_cameraPhi)) + CameraTarget.Y;
                }
            }
        }

        public Matrix ViewMatrix{
            get { return Matrix.CreateLookAt(CameraPosition, CameraTarget, Vector3.Up); }
        }

        #endregion

        public void SetCameraTarget(Vector3 target){
            CameraTarget = target;
            //CameraTarget = new Vector3(-10.65f, -1.68f, -0.488f);

            CameraPosition.X = (float) (_cameraDistance*Math.Sin(_cameraPhi)*Math.Cos(_cameraTheta)) + CameraTarget.X;
            CameraPosition.Z = (float) (_cameraDistance*Math.Sin(_cameraPhi)*Math.Sin(_cameraTheta)) + CameraTarget.Z;
            CameraPosition.Y = (float) (_cameraDistance*Math.Cos(_cameraPhi)) + CameraTarget.Y;
        }
    }
}