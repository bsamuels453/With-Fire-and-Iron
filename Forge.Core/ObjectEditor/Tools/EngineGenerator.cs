using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGameUtility;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Forge.Core.ObjectEditor.Tools {
    class EngineGenerator : IZoneFiller {
        public void Dispose(){
            //throw new NotImplementedException();
        }

        public Color TintColor {
            get;
            set;
        }

        public bool Enabled {
            get;
            set;
        }

        public void Reset(){
            //throw new NotImplementedException();
        }

        public List<ExtendedGameObj> ExtractGeneratedObjects(){
            //throw new NotImplementedException();
            return new List<ExtendedGameObj>();
        }

        public void FillZone(int deck, Vector3 modelspacePos, XZPoint dimensions){
            //throw new NotImplementedException();
        }
    }
}
