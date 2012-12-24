#region

using System.Diagnostics;
using Gondola.Logic;
using OpenCLTemplate;

#endregion

namespace Gondola.GameState.Terrain{
    internal class TerrainGen{
        readonly int _blockWidth;
        readonly int _chunkWidth;
        readonly CLCalc.Program.Variable _terrGeometry;
        readonly CLCalc.Program.Variable _terrNormals;
        readonly CLCalc.Program.MemoryObject[] _terrainGenArgs;
        readonly CLCalc.Program.Kernel _terrainGenKernel;
        readonly int[] _terrainGenWorkers;

        public TerrainGen(){
            CLCalc.InitCL();

            _chunkWidth = Gbl.LoadContent<int>("TGen_ChunkWidthInBlocks");
            _blockWidth = Gbl.LoadContent<int>("TGen_BlockWidthInMeters");
            float lacunarity = Gbl.LoadContent<float>("TGen_Lacunarity");
            float gain = Gbl.LoadContent<float>("TGen_Gain");
            int octaves = Gbl.LoadContent<int>("TGen_Octaves");
            float offset = Gbl.LoadContent<float>("TGen_Offset");
            float hScale = Gbl.LoadContent<float>("TGen_HScale");
            float vScale = Gbl.LoadContent<float>("TGen_VScale");

            CLCalc.Program.Variable constants = new CLCalc.Program.Variable(
                new[]{
                    lacunarity,
                    gain,
                    offset,
                    octaves,
                    hScale,
                    vScale,
                    _blockWidth
                }
                );

            _terrGeometry = new CLCalc.Program.Variable(new float[_chunkWidth*_chunkWidth*4]);
            _terrNormals = new CLCalc.Program.Variable(new float[_chunkWidth*_chunkWidth*4]);

            _terrainGenArgs = new CLCalc.Program.MemoryObject[]{
                constants,
                null,
                null,
                _terrGeometry,
                _terrNormals
            };

            string genScript = Gbl.LoadScript("TGen_GenScript");
            CLCalc.Program.Compile(new[]{genScript});
            _terrainGenKernel = new CLCalc.Program.Kernel("GenTerrain");

            _terrainGenWorkers = new[]{_chunkWidth, _chunkWidth};
        }

        //10 millisecond overhead
        public void Generate(int offsetX, int offsetZ){
            CLCalc.Program.Variable offsetXVar = new CLCalc.Program.Variable(new[]{offsetX});
            CLCalc.Program.Variable offsetZVar = new CLCalc.Program.Variable(new[]{offsetZ});

            _terrainGenArgs[1] = offsetXVar;
            _terrainGenArgs[2] = offsetZVar;
            var sw = new Stopwatch();
            sw.Start();
            _terrainGenKernel.Execute(_terrainGenArgs, _terrainGenWorkers);
            sw.Stop();
            var v = new float[_chunkWidth*_chunkWidth*4];
            _terrGeometry.ReadFromDeviceTo(v);

            double d = sw.Elapsed.Milliseconds;
            int f = 3;
        }
    }
}