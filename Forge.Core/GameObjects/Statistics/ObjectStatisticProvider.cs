#region

using System;
using Forge.Core.ObjectEditor;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Core.GameObjects.Statistics{
    /// <summary>
    /// Used to load statistics about gameobjects. Essentially, GameObject class only contains identifying
    /// information for what the object is, in addition to any data specific to that object such as rotation and position.
    /// This provider is used to provide additional data about gameobjects so that we can extend gameobject functionality
    /// without changing the data struct. This class will provide information such as dimensions, access points, and any
    /// other information that will be identical across objects of the same uid/family.
    /// 
    /// Another bonus is that this class essentially defines every single possible kv field that can be assigned to an object
    /// in its configuration file. Using this class as an interface to those unique kv fields makes it easy to change them later
    /// without changing the interface.
    /// </summary>
    public static class ObjectStatisticProvider{
        static GenericObjectDef[] _gameObjects;

        static ObjectStatisticProvider(){
            _gameObjects = Resource.GameObjectLoader.LoadAllGameObjects();
        }

        public static void Initialize(){
        }

        public static XZPoint GetObjectDims(GameObjectFamily type, long uid){
            throw new NotImplementedException();
        }

        public static Model[] GetModels(GameObjectFamily family, long uid){
            throw new NotImplementedException();
        }

        public static GameObjectEnvironment.SideEffect[] GetSideEffects(GameObjectFamily family, long uid){
            throw new NotImplementedException();
        }

        public static Texture2D GetIcon(GameObjectFamily family, long uid){
            throw new NotImplementedException();
        }

        public static string GetName(GameObjectFamily family, long uid){
            throw new NotImplementedException();
        }
    }
}