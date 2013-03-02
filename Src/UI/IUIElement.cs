using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gondola.Logic;
using Gondola.Util;

namespace Gondola.UI {
    internal interface IUIElement : ILogicUpdates {
        float X { get; set; }
        float Y { get; set; }
        float Width { get; set; }
        float Height { get; set; }
        FloatingRectangle BoundingBox { get; } //move somewhere else?
        float Alpha { get; set; }
        float Depth { get; set; }
        string Texture { get; set; }
        int Identifier { get; }

        TComponent GetComponent<TComponent>(string identifier = null);
        bool DoesComponentExist<TComponent>(string identifier = null);
    }
}
