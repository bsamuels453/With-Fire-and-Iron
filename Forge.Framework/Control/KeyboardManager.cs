#region

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Forge.Framework.Control{
    /// <summary>
    /// Handles the dispatching of keyboard state information to KeyboardControllers.
    /// </summary>
    public class KeyboardManager{
        const int _numKeys = 255;
        readonly Stack<KeyboardController> _cachedBindings;
        readonly ForgeKeyState[] _keyState;
        readonly Keys[] _keys;
        KeyboardController _activeBinding;

        public KeyboardManager(){
            _cachedBindings = new Stack<KeyboardController>();
            _keys = (Keys[]) Enum.GetValues(typeof (Keys));
            _keyState = new ForgeKeyState[_numKeys];
        }

        /// <summary>
        /// Updates the keyboard state and dispatches the state to the current keyboard controller,
        /// which will invoke keyboard events, if applicable.
        /// </summary>
        public void UpdateKeyboard(){
            var curState = Keyboard.GetState();

            foreach (var key in _keys){
                var curKeyState = curState[key];
                _keyState[(int) key].ApplyNewState(curKeyState);
            }

            if (_activeBinding != null){
                _activeBinding.InvokeKeyboardBinds(_keyState);
            }
        }

        /// <summary>
        /// Sets the provided controller as the active controller, and caches the current
        /// active controller if it exists.
        /// </summary>
        /// <param name="controller"></param>
        public void SetActiveController(KeyboardController controller){
            if (_activeBinding != null){
                _cachedBindings.Push(_activeBinding);
            }
            _activeBinding = controller;
        }

        /// <summary>
        /// Releases the current controller and restores the previous controller, if it exists.
        /// </summary>
        public void ReleaseActiveController(){
            _activeBinding = null;
            if (_cachedBindings.Count > 0){
                _activeBinding = _cachedBindings.Pop();
            }
        }
    }
}