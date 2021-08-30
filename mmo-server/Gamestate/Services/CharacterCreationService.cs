using mmo_server.Communication;
using mmo_server.Gamestate;
using mmo_server.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using mmo_server.Persistence;
using mmo_shared;
using mmo_shared.Messages;

namespace mmo_server.Gamestate {
    class CharacterCreationService {
        private Database db;
        private Config config;

        public CharacterCreationService(Database db, Config config) {
            this.db = db;
            this.config = config;
        }

        /// <summary>
        /// Create a new character, if the character creation parameters are valid.
        /// </summary>
        public CreateCharacterResponse.Types CreateCharacter(Player player, CreateCharacter create, out Character newCharacter) {
            CreateCharacterResponse.Types response = CreateCharacterResponse.Types.Invalid;
            newCharacter = null;

            if (create.Name.Length < config.accounts.MinCharactersForUsername
                      || create.Name.Length > config.accounts.MaxCharactersForUsername) {
                return CreateCharacterResponse.Types.NameInvalid;
            }

            Vector2 spawnPoint = config.characters.StartingPosition;
            Character character = new Character(
                player.AccountId,
                create.Name,
                11, 1,
                spawnPoint, spawnPoint,
                0f,
                config.characters.StartingZone,
                create.Slot,
                1000, 1000,
                config.characters.baseAttackRange,
                config.characters.baseAttackCooldown,
                true);

            if (db.Save(character)) {
                newCharacter = character;
                response = CreateCharacterResponse.Types.Success;
            }

            return response;
        }
    }
}
