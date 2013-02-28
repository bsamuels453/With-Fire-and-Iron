using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gondola.Logic;

namespace Gondola.UI.Widgets {
    internal interface IToolbarTool : IInputUpdates, ILogicUpdates {
        bool Enabled { get; set; }
    }
}
