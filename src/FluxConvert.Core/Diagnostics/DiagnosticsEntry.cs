namespace FluxConvert.Core.Diagnostics;

/// <summary>
/// How a single diagnostic line should be read.
/// </summary>
public enum DiagnosticsStatus
{
    /// <summary>Neutral information.</summary>
    Info,

    /// <summary>Everything is as expected.</summary>
    Ok,

    /// <summary>Something is missing but the application still works.</summary>
    Warning,

    /// <summary>Something is wrong and a feature will not work.</summary>
    Error,
}

/// <summary>
/// One line of the diagnostics report.
/// </summary>
/// <param name="Name">The name of the item.</param>
/// <param name="Value">The value that was found.</param>
/// <param name="Status">How the line should be read.</param>
public sealed record DiagnosticsEntry(string Name, string Value, DiagnosticsStatus Status = DiagnosticsStatus.Info);
