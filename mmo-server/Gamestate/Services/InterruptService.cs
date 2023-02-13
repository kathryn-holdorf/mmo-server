using mmo_server.Gamestate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmo_shared.Messages;

namespace mmo_server.Gamestate {
    class InterruptService {
        private readonly BroadcastService broadcastService;
        private readonly PlayerService playerService;
        private readonly MovementService movementService;

        public delegate void InterruptHandler(Character c);
        public event InterruptHandler AttackInterrupt = delegate { };
        public event InterruptHandler MovementInterrupt = delegate { };

        public InterruptService(BroadcastService broadcastService, PlayerService playerService, MovementService movementService) {
            this.broadcastService = broadcastService;
            this.playerService = playerService;
            this.movementService = movementService;
        }

        public void InterruptAttack(Character c) {
            AttackInterrupt(c);
            broadcastService.DistributeNearby(playerService.FindPlayer(c), new InterruptAttack(c.AccountId));
        }

        public void InterruptMovement(Character c) {
            MovementInterrupt(c);
            movementService.StopMoving(c);
        }
    }
}
