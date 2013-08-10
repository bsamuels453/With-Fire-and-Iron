#region

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

#endregion

namespace Forge.Framework.Resources{
    public struct GenericObjectDef{
        public readonly int Family;
        public readonly JObject JObject;
        public readonly long Uid;

        public GenericObjectDef(JObject jObject, long uid, int family){
            JObject = jObject;
            Uid = uid;
            Family = family;
        }
    }

    public class GameObjectLoader : ResourceLoader{
        readonly Dictionary<int, GameObjectTag[]> _gameObjectFamilies;

        public GameObjectLoader(){
            var jobj = Resource.LoadConfig("Config/GameObjectDict.config");

            _gameObjectFamilies = new Dictionary<int, GameObjectTag[]>(jobj.Count);
            foreach (var family in jobj){
                var familyName = int.Parse(family.Key.Substring(2), NumberStyles.HexNumber);
                var familyPath = family.Value.ToObject<string>();

                var familyDict = Resource.LoadConfig(familyPath);
                var tags = new List<GameObjectTag>(familyDict.Count);
                foreach (var familyObj in familyDict){
                    tags.Add
                        (new GameObjectTag
                            (
                            path: familyObj.Value.ToObject<string>(),
                            uid: long.Parse(familyObj.Key)
                            )
                        );
                }
                _gameObjectFamilies.Add
                    (
                        familyName,
                        tags.ToArray()
                    );
            }
        }

        /// <summary>
        /// Loads a single game object config file from the specified family.
        /// </summary>
        /// <param name="familyId"></param>
        /// <param name="uid"></param>
        public JObject LoadGameObject(int familyId, long uid){
            var familyTags = _gameObjectFamilies[familyId];
            var objectTag =
                (
                    from tag in familyTags
                    where tag.Uid == uid
                    select tag
                    ).Single();
            return Resource.LoadConfig(objectTag.Path);
        }

        public IEnumerable<long> GetFamilyUids(int familyId){
            return _gameObjectFamilies[familyId].Select(tag => tag.Uid);
        }

        /// <summary>
        /// Loads the configuration files for a range of game objects from the specified family.
        /// </summary>
        /// <param name="familyId"></param>
        /// <param name="uids"> </param>
        public IEnumerable<JObject> LoadGameObjectRange(int familyId, IEnumerable<long> uids){
            var familyTags = _gameObjectFamilies[familyId];

            var gameObjectConfigs = new List<JObject>(familyTags.Count());

            gameObjectConfigs.AddRange
                (
                    from tag in familyTags
                    where uids.Contains(tag.Uid)
                    select Resource.LoadConfig(tag.Path)
                );

            gameObjectConfigs.TrimExcess();
            return gameObjectConfigs;
        }

        public GenericObjectDef[] LoadAllGameObjects(){
            var ret = new List<GenericObjectDef>();
            foreach (var family in _gameObjectFamilies){
                var familyTags = family.Value;
                int familyId = family.Key;

                foreach (var tag in familyTags){
                    var jobj = Resource.LoadConfig(tag.Path);
                    ret.Add
                        (new GenericObjectDef
                            (
                            jobj,
                            tag.Uid,
                            familyId
                            )
                        );
                }
            }
            return ret.ToArray();
        }

        public override void Dispose(){
            _gameObjectFamilies.Clear();
        }

        #region Nested type: GameObjectTag

        struct GameObjectTag{
            public readonly string Path;
            public readonly long Uid;

            public GameObjectTag(string path, long uid) : this(){
                Path = path;
                Uid = uid;
            }
        }

        #endregion
    }
}