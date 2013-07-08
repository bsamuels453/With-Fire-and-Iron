//#define RECORD_KEYBOARD
#define READ_KEYBOARD_FROM_FILE

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Forge.Framework.Control{
    /// <summary>
    /// Handles the dispatching of keyboard state information to KeyboardControllers.
    /// </summary>
    public static class KeyboardManager{
        const int _numKeys = 255;
        static readonly Stack<KeyboardController> _cachedBindings;
        static readonly ForgeKeyState[] _keyState;
        static readonly Keys[] _keys;
        static KeyboardController _activeBinding;

        static KeyboardManager(){
            _cachedBindings = new Stack<KeyboardController>();
            _keys = (Keys[]) Enum.GetValues(typeof (Keys));
            _keyState = new ForgeKeyState[_numKeys];

#if RECORD_KEYBOARD
            _keyboardWriter = new StreamWriter("keyboard.dat");
            _keyboardWriter.AutoFlush = true;
#endif

#if READ_KEYBOARD_FROM_FILE
            _keyboardReader = new StreamReader("keyboard.dat");
#endif
        }

        /// <summary>
        /// Updates the keyboard state and dispatches the state to the current keyboard controller,
        /// which will invoke keyboard events, if applicable.
        /// </summary>
        public static void UpdateKeyboard(){
            var curState = Keyboard.GetState();

#if RECORD_KEYBOARD
            WriteMouseStateToRecord(curState);
#endif

#if READ_KEYBOARD_FROM_FILE
            curState = ReadMouseStateFromRecord();
#endif


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
        public static void SetActiveController(KeyboardController controller){
            if (_activeBinding != null){
                if (_activeBinding == controller){
                    return;
                }
                _cachedBindings.Push(_activeBinding);
            }
            _activeBinding = controller;
        }

        /// <summary>
        /// Releases the current controller and restores the previous controller, if it exists.
        /// </summary>
        /// <param name="controllerValidation">
        /// You can optionally pass what is supposed to be the currently active controller, in order to validate
        /// that the currently active controller is the one you intend to release. Ignoring this parameter means no
        /// check is performed that you're releasing the controller you think you are. This validation is done with
        /// a debug-only assert.
        /// </param>
        public static void ReleaseActiveController(KeyboardController controllerValidation = null){
            if (controllerValidation != null){
                Debug.Assert(_activeBinding == controllerValidation);
            }
            _activeBinding = null;
            if (_cachedBindings.Count > 0){
                _activeBinding = _cachedBindings.Pop();
            }
        }

        static StreamReader _keyboardReader;
        static StreamWriter _keyboardWriter;


        static void WriteMouseStateToRecord(KeyboardState state) {
            var pressedKeys = new List<Keys>();
            foreach (var key in _keys){
                if (state[key] == KeyState.Down){
                    pressedKeys.Add(key);
                }
            }

            for (int i = 0; i < pressedKeys.Count; i++){
                _keyboardWriter.Write(pressedKeys[i]);
                if (i != pressedKeys.Count - 1){
                    _keyboardWriter.Write(' ');
                }
            }
            _keyboardWriter.Write('\n');
        }

        static KeyboardState ReadMouseStateFromRecord() {
            var line = _keyboardReader.ReadLine();
            if (line.Length > 0) {
                var split = line.Split(' ');
                var pressedKeys = new List<Keys>(split.Length);
                foreach (var s in split) {
                    pressedKeys.Add((Keys)Enum.Parse(typeof(Keys), s));
                }
                var keyboardState = new KeyboardState(pressedKeys.ToArray());
                return keyboardState;
            }
                return new KeyboardState();
        }
    }
}