using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using mmo_server.Gamestate;
using mmo_shared;

namespace mmo_server.Persistence {
    class Database {
        private MySqlConnection db;
        private readonly string connectionString = "Persist Security Info = False; database = mmo; server = localhost; user id = root; Password = kylie";
        private readonly Config config;

        public Database(Config config) {
            db = new MySqlConnection(connectionString);
            this.config = config;
        }

        public bool GetCharacterIds(uint accountId, out List<uint> charIds) {
            charIds = new List<uint>();
            MySqlCommand cmd = new MySqlCommand {
                CommandText = "SELECT id FROM mmo.characters WHERE account_id = @accountId;"
            };
            cmd.Parameters.AddWithValue("@accountId", accountId);
            List<Dictionary<string, object>> result = RunQuery(cmd);
            foreach(var record in result) {
                charIds.Add((uint) record["id"]);
            }
            return true;
        }

        public bool GetCharacter(uint accountId, byte slot, out Character character) {
            character = null;
            MySqlCommand cmd = new MySqlCommand {
                CommandText = "SELECT * FROM mmo.characters WHERE account_id = @accountId AND slot = @slot;"
            };
            cmd.Parameters.AddWithValue("@accountId", accountId);
            cmd.Parameters.AddWithValue("@slot", slot);
            List<Dictionary<string, object>> result = RunQuery(cmd);
            if (result.Count == 0) {
                return false;
            }
            character = GetCharacterFromResult(result[0]);
            return true;
        }

        public bool GetCharacter(uint charId, out Character character) {
            character = null;
            MySqlCommand cmd = new MySqlCommand {
                CommandText = "SELECT * FROM mmo.characters WHERE id = @id;"
            };
            cmd.Parameters.AddWithValue("@id", charId);
            List<Dictionary<string, object>> result = RunQuery(cmd);
            if (result.Count == 0) {
                return false;
            }
            character = GetCharacterFromResult(result[0]);
            return true;
        }

        private Character GetCharacterFromResult(Dictionary<string, object> queryResult) {
            ushort posX16 = (ushort)queryResult["positionX"];
            ushort posY16 = (ushort)queryResult["positionY"];
            float posX = ((float)posX16) / 10;
            float posY = ((float)posY16) / 10;
            Vector2 pos = new Vector2(posX, posY);
            Character character = new Character(
                (uint)queryResult["account_id"],
                (string)queryResult["name"],
                (byte)queryResult["class"],
                (ushort)queryResult["level"],
                pos, pos, 0f,
                (uint)queryResult["zone_id"],
                (byte)queryResult["slot"],
                1000, 1000,
                config.characters.baseAttackRange,
                config.characters.baseAttackCooldown,
                true);
            return character;
        }

        public bool GetAccountId(string username, out uint accountId) {
            MySqlCommand cmd = new MySqlCommand {
                CommandText =
                "SELECT id FROM mmo.accounts " +
                "WHERE username = @username;"
            };
            cmd.Parameters.AddWithValue("@username", username);
            List<Dictionary<string, object>> result = RunQuery(cmd);
            if (result.Count == 0) {
                accountId = 0;
                return false;
            }
            accountId = (uint)result[0]["id"];
            return true;
        }

        public bool GetPassword(string username, out byte[] encryptedPassword) {
            MySqlCommand cmd = new MySqlCommand {
                CommandText =
                "SELECT password FROM mmo.accounts " +
                "WHERE username = @username;"
            };
            cmd.Parameters.AddWithValue("@username", username);
            List<Dictionary<string, object>> result = RunQuery(cmd);
            if (result.Count == 0) {
                encryptedPassword = null;
                return false;
            }
            encryptedPassword = (byte[])result[0]["password"];
            return true;
        }

        /// <summary>
        /// Save the zone and position for a character. Create character if it does not exist.
        /// </summary>
        public bool Save(Character c) {
            uint accountId = c.AccountId;
            string name = c.Name;
            uint zoneId = c.ZoneId;
            ushort posX = (ushort)Math.Round(c.Position.X*10);
            ushort posY = (ushort)Math.Round(c.Position.Y*10);
            byte classId = c.ClassId;
            ushort level = c.Level;
            byte slotId = c.SlotId;

            MySqlCommand command = new MySqlCommand {
                CommandText =
                "INSERT INTO mmo.characters (account_id, slot, name, zone_id, positionX, positionY, class, level)" +
                "VALUES ( @accountId, @slot, @name, @zoneId, @posX, @posY, @classId, @level)" +
                "ON DUPLICATE KEY UPDATE " +
                "slot = @slot," + 
                "zone_id = @zoneId," +
                "positionX = @posX," +
                "positionY = @posY," +
                "class = @classId," +
                "level = @level;"
            };
            command.Parameters.AddWithValue("@accountId", accountId);
            command.Parameters.AddWithValue("@slot", slotId);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@zoneId", zoneId);
            command.Parameters.AddWithValue("@posX", posX);
            command.Parameters.AddWithValue("@posY", posY);
            command.Parameters.AddWithValue("@classId", classId);
            command.Parameters.AddWithValue("@level", level);

            RunTransaction(command);
            return true;
        }

        /// <summary>
        /// Creates new user. Cancels if username already exists.
        /// </summary>
        public bool CreateUser(string username, byte[] password) {
            MySqlCommand cmd = new MySqlCommand {
                CommandText =
                "INSERT IGNORE INTO mmo.accounts (username, password)" +
                "VALUES (@name, @password);"
            };
            cmd.Parameters.AddWithValue("@name", username);
            cmd.Parameters.AddWithValue("@password", password);

            RunTransaction(cmd);

            return true;
        }

        private List<Dictionary<string, object>> RunQuery(MySqlCommand command) {
            MySqlDataReader reader = null;
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            db.Open();
            try {
                command.Connection = db;
                reader = command.ExecuteReader();
                DataTable table = reader.GetSchemaTable();
                if (!reader.HasRows) {
                    return result;
                }
                while (reader.Read()) {
                    Dictionary<string, object> record = new Dictionary<string, object>();
                    for (int column = 0; column < table.Rows.Count; column++) {
                        string columnName = table.Rows[column][table.Columns["ColumnName"]].ToString();
                        record.Add(columnName, reader.GetValue(column));
                    }
                    result.Add(record);
                }
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.Message);
            }
            finally {
                if (reader != null) {
                    reader.Close();
                }
                db.Close();
            }
            return result;
        }

        private void RunTransaction(MySqlCommand command) {
            RunTransaction(new MySqlCommand[]{command});
        }

        private void RunTransaction(MySqlCommand[] commands) {
            db.Open();
            MySqlTransaction t = db.BeginTransaction();
            try {
                foreach (MySqlCommand command in commands) {
                    command.Connection = db;
                    command.Transaction = t;
                    command.ExecuteNonQuery();
                }
                t.Commit();
            }
            catch (Exception e) {
                t.Rollback();
                Console.Error.WriteLine(e.Message);
            }
            finally {
                db.Close();
            }
        }

    }
}
