#region

using Microsoft.Xna.Framework.Input;

#endregion

namespace Forge.Framework.Control{
    internal struct ForgeKeyState{
        /// <summary>
        /// The current state of the key.
        /// </summary>
        public KeyState State;

        /// <summary>
        /// Whether or not the keystate has changed since the last poll.
        /// </summary>
        public bool StateChanged;
    }
}