#region

using Forge.Framework.Control;

#endregion

namespace Forge.Framework.UI{
    /// <summary>
    /// Used to define a UI element/collection.
    /// </summary>
    internal interface IUIElement{
        FrameStrata FrameStrata { get; }
        int X { get; set; }
        int Y { get; set; }
        int Width { get; }
        int Height { get; }
        float Alpha { get; set; }
        MouseController MouseController { get; }
        bool HitTest(int x, int y);
    }
}