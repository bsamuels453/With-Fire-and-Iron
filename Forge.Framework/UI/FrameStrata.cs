#region

using System;
using System.Collections.Generic;

#endregion

namespace Forge.Framework.UI{
    /// <summary>
    /// This class handles frame strata calculation and recording.
    /// </summary>
    public class FrameStrata : IComparable<FrameStrata>{
        #region Level enum

        public enum Level{
            Background,
            BackgroundDetail,
            Border,
            Low,
            Medium,
            High,
            Highlight
        }

        #endregion

        /// <summary>
        /// The floating point equivalent of this frame's strata value.
        /// </summary>
        public readonly float FrameStrataValue;

        /// <summary>
        /// How deeply nested this frame is, as measured where the parent frame has a depth of 0.
        /// </summary>
        readonly int _frameNestingDepth;

        /// <summary>
        /// This is purely a convenience structure for debugging.
        /// </summary>
        readonly List<FrameStackData> _frameStack;

        /// <summary>
        /// This constructor is intended to be used for parent frames.
        /// </summary>
        public FrameStrata()
            : this(new List<FrameStackData>(), Level.Background, 0, 1, "Global Parent"){
        }

        /// <summary>
        /// Standard framestrata constructor.
        /// </summary>
        /// <param name="strata">The target strata for this frame.</param>
        /// <param name="parent">The parent frame for the framestrata being constructed.</param>
        /// <param name="alias">An internal alias used in debugging stratum stacks.</param>
        public FrameStrata(Level strata, FrameStrata parent, string alias = "Unnamed")
            : this(
                parent._frameStack,
                strata,
                parent._frameNestingDepth + 1,
                CalculateStrata(parent._frameNestingDepth, strata, parent.FrameStrataValue),
                alias){
        }

        FrameStrata(List<FrameStackData> parentFrameStack, Level strata, int frameNestingDepth, float strataValue, string frameAlias){
            _frameStack = parentFrameStack;
            _frameStack.Add(new FrameStackData(strata, frameAlias));
            _frameNestingDepth = frameNestingDepth;
            FrameStrataValue = strataValue;
        }

        #region IComparable<FrameStrata> Members

        public int CompareTo(FrameStrata other){
            if (other.FrameStrataValue == FrameStrataValue){
                return 0;
            }
            if (other.FrameStrataValue > FrameStrataValue){
                return -1;
            }
            return 1;
        }

        #endregion

        /// <summary>
        /// Calculates the floating point equivalent of this frame's strata.
        /// </summary>
        static float CalculateStrata(int parentNestingDepth, Level childStrata, float parentStrataValue){
            float magnitude = (float) Math.Pow(10, (parentNestingDepth + 1));
            //Calculated strata levels have + 1 added so that they're always at a higher level than their parent.
            float d = ((float) childStrata + 1)/magnitude;
            return parentStrataValue - d;
        }

        #region Nested type: FrameStackData

        struct FrameStackData{
            // ReSharper disable MemberCanBePrivate.Local
            public readonly string FrameAlias;
            public readonly Level FrameStrata;
            // ReSharper restore MemberCanBePrivate.Local

            public FrameStackData(Level frameStrata, string frameAlias) : this(){
                FrameStrata = frameStrata;
                FrameAlias = frameAlias;
            }
        }

        #endregion
    }
}