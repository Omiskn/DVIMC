namespace FluxConvert.Core.Tools;

/// <summary>
/// The result of looking for an external program.
/// </summary>
public sealed class ExternalTool
{
    private ExternalTool(
        ExternalToolKind kind,
        string displayName,
        string? executablePath,
        string? version,
        string? unavailableReason)
    {
        Kind = kind;
        DisplayName = displayName;
        ExecutablePath = executablePath;
        Version = version;
        UnavailableReason = unavailableReason;
    }

    /// <summary>Which program this describes.</summary>
    public ExternalToolKind Kind { get; }

    /// <summary>The name shown to the user.</summary>
    public string DisplayName { get; }

    /// <summary>Full path of the executable, or <see langword="null"/> when it was not found.</summary>
    public string? ExecutablePath { get; }

    /// <summary>The detected version, or <see langword="null"/> when it is unknown.</summary>
    public string? Version { get; }

    /// <summary>Why the program could not be used, or <see langword="null"/> when it is available.</summary>
    public string? UnavailableReason { get; }

    /// <summary>Whether the program was found and can be used.</summary>
    public bool IsAvailable => ExecutablePath is not null;

    /// <summary>A short status line suitable for a diagnostics view.</summary>
    public string StatusText => IsAvailable
        ? Version is null ? $"{DisplayName}: found" : $"{DisplayName} {Version}"
        : $"{DisplayName}: not available - {UnavailableReason}";

    /// <summary>Creates a result describing a program that was found.</summary>
    /// <param name="kind">Which program this describes.</param>
    /// <param name="displayName">The name shown to the user.</param>
    /// <param name="executablePath">Full path of the executable.</param>
    /// <param name="version">The detected version, if known.</param>
    /// <returns>The result.</returns>
    public static ExternalTool Found(ExternalToolKind kind, string displayName, string executablePath, string? version) =>
        new(kind, displayName, executablePath, version, null);

    /// <summary>Creates a result describing a program that could not be found.</summary>
    /// <param name="kind">Which program this describes.</param>
    /// <param name="displayName">The name shown to the user.</param>
    /// <param name="reason">Why the program could not be used.</param>
    /// <returns>The result.</returns>
    public static ExternalTool Missing(ExternalToolKind kind, string displayName, string reason) =>
        new(kind, displayName, null, null, reason);
}
