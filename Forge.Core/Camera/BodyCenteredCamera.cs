#region

using System;
using Forge.Core.GameState;
using Forge.Framework;
using Forge.Framework.Control;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework.Input;
using MonoGameUtility;

#endregion

namespace Forge.Core.Camera{
    /// <summary>
    ///   this abstract class creates a camera that rotates around a point
    /// </summary>
    public sealed class BodyCenteredCamera : ICamera{
        const float _camAngularSpeed = 0.005f; //0.01f
        const float _camScrollSpeedDivisor = 100f;
        readonly bool _allowCamTargChange;
        readonly MouseController _mouseController;
        public Vector3 CameraPosition;
        public Vector3 CameraTarget;
        Rectangle _boundingBox;
        float _cameraDistance;
        float _cameraPhi;
        float _cameraTheta;

        /// <summary>
        ///   default constructor makes it recieve from entire screen
        /// </summary>
        /// <param name="allowCamTargChange"> whether or not the right click drag should be allowed to change the camera's target </param>
        /// <param name="boundingBox"> </param>
        public BodyCenteredCamera(bool allowCamTargChange = true, Rectangle? boundingBox = null){
            _allowCamTargChange = allowCamTargChange;
            _cameraPhi = 1.19f;
            _cameraTheta = -3.65f;
            _cameraDistance = 29.4f;
            if (boundingBox != null){
                _boundingBox = (Rectangle) boundingBox;
            }
            else{
                _boundingBox = new Rectangle(0, 0, Resource.ScreenSize.X, Resource.ScreenSize.Y);
            }

            _mouseController = new MouseController(this);
            _mouseController.OnMouseMovement += OnMouseMovement;
            _mouseController.OnMouseScroll += OnMouseMovement;

            GamestateManager.MouseManager.AddGlobalController(_mouseController, 100);
        }

        #region ICamera Members

        public void Update(ref InputState state, double timeDelta){
        }

        public Matrix ViewMatrix{
            get { return Matrix.CreateLookAt(CameraPosition, CameraTarget, Vector3.Up); }
        }

        #endregion

        void OnMouseMovement(ForgeMouseState state, float timeDelta){
            if (state.BlockMPosition){
                return;
            }
            var mousePos = state.MousePos;

            if (_boundingBox.Contains(state.MousePos.X, state.MousePos.Y)){
                if (state.RightButtonState == ButtonState.Pressed){
                    if ( /*!state.KeyboardState.IsKeyDown(Keys.LeftControl) || */!_allowCamTargChange){
                        int dx = mousePos.X - state.PrevState.MousePos.X;
                        int dy = mousePos.Y - state.PrevState.MousePos.Y;

                        if (state.RightButtonState == ButtonState.Pressed){
                            _cameraPhi -= dy*_camAngularSpeed;
                            _cameraTheta += dx*_camAngularSpeed;

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
                        state.BlockMPosition = true;
                    }
                    else{
                        if (_allowCamTargChange){
                            int dx = mousePos.X - state.PrevState.MousePos.X;
                            int dy = mousePos.Y - state.PrevState.MousePos.Y;

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
            }


            if (!state.BlockScrollWheel){
                if (_boundingBox.Contains(mousePos.X, mousePos.Y)){
                    if (state.MouseScrollChange != 0){
                        _cameraDistance += -state.MouseScrollChange/_camScrollSpeedDivisor;
                        if (_cameraDistance < 5){
                            _cameraDistance = 5;
                        }

                        CameraPosition.X = (float) (_cameraDistance*Math.Sin(_cameraPhi)*Math.Cos(_cameraTheta)) + CameraTarget.X;
                        CameraPosition.Z = (float) (_cameraDistance*Math.Sin(_cameraPhi)*Math.Sin(_cameraTheta)) + CameraTarget.Z;
                        CameraPosition.Y = (float) (_cameraDistance*Math.Cos(_cameraPhi)) + CameraTarget.Y;
                    }
                }
            }
        }

        public void SetCameraTarget(Vector3 target){
            CameraTarget = target;
            //CameraTarget = new Vector3(-10.65f, -1.68f, -0.488f);

            CameraPosition.X = (float) (_cameraDistance*Math.Sin(_cameraPhi)*Math.Cos(_cameraTheta)) + CameraTarget.X;
            CameraPosition.Z = (float) (_cameraDistance*Math.Sin(_cameraPhi)*Math.Sin(_cameraTheta)) + CameraTarget.Z;
            CameraPosition.Y = (float) (_cameraDistance*Math.Cos(_cameraPhi)) + CameraTarget.Y;
        }
    }
}