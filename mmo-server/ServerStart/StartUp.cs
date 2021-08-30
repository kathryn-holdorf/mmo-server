using mmo_server.Communication;
using mmo_server.Gamestate;
using mmo_server.Debug;
using mmo_server.MessageHandlers;
using mmo_server.Cryptography;
using mmo_server.Persistence;
using mmo_shared;
using mmo_server.ControlTower;
using mmo_server.SkillHandlers;

namespace mmo_server.ServerStart {
    class StartUp {
        static void Main(string[] args) {

            //initializing...

            //config
            Config config = new Config();

            //logger
            Debug.Debug.SetInstance(config);

            //cryptography services
            PacketEncryption encryption = new PacketEncryption();
            PasswordHashing passwordHashing = new PasswordHashing();

            //database
            Database db = new Database(config);

            //communication services
            ClientConnector clientConnector = new ClientConnector();
            PacketPublisher packetPublisher = new PacketPublisher();
            Serializer serializer = new Serializer();
            MessageSender messageSender = new MessageSender(clientConnector, serializer);
            PacketManager packetManager = new PacketManager(clientConnector, serializer, encryption, packetPublisher);

            //game loop
            GameLoop gameLoop = new GameLoop(packetPublisher, config);
            SkillPublisher skillPublisher = new SkillPublisher();

            //gamestate services
            PlayerService playerService = new PlayerService();
            ZoneService zoneService = new ZoneService(config, messageSender, playerService);
            BroadcastService broadcast = new BroadcastService(playerService, zoneService, messageSender, gameLoop);
            UnitVerificationService unitVerificationService = new UnitVerificationService(playerService);
            MovementService movementService = new MovementService(playerService, config, gameLoop, broadcast, zoneService, unitVerificationService);
            InterruptService interrupt = new InterruptService(broadcast, playerService, movementService);
            HealthService healthService = new HealthService(broadcast, zoneService, playerService, interrupt);
            CharacterLoginService characterLoginService = new CharacterLoginService(db, playerService, zoneService, messageSender, config, broadcast);
            SessionService sessionService = new SessionService(db, playerService, zoneService, passwordHashing, config, broadcast, characterLoginService);
            CharSelectService charSelectService = new CharSelectService(db, playerService, zoneService, messageSender, config, sessionService);
            CreateAccountService createAccountService = new CreateAccountService(db, passwordHashing, config);
            CharacterCreationService characterCreation = new CharacterCreationService(db, config);
            ChatService chatService = new ChatService(broadcast, zoneService);
            PlayerTimeoutService playerTimeout = new PlayerTimeoutService(packetPublisher, sessionService, gameLoop);
            AutoAttackService autoAttackService = new AutoAttackService(gameLoop, config, unitVerificationService, playerService, movementService,
                broadcast, zoneService, interrupt, healthService);
            RespawnService respawnService = new RespawnService(healthService, movementService, config, broadcast, playerService);
            CooldownService cooldownService = new CooldownService(gameLoop);
            CollisionService collisionService = new CollisionService(gameLoop, characterLoginService, config);
            ProjectileService projectileService = new ProjectileService(gameLoop, movementService, collisionService);

            //message handlers
            AliveHandler alive = new AliveHandler(packetPublisher, playerTimeout);
            AutoAttackHandler autoAttack = new AutoAttackHandler(packetPublisher, autoAttackService, playerService);
            ChatMessageHandler chatMessage = new ChatMessageHandler(packetPublisher, chatService, playerService);
            CreateAccountHandler createAccount = new CreateAccountHandler(packetPublisher, createAccountService, messageSender);
            CreateCharacterHandler createCharacter = new CreateCharacterHandler(packetPublisher, characterCreation, playerService, messageSender,
                charSelectService);
            LoginCharacterHandler loginCharacter = new LoginCharacterHandler(packetPublisher, characterLoginService, playerService, db, messageSender,
                zoneService);
            MoveCommandHandler moveCommand = new MoveCommandHandler(playerService, zoneService, broadcast, interrupt, packetPublisher, movementService,
                unitVerificationService);
            SessionHandler session = new SessionHandler(packetPublisher, sessionService, messageSender);
            RespawnHandler respawn = new RespawnHandler(packetPublisher, playerService, respawnService);
            SkillHandler skills = new SkillHandler(packetPublisher, playerService, unitVerificationService, skillPublisher, cooldownService);

            //skill handlers
            BlinkHandler blinkHandler = new BlinkHandler(skillPublisher, movementService, unitVerificationService, cooldownService, broadcast);
            PewHandler pewHandler = new PewHandler(skillPublisher, projectileService, collisionService, healthService, broadcast, movementService);

            //server start
            gameLoop.Start();
        }
    }
}
