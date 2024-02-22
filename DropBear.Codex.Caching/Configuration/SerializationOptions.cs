namespace DropBear.Codex.Caching.Configuration;

public class SerializationOptions
{
    public string DefaultSerializer { get; set; } =
        "Json"; // Default serializer: "Json", "MessagePack", or "MemoryPack"
    // Possible extension point for custom serializer settings
}