using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace Mirinae.Services.Database
{
    public class DatabaseService
    {
        private const string ConnectionString = "Data Source=database.db;";

        public DatabaseService()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    UserId TEXT PRIMARY KEY,
                    Level INTEGER DEFAULT 1,
                    Xp INTEGER DEFAULT 0,
                    Energy INTEGER DEFAULT 100,
                    LastLessonTime TEXT,
                    IsPremium INTEGER DEFAULT 0
                );
                CREATE TABLE IF NOT EXISTS Vocabulary (
                    UserId TEXT,
                    Word TEXT,
                    Translation TEXT
                );";
            command.ExecuteNonQuery();
        }

        public void RegenerateEnergy()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Users 
                SET Energy = MIN(100, Energy + 10) 
                WHERE Energy < 100;";
            command.ExecuteNonQuery();
        }

        public UserProfile GetOrCreateUser(ulong userId)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Level, Xp, Energy, LastLessonTime, IsPremium FROM Users WHERE UserId = @id";
            command.Parameters.AddWithValue("@id", userId.ToString());

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new UserProfile
                {
                    UserId = userId,
                    Level = reader.GetInt32(0),
                    Xp = reader.GetInt32(1),
                    Energy = reader.GetInt32(2),
                    LastLessonTime = DateTime.TryParse(reader.GetString(3), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dt) ? dt : DateTime.MinValue,
                    IsPremium = reader.GetInt32(4) == 1
                };
            }

            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT INTO Users (UserId, Level, Xp, Energy, LastLessonTime, IsPremium) 
                VALUES (@id, 1, 0, 100, @time, 0)";
            insertCommand.Parameters.AddWithValue("@id", userId.ToString());
            insertCommand.Parameters.AddWithValue("@time", DateTime.MinValue.ToString("o"));
            insertCommand.ExecuteNonQuery();

            return new UserProfile
            {
                UserId = userId,
                Level = 1,
                Xp = 0,
                Energy = 100,
                LastLessonTime = DateTime.MinValue,
                IsPremium = false
            };
        }

        public void UpdateEnergyAndLesson(ulong userId, int energy, DateTime lastLessonTime)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "UPDATE Users SET Energy = @energy, LastLessonTime = @time WHERE UserId = @id";
            command.Parameters.AddWithValue("@energy", energy);
            command.Parameters.AddWithValue("@time", lastLessonTime.ToString("o"));
            command.Parameters.AddWithValue("@id", userId.ToString());
            command.ExecuteNonQuery();
        }

        public void AddXp(ulong userId, int xpAmount, out bool leveledUp, out int newLevel)
        {
            leveledUp = false;
            var user = GetOrCreateUser(userId);
            int currentXp = user.Xp + xpAmount;
            int currentLevel = user.Level;
            int xpNeeded = currentLevel * 100;

            if (currentXp >= xpNeeded)
            {
                currentXp -= xpNeeded;
                currentLevel++;
                leveledUp = true;
            }
            else if (currentXp < 0)
            {
                currentXp = 0;
            }

            newLevel = currentLevel;

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "UPDATE Users SET Xp = @xp, Level = @lvl WHERE UserId = @id";
            command.Parameters.AddWithValue("@xp", currentXp);
            command.Parameters.AddWithValue("@lvl", currentLevel);
            command.Parameters.AddWithValue("@id", userId.ToString());
            command.ExecuteNonQuery();
        }

        public void SetPremiumStatus(ulong userId, bool isPremium)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "UPDATE Users SET IsPremium = @isPremium WHERE UserId = @id";
            command.Parameters.AddWithValue("@isPremium", isPremium ? 1 : 0);
            command.Parameters.AddWithValue("@id", userId.ToString());
            command.ExecuteNonQuery();
        }

        public void SaveWord(ulong userId, string word, string translation)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Vocabulary (UserId, Word, Translation) VALUES (@id, @word, @trans)";
            command.Parameters.AddWithValue("@id", userId.ToString());
            command.Parameters.AddWithValue("@word", word);
            command.Parameters.AddWithValue("@trans", translation);
            command.ExecuteNonQuery();
        }

        public List<(string Word, string Trans)> GetVocabulary(ulong userId)
        {
            var list = new List<(string Word, string Trans)>();
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Word, Translation FROM Vocabulary WHERE UserId = @id ORDER BY ROWID DESC LIMIT 15";
            command.Parameters.AddWithValue("@id", userId.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add((reader.GetString(0), reader.GetString(1)));
            }
            return list;
        }
    }
}