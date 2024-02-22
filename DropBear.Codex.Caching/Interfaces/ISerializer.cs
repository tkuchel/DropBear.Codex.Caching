namespace DropBear.Codex.Caching.Interfaces;

public interface ISerializer
{
    string Serialize<T>(T obj);
    T Deserialize<T>(string serializedObj);
}