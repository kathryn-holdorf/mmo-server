using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mmo_server.Gamestate {
    /**
     * Holds references to all entities in a zone
     */
    public class Zone {
        public uint Id { get; private set; }
        private List<Character> _characters = new List<Character>();
        public ReadOnlyCollection<Character> Characters {
            get { return new ReadOnlyCollection<Character>(_characters); }
        }

        public Zone(uint id) {
            Id = id;
        }
        
        ///<summary>Try to add a character. Max number of characters in a zone is ushort.MaxValue.</summary>
        public bool AddCharacter(Character p) {
            if (_characters.Count == ushort.MaxValue) {
                return false;
            }
            _characters.Add(p);
            return true;
        }

        public void RemoveCharacter(Character c) {
            _characters.Remove(c);
        }
    }
}
