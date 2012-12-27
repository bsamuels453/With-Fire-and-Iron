#region

using System;
using System.Collections.Generic;
using Cloo;
using Gondola.Logic;

#endregion

namespace Gondola.GameState.Terrain{
    internal class TerrainGen{

        readonly int _blockWidth;
        readonly int _chunkWidthInVerts;
        readonly ComputeCommandQueue _cmdQueue;

        readonly ComputeBuffer<float> _constants;
        readonly ComputeContext _context;
        readonly List<ComputeDevice> _devices;
        readonly ComputeBuffer<float> _geometry;
        readonly ComputeKernel _kernel;
        readonly ComputeProgram _program;
        readonly ComputeContextPropertyList _properties;

        readonly ComputeBuffer<byte> _normals;
        readonly ComputeBuffer<byte> _binormals;
        readonly ComputeBuffer<byte> _tangents;

        public TerrainGen(){
            _chunkWidthInVerts = Gbl.LoadContent<int>("TGen_ChunkWidthInBlocks") + 1;
            _blockWidth = Gbl.LoadContent<int>("TGen_BlockWidthInMeters");
            float lacunarity = Gbl.LoadContent<float>("TGen_Lacunarity");
            float gain = Gbl.LoadContent<float>("TGen_Gain");
            int octaves = Gbl.LoadContent<int>("TGen_Octaves");
            float offset = Gbl.LoadContent<float>("TGen_Offset");
            float hScale = Gbl.LoadContent<float>("TGen_HScale");
            float vScale = Gbl.LoadContent<float>("TGen_VScale");

            var platform = ComputePlatform.Platforms[0];
            _devices = new List<ComputeDevice>();
            _devices.Add(platform.Devices[0]);

            _properties = new ComputeContextPropertyList(platform);
            _context = new ComputeContext(_devices, _properties, null, IntPtr.Zero);

            _cmdQueue = new ComputeCommandQueue(_context, _devices[0], ComputeCommandQueueFlags.None);

            _constants = new ComputeBuffer<float>(_context, ComputeMemoryFlags.ReadOnly, 7);
            var constArr = new[]{
                lacunarity,
                gain,
                offset,
                octaves,
                hScale,
                vScale,
                _blockWidth
            };

            _cmdQueue.WriteToBuffer(constArr, _constants, false, null);

            _program = new ComputeProgram(_context, Gbl.LoadScript("TGen_GenScript"));
            _program.Build(null, null, null, IntPtr.Zero);
            _kernel = _program.CreateKernel("GenTerrain");

            _geometry = new ComputeBuffer<float>(_context, ComputeMemoryFlags.ReadWrite, _chunkWidthInVerts*_chunkWidthInVerts*3);
            _normals = new ComputeBuffer<byte>(_context, ComputeMemoryFlags.WriteOnly, _chunkWidthInVerts * _chunkWidthInVerts * 3);
            _binormals = new ComputeBuffer<byte>(_context, ComputeMemoryFlags.WriteOnly, _chunkWidthInVerts * _chunkWidthInVerts * 3);
            _tangents = new ComputeBuffer<byte>(_context, ComputeMemoryFlags.WriteOnly, _chunkWidthInVerts * _chunkWidthInVerts * 3);

            _kernel.SetMemoryArgument(0, _constants);
            _kernel.SetMemoryArgument(3, _geometry);
            _kernel.SetMemoryArgument(4, _normals);
            _kernel.SetMemoryArgument(5, _binormals);
            _kernel.SetMemoryArgument(6, _tangents);

            _cmdQueue.Finish();
        }

        public void Generate(int offsetX, int offsetZ){
            var result1 = new float[_chunkWidthInVerts*_chunkWidthInVerts * 3];
            var result2 = new byte[_chunkWidthInVerts * _chunkWidthInVerts * 3];
            var result3 = new byte[_chunkWidthInVerts * _chunkWidthInVerts * 3];
            var result4 = new byte[_chunkWidthInVerts * _chunkWidthInVerts * 3];

            _kernel.SetValueArgument(1, offsetX);
            _kernel.SetValueArgument(2, offsetZ);

            _cmdQueue.Execute(_kernel, null, new long[]{_chunkWidthInVerts, _chunkWidthInVerts}, null, null);
            _cmdQueue.ReadFromBuffer(_geometry, ref result1, true, null);
            _cmdQueue.ReadFromBuffer(_normals, ref result2, true, null);
            _cmdQueue.ReadFromBuffer(_binormals, ref result3, true, null);
            _cmdQueue.ReadFromBuffer(_tangents, ref result4, true, null);


            _cmdQueue.Finish();

            int f = 3;
        }
    }
}