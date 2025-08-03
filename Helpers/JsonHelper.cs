using System.Text.Json;

namespace AzureAIServicesDemo.Helpers
{
    public class JsonHelper
    {
        public static JsonElement AddOrChangeValueInJsonElement<T>(JsonElement element, string key, T newValue)
        {
            // Convert JsonElement to dictionary.
            Dictionary<string, JsonElement> dictionary = ObjectFromJsonElement<Dictionary<string, JsonElement>>(element);
            // Convert newValue to JsonElement.
            using (JsonDocument doc = JsonDocument.Parse(JsonSerializer.Serialize(newValue)))
            {
                // Add/update key in dictionary to newValue.
                // NOTE: Always clone the root element of the JsonDocument. Otherwise, once it is disposed,
                // when you try to access any data you obtained from it, you get the error:
                // "Cannot access a disposed object.
                // Object name: 'JsonDocument'."
                // See:
                // https://docs.microsoft.com/dotnet/api/system.text.json.jsonelement.clone
                dictionary[key] = doc.RootElement.Clone();
                // Convert dictionary back to JsonElement.
                using (JsonDocument doc_2 = JsonDocument.Parse(JsonSerializer.Serialize(dictionary)))
                {
                    return doc_2.RootElement.Clone();
                }
            }
        }

        private static T ObjectFromJsonElement<T>(JsonElement element)
        {
            var json = element.GetRawText();
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
