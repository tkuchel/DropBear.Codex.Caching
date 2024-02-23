using FASTER.core;

namespace DropBear.Codex.Caching.Configuration;

/// <summary>
/// Configuration options for FasterKV caching.
/// </summary>
public class FasterKVOptions
{
    public string CacheName { get; set; } = "faster_kv_cache";
    public bool Enabled { get; set; }
    public long IndexCount { get; set; }
    public int MemorySizeBit { get; set; }
    public int PageSizeBit { get; set; }
    public int ReadCacheMemorySizeBit { get; set; }
    public int ReadCachePageSizeBit { get; set; }
    public string LogPath { get; set; } = Directory.GetCurrentDirectory();
    public FasterKV<SpanByte, SpanByte>? CustomStore { get; set; }
}