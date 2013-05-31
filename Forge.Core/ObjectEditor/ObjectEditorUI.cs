#region

using System;
using System.Diagnostics;
using Forge.Core.ObjectEditor.Tools;
using Forge.Framework;
using Forge.Framework.Draw;
using Forge.Framework.UI;
using Forge.Framework.UI.Widgets;

#endregion

namespace Forge.Core.ObjectEditor{
    /// <summary>
    ///   this class handles the display of the prototype airship and all of its components
    /// </summary>
    internal class ObjectEditorUI : IInputUpdates, ILogicUpdates, IDisposable{
        readonly Button _deckDownButton;
        readonly Button _deckUpButton;
        readonly HullDataManager _hullData;

        readonly Toolbar _toolBar;
        bool _disposed;

        public ObjectEditorUI(HullDataManager hullData, RenderTarget target){
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

        #region IDisposable Members

        public void Dispose(){
            Debug.Assert(!_disposed);
            _toolBar.Dispose();
            _disposed = true;
        }

        #endregion

        #region IInputUpdates Members

        public void UpdateInput(ref InputState state){
            _toolBar.UpdateInput(ref state);
        }

        #endregion

        #region ILogicUpdates Members

        public void UpdateLogic(double timeDelta){
            _toolBar.UpdateLogic(timeDelta);
        }

        #endregion

        void AddVisibleLevel(int identifier){
            _hullData.MoveUpOneDeck();
        }

        void RemoveVisibleLevel(int identifier){
            _hullData.MoveDownOneDeck();
        }

        ~ObjectEditorUI(){
            Debug.Assert(_disposed);
        }
    }
}