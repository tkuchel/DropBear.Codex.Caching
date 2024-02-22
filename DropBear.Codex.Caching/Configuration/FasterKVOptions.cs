namespace DropBear.Codex.Caching.Configuration;

public class FasterKVOptions
{
    public string LogDirectory { get; set; } // Directory for FasterKV logs
    public long SizeLimitBytes { get; set; } = 10L * 1024 * 1024 * 1024; // 10GB default size limit for the store
}