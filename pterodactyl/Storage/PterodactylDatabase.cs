using Microsoft.Data.Sqlite;
using pterodactyl.DataObjects;

namespace pterodactyl.Storage
{
   public class PterodactylDatabase
   {
      private static readonly PterodactylDatabase instance = new PterodactylDatabase();
      private readonly string _connectionString = @"Data Source=/home/container/database.db";

      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static PterodactylDatabase()
      {
      }

      private PterodactylDatabase()
      {
         var path = "/home/container";
         if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
         if (!File.Exists(path + "/database.db"))
         {
            Console.WriteLine("Creating DB");
            CreateDatabase();
         }
      }

      public static PterodactylDatabase Instance
      {
         get
         {
            return instance;
         }
      }

      public void CreateDatabase()
      {

         // Create the table
         using (var connection = new SqliteConnection(_connectionString))
         {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
               command.CommandText = @"CREATE TABLE IF NOT EXISTS Users (
                                                ID INTEGER PRIMARY KEY,
                                                DiscordUserID INTEGER,
                                                PterodactylApiToken TEXT)";
               command.ExecuteNonQuery();
            }
         }
      }

      public int InsertUser(UserDto user)
      {
         // Insert records into the table
         using (var connection = new SqliteConnection(_connectionString))
         {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
               // Insert a record
               command.CommandText = @"INSERT INTO Users (DiscordUserID, PterodactylApiToken)
                                       VALUES (@discordUserID, @pterodactylApiToken)";
               command.Parameters.AddWithValue("@discordUserID", user.DiscordID);
               command.Parameters.AddWithValue("@pterodactylApiToken", user.PterodactylApiKey);

               return command.ExecuteNonQuery();
            }
         }
      }

      public UserDto? FindUser(int id)
      {
         using (var connection = new SqliteConnection(_connectionString))
         {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
               command.CommandText = "SELECT ID, DiscordUserID, PterodactylApiToken FROM Users WHERE ID = @id";
               command.Parameters.AddWithValue("@id", id);

               using (var reader = command.ExecuteReader())
               {
                  while (reader.Read())
                  {
                     var record = new UserDto
                     {
                        Id = reader.GetInt32(0),
                        DiscordID = reader.GetInt64(1),
                        PterodactylApiKey = reader.GetString(2)
                     };

                     return record;
                  }
                  return null;
               }
            }
         }
      }

      public UserDto? FindUser(long discordUserID)
      {
         using (var connection = new SqliteConnection(_connectionString))
         {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
               command.CommandText = "SELECT ID, DiscordUserID, PterodactylApiToken FROM Users WHERE DiscordUserID = @discordUserID";
               command.Parameters.AddWithValue("@discordUserID", discordUserID);

               using (var reader = command.ExecuteReader())
               {
                  while (reader.Read())
                  {
                     var record = new UserDto
                     {
                        Id = reader.GetInt32(0),
                        DiscordID = reader.GetInt64(1),
                        PterodactylApiKey = reader.GetString(2)
                     };

                     return record;
                  }
                  return null;
               }
            }
         }
      }

      public int UpdateUser(UserDto user)
      {
         // Insert records into the table
         using (var connection = new SqliteConnection(_connectionString))
         {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
               // Insert a record
               command.CommandText = @"UPDATE Users
                                       SET PterodactylApiToken = @pterodactylApiToken
                                       WHERE DiscordUserID = @discordUserID";
               command.Parameters.AddWithValue("@pterodactylApiToken", user.PterodactylApiKey);
               command.Parameters.AddWithValue("@discordUserID", user.DiscordID);

               return command.ExecuteNonQuery();
            }
         }
      }

      public IEnumerable<UserDto> GetUsers()
      {
         var records = new List<UserDto>();

         using (var connection = new SqliteConnection(_connectionString))
         {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
               command.CommandText = "SELECT ID, DiscordUserID, PterodactylApiToken FROM Users";

               using (var reader = command.ExecuteReader())
               {
                  while (reader.Read())
                  {
                     var record = new UserDto
                     {
                        Id = reader.GetInt32(0),
                        DiscordID = reader.GetInt64(1),
                        PterodactylApiKey = reader.GetString(2)
                     };

                     records.Add(record);
                  }
               }
            }
         }

         return records;
      }

      public int DeleteUser(long discordUserID)
      {
         // Insert records into the table
         using (var connection = new SqliteConnection(_connectionString))
         {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
               // Insert a record
               command.CommandText = @"DELETE FROM Users
                                       WHERE DiscordUserID = @discordUserID";
               command.Parameters.AddWithValue("@discordUserID", (ulong) discordUserID);

               return command.ExecuteNonQuery();
            }
         }
      }
   }
}