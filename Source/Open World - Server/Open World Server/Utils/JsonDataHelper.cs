using System.IO;
using System.Text.Json;

namespace OpenWorldServer.Utils
{
    public static class JsonDataHelper
    {
        public static void Save(object obj, string path)
            => File.WriteAllText(path, JsonSerializer.Serialize(obj, new JsonSerializerOptions() { WriteIndented = true }));

        public static T Load<T>(string path)
            => JsonSerializer.Deserialize<T>(File.ReadAllText(path));
    }
}
