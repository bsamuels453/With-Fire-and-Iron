#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Cloo;

#endregion

namespace Gondola.Logic.Terrain{
    internal class TerrainGen{

        readonly int _blockWidth;
        readonly int _chunkWidthInVerts;
        readonly ComputeCommandQueue _cmdQueue;

        readonly ComputeKernel _genKernel;
        readonly ComputeProgram _genPrgm;
        readonly ComputeBuffer<float> _genConstants;
        readonly ComputeBuffer<byte> _binormals;
        readonly ComputeBuffer<byte> _tangents;
        readonly ComputeBuffer<byte> _normals;

        readonly ComputeKernel _qTreeKernel;
        readonly ComputeProgram _qTreePrgm;
        readonly ComputeBuffer<byte> _activeVerts;
        readonly ComputeBuffer<int> _dummy; 

        readonly ComputeKernel _winderKernel;
        readonly ComputeProgram _winderPrgm;
        readonly ComputeBuffer<int> _indicies; 

        readonly ComputeContext _context;
        readonly List<ComputeDevice> _devices;
        readonly ComputeBuffer<float> _geometry;
        readonly ComputeContextPropertyList _properties;

        public TerrainGen(){
            var platform = ComputePlatform.Platforms[0];
            _devices = new List<ComputeDevice>();
            _devices.Add(platform.Devices[0]);

            _properties = new ComputeContextPropertyList(platform);
            _context = new ComputeContext(_devices, _properties, null, IntPtr.Zero);
            _cmdQueue = new ComputeCommandQueue(_context, _devices[0], ComputeCommandQueueFlags.None);

            #region setup generator kernel

            _chunkWidthInVerts = Gbl.LoadContent<int>("TGen_ChunkWidthInBlocks") + 1;
            _blockWidth = Gbl.LoadContent<int>("TGen_BlockWidthInMeters");
            float lacunarity = Gbl.LoadContent<float>("TGen_Lacunarity");
            float gain = Gbl.LoadContent<float>("TGen_Gain");
            int octaves = Gbl.LoadContent<int>("TGen_Octaves");
            float offset = Gbl.LoadContent<float>("TGen_Offset");
            float hScale = Gbl.LoadContent<float>("TGen_HScale");
            float vScale = Gbl.LoadContent<float>("TGen_VScale");

            _genConstants = new ComputeBuffer<float>(_context, ComputeMemoryFlags.ReadOnly, 7);
            var genArr = new[]{
                lacunarity,
                gain,
                offset,
                octaves,
                hScale,
                vScale,
                _blockWidth
            };

            _cmdQueue.WriteToBuffer(genArr, _genConstants, false, null);

            _genPrgm = new ComputeProgram(_context, Gbl.LoadScript("TGen_Generator"));
            //string scriptDir = Gbl.GetScriptDirectory("TGen_Generator"); //use option -I + scriptDir for header search
            _genPrgm.Build(null, "", null, IntPtr.Zero);
            _genKernel = _genPrgm.CreateKernel("GenTerrain");

            //despite the script using float3 for these fields, we need to consider it to be float4 because the 
            //implementation is basically a float4 wrapper that uses zero for the last variable
            _geometry = new ComputeBuffer<float>(_context, ComputeMemoryFlags.None, _chunkWidthInVerts * _chunkWidthInVerts * 4);
            _normals = new ComputeBuffer<byte>(_context, ComputeMemoryFlags.None, _chunkWidthInVerts * _chunkWidthInVerts * 4);
            _binormals = new ComputeBuffer<byte>(_context, ComputeMemoryFlags.None, _chunkWidthInVerts * _chunkWidthInVerts * 4);
            _tangents = new ComputeBuffer<byte>(_context, ComputeMemoryFlags.None, _chunkWidthInVerts * _chunkWidthInVerts * 4);

            _genKernel.SetMemoryArgument(0, _genConstants);
            _genKernel.SetMemoryArgument(3, _geometry);
            _genKernel.SetMemoryArgument(4, _normals);
            _genKernel.SetMemoryArgument(5, _binormals);
            _genKernel.SetMemoryArgument(6, _tangents);

            #endregion

            #region setup quadtree kernel

            //Debug.Assert(_chunkWidthInVerts == 129);

            _qTreePrgm = new ComputeProgram(_context, Gbl.LoadScript("TGen_QTree"));
            _qTreePrgm.Build(null, "", null, IntPtr.Zero);
            _qTreeKernel = _qTreePrgm.CreateKernel("QuadTree");

            _activeVerts = new ComputeBuffer<byte>(_context, ComputeMemoryFlags.None, _chunkWidthInVerts * _chunkWidthInVerts);

            _dummy = new ComputeBuffer<int>(_context, ComputeMemoryFlags.None, 50);
            var rawNormals = new byte[_chunkWidthInVerts*_chunkWidthInVerts*4];
            var dsada = new byte[_chunkWidthInVerts * _chunkWidthInVerts];
            for (int i = 0; i < dsada.Length; i++){
                dsada[i] = 1;
            }
            _cmdQueue.WriteToBuffer(rawNormals, _normals, true, null);
            _cmdQueue.WriteToBuffer(dsada, _activeVerts, true, null);

            _qTreeKernel.SetValueArgument(0, _chunkWidthInVerts - 1);
            _qTreeKernel.SetValueArgument(1, 1);
            _qTreeKernel.SetMemoryArgument(2, _normals);
            _qTreeKernel.SetMemoryArgument(3, _activeVerts);
            _qTreeKernel.SetMemoryArgument(4, _dummy);
            #endregion

            #region setup winding kernel

            _winderPrgm = new ComputeProgram(_context, Gbl.LoadScript("TGen_VertexWinder"));
            _winderPrgm.Build(null, "", null, IntPtr.Zero);
            _winderKernel = _winderPrgm.CreateKernel("VertexWinder");

            _indicies = new ComputeBuffer<int>(_context, ComputeMemoryFlags.None, (_chunkWidthInVerts-1) * (_chunkWidthInVerts-1) * 8);

            _winderKernel.SetMemoryArgument(0, _activeVerts);
            _winderKernel.SetMemoryArgument(1, _indicies);

            #endregion

            _cmdQueue.Finish();
        }

        public TerrainChunk GenerateChunk(int offsetX, int offsetZ){
            var rawGeometry = new float[_chunkWidthInVerts*_chunkWidthInVerts*4];
            var rawNormals = new byte[_chunkWidthInVerts*_chunkWidthInVerts*4];
            var rawBinormals = new byte[_chunkWidthInVerts*_chunkWidthInVerts*4];
            var rawTangents = new byte[_chunkWidthInVerts*_chunkWidthInVerts*4];
            var norm = new int[50];
            var actNode = new byte[_chunkWidthInVerts * _chunkWidthInVerts];
            var inds = new int[(_chunkWidthInVerts - 1) * (_chunkWidthInVerts - 1) * 8];
            var sw = new Stopwatch();
            sw.Start();

            _genKernel.SetValueArgument(1, offsetX);
            _genKernel.SetValueArgument(2, offsetZ);

            //_cmdQueue.Execute(_genKernel, null, new long[]{_chunkWidthInVerts, _chunkWidthInVerts}, null, null);
            _cmdQueue.Execute(_qTreeKernel, null, new long[] { (_chunkWidthInVerts - 1) / 2 - 1 , (_chunkWidthInVerts - 1)}, null, null);
            _cmdQueue.Execute(_winderKernel, null, new long[] { (_chunkWidthInVerts - 1), (_chunkWidthInVerts - 1) }, null, null);

            _cmdQueue.ReadFromBuffer(_dummy, ref norm, true, null);
            _cmdQueue.ReadFromBuffer(_activeVerts, ref actNode, true, null);
            _cmdQueue.ReadFromBuffer(_indicies, ref inds, true, null);
            _cmdQueue.Finish();
            sw.Stop();
            double d = sw.ElapsedMilliseconds;
            var sww = new StreamWriter("out.txt");
            int i=0;
            for (int x = 0; x < _chunkWidthInVerts; x++) {
                for (int z = 0; z < _chunkWidthInVerts; z++) {
                    sww.Write(actNode[i]+" ");
                    i++;
                }
                sww.Write('\n');
            }
            sww.Close();
            //var texNormal = new Texture2D(Gbl.Device, _chunkWidthInVerts, _chunkWidthInVerts, false, SurfaceFormat.Color);
            //var texBinormal = new Texture2D(Gbl.Device, _chunkWidthInVerts, _chunkWidthInVerts, false, SurfaceFormat.Color);
            //var texTangent = new Texture2D(Gbl.Device, _chunkWidthInVerts, _chunkWidthInVerts, false, SurfaceFormat.Color);
            //texNormal.SetData(rawNormals);
            //texBinormal.SetData(rawBinormals);
            //texTangent.SetData(rawTangents);

           return null;
        }
    }
}