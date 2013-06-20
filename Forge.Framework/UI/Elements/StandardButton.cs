#region

using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Framework.UI.Elements{
    internal class StandardButton : UIElementCollection{
        public StandardButton(
            UIElementCollection parent,
            FrameStrata.FrameStratum depth,
            Rectangle boundingBox,
            Texture2D texture,
            bool clickMask,
            bool enableMouseoverMask
            ) : base(parent, depth, boundingBox, "StandardButton"){
        }
    }
}