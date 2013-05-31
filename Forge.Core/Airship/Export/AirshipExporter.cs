using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Forge.Core.Airship.Data;
using Forge.Core.Logic;
using Forge.Framework;
using Forge.Framework.Draw;
using Forge.Framework.MonoGame;
using Microsoft.Xna.Framework.Graphics;
using ProtoBuf;

namespace Forge.Core.Airship.Export {
    static class AirshipExporter {
        public static void Import(){
            AirshipExport airship;

            var sw = new Stopwatch();
            sw.Start();
            
            using (var file = File.Open("airship.bin", FileMode.Open)) {
                airship = Serializer.Deserialize<AirshipExport>(file);
            }

            sw.Stop();
            DebugConsole.WriteLine("Airship deserialized in " + sw.ElapsedMilliseconds + " ms");
        }

        public static void Export(string fileName, HullSectionContainer hull, DeckSectionContainer decks, ModelAttributes modelAttribs) {
            DebugConsole.WriteLine("Starting airship serialization...");
            var sw = new Stopwatch();
            sw.Start();

            //hull sections
            var sections = hull.HullSections;
            var hullSectionData = new List<HullSectionData>(sections.Length);
            foreach (var section in sections){
                hullSectionData.Add(new HullSectionData(section));
            }

            //hull buffers
            var hullBuffers = new List<BufferDataContainer<int>>(hull.NumDecks);
            foreach (var buffer in hull.HullBuffersByDeck) {
                var bufferDump = buffer.DumpObjectData();

                var array = new List<HullBufferObjectData<int>>(bufferDump.Length);
                foreach (var obj in bufferDump){
                    array.Add(new HullBufferObjectData<int>(obj));
                }
                hullBuffers.Add(new BufferDataContainer<int>(array));
            }

            //deck bounding boxes
            var boundingBoxes = new BoundingBoxContainer[decks.BoundingBoxesByDeck.Length];
            for (int i = 0; i < boundingBoxes.Length; i++){
                boundingBoxes[i] = new BoundingBoxContainer(decks.BoundingBoxesByDeck[i]);
            }

            //deck vertexes
            var deckVertexes = new DeckVertexContainer[decks.DeckVertexesByDeck.Length];
            for (int i = 0; i < deckVertexes.Length; i++) {
                deckVertexes[i] = new DeckVertexContainer(decks.DeckVertexesByDeck[i]);
            }

            //deck buffers
            var deckBuffers = new List<DeckBufferDataContainer>(hull.NumDecks);
            foreach (var buffer in decks.DeckBufferByDeck){
                var bufferDump = buffer.DumpObjectData();

                var array = new List<DeckBufferObjectData>(bufferDump.Length);
                foreach (var obj in bufferDump) {
                    array.Add(new DeckBufferObjectData(obj));
                }
                deckBuffers.Add(new DeckBufferDataContainer(array));
            }

            var airship = new AirshipExport();
            airship.HullBuffers = hullBuffers;
            airship.HullSections = hullSectionData;
            airship.DeckVertexes = deckVertexes;
            airship.BoundingBoxes = boundingBoxes;
            airship.DeckBuffers = deckBuffers;
            using (var file = File.Create("airship.bin")) {
                Serializer.Serialize(file, airship);
            }

            sw.Stop();
            DebugConsole.WriteLine("Airship serialization finished in " + sw.ElapsedMilliseconds + " ms");
        }

        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable NotAccessedField.Local

        [ProtoContract]
        struct AirshipExport{
            [ProtoMember(1)]
            public List<HullSectionData> HullSections;
            [ProtoMember(2)]
            public List<BufferDataContainer<int>> HullBuffers;
            [ProtoMember(3)]
            public DeckVertexContainer[] DeckVertexes;
            [ProtoMember(4)]
            public BoundingBoxContainer[] BoundingBoxes;
            [ProtoMember(5)]
            public List<DeckBufferDataContainer> DeckBuffers;

        }

        #region deck wrappers
        [ProtoContract]
        struct DeckVertexContainer{
            [ProtoMember(1)]
            public List<Vec3Wrap> Vertexes;

            public DeckVertexContainer(List<Vector3> vertexes){
                Vertexes = new List<Vec3Wrap>(vertexes.Count);
                foreach (var vertex in vertexes){
                    Vertexes.Add(vertex);
                }
            }
        }

        [ProtoContract]
        struct BoundingBoxContainer{
            [ProtoMember(1)]
            public List<BoundingBoxWrap> BoundingBoxes;

            public BoundingBoxContainer(List<BoundingBox> other){
                BoundingBoxes = new List<BoundingBoxWrap>(other.Count);
                foreach (var boundingBox in other){
                    BoundingBoxes.Add(new BoundingBoxWrap(boundingBox));
                }
            }
        }

        [ProtoContract]
        struct ObjectIdentifier{
            [ProtoMember(1)]
            public Vec3Wrap Position;
            [ProtoMember(2)]
            public ObjectType ObjectType;

            public ObjectIdentifier(AirshipObjectIdentifier other){
                Position = other.Position;
                ObjectType = other.ObjectType;
            }

        }

        #endregion

        #region buffer wrappers
        [ProtoContract]
        struct BufferDataContainer<T>{
            [ProtoMember(1)]
            public List<HullBufferObjectData<T>> HullBufferObjData; 

            public BufferDataContainer(List<HullBufferObjectData<T>> hullBufferObjData){
                HullBufferObjData = hullBufferObjData;
            }
        }

        [ProtoContract]
        struct DeckBufferDataContainer {
            [ProtoMember(1)]
            public List<DeckBufferObjectData> HullBufferObjData;

            public DeckBufferDataContainer(List<DeckBufferObjectData> hullBufferObjData) {
                HullBufferObjData = hullBufferObjData;
            }
        }


        [ProtoContract]
        struct HullBufferObjectData<T>{
            [ProtoMember(1)]
            public T Identifier;
            [ProtoMember(2)]
            public int[] Indicies;
            [ProtoMember(3)]
            public int ObjectOffset;
            [ProtoMember(4)]
            public VertexWrap[] Verticies;
            [ProtoMember(5)]
            public bool Enabled;

            public HullBufferObjectData(ObjectBuffer<int>.ObjectData objData){
                Identifier = (T)objData.Identifier;
                Indicies = objData.Indicies;
                ObjectOffset = objData.ObjectOffset;
                Verticies = new VertexWrap[objData.Verticies.Length];
                for (int i = 0; i < Verticies.Length; i++) {
                    Verticies[i] = objData.Verticies[i];
                }

                Enabled = objData.Enabled;
            }
        }

        [ProtoContract]
        struct DeckBufferObjectData {
            [ProtoMember(1)]
            public ObjectIdentifier Identifier;
            [ProtoMember(2)]
            public int[] Indicies;
            [ProtoMember(3)]
            public int ObjectOffset;
            [ProtoMember(4)]
            public VertexWrap[] Verticies;
            [ProtoMember(5)]
            public bool Enabled;

            public DeckBufferObjectData(ObjectBuffer<AirshipObjectIdentifier>.ObjectData objData) {
                Identifier = (new ObjectIdentifier((AirshipObjectIdentifier)objData.Identifier));
                Indicies = objData.Indicies;
                ObjectOffset = objData.ObjectOffset;
                Verticies = new VertexWrap[objData.Verticies.Length];
                for (int i = 0; i < Verticies.Length; i++) {
                    Verticies[i] = objData.Verticies[i];
                }

                Enabled = objData.Enabled;
            }
        }

        #endregion

        #region hull section wrappers
        [ProtoContract] 
        struct HullSectionData{
            [ProtoMember(1)]
            public int Uid;
            [ProtoMember(2)]
            public Vec3Wrap[] AliasedVertexes;
            [ProtoMember(3)]
            public int Deck;
            [ProtoMember(4)]
            public Quadrant.Side Side;
            [ProtoMember(6)]
            public int YPanel;
            //public ObjectBuffer<int> HullBuffer;

            public HullSectionData(HullSection section){
                Uid = section.Uid;
                AliasedVertexes = new Vec3Wrap[section.AliasedVertexes.Length];
                for (int i = 0; i < AliasedVertexes.Length; i++){
                    AliasedVertexes[i] = section.AliasedVertexes[i];
                }
                Deck = section.Deck;
                Side = section.Side;
                YPanel = section.YPanel;
            }
        }
        #endregion

        #region xna class wrappers
        [ProtoContract]
        struct Vec3Wrap{
            [ProtoMember(1)]
            public float X;
            [ProtoMember(2)]
            public float Y;
            [ProtoMember(3)]
            public float Z;

            public static implicit operator Vector3(Vec3Wrap obj) {
                return new Vector3(obj.X, obj.Y, obj.Z);
            }
            public static implicit operator Vec3Wrap(Vector3 obj) {
                var ret = new Vec3Wrap();
                ret.X = obj.X;
                ret.Y = obj.Y;
                ret.Z = obj.Z;
                return ret;
            }

            public Vec3Wrap(Vector3 vec){
                X = vec.X;
                Y = vec.Y;
                Z = vec.Z;
            }
        }

        [ProtoContract]
        struct Vec2Wrap {
            [ProtoMember(1)]
            public float X;
            [ProtoMember(2)]
            public float Y;

            public static implicit operator Vec2Wrap(Vector2 obj) {
                var ret = new Vec2Wrap();
                ret.X = obj.X;
                ret.Y = obj.Y;
                return ret;
            }


            public Vec2Wrap(Vector2 vec){
                X = vec.X;
                Y = vec.Y;
            }
        }

        [ProtoContract]
        struct VertexWrap{
            [ProtoMember(1)]
            public Vec3Wrap Position;
            [ProtoMember(2)]
            public Vec3Wrap Normal;
            [ProtoMember(3)]
            public Vec2Wrap TextureCoordinate;


            public static implicit operator VertexWrap(VertexPositionNormalTexture obj) {
                var ret = new VertexWrap();
                ret.Position = obj.Position;
                ret.Normal = obj.Normal;
                ret.TextureCoordinate = obj.TextureCoordinate;
                return ret;
            }
        }

        [ProtoContract]
        struct BoundingBoxWrap{
            [ProtoMember(1)]
            public Vec3Wrap Min;
            [ProtoMember(2)]
            public Vec3Wrap Max;

            public BoundingBoxWrap(BoundingBox other){
                Min = other.Min;
                Max = other.Max;
            }
        }

        #endregion

        // ReSharper restore NotAccessedField.Local
        // ReSharper restore MemberCanBePrivate.Local
    }
}