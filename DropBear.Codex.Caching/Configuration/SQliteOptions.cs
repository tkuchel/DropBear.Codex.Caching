namespace DropBear.Codex.Caching.Configuration;

public class SQLiteOptions
{
    public string ConnectionString { get; set; } // SQLite database connection string
    public string TableName { get; set; } = "CacheTable"; // Name of the table to store cache entries
}