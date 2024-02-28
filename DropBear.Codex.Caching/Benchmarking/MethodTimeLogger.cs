using System.Reflection;

namespace DropBear.Codex.Caching.Benchmarking;

public static class MethodTimeLogger
{
    public static void Log(MethodBase method, TimeSpan timeSpan, string message = "")
    {
        // Format the message to include method name, execution time, and any additional message provided.
        var template =
            $"Method: {method.DeclaringType?.FullName}.{method.Name}, Execution Time: {timeSpan.TotalMilliseconds} ms, Message: {message}";
        Console.WriteLine(template);
    }
}