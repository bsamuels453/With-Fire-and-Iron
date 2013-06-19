#region

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace Forge.Framework.Draw{
    /// <summary>
    /// Debugging class used to display text on the screen at the highest display level avail.
    /// </summary>
    public static class DebugText{
        //static readonly Dictionary<string, TextBox> _text;

        static DebugText(){
            throw new Exception();
            //_text = new Dictionary<string, TextBox>();
        }

        public static void CreateText(string id, int x, int y){
            throw new Exception();
            //_text.Add(id, new TextBox(x, y, DepthLevel.High, Color.LimeGreen));
        }

        public static void SetText(string id, string text){
            throw new Exception();
            //_text[id].SetText(text);
        }
    }
}