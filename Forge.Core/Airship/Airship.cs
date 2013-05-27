using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        //public Vector3 Centroid { get; private set; }
        public ObjectBuffer<AirshipObjectIdentifier>[] DeckBuffers { get; private set; }
        public ObjectBuffer<int>[] HullBuffers { get; private set; }
        public HullSectionContainer HullSections { get; private set; }

        /// <summary>
        /// Reflects  the current position of the airship, as measured from its center.
        /// </summary>
        public Vector3 Position{
            get { return _controller.Position; }
        }

        public float Velocity{
            get { return _controller.Velocity; }
        }

        public Airship(
            ModelAttributes airshipModel,
            ObjectBuffer<AirshipObjectIdentifier>[] deckBuffers,
            ObjectBuffer<int>[] hullBuffers,
            HullSectionContainer hullSections
            ){
            _curDeck = 0;
            _numDecks = airshipModel.NumDecks;
            ModelAttributes = airshipModel;
            HullSections = hullSections;
            _projectilePhysics = new ProjectilePhysics();//oh my god get this out of here
            DeckBuffers = deckBuffers;
            HullBuffers = hullBuffers;


            var movementState = new AirshipMovementState();
            movementState.Angle = new Vector3(0, 0, 0);
            movementState.CurPosition = new Vector3(airshipModel.Length / 3, 2000, 0);

            _controller = new PlayerAirshipController(
                SetAirshipWMatrix,
                ModelAttributes,
                movementState
                );

            _hullIntegrityMesh = new HullIntegrityMesh(HullBuffers, HullSections, _projectilePhysics, _controller.Position, ModelAttributes.Length);

            _hardPoints = new List<Hardpoint>();
            _hardPoints.Add(new Hardpoint(new Vector3(-25, 0, 0), new Vector3(1, 0, 0), _projectilePhysics, ProjectilePhysics.EntityVariant.EnemyShip));
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
            _hullIntegrityMesh.WorldMatrix = worldMatrix;

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

        bool _disposed;

        public void Dispose(){
            Debug.Assert(!_disposed);
            _projectilePhysics.Dispose();
            _hullIntegrityMesh.Dispose();

            foreach (var buffer in DeckBuffers){
                buffer.Dispose();
            }
            foreach (var buffer in HullBuffers){
                buffer.Dispose();
            }
            _disposed = true;
        }

        ~Airship(){
            Debug.Assert(_disposed);
        }
    }
}
