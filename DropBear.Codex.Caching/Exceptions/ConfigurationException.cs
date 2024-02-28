namespace DropBear.Codex.Caching.Exceptions;

/// <summary>
///     Represents errors that occur during application configuration.
/// </summary>
[Serializable]
public class ConfigurationException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ConfigurationException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationException" /> class with a specified error message and a
    ///     reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference if no inner
    ///     exception is specified.
    /// </param>
    public ConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationException" /> class with a specified error message and
    ///     property name.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="propertyName">The name of the property that caused the current exception.</param>
    public ConfigurationException(string message, string? propertyName) : base($"{message} Property: {propertyName}")
    {
        PropertyName = propertyName;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationException" /> class with a specified error message,
    ///     property name, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="propertyName">The name of the property that caused the current exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference if no inner
    ///     exception is specified.
    /// </param>
    public ConfigurationException(string message, string? propertyName, Exception innerException) : base(
        $"{message} Property: {propertyName}", innerException)
    {
        PropertyName = propertyName;
    }

    /// <summary>
    ///     Gets the name of the property that caused the current exception.
    /// </summary>
    public string? PropertyName { get; }

    public ConfigurationException()
    {
    }
}