#region

using Forge.Framework.Draw;
using Forge.Framework.Resources;
using MonoGameUtility;

#endregion

namespace Forge.Framework.UI.Elements{
    public class DialogueButton : UIElementCollection{
        public DialogueButton(
            UIElementCollection parent,
            FrameStrata.Level depth,
            Rectangle boundingBox,
            string buttonTex
            ) : base(parent, depth, boundingBox, "DialogueButton"){

                var jobj = Resource.LoadJObject("UiTemplates/DialogueButton.json");

                var clickMask = new ClickMask(boundingBox, this);
                AddElement(clickMask);

                var mouseoverMask = new MouseoverMask(boundingBox, this);
                AddElement(mouseoverMask);

            var texture = new Sprite2D(buttonTex, boundingBox, FrameStrata);
            this.AddElement(texture);
        }
    }
}