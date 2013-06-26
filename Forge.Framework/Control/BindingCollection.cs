#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;

#endregion

namespace Forge.Framework.Control{
    internal class BindingCollection{
        #region Delegates

        public delegate void OnKeyPress(object caller, int bindAlias, KeyState keyState);

        #endregion

        readonly List<BindDefinition> _bindDefinitions;

        public BindingCollection(){
            _bindDefinitions = new List<BindDefinition>(50);
        }

        public void LoadFromFile<T>(string fileName){
            var strmrdr = new StreamReader(fileName);
            var contents = strmrdr.ReadToEnd();
            strmrdr.Close();
            var jObj = JObject.Parse(contents);

            var allBindings = (T[]) Enum.GetValues(typeof (T));

            foreach (var jtoken in jObj){
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


        public void CreateNewBind(Keys associatedKey, object bindAlias, OnKeyPress callback, Keys modifierKey = Keys.None){
            var doubles = from b in _bindDefinitions
                where b.BindAlias == bindAlias || (b.AssociatedKey == associatedKey && b.ModifierKey == modifierKey)
                select b;
            Debug.Assert(!doubles.Any());
            _bindDefinitions.Add
                (
                    new BindDefinition
                        (
                        bindAlias,
                        associatedKey,
                        callback,
                        modifierKey
                        )
                );
        }

        /// <summary>
        /// Adds the callback to a BindDefinition that was created from loading the bindings from
        /// a settings file.
        /// </summary>
        /// <param name="bindAlias"></param>
        /// <param name="callback"></param>
        public void AddBindCallback(object bindAlias, OnKeyPress callback){
            var bind = (
                from b in _bindDefinitions
                where b.BindAlias == bindAlias
                select b
                ).Single();
            Debug.Assert(bind.Callback == null);
            bind.Callback = callback;
        }

        #region Nested type: BindDefinition

        class BindDefinition{
            /// <summary>
            /// The key that this bind is associated with. This bind fires when this key changes.
            /// </summary>
            public readonly Keys AssociatedKey;

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
            /// delegate representing the method to call when the key associated with this
            /// binding is pressed or released
            /// </summary>
            public OnKeyPress Callback;

            public BindDefinition(object bindAlias, Keys associatedKey, OnKeyPress callback, Keys modifierKey){
                BindAlias = bindAlias;
                AssociatedKey = associatedKey;
                Callback = callback;
                ModifierKey = modifierKey;
            }

            public BindDefinition(object bindAlias, Keys associatedKey, Keys modifierKey){
                BindAlias = bindAlias;
                AssociatedKey = associatedKey;
                ModifierKey = modifierKey;
            }
        }

        #endregion
    }
}