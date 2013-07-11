#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;

#endregion

namespace Forge.Framework.Control{
    /// <summary>
    /// This class is used to organize groups of keybindings. Binding groups can be loaded
    /// from a file using LoadFromFile, or specified manually using CreateNewBind.
    /// </summary>
    public class KeyboardController{
        #region Delegates

        public delegate void OnKeyPress(object caller, int bindAlias, ForgeKeyState keyState);

        #endregion

        readonly List<BindDefinition> _bindDefinitions;

        public KeyboardController(){
            _bindDefinitions = new List<BindDefinition>(50);
        }

        /// <summary>
        /// Loads a set of keybindings from a json file. After these bindings are loaded, the binds'
        /// callbacks must be set using the AddBindCallback method, or else they won't fire.
        /// </summary>
        /// <typeparam name="T">The type of the BindIdentifier enum used for this binding group.</typeparam>
        /// <param name="binds"> </param>
        public void LoadFromFile<T>(JObject binds){
            var allBindings = (T[]) Enum.GetValues(typeof (T));

            foreach (var jtoken in binds){
                //convert the key from a string to the binding identifier enum
                var bindIdentifier = (T) Enum.Parse(typeof (T), jtoken.Key);
                Debug.Assert(allBindings.Contains(bindIdentifier));

                //this next part gets a bit tricky because we have to parse out any modifier keys
                var bindStr = jtoken.Value.ToObject<string>();

                Keys bindKey;
                Keys bindModifier = Keys.None;

                if (bindStr.Contains('+')){
                    //contains modifier
                    int splitIdx = bindStr.IndexOf('+');
                    string modStr = bindStr.Substring(0, splitIdx);
                    string keyStr = bindStr.Substring(splitIdx + 1, bindStr.Count() - splitIdx - 1);

                    bindModifier = (Keys) Enum.Parse(typeof (Keys), modStr);
                    bindKey = (Keys) Enum.Parse(typeof (Keys), keyStr);
                }
                else{
                    bindKey = (Keys) Enum.Parse(typeof (Keys), bindStr);
                }

                _bindDefinitions.Add
                    (new BindDefinition
                        (
                        bindIdentifier,
                        bindKey,
                        bindModifier
                        )
                    );
            }
        }

        public void SaveToFile(string fileName){
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new bind.
        /// </summary>
        /// <param name="triggerKey"></param>
        /// <param name="bindAlias">The enum-based alias for this binding</param>
        /// <param name="callback"></param>
        /// <param name="condition">The condition the triggerKey must be under in order for the bind to fire.</param>
        /// <param name="modifierKey"></param>
        public void CreateNewBind(Keys triggerKey, object bindAlias, OnKeyPress callback, BindCondition condition, Keys modifierKey = Keys.None){
            var doubles = from b in _bindDefinitions
                where b.BindAlias == bindAlias || (b.TriggerKey == triggerKey && b.ModifierKey == modifierKey)
                select b;
            Debug.Assert(!doubles.Any());
            _bindDefinitions.Add
                (
                    new BindDefinition
                        (
                        bindAlias,
                        triggerKey,
                        callback,
                        modifierKey,
                        condition
                        )
                );
        }

        /// <summary>
        /// Adds the callback to a BindDefinition that was created from loading the bindings from
        /// a settings file.
        /// </summary>
        /// <param name="bindAlias">The enum-based alias for this binding.</param>
        /// <param name="fireCondition"> </param>
        /// <param name="callback"></param>
        public void AddBindCallback(object bindAlias, BindCondition fireCondition, OnKeyPress callback){
            var bind = (
                from b in _bindDefinitions
                where b.BindAlias.Equals(bindAlias)
                select b
                ).Single();
            Debug.Assert(bind.Callback == null);
            bind.Callback = callback;
            bind.FireCondition = fireCondition;
        }

        /// <summary>
        /// Invokes any bindings that satisfy their firing conditions.
        /// </summary>
        /// <param name="keyState"></param>
        public void InvokeKeyboardBinds(ForgeKeyState[] keyState){
            foreach (var bind in _bindDefinitions){
                bind.InvokeBind(keyState);
            }
        }

        /// <summary>
        /// Absorbs another keyboard controller's bindings.
        /// </summary>
        /// <param name="other"></param>
        public void AbsorbController(KeyboardController other){
            _bindDefinitions.AddRange(other._bindDefinitions);
        }

        #region Nested type: BindDefinition

        /// <summary>
        /// This class defines a binding. Since it's only used by the keyboard controller, it's nested.
        /// </summary>
        class BindDefinition{
            /// <summary>
            /// An object representing the name of the function associated with this bind.
            /// This is typically an enum that's used to identify binds so that a game component
            /// can tell this collection "Here's the callback for the MoveForward binding", where
            /// MoveForward is an enum that can be compared to this BindAlias value so that this
            /// class knows which binding to associate with the delegate passed through the Bind
            /// function.
            /// </summary>
            public readonly object BindAlias;

            /// <summary>
            /// The key that must be held down in conjunction with the associated key being pressed in
            /// order for this bind to fire. If there is no modifier, this will be Keys.None
            /// </summary>
            public readonly Keys ModifierKey;

            /// <summary>
            /// The key that this bind is associated with. This bind fires when this key changes.
            /// </summary>
            public readonly Keys TriggerKey;

            /// <summary>
            /// delegate representing the method to call when the key associated with this
            /// binding is pressed or released
            /// </summary>
            public OnKeyPress Callback;

            /// <summary>
            /// The condition which the TriggerKey must be under in order for the bind to fire.
            /// </summary>
            public BindCondition FireCondition;

            public BindDefinition(object bindAlias, Keys triggerKey, OnKeyPress callback, Keys modifierKey, BindCondition fireCondition){
                BindAlias = bindAlias;
                TriggerKey = triggerKey;
                Callback = callback;
                ModifierKey = modifierKey;
                FireCondition = fireCondition;
            }

            public BindDefinition(object bindAlias, Keys triggerKey, Keys modifierKey){
                BindAlias = bindAlias;
                TriggerKey = triggerKey;
                ModifierKey = modifierKey;
            }

            /// <summary>
            /// Tests whether or not this bind can be fired based off the current keyboard state, and fires
            /// the bind if the conditions are sufficient. For the conditions to be sufficient, the ModifierKey
            /// must be pressed if it was specified, and the TriggerKey must be in the condition specified by
            /// FireCondition.
            /// </summary>
            /// <param name="keyState"></param>
            public void InvokeBind(ForgeKeyState[] keyState){
                var modifierState = keyState[(int) ModifierKey];
                var triggerState = keyState[(int) TriggerKey];

                if (ModifierKey != Keys.None){
                    if (modifierState.State == KeyState.Up)
                        return;
                }
                if (triggerState.SatisfiesCondition(FireCondition)){
                    if (Callback != null){
                        Callback(this, (int) BindAlias, triggerState);
                    }
                }
            }
        }

        #endregion
    }
}