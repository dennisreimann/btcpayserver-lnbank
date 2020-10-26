namespace LNbank.Configuration
{
    public interface IAppOptions
    {
        string DatabaseConnectionString { get; set; }
        DatabaseType DatabaseType { get; set; }
        string RootPath { get; set; }
    }

    public enum DatabaseType
    {
        Sqlite,
        Postgres
    }
}
