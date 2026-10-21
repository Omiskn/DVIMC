namespace FluxConvert.Core.Process;

/// <summary>
/// Thrown when an external program cannot be started or fails unexpectedly.
/// </summary>
public sealed class ExternalToolException : Exception
{
    /// <summary>
    /// Initialises a new instance of the <see cref="ExternalToolException"/> class.
    /// </summary>
    /// <param name="message">A message that can be shown to the user.</param>
    /// <param name="fileName">The program that was being run.</param>
    /// <param name="innerException">The underlying cause.</param>
    public ExternalToolException(string message, string fileName, Exception? innerException = null)
        : base(message, innerException)
    {
        FileName = fileName;
    }

    /// <summary>The program that was being run.</summary>
    public string FileName { get; }
}
