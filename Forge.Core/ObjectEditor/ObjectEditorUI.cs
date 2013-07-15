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
        readonly HullDataManager _hullData;
        readonly NavBar _navBar;
        readonly UIElementCollection _uiElementCollection;
        bool _disposed;

        public ObjectEditorUI(HullDataManager hullData, RenderTarget target){
            _hullData = hullData;

            _uiElementCollection = new UIElementCollection(GamestateManager.MouseManager);
            _uiElementCollection.Bind();

            _navBar = new NavBar(_uiElementCollection, FrameStrata.Level.Medium, new Point(50, 50));

            _navBar.OnUpPressed =
                (state, f, arg3) =>{
                    if (arg3.ContainsMouse){
                        _hullData.MoveUpOneDeck();
                    }
                };
            _navBar.OnDownPressed =
                (state, f, arg3) =>{
                    if (arg3.ContainsMouse){
                        _hullData.MoveDownOneDeck();
                    }
                };
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