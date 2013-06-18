#region

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using MonoGameUtility;
using ProtoBuf;

#endregion

namespace Forge.Framework{
    /// <summary>
    /// This utility class is used to define several procotol buffer wrappers for complex objects
    /// such as nested arrays and certain structures that can't have their attributes tagged.
    /// </summary>
    public class ProtoBuffWrappers{
        #region Nested type: BoundingBoxContainer

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

        #endregion

        #region Nested type: Vector3Container

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

        #endregion

        #region Nested type: VertexContainer

        [ProtoContract]
        public struct VertexContainer{
            [ProtoMember(1)] public VertexWrapper[] Vertexes;

            public VertexContainer(IList<VertexPositionNormalTexture> vertexes){
                Vertexes = new VertexWrapper[vertexes.Count()];
                for (int i = 0; i < Vertexes.Length; i++){
                    Vertexes[i] = vertexes[i];
                }
            }
        }

        #endregion

        #region Nested type: VertexWrapper

        [ProtoContract]
        public struct VertexWrapper{
            [ProtoMember(2)] public Vector3 Normal;
            [ProtoMember(1)] public Vector3 Position;
            [ProtoMember(3)] public Vector2 TextureCoordinate;

            public static implicit operator VertexWrapper(VertexPositionNormalTexture obj){
                var ret = new VertexWrapper();
                ret.Position = obj.Position;
                ret.Normal = obj.Normal;
                ret.TextureCoordinate = obj.TextureCoordinate;
                return ret;
            }

            public static implicit operator VertexPositionNormalTexture(VertexWrapper obj){
                var ret = new VertexPositionNormalTexture();
                ret.Position = obj.Position;
                ret.Normal = obj.Normal;
                ret.TextureCoordinate = obj.TextureCoordinate;
                return ret;
            }
        }

        #endregion
    }
}