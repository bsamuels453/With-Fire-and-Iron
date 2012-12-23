using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gondola.Common;
using Gondola.Draw;

namespace Gondola.GameState.Terrain {
    struct ChunkData {
        public XZPair Identifier;
        public TerrainBuffer Buffer;
    }
}
