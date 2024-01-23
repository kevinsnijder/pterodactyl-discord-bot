using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DiscordBot
{
   public class Messages
   {
      private static readonly Dictionary<string, string> messages;

      static Messages()
      {
         messages = LoadMessagesFromJson();
      }

      private static Dictionary<string, string> LoadMessagesFromJson()
      {
         string json = File.ReadAllText("messages.json");
         var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
         var messageData = JsonSerializer.Deserialize<MessageData>(json, options);

         var loadedMessages = new Dictionary<string, string>();
         foreach (var message in messageData.Messages)
         {
            foreach (var kvp in message)
            {
               loadedMessages[kvp.Key] = kvp.Value;
            }
         }

         return loadedMessages;
      }

      public static string Get(string key)
      {
         if (messages.ContainsKey(key))
         {
            return messages[key];
         }

         return null;
      }
   }

   public class MessageData
   {
      public Dictionary<string, string>[] Messages { get; set; }
   }
}