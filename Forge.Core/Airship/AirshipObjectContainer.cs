#region

using System;
using System.Collections.Generic;
using Forge.Core.ObjectEditor;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship{
    internal class AirshipObjectContainer : IDisposable{
        public AirshipObjectContainer(List<GameObject> gameObjects){
        }

        public Matrix WorldTransform { get; set; }

        #region IDisposable Members

        public void Dispose(){
            throw new NotImplementedException();
        }

        #endregion

        public void SetTopVisibleDeck(int deck){
            throw new NotImplementedException();
        }

        public void Update(){
            throw new NotImplementedException();
        }
    }
}