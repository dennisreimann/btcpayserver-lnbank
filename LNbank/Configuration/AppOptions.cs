using Microsoft.Extensions.Configuration;

namespace LNbank.Configuration
{
    public class AppOptions
    {
        public const string EnvVarPrefix = "LNBANK_";

        public string DatabaseConnectionString { get; set; }
        public DatabaseType DatabaseType { get; set; }
        public string RootPath { get; set; }

        public AppOptions Configure(IConfiguration configuration)
        {
            DatabaseConnectionString = configuration.GetValue<string>("Database");
            DatabaseType = configuration.GetValue("DatabaseType", DatabaseType.Sqlite);
            RootPath = configuration.GetValue("RootPath", "");
            return this;
        }
    }
    

    public enum DatabaseType
    {
        Sqlite,
        Postgres
    }
}
