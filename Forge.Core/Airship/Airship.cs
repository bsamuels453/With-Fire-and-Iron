using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Forge.Core.Airship.Data;
using Forge.Core.Logic;
using Forge.Core.ObjectEditor;
using Forge.Core.Physics;
using Forge.Framework.Draw;
using Forge.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameUtility;

namespace Forge.Core.Airship {
    internal class Airship : IDisposable{
        public ModelAttributes ModelAttributes { get; private set; }
        readonly List<Hardpoint> _hardPoints;
        readonly ProjectilePhysics _projectilePhysics;
        readonly AirshipController _controller;
        readonly HullIntegrityMesh _hullIntegrityMesh;

        //public Vector3 Centroid { get; private set; }
        public HullSectionContainer HullSectionContainer { get; private set; }
        public DeckSectionContainer DeckSectionContainer { get; private set; }

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
            DeckSectionContainer deckSectionContainer,
            HullSectionContainer hullSectionContainer
            ){

            ModelAttributes = airshipModel;
            HullSectionContainer = hullSectionContainer;
            DeckSectionContainer = deckSectionContainer;
            _projectilePhysics = new ProjectilePhysics();//oh my god get this out of here

            var movementState = new AirshipMovementData();
            movementState.Angle = new Vector3(0, 0, 0);
            movementState.CurPosition = new Vector3(airshipModel.Length / 3, 2000, 0);

            _controller = new PlayerAirshipController(
                SetAirshipWMatrix,
                ModelAttributes,
                movementState
                );

            _hullIntegrityMesh = new HullIntegrityMesh(HullSectionContainer, _projectilePhysics, _controller.Position, ModelAttributes.Length);

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
            HullSectionContainer.SetTopVisibleDeck(HullSectionContainer.TopExpIdx - 1);
            DeckSectionContainer.SetTopVisibleDeck(DeckSectionContainer.TopExpIdx - 1);
        }

        public void RemoveVisibleLayer(int _){
            HullSectionContainer.SetTopVisibleDeck(HullSectionContainer.TopExpIdx + 1);
            DeckSectionContainer.SetTopVisibleDeck(DeckSectionContainer.TopExpIdx + 1);
        }

        void SetAirshipWMatrix(Matrix worldMatrix) {
            _hullIntegrityMesh.WorldMatrix = worldMatrix;

            foreach (var hullLayer in HullSectionContainer.HullBuffersByDeck){
                hullLayer.WorldMatrix = worldMatrix;
            }

            foreach (var deckLayer in DeckSectionContainer.DeckBufferByDeck) {
                deckLayer.WorldMatrix = worldMatrix;
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

            DeckSectionContainer.Dispose();
            HullSectionContainer.Dispose();

            foreach (var hardPoint in _hardPoints){
                hardPoint.Dispose();
            }
            _disposed = true;
        }

        ~Airship(){
            Debug.Assert(_disposed);
        }
    }
}
