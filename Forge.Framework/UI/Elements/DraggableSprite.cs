#region

using Forge.Framework.Draw;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Framework.UI.Elements{
    internal class DraggableSprite : DraggableCollection{
        public DraggableSprite(UIElementCollection parent, FrameStrata.Level depth, Rectangle boundingBox, Texture2D texture) :
            base(parent, depth, boundingBox, "Sprite"){
            var sprite = new Sprite2D
                (
                texture,
                boundingBox,
                this.FrameStrata,
                FrameStrata.Level.Medium
                );
            this.AddElement(sprite);
        }
    }
}