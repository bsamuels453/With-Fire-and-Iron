#region

using System.Collections.Generic;

#endregion

namespace Forge.Framework{
    /// <summary>
    /// This class is used for recording when a value shifts from one state to another.  This class is to be
    /// used when there's a stream of state data that has to be output, but we're only interested in displaying
    /// that state data when it changes.
    /// </summary>
    public static class DebugStateShift{
        static readonly Dictionary<string, bool> _setStates;

        static DebugStateShift(){
            _setStates = new Dictionary<string, bool>();
        }

        public static void AddNewSet(string identifier, bool initlState){
            _setStates.Add(identifier, initlState);
        }

        public static void UpdateSet(string identifier, bool state, string changeMsg){
            if (_setStates[identifier] != state){
                _setStates[identifier] = state;
                DebugConsole.WriteLine(changeMsg);
            }
        }
    }
}