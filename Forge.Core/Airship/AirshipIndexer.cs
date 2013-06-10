#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Forge.Core.Airship{
    /// <summary>
    ///   Container class used to index airships according to uid. This class is typically used by the autopilot/AI nav for identifying the locations of and communicating with other airships.
    /// </summary>
    public class AirshipIndexer : IEnumerable{
        readonly List<Airship> _airships;

        public AirshipIndexer(){
            _airships = new List<Airship>();
            OnAirshipAdded = null;
            OnAirshipRemoved = null;
        }

        public Airship this[int uid]{
            get{
                var ret = from ship in _airships
                    where ship.Uid == uid
                    select ship;
                return ret.Single();
            }
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator(){
            return _airships.GetEnumerator();
        }

        #endregion

        public void Add(Airship airship){
            _airships.Add(airship);
            if (OnAirshipAdded != null){
                OnAirshipAdded.Invoke(airship.Uid);
            }
        }

        public bool Remove(int uid){
            var airshipToRemove = from airship in _airships
                where airship.Uid == uid
                select airship;

            bool ret = _airships.Remove(airshipToRemove.Single());
            if (ret){
                if (OnAirshipRemoved != null){
                    OnAirshipRemoved.Invoke(uid);
                }
            }
            return ret;
        }

        /// <summary>
        ///   Called when an airship is added to the field. Passes the Uid of the airship added.
        /// </summary>
        public event Action<int> OnAirshipAdded;

        /// <summary>
        ///   Called when an airship is removed from the field. Passes the Uid of the airship removed.
        /// </summary>
        public event Action<int> OnAirshipRemoved;
    }
}