#region

using System;
using System.Collections.Generic;
using Forge.Framework.Control;

#endregion

namespace Forge.Framework.UI{
    /// <summary>
    /// Used to define a UI element/collection.
    /// </summary>
    public interface IUIElement : IDisposable{
        FrameStrata FrameStrata { get; }
        int X { get; set; }
        int Y { get; set; }
        int Width { get; }
        int Height { get; }
        float Alpha { get; set; }
        MouseController MouseController { get; }
        bool HitTest(int x, int y);
        List<IUIElement> GetElementStackAtPoint(int x, int y);
        void Update(float timeDelta);
    }
}