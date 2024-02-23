namespace DropBear.Codex.Caching.Configuration;

/// <summary>
/// Configuration options for in-memory caching.
/// </summary>
public class InMemoryOptions
{
    public string CacheName { get; set; } = "mem_cache";
    public bool Enabled { get; set; } = false;
    public int ExpirationScanFrequency { get; set; } = 60;
    public int? SizeLimit { get; set; }
    public bool EnableReadDeepClone { get; set; } = false;
    public bool EnableWriteDeepClone { get; set; } = false;
    public int MaxRdSecond { get; set; } = 0;
    public bool EnableLogging { get; set; } = false;
    public int LockMs { get; set; } = 10;
    public int SleepMs { get; set; } = 300;
}