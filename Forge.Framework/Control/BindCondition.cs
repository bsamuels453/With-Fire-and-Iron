namespace Forge.Framework.Control{
    /// <summary>
    /// Defines the conditions under which a bind may execute.
    /// </summary>
    public enum BindCondition{
        /// <summary>
        /// Fire event when key's state changes.
        /// </summary>
        KeyChange,

        /// <summary>
        /// Fire event for every tick that the key is held down.
        /// </summary>
        KeyDown,

        /// <summary>
        /// Fire the event for every tick that the key is released.
        /// </summary>
        KeyUp,

        /// <summary>
        /// Fire the event once when the state changes from up to down.
        /// </summary>
        KeyChangeDown,

        /// <summary>
        /// Fire the event once when the state changes from down to up.
        /// </summary>
        KeyChangeUp
    }
}