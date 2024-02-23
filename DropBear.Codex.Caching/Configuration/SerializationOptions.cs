using DropBear.Codex.Caching.Enums;

namespace DropBear.Codex.Caching.Configuration;

/// <summary>
/// Configuration options for serialization.
/// </summary>
public class SerializationOptions
{
    public bool Enabled { get; set; } = false;
    public SerializationFormat Format { get; set; } = SerializationFormat.None;
}