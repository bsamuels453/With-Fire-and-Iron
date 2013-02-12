#region

using Gondola.Logic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;

#endregion

namespace Gondola.UI.Components{
    /// <summary>
    ///   prevents mouse interactions from falling through the owner's bounding box
    /// </summary>
    internal class PanelComponent : IUIComponent{
        Button _owner;

        public PanelComponent(){
            IsEnabled = true;
        }

        #region IUIComponent Members

        public void ComponentCtor(Button owner){
            _owner = owner;
        }

        public bool IsEnabled { get; set; }

        public void Update(InputState state, double timeDelta) {
            if (IsEnabled){
                if (_owner.ContainsMouse)
                    state.AllowLeftButtonInterpretation = false;
            }
        }

        public void Draw(){
            
        }

        public string Identifier { get; private set; }

        #endregion

        public static PanelComponent ConstructFromObject(JObject obj){
            return new PanelComponent();
        }
    }
}