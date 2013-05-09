using System;
using System.Collections.Generic;
using System.Linq;
using Forge.Core.Logic;
using Forge.Core.ObjectEditor;
using Forge.Framework.Draw;
using Forge.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Forge.Core.Airship {
    internal class Airship : IDisposable{
        public ModelAttributes ModelAttributes { get; private set; }

        int _curDeck;
        readonly int _numDecks;
        readonly List<Hardpoint> _hardPoints;
        readonly ProjectilePhysics _projectilePhysics;
        readonly AirshipController _controller;
        readonly HullIntegrityMesh _hullIntegrityMesh;

        public Vector3 Centroid { get; private set; }
        public ObjectBuffer<ObjectIdentifier>[] DeckBuffers { get; private set; }
        public ObjectBuffer<HullSection>[] HullBuffers { get; private set; }

        public Vector3 Position{
            get { return _controller.Position; }
        }

        public float Velocity{
            get { return _controller.Velocity; }
        }

        public Airship(
            ModelAttributes airshipModel,
            ObjectBuffer<ObjectIdentifier>[] deckBuffers,
            List<HullMesh>[] hullBuffers
            ){
            _curDeck = 0;
            _numDecks = airshipModel.NumDecks;
            ModelAttributes = airshipModel;
            _projectilePhysics = new ProjectilePhysics();
            DeckBuffers = deckBuffers;

            HullBuffers = new ObjectBuffer<HullSection>[hullBuffers.Length-1];

            //this minus 1 is because of the faux lowest layer
            for(int i=0; i<hullBuffers.Length-1; i++){
                var layerBuffs = (from mesh in hullBuffers[i]
                                    select  mesh.HullBuff).ToList();

                HullBuffers[i] = new ObjectBuffer<HullSection>(
                    layerBuffs.Count * layerBuffs[0].MaxObjects,
                    layerBuffs[0].IndiciesPerObject / 3,
                    layerBuffs[0].VerticiesPerObject,
                    layerBuffs[0].IndiciesPerObject,
                    "Shader_AirshipHull"
                    );
                foreach (var buffer in layerBuffs){
                    HullBuffers[i].AbsorbBuffer(buffer, true);
                }
            }

            foreach (var buffer in HullBuffers){
                buffer.ApplyTransform((vert) => {
                    vert.Position.X *= -1;
                    return vert;
                }
                );
            }

            foreach (var buffer in DeckBuffers) {
                buffer.ApplyTransform((vert) => {
                    vert.Position.X *= -1;
                    return vert;
                }
                );
            }

            //_hullIntegrityMesh = new HullIntegrityMesh(HullBuffers, ModelAttributes.Length);

            _hardPoints = new List<Hardpoint>();
            _hardPoints.Add(new Hardpoint(new Vector3(0, 0, 0), new Vector3(1, 0, 0), _projectilePhysics, ProjectilePhysics.EntityVariant.EnemyShip));

            var movementState = new AirshipMovementState();
            movementState.Angle = new Vector3(0, 0, 0);
            movementState.CurPosition = new Vector3(airshipModel.Length/ 3, 1000, 0);

            _controller = new PlayerAirshipController(
                SetAirshipWMatrix,
                ModelAttributes,
                movementState
                );
        }

        public void Update(ref InputState state, double timeDelta){
            _controller.Update(ref state, timeDelta);



            foreach (var hardPoint in _hardPoints){
                hardPoint.Update(timeDelta);
            }
            if (state.PrevState.KeyboardState.IsKeyDown(Keys.Space)) {
                foreach (var hardpoint in _hardPoints) {
                    hardpoint.Fire();
                }
            }

            _projectilePhysics.Update(timeDelta);

        }

        public void AddVisibleLayer(int _){
            if (_curDeck != 0){
                var tempFloorBuff = DeckBuffers.Reverse().ToArray();
                var tempHullBuff = HullBuffers.Reverse().ToArray();
                //var tempWWallBuff = WallBuffers.Reverse().ToArray();
                for (int i = 0; i < tempFloorBuff.Count(); i++){
                    if (tempFloorBuff[i].Enabled == false){
                        _curDeck--;
                        tempFloorBuff[i].Enabled = true;
                        //tempWWallBuff[i].Enabled = true;

                        if (i < _numDecks - 1){
                            tempHullBuff[i].Enabled = true;
                        }
                        tempHullBuff[i-1].CullMode = CullMode.None;
                        break;
                    }
                }
            }
        }

        public void RemoveVisibleLayer(int _){
            if (_curDeck < _numDecks - 1){
                for (int i = 0; i < DeckBuffers.Count(); i++){
                    if (DeckBuffers[i].Enabled){
                        _curDeck++;
                        DeckBuffers[i].Enabled = false;
                        HullBuffers[i].CullMode = CullMode.CullCounterClockwiseFace;
                        if (i > 0){
                            HullBuffers[i - 1].Enabled = false;
                        }
                        //WallBuffers[i].Enabled = false;
                        break;
                    }
                }
            }
        }

        void SetAirshipWMatrix(Matrix worldMatrix) {
//_hullIntegrityMesh.WorldMatrix = worldMatrix;

            foreach (var deck in DeckBuffers) {
                deck.WorldMatrix = worldMatrix;
            }
            foreach (var layer in HullBuffers) {
                layer.WorldMatrix = worldMatrix;
            }

            foreach (var hardPoint in _hardPoints) {
                hardPoint.ShipTranslationMtx = worldMatrix;
            }
        }

        public void Dispose(){
            _projectilePhysics.Dispose();
        }
    }
}
