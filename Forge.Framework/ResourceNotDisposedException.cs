#region

using System;

#endregion

namespace Forge.Framework{
    /// <summary>
    /// This exception is thrown when an object's finalizer is called, but the object has
    /// not been disposed of and inherits from IDisposable. While not really being a fatal exception,
    /// for the purposes of debugging and catching resource bugs, it exists.
    /// </summary>
    public class ResourceNotDisposedException : ApplicationException{
    }
}