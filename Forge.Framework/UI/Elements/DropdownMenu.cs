#region

using MonoGameUtility;

#endregion

namespace Forge.Framework.UI.Elements{
    internal class DropdownMenu : UIElementCollection{
        public DropdownMenu(UIElementCollection parent, FrameStrata.Level depth, Rectangle boundingBox, string alias) : base(parent, depth, boundingBox, alias){
        }
    }
}