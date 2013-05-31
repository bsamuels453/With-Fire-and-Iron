#region

using System;

#endregion

namespace Forge.Framework.UI.Widgets{
    public interface IToolbarTool : IInputUpdates, ILogicUpdates, IDisposable{
        bool Enabled { get; set; }
    }
}