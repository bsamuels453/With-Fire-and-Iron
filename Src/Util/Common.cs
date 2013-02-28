using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gondola.Util {
    internal static class Common {
        /// <summary>
        ///   gets the components of a vector
        /// </summary>
        /// <param name="angle"> </param>
        /// <param name="length"> </param>
        /// <returns> </returns>
        public static Vector2 GetComponentFromAngle(float angle, float length) { //LINEAR ALGEBRA STRIKES BACK
            var up = new Vector2(1, 0);
            Matrix rotMatrix = Matrix.CreateRotationZ(angle);
            Vector2 direction = Vector2.Transform(up, rotMatrix);
            return new Vector2(direction.X * length, direction.Y * length);
        }

        public static void GetAngleFromComponents(out float angle, out float magnitude, float x, float y) {
            angle = (float)Math.Atan2(y, x);
            magnitude = (float)Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        ///   gets angle of rotation around the origin, in radians
        /// </summary>
        /// <param name="x0"> </param>
        /// <param name="y0"> </param>
        /// <param name="dx"> the change in the X coordinate from the translation </param>
        /// <param name="dy"> the change in the Y coordinate from the translation </param>
        /// <returns> </returns>
        public static float GetAngleOfRotation(int x0, int y0, int dx, int dy) {
            var v0 = new Vector2(x0, y0);
            var v1 = new Vector2(v0.X + dx, v0.Y + dy);
            var angle = (float)Math.Acos(Vector2.Dot(v0, v1) / (v0.Length() * v1.Length()));
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
        public static float GetAngleOfRotation(float x0, float y0, float dx, float dy) {
            var v0 = new Vector2(x0, y0);
            var v1 = new Vector2(v0.X + dx, v0.Y + dy);
            var angle = (float)Math.Acos(Vector2.Dot(v0, v1) / (v0.Length() * v1.Length()));
            return angle;
        }

        /// <summary>
        /// </summary>
        /// <param name="x0"> </param>
        /// <param name="y0"> </param>
        /// <param name="x1"> </param>
        /// <param name="y1"> </param>
        /// <returns> </returns>
        public static float GetDist(int x0, int y0, int x1, int y1) {
            return (float)Math.Sqrt((x0 - x1) * (x0 - x1) + (y0 - y1) * (y0 - y1));
        }

        public static float GetDist(float x0, float y0, float x1, float y1) {
            return (float)Math.Sqrt((x0 - x1) * (x0 - x1) + (y0 - y1) * (y0 - y1));
        }

        public static List<Vector2> Bresenham(Vector2 p1, Vector2 p2) {
            var retList = new List<Vector2>();

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (p1.X == p2.X) {
                if (p1.Y > p2.Y) {
                    var temp = (int)p2.Y;
                    p2.Y = p1.Y;
                    p1.Y = temp;
                }
                while (p1.Y <= p2.Y) {
                    if (retList.Count > 0) {
                        if (retList[retList.Count - 1].X == p1.X && retList[retList.Count - 1].Y == p1.Y) {
                            p1.Y++;
                            continue;
                        }
                    }
                    retList.Add(new Vector2(p1.X, p1.Y++));
                }
            }
            else {
                if (p1.Y == p2.Y) {
                    if (p1.X > p2.X) {
                        var tmp = (int)p2.X;
                        p2.X = p1.X;
                        p1.X = tmp;
                    }
                    while (p1.X <= p2.X) {
                        if (retList.Count > 0) {
                            if (retList[retList.Count - 1].X == p1.X && retList[retList.Count - 1].Y == p1.Y) {
                                p1.X++;
                                continue;
                            }
                        }
                        retList.Add(new Vector2(p1.X++, p1.Y++));
                    }
                }
                else {
                    var x = (int)p1.X;
                    var y = (int)p1.Y;
                    int s1 = 1;
                    int s2 = 1;

                    var dx = (int)(p2.X - p1.X);
                    if (dx < 0) {
                        dx *= -1;
                        s1 = -1;
                    }

                    var dy = (int)(p2.Y - p1.Y);
                    if (dy < 0) {
                        dy *= -1;
                        s2 = -1;
                    }

                    bool xchange = false;

                    if (dy > dx) {
                        int tmp = dx;
                        dx = dy;
                        dy = tmp;
                        xchange = true;
                    }

                    int e = (dy << 1) - dx;
                    int j = 0;

                    while (j <= dx) {
                        j++;

                        if (retList.Count > 0) {
                            if (retList[retList.Count - 1].X != x || retList[retList.Count - 1].Y != y) {
                                retList.Add(new Vector2(x, y));
                            }
                        }
                        else {
                            retList.Add(new Vector2(x, y));
                        }
                        //path.push_back(pair_s(x,y)); //we want last coord too

                        if (e >= 0) {
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
    }
}
