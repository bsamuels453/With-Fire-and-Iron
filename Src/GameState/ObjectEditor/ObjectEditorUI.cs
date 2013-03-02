using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gondola.Draw;
using Gondola.GameState.HullEditor.Tools;
using Gondola.Logic;
using Gondola.UI;
using Gondola.UI.Widgets;

namespace Gondola.GameState.ObjectEditor {
    /// <summary>
    ///   this class handles the display of the prototype airship and all of its components
    /// </summary>
    internal class ObjectEditorUI : IInputUpdates, ILogicUpdates {
        readonly Button _deckDownButton;
        readonly Button _deckUpButton;
        readonly HullDataManager _hullData;

        readonly Toolbar _toolBar;

        public ObjectEditorUI(HullDataManager hullData, RenderTarget target) {
            _hullData = hullData;

            var buttonGen = new ButtonGenerator("ToolbarButton64.json");
            buttonGen.X = 50;
            buttonGen.Y = 50;
            buttonGen.TextureName = "Icons/UpArrow";
            _deckUpButton = buttonGen.GenerateButton();
            buttonGen.Y = 50 + 64;
            buttonGen.TextureName = "Icons/DownArrow";
            _deckDownButton = buttonGen.GenerateButton();
            _deckUpButton.OnLeftClickDispatcher += AddVisibleLevel;
            _deckDownButton.OnLeftClickDispatcher += RemoveVisibleLevel;


            _toolBar = new Toolbar(target, "Templates/DoodadToolbar.json");

            _toolBar.BindButtonToTool(0, new WallMenuTool(hullData, target));

            _toolBar.BindButtonToTool(1, new LadderBuildTool(hullData));
        }

        #region IInputUpdates Members

        public void UpdateInput(ref InputState state) {
            _toolBar.UpdateInput(ref state);
        }

        #endregion

        #region ILogicUpdates Members

        public void UpdateLogic(double timeDelta) {
            _toolBar.UpdateLogic(timeDelta);
        }

        #endregion

        void AddVisibleLevel(int identifier) {
            _hullData.MoveUpOneDeck();
        }

        void RemoveVisibleLevel(int identifier) {
            _hullData.MoveDownOneDeck();
        }
    }
}
