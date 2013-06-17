#region

using System.Collections.Generic;
using Forge.Framework.UI;
using Forge.Framework.UI.Widgets;
using Microsoft.Xna.Framework;

#endregion

namespace Forge.Framework.Draw{
    public static class DebugText{
        static readonly Dictionary<string, TextBox> _text;

        static DebugText(){
            _text = new Dictionary<string, TextBox>();
        }

        public static void CreateText(string id, int x, int y){
            _text.Add(id, new TextBox(x, y, DepthLevel.High, Color.LimeGreen));
        }

        public static void SetText(string id, string text){
            _text[id].SetText(text);
        }
    }
}