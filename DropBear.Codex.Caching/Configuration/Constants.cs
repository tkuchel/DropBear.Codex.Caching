namespace DropBear.Codex.Caching.Configuration;

public static class Constants
{
    public static string JsonSerializerName { get; } = "json";
    public static string MessagePackSerializerName { get; } = "message_pack";
    public static string MemoryPackSerializerName { get; } = "memory_pack";

    public static string BrotliCompressionName { get; } = "brotli";
    public static string Lz4CompressionName { get; } = "lz4";
    
    // public static string SnappyCompressionName { get; } = "snappy";
    // public static string GZipCompressionName { get; } = "gzip";
    // public static string DeflateCompressionName { get; } = "deflate";
}