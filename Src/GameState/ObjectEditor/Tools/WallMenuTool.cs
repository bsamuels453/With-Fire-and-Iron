using Gondola.Draw;
using Gondola.Logic;
using Gondola.UI.Widgets;

namespace Gondola.GameState.ObjectEditor.Tools {
    internal class WallMenuTool : IToolbarTool {
        readonly Toolbar _toolbar;
        //todo: break this and put it in doodadui
        public WallMenuTool(HullDataManager hullData, RenderTarget target) {
            _toolbar = new Toolbar(target, "Templates/BuildToolbar.json");
            _toolbar.Enabled = false;

            _toolbar.BindButtonToTool(
                0,
                new WallBuildTool(hullData)
                );

            _toolbar.BindButtonToTool(
                1,
                new WallDeleteTool(hullData)
                );
        }

        #region IToolbarTool Members

        public void UpdateInput(ref InputState state) {
            _toolbar.UpdateInput(ref state);
        }

        public void UpdateLogic(double timeDelta) {
            _toolbar.UpdateLogic(timeDelta);
        }

        public bool Enabled {
            get { return _toolbar.Enabled; }
            set { _toolbar.Enabled = value; }
        }
        #endregion
    }
}
