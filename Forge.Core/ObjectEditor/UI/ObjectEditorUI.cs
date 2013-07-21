#region

using System;
using System.Diagnostics;
using Forge.Core.GameState;
using Forge.Framework.Draw;
using Forge.Framework.UI;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor.UI{
    /// <summary>
    ///   this class handles the display of the prototype airship and all of its components
    /// </summary>
    public class ObjectEditorUI : IDisposable{
        readonly EditorToolbar _editorToolbar;
        readonly HullEnvironment _hullEnv;
        readonly NavBar _navBar;
        readonly UIElementCollection _uiElementCollection;
        bool _disposed;

        public ObjectEditorUI(HullEnvironment hullEnv, DeckObjectEnvironment deckObjEnv, InternalWallEnvironment wallEnv, RenderTarget target){
            _hullEnv = hullEnv;

            _uiElementCollection = new UIElementCollection(GameStateManager.MouseManager);
            _uiElementCollection.Bind();

            _navBar = new NavBar(hullEnv, _uiElementCollection, FrameStrata.Level.Medium, new Point(50, 50));
            _editorToolbar = new EditorToolbar(hullEnv, deckObjEnv, wallEnv, _uiElementCollection, FrameStrata.Level.Medium, new Point(50, 150));
        }

        #region IDisposable Members

        public void Dispose(){
            Debug.Assert(!_disposed);
            _uiElementCollection.Dispose();
            _disposed = true;
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