namespace DropBear.Codex.Caching.Configuration;

/// <summary>
/// Configuration options for in-memory caching.
/// </summary>
public class InMemoryOptions
{
    public string CacheName { get; set; } = "mem_cache";
    public bool Enabled { get; set; } = false;
    public int ExpirationScanFrequency { get; set; } = 60;
    public int? SizeLimit { get; set; } = 1024;

}