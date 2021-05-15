using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace OracleTest
{
    static class Program
    {
        static Task Main(string[] args)
        {
            var config = FileHelpers.ReadJson<AppConfig>("./app-config.json");

            return App.Execute(
                config.ConnectionString,
                config.DatabaseConnection
                );
        }

        class AppConfig
        {
            [JsonProperty("connectionString")]
            public string ConnectionString;
            [JsonProperty("databaseConnection")]
            public string DatabaseConnection;
        }

    }
}
