using System;
using Forge.Framework.Resources;
using MonoGameUtility;

namespace Forge.Core.ObjectEditor{
    /// <summary>
    /// Used to load statistics about gameobjects. Essentially, GameObject class only contains identifying
    /// information for what the object is, in addition to any data specific to that object such as rotation and position.
    /// This provider is used to provide additional data about gameobjects so that we can extend gameobject functionality
    /// without changing the data struct. This class will provide information such as dimensions, access points, and any
    /// other information that will be identical across objects of the same uid/family.
    /// </summary>
    public class StatisticProvider{


        public StatisticProvider(){
            //Resource.GameObjectLoader.
        }

        public XZPoint GetObjectDims(GameObject obj){
            throw new NotImplementedException();
        }

        public XZPoint GetObjectDims(GameObjectType type, long uid) {
            throw new NotImplementedException();
        }
    }
}