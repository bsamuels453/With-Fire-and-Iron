using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Forge.Core.Airship {
    class HullSectionContainer :IEnumerable{

        readonly HullSection[] _hullSections;
        public HullSection[][] ByDeck;

        public HullSectionContainer(List<HullSection> hullSections){
            _hullSections = hullSections.ToArray();
            var groupedByDeck = (from section in hullSections
                                 group section by section.Deck).ToArray();

            ByDeck = new HullSection[groupedByDeck.Length][];
            foreach (var grouping in groupedByDeck){
                ByDeck[grouping.Key] = grouping.ToArray();
            }
            foreach (var deck in ByDeck){
                Debug.Assert(deck != null);
            }
        }


        public IEnumerator GetEnumerator(){
            return _hullSections.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }
    }
}
