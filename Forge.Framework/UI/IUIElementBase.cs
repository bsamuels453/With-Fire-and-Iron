﻿#region

using System.Collections.Generic;

#endregion

namespace Forge.Framework.UI{
    public interface IUIElementBase : ILogicUpdates, IInputUpdates{
        float X { get; set; }
        float Y { get; set; }
        float Width { get; set; }
        float Height { get; set; }
        float Alpha { get; set; }
        float Depth { get; set; }
        bool HitTest(int x, int y);
        List<IUIElementBase> GetElementStack(int x, int y);
    }
}