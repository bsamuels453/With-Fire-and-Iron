#region

using System;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Forge.Framework.Control{
    public struct ForgeKeyState{
        /// <summary>
        /// The current state of the key.
        /// </summary>
        public KeyState State;

        /// <summary>
        /// Whether or not the keystate has changed since the last poll.
        /// </summary>
        public bool StateChanged;

        public void ApplyNewState(KeyState state){
            if (State != state){
                StateChanged = true;
                State = state;
            }
            else{
                StateChanged = false;
                State = state;
            }
        }

        /// <summary>
        /// Determines whether or not the current key state satisfies the provided bind condition.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public bool SatisfiesCondition(BindCondition condition){
            switch (condition){
                case BindCondition.OnKeyUp:
                    if (State == KeyState.Up && StateChanged)
                        return true;
                    return false;

                case BindCondition.OnKeyDown:
                    if (State == KeyState.Down && StateChanged)
                        return true;
                    return false;

                case BindCondition.KeyHeldUp:
                    if (State == KeyState.Up)
                        return true;
                    return false;

                case BindCondition.KeyHeldDown:
                    if (State == KeyState.Down)
                        return true;
                    return false;

                case BindCondition.KeyChange:
                    return StateChanged;

                case BindCondition.Tick:
                    return true;

                default:
                    throw new Exception();
            }
        }
    }
}