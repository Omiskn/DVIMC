using System.Text;

namespace FluxConvert.Core.Diagnostics;

/// <summary>
/// The information the diagnostics command prints.
/// </summary>
public sealed class DiagnosticsReport
{
    private readonly List<DiagnosticsSection> _sections = new();

    /// <summary>Creates an empty report.</summary>
    public DiagnosticsReport()
    {
    }

    /// <summary>Creates a report with the given sections.</summary>
    /// <param name="title">The report title.</param>
    /// <param name="sections">The sections, in the order they should be printed.</param>
    public DiagnosticsReport(string title, IEnumerable<DiagnosticsSection> sections)
    {
        Title = title;
        _sections.AddRange(sections);
    }

    /// <summary>The report title.</summary>
    public string Title { get; init; } = "FluxConvert diagnostics";

    /// <summary>The sections of the report.</summary>
    public IReadOnlyList<DiagnosticsSection> Sections => _sections;

    /// <summary>
    /// Appends a section to the report.
    /// </summary>
    /// <param name="section">The section to append.</param>
    public void AddSection(DiagnosticsSection section) => _sections.Add(section);

    /// <summary>
    /// Renders the report as plain text.
    /// </summary>
    /// <returns>The formatted report, one line per row, with a trailing newline.</returns>
    public string Format()
    {
        var builder = new StringBuilder();

        builder.Append(Title).AppendLine();
        builder.Append(new string('=', Title.Length)).AppendLine();

        var nameWidth = _sections
            .SelectMany(section => section.Entries)
            .Select(entry => entry.Name.Length)
            .DefaultIfEmpty(0)
            .Max();

        foreach (var section in _sections)
        {
            builder.AppendLine();
            builder.Append(section.Heading).AppendLine();
            builder.Append(new string('-', section.Heading.Length)).AppendLine();

            if (section.Entries.Count == 0)
            {
                builder.AppendLine("  (nothing to report)");
                continue;
            }

            foreach (var entry in section.Entries)
            {
                builder
                    .Append("  ")
                    .Append(entry.Name.PadRight(nameWidth))
                    .Append("  ")
                    .AppendLine(entry.Value);
            }
        }

        return builder.ToString();
    }
}

/// <summary>
/// A titled group of diagnostic entries.
/// </summary>
public sealed class DiagnosticsSection
{
    /// <summary>Creates an empty section.</summary>
    /// <param name="heading">The section heading.</param>
    public DiagnosticsSection(string heading) => Heading = heading;

    /// <summary>The section heading.</summary>
    public string Heading { get; }

    /// <summary>The entries of this section.</summary>
    public List<DiagnosticsEntry> Entries { get; } = new();
}
