#region

using System.Diagnostics;
using Gondola.Logic;
using OpenCLTemplate;

#endregion

namespace Gondola.GameState.Terrain{
    internal class TerrainGen{
        readonly int _blockWidth;
        readonly int _chunkVertWidth;
        readonly CLCalc.Program.Variable _terrGeometry;
        readonly CLCalc.Program.Image2D _terrNormals;
        readonly CLCalc.Program.Image2D _terrBiNormals;
        readonly CLCalc.Program.Image2D _terrTangents;
        readonly CLCalc.Program.MemoryObject[] _terrainGenArgs;
        readonly CLCalc.Program.Kernel _terrainGenKernel;
        readonly int[] _terrainGenWorkers;

        public TerrainGen(){
            CLCalc.InitCL();

            _chunkVertWidth = Gbl.LoadContent<int>("TGen_ChunkWidthInBlocks") + 1;
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

            _terrGeometry = new CLCalc.Program.Variable(new float[_chunkVertWidth*_chunkVertWidth*4]);
            _terrNormals = new CLCalc.Program.Image2D(new float[_chunkVertWidth*_chunkVertWidth*4], _chunkVertWidth, _chunkVertWidth);
            _terrBiNormals = new CLCalc.Program.Image2D(new float[_chunkVertWidth*_chunkVertWidth*4], _chunkVertWidth, _chunkVertWidth);
            _terrTangents = new CLCalc.Program.Image2D(new float[_chunkVertWidth*_chunkVertWidth*4], _chunkVertWidth, _chunkVertWidth);

            _terrainGenArgs = new CLCalc.Program.MemoryObject[]{
                constants,
                null,
                null,
                _terrGeometry,
                _terrNormals,
                _terrBiNormals,
                _terrTangents
            };

            string genScript = Gbl.LoadScript("TGen_GenScript");
            CLCalc.Program.Compile(new[]{genScript});
            _terrainGenKernel = new CLCalc.Program.Kernel("GenTerrain");

            _terrainGenWorkers = new[]{_chunkVertWidth, _chunkVertWidth};
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
            var v = new float[_chunkVertWidth*_chunkVertWidth*4];
            _terrGeometry.ReadFromDeviceTo(v);
            _terrBiNormals.ReadFromDeviceTo(v);
            _terrNormals.ReadFromDeviceTo(v);
            _terrTangents.ReadFromDeviceTo(v);
            sw.Stop();

            double d = sw.Elapsed.Milliseconds;
            int f = 3;
        }
    }
}