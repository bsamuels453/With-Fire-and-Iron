#region

using System;
using System.Collections.Generic;
using Forge.Framework.UI;
using Forge.Framework.UI.Elements;
using Microsoft.Xna.Framework;
using MonoGameUtility;
using Point = MonoGameUtility.Point;

#endregion

namespace Forge.Framework.Draw{
    /// <summary>
    /// Debugging class used to display text on the screen at the highest display level avail.
    /// </summary>
    public static class DebugText{
        static readonly Dictionary<string, TextBox> _text;

        static DebugText(){
            _text = new Dictionary<string, TextBox>();
        }

        public static void CreateText(string id, int x, int y){
            _text.Add(id, new TextBox(new Point(x,y), FrameStrata.Level.DebugHigh, Color.LimeGreen));
            int g = 5;
        }

        public static void SetText(string id, string text){
            _text[id].SetText(text);
        }
    }
}