
namespace DropBear.Codex.Caching.Configuration;

/// <summary>
/// Configuration options for FasterKV caching.
/// </summary>
public class FasterKVOptions
{
    public string CacheName { get; set; } = "faster_kv_cache";
    public bool Enabled { get; set; }
    

}