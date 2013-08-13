#region

using System;
using System.Linq;
using Forge.Framework.Resources;
using Newtonsoft.Json.Linq;
using ProtoBuf;

#endregion

namespace Forge.Core.GameObjects{
    [ProtoContract]
    public struct GameObjectType : IEquatable<GameObjectType>{
        [ProtoMember(2)] public readonly GameObjectFamily Family;
        [ProtoMember(1)] public readonly int Uid;

        public GameObjectType(GameObjectFamily family, int uid){
            Family = family;
            Uid = uid;
        }

        #region IEquatable<GameObjectType> Members

        public bool Equals(GameObjectType other){
            if (other.Family == this.Family){
                if (other.Uid == this.Uid){
                    return true;
                }
            }
            return false;
        }

        #endregion

        public T Attribute<T>(GameObjectAttr attribute){
            int familyIdx = (int) Family;
            int attrIdx = (int) attribute;

            if (familyIdx >= _objectData.Length){
                throw new Exception("family out of range");
            }
            if (Uid >= _objectData[familyIdx].Length){
                throw new Exception("uid out of range");
            }
            if (attrIdx >= _objectData[familyIdx][Uid].Length){
                throw new Exception("attribute out of range");
            }

            var token = _objectData[familyIdx][Uid][attrIdx];
            return token.ToObject<T>();
        }

        #region static

        //family list -> uid list -> attribute list
        static JToken[][][] _objectData;

        static GameObjectType(){
            Initialize();
        }

        public static void Initialize(){
            var gameObjects = Resource.GameObjectLoader.LoadAllGameObjects();
            var sortedByFamily = gameObjects.GroupBy(x => x.Family).ToArray();

            var attributes = Enum.GetNames(typeof (GameObjectAttr)).ToList();

            _objectData = new JToken[sortedByFamily.Length][][];
            foreach (var family in sortedByFamily){
                var familyIdx = family.Key;
                var familyObjects = new JToken[family.Count()][];

                foreach (var gameObj in family){
                    var objAttributes = new JToken[attributes.Count];
                    long uid = gameObj.Uid;

                    foreach (var attribute in gameObj.JObject){
                        var attributeIdx = attributes.IndexOf(attribute.Key);
                        if (attributeIdx == -1){
                            throw new Exception("Attribute found that isn't referenced in GameObjectAttr: " + attribute.Key + " :: " + attribute.Value);
                        }
                        objAttributes[attributeIdx] = attribute.Value;
                    }
                    familyObjects[uid] = objAttributes;
                }
                _objectData[familyIdx] = familyObjects;
            }

            int g = 5;
        }

        #endregion
    }
}