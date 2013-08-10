#region

using System;
using System.Collections.Generic;
using System.Linq;
using Forge.Core.ObjectEditor;
using Forge.Framework.Draw;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Core.Airship{
    internal class AirshipObjectContainer : IDisposable{
        const string _objectShader = "Config/Shaders/TintedModel.config";
        readonly ObjectModelBuffer<ObjectIdentifier>[] _objectBuffers;
        readonly WeaponSystems _weaponSystems;

        public AirshipObjectContainer(List<GameObject> gameObjects, WeaponSystems weaponSystems){
            if (gameObjects == null){
                gameObjects = new List<GameObject>();
            }

            var objectsByDeck = (
                from obj in gameObjects
                group obj by obj.Deck
                ).ToArray();

            _objectBuffers = new ObjectModelBuffer<ObjectIdentifier>[objectsByDeck.Length];
            for (int i = 0; i < _objectBuffers.Length; i++){
                _objectBuffers[i] = new ObjectModelBuffer<ObjectIdentifier>(objectsByDeck[i].Count(), _objectShader);
            }

            //set up buffer
            foreach (var obj in gameObjects){
                var jobj = Resource.GameObjectLoader.LoadGameObject((int) obj.Type, obj.ObjectUid);
                var model = Resource.LoadContent<Model>(jobj["Model"].ToObject<string>());

                var rotation = Matrix.CreateFromYawPitchRoll(obj.Rotation, 0, 0);
                var translation = Matrix.CreateTranslation(obj.ModelspacePosition);
                _objectBuffers[obj.Deck].AddObject(obj.Identifier, model, rotation*translation);
            }


            var weapons = (
                from obj in gameObjects
                where obj.Type == GameObjectType.Cannons
                select obj
                ).ToList();

            _weaponSystems = weaponSystems;
            _weaponSystems.AddWeapons(weapons);
        }

        public Matrix WorldTransform{
            set{
                foreach (var buffer in _objectBuffers){
                    buffer.GlobalTransform = value;
                }
            }
        }

        #region IDisposable Members

        public void Dispose(){
            foreach (var buffer in _objectBuffers){
                buffer.Dispose();
            }
        }

        #endregion

        public void SetTopVisibleDeck(int deck){
            //throw new NotImplementedException();
        }

        public void Update(){
            //throw new NotImplementedException();
        }
    }
}