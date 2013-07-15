#region

using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Forge.Framework.UI;
using Forge.Framework.UI.Elements;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor{
    internal class ToolbarButton : UIElementCollection{
        public ToolbarButton(UIElementCollection parent, FrameStrata.Level depth, Point position, string texture)
            : base(parent, depth, new Rectangle(), "ToolbarButton"){
            var obj = Resource.LoadJObject("UiTemplates/Specialized/ToolbarButton.json");
            int width = obj["Width"].ToObject<int>();
            int height = obj["Height"].ToObject<int>();

            this.BoundingBox = new Rectangle(position.X, position.Y, width, height);

            var mouseMask = new MouseoverMask(this.BoundingBox, this);
            var clickMask = new ClickMask(this.BoundingBox, this);
            var sprite = new Sprite2D(texture, this.BoundingBox, this.FrameStrata, FrameStrata.Level.Medium);

            base.AddElement(mouseMask);
            base.AddElement(clickMask);
            base.AddElement(sprite);
        }
    }
}