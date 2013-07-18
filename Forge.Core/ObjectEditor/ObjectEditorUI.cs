#region

using System;
using System.Diagnostics;
using Forge.Core.GameState;
using Forge.Framework.Draw;
using Forge.Framework.UI;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor{
    /// <summary>
    ///   this class handles the display of the prototype airship and all of its components
    /// </summary>
    public class ObjectEditorUI : IDisposable{
        readonly EditorToolbar _editorToolbar;
        readonly HullDataManager _hullData;
        readonly NavBar _navBar;
        readonly UIElementCollection _uiElementCollection;
        bool _disposed;

        public ObjectEditorUI(HullDataManager hullData, RenderTarget target){
            _hullData = hullData;

            _uiElementCollection = new UIElementCollection(GameStateManager.MouseManager);
            _uiElementCollection.Bind();

            _navBar = new NavBar(hullData, _uiElementCollection, FrameStrata.Level.Medium, new Point(50, 50));
            _editorToolbar = new EditorToolbar(hullData, _uiElementCollection, FrameStrata.Level.Medium, new Point(50, 150));
        }

        #region IDisposable Members

        public void Dispose(){
            Debug.Assert(!_disposed);
            _uiElementCollection.Dispose();
            _disposed = true;
            _editorToolbar.DisposeTools();
        }

        #endregion

        public void UpdateLogic(double timeDelta){
            _uiElementCollection.Update((float) timeDelta);
        }

        ~ObjectEditorUI(){
            Debug.Assert(_disposed);
        }
    }
}