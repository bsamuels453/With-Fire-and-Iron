#region

using System;

#endregion

namespace Forge.Core.ObjectEditor.Tools{
    internal interface IToolbarTool : IDisposable{
        bool Enabled { get; set; }
    }
}