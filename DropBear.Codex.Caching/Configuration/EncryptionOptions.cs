namespace DropBear.Codex.Caching.Configuration;
/// <summary>
/// Configuration options for Encryption.
/// </summary>
public class EncryptionOptions
{
    /// <summary>
    ///     Gets or sets a value indicating whether encryption is used for cache entries.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     Gets or sets the application name used for scoping the data protection purposes in encryption.
    ///     This helps isolate encryption keys from other applications on the same host.
    /// </summary>
    public string? EncryptionApplicationName { get; set; }

    /// <summary>
    ///     Gets or sets the path to the key storage location for encryption.
    ///     This is used to store the encryption keys.
    /// </summary>
    public string? KeyStoragePath { get; set; }
}