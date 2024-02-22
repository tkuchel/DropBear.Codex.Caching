namespace DropBear.Codex.Caching.Configuration;

public class InMemoryOptions
{
    public int SizeLimit { get; set; } = 1000; // Default maximum number of items
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromHours(1); // Default cache duration
}