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
        readonly RenderTarget _renderTarget;

        public TerrainManager(GamestateManager mgr){
            _renderTarget = new RenderTarget(0.0f);
            _renderTarget.Bind();
            _loadedChunks = new List<TerrainChunk>();
            _manager = mgr;
            _generator = new TerrainGen();
            for (int x = 0; x < 3; x++){
                for (int z = 0; z < 3; z++){
                    var chunk = _generator.GenerateChunk(new XZPair(x, z));
                    chunk.SetBufferData();
                    _loadedChunks.Add(chunk);
                }
            }


            _renderTarget.Unbind();
        }

        public void Dispose(){
            foreach (var chunk in _loadedChunks){
                chunk.Dispose();
            }
            _loadedChunks.Clear();
        }

        public void Update(InputState state, double timeDelta){
            _renderTarget.Bind();
            var playerPos = (Vector3)_manager.QuerySharedData(SharedStateData.PlayerPosition);
            _renderTarget.Unbind();
        }

        public void Draw(){
            var playerPos = (Vector3)_manager.QuerySharedData(SharedStateData.PlayerPosition);
            var playerLook = (Angle3)_manager.QuerySharedData(SharedStateData.PlayerLook);
            var matrix = RenderHelper.CalculateViewMatrix(playerPos, playerLook);
            _renderTarget.Draw(matrix, Color.Transparent);
        }
    }
}
