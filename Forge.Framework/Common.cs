#region

using System;
using System.Collections.Generic;
using MonoGameUtility;

#endregion

namespace Forge.Framework{
    public static class Common{
        public const float DegreesPerRadian = 0.0174532925f;

        /// <summary>
        ///   gets the components of a vector
        /// </summary>
        /// <param name="angle"> </param>
        /// <param name="length"> </param>
        /// <returns> </returns>
        public static Vector2 GetComponentFromAngle(float angle, float length){ //LINEAR ALGEBRA STRIKES BACK
            var up = new Vector2(1, 0);
            Matrix rotMatrix = Matrix.CreateRotationZ(angle);
            Vector2 direction = Vector2.Transform(up, rotMatrix);
            return new Vector2(direction.X*length, direction.Y*length);
        }

        public static void GetAngleFromComponents(out float angle, out float magnitude, float x, float y){
            angle = (float) Math.Atan2(y, x);
            magnitude = (float) Math.Sqrt(x*x + y*y);
        }

        /// <summary>
        ///   gets angle of rotation around the origin, in radians
        /// </summary>
        /// <param name="x0"> </param>
        /// <param name="y0"> </param>
        /// <param name="dx"> the change in the X coordinate from the translation </param>
        /// <param name="dy"> the change in the Y coordinate from the translation </param>
        /// <returns> </returns>
        public static float GetAngleOfRotation(int x0, int y0, int dx, int dy){
            var v0 = new Vector2(x0, y0);
            var v1 = new Vector2(v0.X + dx, v0.Y + dy);
            var angle = (float) Math.Acos(Vector2.Dot(v0, v1)/(v0.Length()*v1.Length()));
            return angle;
        }

        /// <summary>
        ///   gets angle of rotation around the origin, in radians
        /// </summary>
        /// <param name="x0"> </param>
        /// <param name="y0"> </param>
        /// <param name="dx"> the change in the X coordinate from the translation </param>
        /// <param name="dy"> the change in the Y coordinate from the translation </param>
        /// <returns> </returns>
        public static float GetAngleOfRotation(float x0, float y0, float dx, float dy){
            var v0 = new Vector2(x0, y0);
            var v1 = new Vector2(v0.X + dx, v0.Y + dy);
            var angle = (float) Math.Acos(Vector2.Dot(v0, v1)/(v0.Length()*v1.Length()));
            return angle;
        }

        /// <summary>
        /// </summary>
        /// <param name="x0"> </param>
        /// <param name="y0"> </param>
        /// <param name="x1"> </param>
        /// <param name="y1"> </param>
        /// <returns> </returns>
        public static float GetDist(int x0, int y0, int x1, int y1){
            return (float) Math.Sqrt((x0 - x1)*(x0 - x1) + (y0 - y1)*(y0 - y1));
        }

        public static float GetDist(float x0, float y0, float x1, float y1){
            return (float) Math.Sqrt((x0 - x1)*(x0 - x1) + (y0 - y1)*(y0 - y1));
        }

        public static List<Vector2> Bresenham(Vector2 p1, Vector2 p2){
            var retList = new List<Vector2>();

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (p1.X == p2.X){
                if (p1.Y > p2.Y){
                    var temp = (int) p2.Y;
                    p2.Y = p1.Y;
                    p1.Y = temp;
                }
                while (p1.Y <= p2.Y){
                    if (retList.Count > 0){
                        if (retList[retList.Count - 1].X == p1.X && retList[retList.Count - 1].Y == p1.Y){
                            p1.Y++;
                            continue;
                        }
                    }
                    retList.Add(new Vector2(p1.X, p1.Y++));
                }
            }
            else{
                if (p1.Y == p2.Y){
                    if (p1.X > p2.X){
                        var tmp = (int) p2.X;
                        p2.X = p1.X;
                        p1.X = tmp;
                    }
                    while (p1.X <= p2.X){
                        if (retList.Count > 0){
                            if (retList[retList.Count - 1].X == p1.X && retList[retList.Count - 1].Y == p1.Y){
                                p1.X++;
                                continue;
                            }
                        }
                        retList.Add(new Vector2(p1.X++, p1.Y++));
                    }
                }
                else{
                    var x = (int) p1.X;
                    var y = (int) p1.Y;
                    int s1 = 1;
                    int s2 = 1;

                    var dx = (int) (p2.X - p1.X);
                    if (dx < 0){
                        dx *= -1;
                        s1 = -1;
                    }

                    var dy = (int) (p2.Y - p1.Y);
                    if (dy < 0){
                        dy *= -1;
                        s2 = -1;
                    }

                    bool xchange = false;

                    if (dy > dx){
                        int tmp = dx;
                        dx = dy;
                        dy = tmp;
                        xchange = true;
                    }

                    int e = (dy << 1) - dx;
                    int j = 0;

                    while (j <= dx){
                        j++;

                        if (retList.Count > 0){
                            if (retList[retList.Count - 1].X != x || retList[retList.Count - 1].Y != y){
                                retList.Add(new Vector2(x, y));
                            }
                        }
                        else{
                            retList.Add(new Vector2(x, y));
                        }
                        //path.push_back(pair_s(x,y)); //we want last coord too

                        if (e >= 0){
                            if (xchange)
                                x += s1;
                            else
                                y += s2;
                            e -= (dx << 1);
                        }
                        if (xchange)
                            y += s2;
                        else
                            x += s1;
                        e += (dy << 1);
                        // ReSharper restore CompareOfFloatsByEqualityOperator
                    }
                }
            }
            return retList;
        }

        public static Matrix GetWorldTranslation(Vector3 position, Vector3 angle, float objLength){
            var worldMatrix = Matrix.Identity;
            worldMatrix *= Matrix.CreateTranslation(objLength/2, 0, 0);
            worldMatrix *= Matrix.CreateRotationX(angle.X)*Matrix.CreateRotationY(angle.Y)*Matrix.CreateRotationZ(angle.Z);
            worldMatrix *= Matrix.CreateTranslation(position.X, position.Y, position.Z);
            return worldMatrix;
        }

        /// <summary>
        ///   multiplies a 4x4 and 1x3 matrix together for use in projection calcs
        /// </summary>
        /// <param name="m"> </param>
        /// <param name="v"> </param>
        /// <returns> </returns>
        public static Vector3 MultMatrix(Matrix m, Vector3 v){
            return new Vector3
                (
                v.X*m.M11 + v.Y*m.M21 + v.Z*m.M31 + m.M41,
                v.X*m.M12 + v.Y*m.M22 + v.Z*m.M32 + m.M42,
                v.X*m.M13 + v.Y*m.M23 + v.Z*m.M33 + m.M43
                );
        }

        /// <summary>
        ///   these references are to prevent CLR from calling Vector3/Ray's copy constructor
        /// </summary>
        /// <param name="ray"> </param>
        /// <param name="vertex1"> </param>
        /// <param name="vertex2"> </param>
        /// <param name="vertex3"> </param>
        /// <param name="result"> this is the only reference type that actually has reference operations performed on it </param>
        public static void RayIntersectsTriangle(
            ref Ray ray,
            ref Vector3 vertex1,
            ref Vector3 vertex2,
            ref Vector3 vertex3, out float? result){
            // Compute vectors along two edges of the triangle.
            Vector3 edge1, edge2;

            Vector3.Subtract(ref vertex2, ref vertex1, out edge1);
            Vector3.Subtract(ref vertex3, ref vertex1, out edge2);

            // Compute the determinant.
            Vector3 directionCrossEdge2;
            Vector3.Cross(ref ray.Direction, ref edge2, out directionCrossEdge2);

            float determinant;
            Vector3.Dot(ref edge1, ref directionCrossEdge2, out determinant);

            // If the ray is parallel to the triangle plane, there is no collision.
            if (determinant > -float.Epsilon && determinant < float.Epsilon){
                result = null;
                return;
            }

            float inverseDeterminant = 1.0f/determinant;

            // Calculate the U parameter of the intersection point.
            Vector3 distanceVector;
            Vector3.Subtract(ref ray.Position, ref vertex1, out distanceVector);

            float triangleU;
            Vector3.Dot(ref distanceVector, ref directionCrossEdge2, out triangleU);
            triangleU *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleU < 0 || triangleU > 1){
                result = null;
                return;
            }

            // Calculate the V parameter of the intersection point.
            Vector3 distanceCrossEdge1;
            Vector3.Cross(ref distanceVector, ref edge1, out distanceCrossEdge1);

            float triangleV;
            Vector3.Dot(ref ray.Direction, ref distanceCrossEdge1, out triangleV);
            triangleV *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleV < 0 || triangleU + triangleV > 1){
                result = null;
                return;
            }

            // Compute the distance along the ray to the triangle.
            float rayDistance;
            Vector3.Dot(ref edge2, ref distanceCrossEdge1, out rayDistance);
            rayDistance *= inverseDeterminant;

            // Is the triangle behind the ray origin?
            if (rayDistance < 0){
                result = null;
                return;
            }

            result = rayDistance;
        }
    }
}