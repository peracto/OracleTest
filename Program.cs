using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Bourne.BatchLoader
{
    // ReSharper disable once ArrangeTypeModifiers
    static class Program
    {
        // ReSharper disable once ArrangeTypeMemberModifiers
        static Task Main(string[] args)
        {
            var config = FileHelpers.ReadJson<AppConfig>("./app-config.json");

            return App.Execute(
                config.ConnectionString,
                config.DatabaseConnection
            );
        }

        private class AppConfig
        {
            [JsonProperty("connectionString")]
            public string ConnectionString;
            [JsonProperty("databaseConnection")]
            public string DatabaseConnection;
        }

    }
}
