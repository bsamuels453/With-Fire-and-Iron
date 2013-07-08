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
        static readonly UIElementCollection _collection;

        static DebugText(){
            _text = new Dictionary<string, TextBox>();
            _collection = new UIElementCollection(null);
        }

        public static void CreateText(string id, int x, int y){
            _text.Add(id, new TextBox(new Point(x,y), _collection, FrameStrata.Level.DebugHigh, Color.LimeGreen));
        }

        public static void SetText(string id, string text){
            _text[id].SetText(text);
        }
    }
}