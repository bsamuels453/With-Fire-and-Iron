#region

using System;

#endregion

namespace Forge.Core.ObjectEditor{
    /// <summary>
    ///this is a pretty shitty hack for a identifier class, but for the purposes of the object editor, it is fine.
    ///the object editor uses this to identify objects to be added/removed from the environment
    /// </summary>
    public class ObjectIdentifier : IEquatable<ObjectIdentifier>{
        static long _maxUid;

        readonly long _uid;

        static ObjectIdentifier(){
            _maxUid = 0;
        }

        public ObjectIdentifier(){
            _uid = _maxUid;
            _maxUid++;
        }

        #region IEquatable<ObjectIdentifier> Members

        public bool Equals(ObjectIdentifier other){
            if (_uid == other._uid)
                return true;
            return false;
        }

        #endregion
    }
}