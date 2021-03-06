﻿#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Cloo;
using Forge.Core.Util;
using Forge.Framework;
using Forge.Framework.Resources;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;

#endregion

namespace Forge.Core.Terrain{
    public class TerrainGen : IDisposable{
        readonly ComputeBuffer<byte> _activeVerts;
        readonly ComputeBuffer<byte> _binormals;
        readonly int _blockWidth;
        readonly int _chunkWidthInBlocks;
        readonly int _chunkWidthInVerts;
        readonly ComputeCommandQueue _cmdQueue;
        readonly ComputeContext _context;
        readonly ComputeKernel _crossCullKernel;
        readonly ComputeBuffer<int> _dummy;
        readonly int[] _emptyIndices;
        readonly byte[] _emptyVerts;
        readonly ComputeBuffer<float> _genConstants;

        readonly ComputeProgram _generationPrgm;
        readonly ComputeBuffer<float> _geometry;
        readonly ComputeBuffer<int> _indicies;
        readonly ComputeKernel _normalGenKernel;
        readonly ComputeBuffer<ushort> _normals;
        readonly ComputeKernel _qTreeKernel;
        readonly ComputeProgram _qTreePrgm;
        readonly ComputeBuffer<byte> _tangents;
        readonly ComputeKernel _terrainGenKernel;
        readonly ComputeBuffer<float> _uvCoords;
        readonly ComputeKernel _winderKernel;
        readonly ComputeProgram _winderPrgm;
        bool _disposed;

        public TerrainGen(){
            _context = Resource.CLContext;
            _cmdQueue = Resource.CLQueue;

            #region setup generator kernel

            var jobj = Resource.LoadConfig("Config/TerrainGenerator.config");

            _chunkWidthInBlocks = jobj["ChunkWidthInBlocks"].ToObject<int>();
            _chunkWidthInVerts = _chunkWidthInBlocks + 1;
            _blockWidth = jobj["BlockWidthInMeters"].ToObject<int>();
            float lacunarity = jobj["Lacunarity"].ToObject<float>();
            float gain = jobj["Gain"].ToObject<float>();
            int octaves = jobj["Octaves"].ToObject<int>();
            float offset = jobj["Offset"].ToObject<float>();
            float hScale = jobj["HScale"].ToObject<float>();
            float vScale = jobj["VScale"].ToObject<float>();

            _genConstants = new ComputeBuffer<float>(_context, ComputeMemoryFlags.ReadOnly, 8);
            var genArr = new[]{
                lacunarity,
                gain,
                offset,
                octaves,
                hScale,
                vScale,
                _blockWidth,
                _chunkWidthInBlocks
            };

            _cmdQueue.WriteToBuffer(genArr, _genConstants, false, null);
            _generationPrgm = Resource.LoadCLScript("Scripts/Opencl/GenTerrain.cl");

            _terrainGenKernel = _generationPrgm.CreateKernel("GenTerrain");
            _normalGenKernel = _generationPrgm.CreateKernel("GenNormals");

            //despite the script using float3 for these fields, we need to consider it to be float4 because the 
            //implementation is basically a float4 wrapper that uses zero for the last variable
            _geometry = new ComputeBuffer<float>(_context, ComputeMemoryFlags.None, _chunkWidthInVerts*_chunkWidthInVerts*4);
            _normals = new ComputeBuffer<ushort>(_context, ComputeMemoryFlags.None, _chunkWidthInVerts*_chunkWidthInVerts*4);
            _binormals = new ComputeBuffer<byte>(_context, ComputeMemoryFlags.None, _chunkWidthInVerts*_chunkWidthInVerts*4);
            _tangents = new ComputeBuffer<byte>(_context, ComputeMemoryFlags.None, _chunkWidthInVerts*_chunkWidthInVerts*4);
            _uvCoords = new ComputeBuffer<float>(_context, ComputeMemoryFlags.None, _chunkWidthInVerts*_chunkWidthInVerts*2);

            _terrainGenKernel.SetMemoryArgument(0, _genConstants);
            _terrainGenKernel.SetMemoryArgument(3, _geometry);
            _terrainGenKernel.SetMemoryArgument(4, _uvCoords);

            _normalGenKernel.SetMemoryArgument(0, _genConstants);
            _normalGenKernel.SetMemoryArgument(3, _geometry);
            _normalGenKernel.SetMemoryArgument(4, _normals);
            _normalGenKernel.SetMemoryArgument(5, _binormals);
            _normalGenKernel.SetMemoryArgument(6, _tangents);

            #endregion

            #region setup quadtree kernel

            _qTreePrgm = Resource.LoadCLScript("Scripts/Opencl/Quadtree.cl");

            _qTreeKernel = _qTreePrgm.CreateKernel("QuadTree");
            _crossCullKernel = _qTreePrgm.CreateKernel("CrossCull");

            _activeVerts = new ComputeBuffer<byte>(_context, ComputeMemoryFlags.None, _chunkWidthInVerts*_chunkWidthInVerts);

            _dummy = new ComputeBuffer<int>(_context, ComputeMemoryFlags.None, 50);
            var rawNormals = new ushort[_chunkWidthInVerts*_chunkWidthInVerts*4];
            _emptyVerts = new byte[_chunkWidthInVerts*_chunkWidthInVerts];
            for (int i = 0; i < _emptyVerts.Length; i++){
                _emptyVerts[i] = 1;
            }
            _cmdQueue.WriteToBuffer(rawNormals, _normals, true, null);
            _cmdQueue.WriteToBuffer(_emptyVerts, _activeVerts, true, null);

            _qTreeKernel.SetValueArgument(1, _chunkWidthInBlocks);
            _qTreeKernel.SetMemoryArgument(2, _normals);
            _qTreeKernel.SetMemoryArgument(3, _activeVerts);
            _qTreeKernel.SetMemoryArgument(4, _dummy);

            _crossCullKernel.SetValueArgument(1, _chunkWidthInBlocks);
            _crossCullKernel.SetMemoryArgument(2, _normals);
            _crossCullKernel.SetMemoryArgument(3, _activeVerts);
            _crossCullKernel.SetMemoryArgument(4, _dummy);

            #endregion

            #region setup winding kernel

            _winderPrgm = Resource.LoadCLScript("Scripts/Opencl/VertexWinder.cl");

            _winderKernel = _winderPrgm.CreateKernel("VertexWinder");
            _indicies = new ComputeBuffer<int>(_context, ComputeMemoryFlags.None, (_chunkWidthInBlocks)*(_chunkWidthInBlocks)*8);

            _winderKernel.SetMemoryArgument(0, _activeVerts);
            _winderKernel.SetMemoryArgument(1, _indicies);

            _emptyIndices = new int[(_chunkWidthInBlocks)*(_chunkWidthInBlocks)*8];
            for (int i = 0; i < (_chunkWidthInBlocks)*(_chunkWidthInBlocks)*8; i++){
                _emptyIndices[i] = 0;
            }
            _cmdQueue.WriteToBuffer(_emptyIndices, _indicies, true, null);

            #endregion

            _cmdQueue.Finish();
        }

        #region IDisposable Members

        public void Dispose(){
            Debug.Assert(!_disposed);
            _activeVerts.Dispose();
            _binormals.Dispose();
            _crossCullKernel.Dispose();
            _dummy.Dispose();
            _genConstants.Dispose();
            _geometry.Dispose();
            _normals.Dispose();
            _normalGenKernel.Dispose();
            _indicies.Dispose();
            _qTreeKernel.Dispose();
            _tangents.Dispose();
            _terrainGenKernel.Dispose();
            _uvCoords.Dispose();
            _winderKernel.Dispose();
            _disposed = true;
        }

        #endregion

        public TerrainChunk GenerateChunk(XZPoint id) {
            int offsetX = id.X*_blockWidth*(_chunkWidthInBlocks);
            int offsetZ = id.Z*_blockWidth*(_chunkWidthInBlocks);

            var rawGeometry = new float[_chunkWidthInVerts*_chunkWidthInVerts*4];
            var rawNormals = new ushort[_chunkWidthInVerts*_chunkWidthInVerts*4];
            var rawBinormals = new byte[_chunkWidthInVerts*_chunkWidthInVerts*4];
            var rawTangents = new byte[_chunkWidthInVerts*_chunkWidthInVerts*4];
            var rawUVCoords = new float[_chunkWidthInVerts*_chunkWidthInVerts*2];
            var indicies = new int[(_chunkWidthInBlocks)*(_chunkWidthInBlocks)*8];
            var activeVerts = new byte[_chunkWidthInVerts*_chunkWidthInVerts];

            _terrainGenKernel.SetValueArgument(1, offsetX);
            _terrainGenKernel.SetValueArgument(2, offsetZ);
            _normalGenKernel.SetValueArgument(1, offsetX);
            _normalGenKernel.SetValueArgument(2, offsetZ);

            _cmdQueue.WriteToBuffer(_emptyVerts, _activeVerts, true, null);
            _cmdQueue.WriteToBuffer(_emptyIndices, _indicies, true, null);
            _cmdQueue.Execute(_terrainGenKernel, null, new long[]{_chunkWidthInVerts, _chunkWidthInVerts}, null, null);
            _cmdQueue.Execute(_normalGenKernel, null, new long[]{_chunkWidthInVerts, _chunkWidthInVerts}, null, null);

            for (int depth = 0; depth < 5; depth++){
                _qTreeKernel.SetValueArgument(0, depth);
                _crossCullKernel.SetValueArgument(0, depth);
                int cellWidth = (int) Math.Pow(2, depth)*2;
                int qTreeWidth = _chunkWidthInBlocks/(cellWidth);
                _cmdQueue.Execute(_qTreeKernel, null, new long[]{(qTreeWidth) - 1, (qTreeWidth*2)}, null, null);
                _cmdQueue.Execute(_crossCullKernel, null, new long[]{_chunkWidthInBlocks/cellWidth, _chunkWidthInBlocks/cellWidth}, null, null);
            }
            _cmdQueue.Execute(_winderKernel, null, new long[]{(_chunkWidthInBlocks), (_chunkWidthInBlocks*2)}, null, null);

            _cmdQueue.ReadFromBuffer(_geometry, ref rawGeometry, true, null);
            _cmdQueue.ReadFromBuffer(_normals, ref rawNormals, true, null);
            _cmdQueue.ReadFromBuffer(_binormals, ref rawBinormals, true, null);
            _cmdQueue.ReadFromBuffer(_tangents, ref rawTangents, true, null);
            _cmdQueue.ReadFromBuffer(_uvCoords, ref rawUVCoords, true, null);
            _cmdQueue.ReadFromBuffer(_indicies, ref indicies, true, null);
            _cmdQueue.ReadFromBuffer(_activeVerts, ref activeVerts, true, null);

            _cmdQueue.Finish();

            for (int v = 3; v < rawNormals.Length; v += 4){
                rawNormals[v] = 1;
            }
            for (int v = 3; v < rawBinormals.Length; v += 4){
                rawBinormals[v] = 1;
            }
            for (int v = 3; v < rawTangents.Length; v += 4){
                rawTangents[v] = 1;
            }


            var texNormal = new Texture2D(Resource.Device, _chunkWidthInVerts, _chunkWidthInVerts, false, SurfaceFormat.Rgba64);
            var texBinormal = new Texture2D(Resource.Device, _chunkWidthInVerts, _chunkWidthInVerts, false, SurfaceFormat.Color);
            var texTangent = new Texture2D(Resource.Device, _chunkWidthInVerts, _chunkWidthInVerts, false, SurfaceFormat.Color);

            texNormal.SetData(rawNormals);
            texBinormal.SetData(rawBinormals);
            texTangent.SetData(rawTangents);

            //var maxHeight = verts.Aggregate((agg, next) => next.Y > agg.Y ? next : agg).Y;
            //var minHeight = verts.Aggregate((agg, next) => next.Y < agg.Y ? next : agg).Y;

            //these functions take the raw buffer data from opencl and remove stride and empty fields
            var parsedUV = ParseUV(rawUVCoords);
            var parsedGeometry = ParseGeometry(rawGeometry);
            var parsedIndicies = ParseIndicies(indicies);

            var sw = new Stopwatch();
            sw.Start();

            int[] culledIndexes;
            VertexPositionTexture[] culledVertexes;

            CullVertexes(activeVerts, parsedIndicies.ToList(), parsedGeometry, parsedUV, out culledIndexes, out culledVertexes);
            sw.Stop();
            double elapsed = sw.ElapsedMilliseconds;
            var chunkData = new TerrainChunk(id, culledVertexes, culledIndexes, texNormal, texBinormal, texTangent);

            return chunkData;
        }

        Vector2[] ParseUV(float[] rawVertexes){
            int destIdx = 0;
            var outVertexes = new Vector2[rawVertexes.Length/2];
            for (
                int srcIdx = 0;
                srcIdx < rawVertexes.Length;
                srcIdx
                    += 2){
                outVertexes[destIdx] = new Vector2
                    (
                    rawVertexes[srcIdx],
                    rawVertexes[srcIdx + 1]);

                destIdx++;
            }
            return outVertexes;
        }

        Vector3[] ParseGeometry(float[] rawVertexes){
            int destIdx = 0;
            var outVertexes = new Vector3[rawVertexes.Length/4];
            for (
                int srcIdx = 0;
                srcIdx < rawVertexes.Length;
                srcIdx
                    += 4){
                outVertexes[destIdx] = new Vector3
                    (
                    rawVertexes[srcIdx],
                    rawVertexes[srcIdx + 1],
                    rawVertexes[srcIdx + 2]);

                destIdx++;
            }
            return
                outVertexes;
        }

        /*
        //IM HAVING FUN AND THERES NOTHING YOU CAN DO TO STOP ME
        T[] ParseStridedData<T, S>(S[] src, int stride, int numDummyElems){
            Type outType = typeof (T);
            int destIdx = 0;
            var dataOut = new T[src.Length/stride];
            var ctorTypes = new Type[stride - numDummyElems];
            for (int i = 0; i < stride - numDummyElems; i++)
                ctorTypes[i] = typeof(S);

            var ctor = outType.GetConstructor(ctorTypes);
            for (int srcIdx = 0; srcIdx < src.Length; srcIdx += stride){
                object[] ctorArgs = new object[stride - numDummyElems];
                for (int i = 0; i < stride - numDummyElems; i++){
                    ctorArgs[i] = src[srcIdx + i];
                }
                dataOut[destIdx] = (T) ctor.Invoke(ctorArgs);
                destIdx++;
            }
            return dataOut;
        }
        */

        IEnumerable<int> ParseIndicies(int[] indicies){
            var outIndicies = new List<int>();

            for (int i = 0; i < indicies.Length; i += 4){
                if (indicies[i] == 0 && indicies[i + 1] == 0 && indicies[i + 2] == 0){
                    continue;
                }
                outIndicies.Add(indicies[i]);
                outIndicies.Add(indicies[i + 1]);
                outIndicies.Add(indicies[i + 2]);
            }

            return outIndicies;
        }

        void CullVertexes(
            byte[] activeNodes,
            List<int> indicies,
            Vector3[] geometry,
            Vector2[] uvCoords,
            out int[] reparsedIndicies,
            out VertexPositionTexture[] oVertexes){
            //first get a list of numeric indicies that will be used by the index buffer
            //there will be missing indexes because of the culling

            var indicieMap = new Dictionary<int, int>(indicies.Count);
            for (int i = 0; i < activeNodes.Length; i++){
                if (activeNodes[i] == 1){
                    indicieMap.Add(i, indicieMap.Count);
                }
            }

            var vertexList = new List<VertexPositionTexture>();
            reparsedIndicies = new int[indicies.Count];
            for (int i = 0; i < indicies.Count; i++){
                reparsedIndicies[i] = indicieMap[indicies[i]];
            }

            foreach (var pair in indicieMap){
                vertexList.Add(new VertexPositionTexture(geometry[pair.Key], uvCoords[pair.Key]));
            }
            oVertexes = vertexList.ToArray();
        }

        void DumpArray(byte[] array, int width){
            var sw = new StreamWriter("dump.txt");
            for (int x = 0; x < width; x++){
                for (int z = 0; z < width; z++){
                    sw.Write(array[x + z*width] + " ");
                }
                sw.Write('\n');
            }
            sw.Close();
        }

        ~TerrainGen(){
            if (!_disposed){
                throw new ResourceNotDisposedException();
            }
        }
    }
}