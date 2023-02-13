﻿using mmo_server.ControlTower;
using mmo_shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mmo_server.Gamestate {
    class CooldownService {
        private readonly GameLoop gameLoop;

        private Dictionary<Character, Dictionary<Skill, float>> activeCooldowns = new Dictionary<Character, Dictionary<Skill, float>>();

        public CooldownService(GameLoop gameLoop) {
            this.gameLoop = gameLoop;
            gameLoop.Tick += Update;
        }

        public void StartCooldown(Skill skill, Character c) {
            if (!activeCooldowns.ContainsKey(c)) {
                activeCooldowns[c] = new Dictionary<Skill, float>();
            }
            activeCooldowns[c][skill] = skill.Cooldown;
        }

        public bool OnCooldown(Skill skill, Character c) {
            return activeCooldowns.ContainsKey(c) && activeCooldowns[c].ContainsKey(skill);
        }

        private void Update(float elapsedMs) {
            Dictionary<Character, List<Skill>> remove = new Dictionary<Character, List<Skill>>();

            foreach (Character c in activeCooldowns.Keys) {
                foreach(Skill skill in activeCooldowns[c].Keys.ToList()) {
                    activeCooldowns[c][skill] = Math.Max(0, activeCooldowns[c][skill] - elapsedMs / 1000);

                    if (activeCooldowns[c][skill] == 0) {
                        if (!remove.ContainsKey(c)) {
                            remove[c] = new List<Skill>();
                        }
                        remove[c].Add(skill);
                    }
                }
            }

            foreach(Character c in remove.Keys) {
                foreach(Skill skill in remove[c]) {
                    activeCooldowns[c].Remove(skill);
                }
                if (activeCooldowns[c].Count == 0) {
                    activeCooldowns.Remove(c);
                }
            }
        }
    }
}
