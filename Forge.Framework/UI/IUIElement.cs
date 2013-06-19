namespace Forge.Framework.UI{
    /// <summary>
    /// Used to define a UI element/collection.
    /// </summary>
    internal interface IUIElement{
        FrameStrata FrameStrata { get; }
        float X { get; set; }
        float Y { get; set; }
        float Width { get; set; }
        float Height { get; set; }
        float Alpha { get; set; }
        bool HitTest(int x, int y);
    }
}