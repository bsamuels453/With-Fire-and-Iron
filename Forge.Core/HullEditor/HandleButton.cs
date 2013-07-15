#region

using Forge.Framework.Control;
using Forge.Framework.UI;
using MonoGameUtility;

#endregion

namespace Forge.Core.HullEditor{
    internal class HandleButton : UIElementCollection{
        public HandleButton(MouseManager mouseManager) : base(mouseManager){
        }

        public HandleButton(UIElementCollection parent, FrameStrata.Level depth, Rectangle boundingBox, string alias) : base(parent, depth, boundingBox, alias){
        }

        public Vector2 CentPosition{
            get{
                float x = base.BoundingBox.X + base.BoundingBox.Width/2f;
                float y = base.BoundingBox.Y + base.BoundingBox.Height/2f;
                return new Vector2(x, y);
            }
            set{
                float newX = value.X -= base.BoundingBox.Width/2f;
                float newY = value.Y -= base.BoundingBox.Height/2f;
                base.X = (int) newX;
                base.Y = (int) newY;
            }
        }
    }
}