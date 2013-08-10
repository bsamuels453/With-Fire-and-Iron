#region

using System.Linq;
using Forge.Core.ObjectEditor;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;
using Newtonsoft.Json.Linq;

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
        static readonly GenericObjectDef[] _gameObjects;

        static ObjectStatisticProvider(){
            _gameObjects = Resource.GameObjectLoader.LoadAllGameObjects();
        }

        public static void Initialize(){
        }

        static JObject GetObject(GameObjectFamily family, long uid){
            return _gameObjects.Single(o => o.Family == (int) family && o.Uid == uid).JObject;
        }

        public static XZPoint GetObjectDims(GameObjectFamily family, long uid){
            var obj = GetObject(family, uid);
            return obj["Dimensions"].ToObject<XZPoint>();
        }

        public static Model[] GetModels(GameObjectFamily family, long uid){
            var obj = GetObject(family, uid);
            var modelStrs = obj["Model"].ToObject<string[]>();
            var models = modelStrs.Select(Resource.LoadContent<Model>).ToArray();
            return models;
        }

        public static GameObjectEnvironment.SideEffect[] GetSideEffects(GameObjectFamily family, long uid){
            var obj = GetObject(family, uid);
            var sideEffects = obj["SideEffects"].ToObject<GameObjectEnvironment.SideEffect[]>();
            return sideEffects;
        }

        public static Texture2D GetIcon(GameObjectFamily family, long uid){
            var obj = GetObject(family, uid);
            var iconTex = obj["Icon"].ToObject<string>();
            return Resource.LoadContent<Texture2D>(iconTex);
        }

        public static string GetName(GameObjectFamily family, long uid){
            var obj = GetObject(family, uid);
            var name = obj["Name"].ToObject<string>();
            return name;
        }

        public static Vector3 GetProjectileEmitterOffset(GameObjectFamily family, long uid){
            var obj = GetObject(family, uid);
            var offset = obj["ProjectileEmitterOffset"].ToObject<Vector3>();
            return offset;
        }

        public static float GetFiringForce(GameObjectFamily family, long uid){
            var obj = GetObject(family, uid);
            var firingForce = obj["FiringForce"].ToObject<float>();
            return firingForce;
        }

        public static float GetMass(GameObjectFamily family, long uid){
            var obj = GetObject(family, uid);
            var mass = obj["Mass"].ToObject<float>();
            return mass;
        }

        public static float GetRadius(GameObjectFamily family, long uid){
            var obj = GetObject(family, uid);
            var radius = obj["Radius"].ToObject<float>();
            return radius;
        }
    }
}