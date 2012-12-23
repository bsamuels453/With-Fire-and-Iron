#region

using System.Diagnostics;
using Gondola.Logic;
using OpenCLTemplate;

#endregion

namespace Gondola.GameState.Terrain{
    internal class TerrainGen{
        readonly CLCalc.Program.Variable[] _args; //6, 7, 8 are int chunkofstX, chunkofstZ, float result
        readonly int _blockWidth;
        readonly int _chunkWidth;
        readonly CLCalc.Program.Kernel _kernel;
        readonly CLCalc.Program.Variable _results;
        readonly int[] _workers;

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

            CLCalc.Program.Variable lacunarityVar = new CLCalc.Program.Variable(new[]{lacunarity});
            CLCalc.Program.Variable gainVar = new CLCalc.Program.Variable(new[]{gain});
            CLCalc.Program.Variable offsetVar = new CLCalc.Program.Variable(new[]{offset});
            CLCalc.Program.Variable octavesVar = new CLCalc.Program.Variable(new[]{octaves});
            CLCalc.Program.Variable hScaleVar = new CLCalc.Program.Variable(new[]{hScale});
            CLCalc.Program.Variable vScaleVar = new CLCalc.Program.Variable(new[]{vScale});
            _results = new CLCalc.Program.Variable(new float[_chunkWidth*_chunkWidth]);

            _args = new[]{lacunarityVar, gainVar, offsetVar, octavesVar, hScaleVar, vScaleVar, null, null, _results};

            string genScript = Gbl.LoadScript("TGen_GenScript");
            CLCalc.Program.Compile(new[]{genScript});
            _kernel = new CLCalc.Program.Kernel("GenTerrain");

            _workers = new int[]{_chunkWidth, _chunkWidth};
        }

        public void Generate(int offsetX, int offsetZ){
            float[] result = new float[_chunkWidth*_chunkWidth];

            CLCalc.Program.Variable offsetXVar = new CLCalc.Program.Variable(new []{offsetX});
            CLCalc.Program.Variable offsetZVar = new CLCalc.Program.Variable(new []{offsetZ});
            _args[6] = offsetXVar;
            _args[7] = offsetZVar;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            _kernel.Execute(_args, _workers);
            _results.ReadFromDeviceTo(result);
            sw.Stop();

            double d = sw.ElapsedMilliseconds;
            int f = 3;
        }
    }
}