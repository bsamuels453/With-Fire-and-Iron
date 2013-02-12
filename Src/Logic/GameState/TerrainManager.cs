using System.Collections.Generic;
using Gondola.Common;
using Gondola.Draw;
using Gondola.Logic.Terrain;
using Microsoft.Xna.Framework;

namespace Gondola.Logic.GameState {
    class TerrainManager : IGameState {
        readonly GamestateManager _manager;
        readonly List<TerrainChunk> _loadedChunks;
        readonly TerrainGen _generator;
        RenderTarget _renderTarget;

        public TerrainManager(GamestateManager mgr){
            _loadedChunks = new List<TerrainChunk>();
            _manager = mgr;
            _generator = new TerrainGen();
            var chunk = _generator.GenerateChunk(new XZPair(0, 0));
            _loadedChunks.Add(chunk);
            _renderTarget = new RenderTarget();
        }

        public void Dispose(){
            foreach (var chunk in _loadedChunks){
                chunk.Dispose();
            }
            _loadedChunks.Clear();
        }

        public void Update(InputState state, double timeDelta){
            var playerPos = (Vector3)_manager.QuerySharedData(SharedStateData.PlayerPosition);
        }

        public void Draw(){
            _renderTarget.Bind();
            var playerPos = (Vector3)_manager.QuerySharedData(SharedStateData.PlayerPosition);
            var playerLook = (Angle3)_manager.QuerySharedData(SharedStateData.PlayerLook);
            var matrix = RenderHelper.CalculateViewMatrix(playerPos, playerLook);
            foreach (var chunk in _loadedChunks){
                chunk.Draw(matrix);
            }
            _renderTarget.Unbind();
        }
    }
}
