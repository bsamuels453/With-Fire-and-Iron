#region

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGameUtility;
using Vector3 = Microsoft.Xna.Framework.Vector3;

#endregion

namespace Forge.Core.ObjectEditor.Tools{
    internal interface IZoneFiller : IDisposable{
        Color TintColor { set; }
        bool Enabled { get; set; }
        void Reset();
        List<ExtendedGameObj> ExtractGeneratedObjects();
        void FillZone(int deck, Vector3 modelspacePos, XZPoint dimensions);
    }
}