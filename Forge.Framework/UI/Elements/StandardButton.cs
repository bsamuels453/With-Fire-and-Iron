#region

using Forge.Framework.Draw;
using MonoGameUtility;

#endregion

namespace Forge.Framework.UI.Elements{
    public class StandardButton : UIElementCollection{
        public StandardButton(
            UIElementCollection parent,
            FrameStrata.Level depth,
            Rectangle boundingBox,
            string buttonTex,
            bool enableClickMask,
            bool enableMouseoverMask
            ) : base(parent, depth, boundingBox, "StandardButton"){
            if (enableClickMask){
                var mask = new ClickMask(boundingBox, this);
                AddElement(mask);
            }
            if (enableMouseoverMask){
                var mask = new MouseoverMask(boundingBox, this);
                AddElement(mask);
            }
            var texture = new Sprite2D(buttonTex, boundingBox, FrameStrata);
            this.AddElement(texture);
        }
    }
}