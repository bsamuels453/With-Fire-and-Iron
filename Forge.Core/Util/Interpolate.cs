#region

using System;
using System.Collections.Generic;
using System.Linq;
using MonoGameUtility;

#endregion

namespace Forge.Core.Util{
    //depreciated
    public class Interpolate{
        /// <summary>
        ///   the distance between the two points being interpolated. Only matters if value is going to be queried for distances outside the range of 0-1.
        /// </summary>
        float _dist;

        float _value1;
        float _value2;

        #region constructors and setters

        public Interpolate(){
            _value1 = 0;
            _value2 = 0;
            _dist = 1;
        }

        public Interpolate(float value1, float value2, float dist){
            _value1 = value1;
            _value2 = value2;
            _dist = dist;
        }

        public void SetBounds(float value1, float value2){
            _value1 = value1;
            _value2 = value2;
        }

        public void SetDistBetweenBounds(float dist){
            _dist = dist;
        }

        #endregion

        #region normal interpol functions

        /// <summary>
        ///   Linear interpol function. Nothing fancy.
        /// </summary>
        /// <param name="t"> </param>
        /// <returns> </returns>
        public float GetLinearValue(float t){
            float d = t/_dist;
            return _value1 + d*(_value2 - _value1);
        }

        /// <summary>
        ///   Interpol function that looks just like cosine but uses a fraction of the processing power(?)
        /// </summary>
        /// <param name="t"> </param>
        /// <returns> </returns>
        public float GetSmoothValue(float t){
            float d = t/_dist;
            return _value1 + ((float) Math.Pow(d, 2)*(3 - 2*d))*(_value2 - _value1);
        }

        /// <summary>
        ///   Exponential interpol function, starts slow as t=0 and dt increases as you approach t=_dist
        /// </summary>
        /// <param name="t"> </param>
        /// <returns> </returns>
        public float GetAccelerationValue(float t){
            float d = t/_dist;
            return _value1 + ((float) Math.Pow(d, 2))*(_value2 - _value1);
        }

        /// <summary>
        ///   Exponential interpol function, starts fast at t=0 and dt decreases as you approach t=_dist
        /// </summary>
        /// <param name="t"> </param>
        /// <returns> </returns>
        public float GetDecelerationValue(float t){
            float d = t/_dist;
            return _value1 + (1f - (float) Math.Pow(1f - d, 2))*(_value2 - _value1);
        }

        #endregion

        #region reverse interpol functions

        public float GetReverseLinearValue(float t){
            t = _dist - t;
            float d = t/_dist;
            return _value1 + d*(_value2 - _value1);
        }

        public float GetReverseSmoothValue(float t){
            t = _dist - t;
            float d = t/_dist;
            return _value1 + ((float) Math.Pow(d, 2)*(3 - 2*d))*(_value2 - _value1);
        }

        public float GetReverseAccelerationValue(float t){
            t = _dist - t;
            float d = t/_dist;
            return _value1 + ((float) Math.Pow(d, 2))*(_value2 - _value1);
        }

        public float GetReverseDecelerationValue(float t){
            t = _dist - t;
            float d = t/_dist;
            return _value1 + (1f - (float) Math.Pow(1f - d, 2))*(_value2 - _value1);
        }

        #endregion
    }

    public static class Lerp{
        public static Vector3 Lerp3(Vector3 start, Vector3 end, float t){
            var posVec = (end - start);
            var uVec = posVec;
            uVec.Normalize();
            float dist = posVec.Length()*t;
            return uVec*dist;
        }

        public static Vector3 Trace3X(Vector3 start, Vector3 end, float x){
            var uVec = (end - start);
            uVec.Normalize();

            float absX = x - start.X;
            float dist = absX/uVec.X;

            return uVec*dist + start;
        }

        public static Vector3 Trace3Y(Vector3 start, Vector3 end, float y){
            var uVec = (end - start);
            uVec.Normalize();

            float absY = y - start.Y;
            float dist = absY/uVec.Y;

            return uVec*dist + start;
        }

        //todo: this belongs in another class (UNTESTED)
        public static Vector3 Intersection(Vector3 p1, Vector3 p2, Vector3 v1, Vector3 v2, int resolution = 100){
            var vec1Step = (p2 - p1).Length()/resolution;
            var vec2Step = (v2 - v1).Length()/resolution;

            var uVec1 = (p2 - p1);
            uVec1.Normalize();
            var uVec2 = (v2 - v1);
            uVec2.Normalize();

            var distTable = new List<float>(resolution);
            for (int i = 0; i < resolution; i++){
                distTable.Add(Vector3.Distance(p1 + uVec1*(i*vec1Step), v1 + uVec2*(i*vec2Step)));
            }
            float min = distTable.Min();
            int stepIdx = distTable.IndexOf(min);
            return p1 + uVec1*stepIdx*vec1Step;
        }
    }
}