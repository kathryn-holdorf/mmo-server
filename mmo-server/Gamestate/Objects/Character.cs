using mmo_shared;

namespace mmo_server.Gamestate {
    public class Character : Positionable{
        public uint AccountId { get; protected set; }
        public string Name { get; set; }
        public byte ClassId { get; set; }
        public ushort Level { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Destination { get; set; }
        public float Velocity { get; set; } = 0f; //meters per second
        public uint ZoneId { get; set; }
        public byte SlotId { get; set; }
        public float MaxHealth { get; set; }
        public float CurrentHealth { get; set; }
        public float AttackRange { get; set; }
        public float AttackCooldown { get; set; }
        public bool Alive { get; set; }

        public Character(uint accountId, string name, byte classId, ushort level, Vector2 position, Vector2 destination,
            float velocity, uint zoneId, byte slotId, float maxHealth, float currentHealth, float attackRange, float attackCooldown,
            bool alive) {
            AccountId = accountId;
            Name = name;
            ClassId = classId;
            Level = level;
            Position = position;
            Destination = destination;
            Velocity = velocity;
            ZoneId = zoneId;
            SlotId = slotId;
            MaxHealth = maxHealth;
            CurrentHealth = currentHealth;
            AttackRange = attackRange;
            AttackCooldown = attackCooldown;
            Alive = alive;
        }
    }
}
