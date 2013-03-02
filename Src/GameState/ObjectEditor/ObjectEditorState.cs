using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gondola.Draw;
using Gondola.Logic;
using Gondola.UI;
using Gondola.Util;
using Microsoft.Xna.Framework;

namespace Gondola.GameState.ObjectEditor {
    internal class ObjectEditorState : IGameState {
        const int _primsPerDeck = 3;

        readonly BodyCenteredCamera _cameraController;
        readonly ObjectEditorUI _doodadUI;
        readonly HullDataManager _hullData;
        readonly RenderTarget _renderTarget;

        readonly UIElementCollection _uiElementCollection;

        public ObjectEditorState(List<BezierInfo> backCurveInfo, List<BezierInfo> sideCurveInfo, List<BezierInfo> topCurveInfo) {
            _renderTarget = new RenderTarget(0, 0, Gbl.ScreenSize.X, Gbl.ScreenSize.Y);
            _renderTarget.Bind();
            _uiElementCollection = new UIElementCollection();
            _cameraController = new BodyCenteredCamera();
            GamestateManager.CameraController = _cameraController;

            UIElementCollection.BindCollection(_uiElementCollection);
            var geometryInfo = HullGeometryGenerator.GenerateShip(backCurveInfo, sideCurveInfo, topCurveInfo, _primsPerDeck);
            _hullData = new HullDataManager(geometryInfo);
            _doodadUI = new ObjectEditorUI(_hullData, _renderTarget);
            UIElementCollection.UnbindCollection();

            _cameraController.SetCameraTarget(_hullData.CenterPoint);
            _renderTarget.Unbind();
        }

        public void Dispose() {
            throw new System.NotImplementedException();
        }

        public void Update(InputState state, double timeDelta) {
            _renderTarget.Bind();
            UIElementCollection.BindCollection(_uiElementCollection);

            #region update input

            UIElementCollection.Collection.UpdateInput(ref state);
            _doodadUI.UpdateInput(ref state);
            _cameraController.Update(ref state, timeDelta);

            #endregion

            #region update logic

            UIElementCollection.Collection.UpdateLogic(timeDelta);
            _doodadUI.UpdateLogic(timeDelta);

            #endregion

            UIElementCollection.UnbindCollection();
            _renderTarget.Unbind();
        }

        public void Draw() {
            _renderTarget.Draw(_cameraController.ViewMatrix, Color.CornflowerBlue);
        }
    }
}
