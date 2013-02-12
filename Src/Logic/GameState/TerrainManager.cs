using System.Collections.Generic;
using Gondola.Common;
using Gondola.Logic.Terrain;

namespace Gondola.Logic.GameState {
    class TerrainManager : IGameState {
        List<TerrainChunk> _loadedChunks;
        Vec3Ref _viewportPos;

        public TerrainManager(){

        }

        public void Dispose(){

        }

        public void Update(InputState state, double timeDelta){

        }

        public void Draw(){
            foreach (var chunk in _loadedChunks){
                
            }
        }
    }
}
