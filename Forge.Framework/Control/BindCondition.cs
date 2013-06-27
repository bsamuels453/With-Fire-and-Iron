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
        KeyHeldDown,

        /// <summary>
        /// Fire the event for every tick that the key is released.
        /// </summary>
        KeyHeldUp,

        /// <summary>
        /// Fire the event once when the state changes from up to down.
        /// </summary>
        OnKeyDown,

        /// <summary>
        /// Fire the event once when the state changes from down to up.
        /// </summary>
        OnKeyUp,

        /// <summary>
        /// Fire every time the keyboard is updated, regardless of key state or whether the state has changed.
        /// </summary>
        Tick
    }
}