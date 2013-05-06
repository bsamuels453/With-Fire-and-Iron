using System.Collections.Generic;
using Forge.Core.Airship;
using Forge.Core.GameState;
using Forge.Framework.Draw;
using Forge.Core.Logic;
using Forge.Framework.UI;
using Forge.Core.Util;
using Forge.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Forge.Core.ObjectEditor {
    internal class ObjectEditorState : IGameState {
        readonly BodyCenteredCamera _cameraController;
        readonly ObjectEditorUI _doodadUI;
        readonly HullDataManager _hullData;
        readonly RenderTarget _renderTarget;

        readonly BezierInfo[] _backCurveInfo;
        readonly BezierInfo[] _sideCurveInfo;
        readonly BezierInfo[] _topCurveInfo;

        readonly UIElementCollection _uiElementCollection;

        public ObjectEditorState(List<BezierInfo> backCurveInfo, List<BezierInfo> sideCurveInfo, List<BezierInfo> topCurveInfo) {
            _renderTarget = new RenderTarget(0, 0, Gbl.ScreenSize.X, Gbl.ScreenSize.Y);
            _renderTarget.Bind();
            _uiElementCollection = new UIElementCollection();
            _uiElementCollection.Bind();
            _cameraController = new BodyCenteredCamera();
            GamestateManager.CameraController = _cameraController;

            _sideCurveInfo = (BezierInfo[])sideCurveInfo.ToArray().Clone();
            _topCurveInfo = (BezierInfo[])topCurveInfo.ToArray().Clone();
            _backCurveInfo = (BezierInfo[])backCurveInfo.ToArray().Clone();
            var geometryInfo = HullGeometryGenerator.GenerateShip(backCurveInfo, sideCurveInfo, topCurveInfo);
            _hullData = new HullDataManager(geometryInfo);
            _doodadUI = new ObjectEditorUI(_hullData, _renderTarget);
            
            _uiElementCollection.Unbind();

            _cameraController.SetCameraTarget(_hullData.CenterPoint);
            _renderTarget.Unbind();
        }

        public void Dispose() {
            throw new System.NotImplementedException();
        }

        public void Update(InputState state, double timeDelta) {
            _renderTarget.Bind();
            _uiElementCollection.Bind();

            #region update input

            _uiElementCollection.UpdateInput(ref state);
            _doodadUI.UpdateInput(ref state);
            _cameraController.Update(ref state, timeDelta);


            if(state.KeyboardState.IsKeyDown(Keys.LeftControl) && state.KeyboardState.IsKeyDown(Keys.S)){
                AirshipPackager.Export("Export.airship", _backCurveInfo, _sideCurveInfo, _topCurveInfo);
            }

            #endregion

            #region update logic

            UIElementCollection.BoundCollection.UpdateLogic(timeDelta);
            _doodadUI.UpdateLogic(timeDelta);

            #endregion

            _uiElementCollection.Unbind();
            _renderTarget.Unbind();
        }

        public void Draw() {
            _renderTarget.Draw(_cameraController.ViewMatrix, Color.CornflowerBlue);
        }
    }
}
