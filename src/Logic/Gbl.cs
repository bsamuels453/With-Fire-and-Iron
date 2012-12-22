#region

using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

#endregion

namespace Gondola.Logic{
    internal static class Gbl{
        public static Vector3 CameraPosition;
        public static Vector3 CameraTarget;
        public static GraphicsDevice Device;
        public static ContentManager ContentManager;
        public static Dictionary<string, string> ContentStrLookup;
        public static Matrix ProjectionMatrix;
        public static Point ScreenSize;

        static Gbl(){
            var sr = new StreamReader("Raw/ContentReferences.json");
            ContentStrLookup = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
            sr.Close();

        }
    }
}