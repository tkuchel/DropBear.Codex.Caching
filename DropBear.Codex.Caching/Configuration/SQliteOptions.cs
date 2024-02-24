using Microsoft.Data.Sqlite;

namespace DropBear.Codex.Caching.Configuration;

/// <summary>
/// Configuration options for SQLite caching.
/// </summary>
public class SQLiteOptions
{
    public string CacheName { get; set; } = "sqlite_cache";
    public bool Enabled { get; set; } = false;
    public string FilePath { get; set; } = Directory.GetCurrentDirectory();
    public string FileName { get; set; } = "cache.db";

}