using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;
using ProtoBuf;

namespace Forge.Framework {
    public class ProtoBuffWrappers{
        /// <summary>
        /// Contains references to all of the in-between structures used by the protocol buffer serializer/deserializer.
        /// </summary>
        /// 
        #region containers

        [ProtoContract]
        public struct BoundingBoxContainer{
            [ProtoMember(1)] public List<BoundingBox> BoundingBoxes;

            public BoundingBoxContainer(List<BoundingBox> boundingBoxes){
                BoundingBoxes = new List<BoundingBox>(boundingBoxes.Count);
                foreach (var boundingBox in boundingBoxes){
                    BoundingBoxes.Add(boundingBox);
                }
            }
        }

        [ProtoContract]
        public struct Vector3Container{
            [ProtoMember(1)] public List<Vector3> Vertexes;

            public Vector3Container(List<Vector3> vertexes){
                Vertexes = new List<Vector3>(vertexes.Count);
                foreach (var vertex in vertexes){
                    Vertexes.Add(vertex);
                }
            }
        }

        [ProtoContract]
        public struct VertexWrapper{
            [ProtoMember(1)] public Vector3 Position;
            [ProtoMember(2)] public Vector3 Normal;
            [ProtoMember(3)] public Vector2 TextureCoordinate;

            public static implicit operator VertexWrapper(VertexPositionNormalTexture obj){
                var ret = new VertexWrapper();
                ret.Position = obj.Position;
                ret.Normal = obj.Normal;
                ret.TextureCoordinate = obj.TextureCoordinate;
                return ret;
            }
        }

        #endregion

    }
}
