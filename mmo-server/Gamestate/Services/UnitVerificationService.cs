using mmo_server.Gamestate;
using mmo_shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmo_shared.Messages;

namespace mmo_server.Gamestate {
    class UnitVerificationService {
        private readonly PlayerService playerService;

        public UnitVerificationService(PlayerService playerService) {
            this.playerService = playerService;
        }

        public bool LoggedIn(Character c) {
            Player p = playerService.FindPlayer(c);
            return (p != null && p.CurrentCharacter == c);
        }

        public bool CanAttack(Character c) {
            return LoggedIn(c) && c.Alive;
        }

        public bool CanMove(Character c) {
            return LoggedIn(c) && c.Alive;
        }

        public bool CanUseSkills(Character c) {
            return LoggedIn(c) && c.Alive;
        }

    }
}
