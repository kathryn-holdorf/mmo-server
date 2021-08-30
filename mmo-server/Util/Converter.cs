using mmo_server.Gamestate;
using mmo_shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmo_shared.Messages;

namespace mmo_server {
    class Converter {
        public static CharInfo CreateCharInfo(Character c) {
            return new CharInfo(c.AccountId, c.Name, c.ClassId, c.Level,
                c.Position, c.Destination, c.Velocity, c.MaxHealth, c.CurrentHealth,
                c.AttackRange, c.AttackCooldown, c.Alive);
        }
    }
}
