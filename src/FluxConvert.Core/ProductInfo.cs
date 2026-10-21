using System.Reflection;

namespace FluxConvert.Core;

/// <summary>
/// Single source of truth for product identity.
/// </summary>
/// <remarks>
/// "FluxConvert" is a working name while branding is finalised. Keeping it here means a rename
/// touches one file (plus the assembly metadata in <c>Directory.Build.props</c>) instead of
/// dozens of XAML and code references.
/// </remarks>
public static class ProductInfo
{
    /// <summary>The product name as shown in the interface.</summary>
    public const string ProductName = "FluxConvert";

    /// <summary>The publisher / organisation name.</summary>
    public const string CompanyName = "FluxConvert";

    /// <summary>Short product description.</summary>
    public const string Description = "A lightweight floating file converter for Windows.";

    /// <summary>Title used for the main application window.</summary>
    public static string WindowTitle => ProductName;

    /// <summary>Tooltip / notification prefix used for background notifications.</summary>
    public static string NotificationPrefix => ProductName;

    /// <summary>The product version, taken from the assembly metadata.</summary>
    public static string Version { get; } = ResolveVersion();

    private static string ResolveVersion()
    {
        var assembly = typeof(ProductInfo).Assembly;

        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (informational is not null && !string.IsNullOrWhiteSpace(informational.InformationalVersion))
        {
            // The SDK appends "+<commit sha>" to the informational version; it is noise here.
            var plus = informational.InformationalVersion.IndexOf('+', StringComparison.Ordinal);
            return plus < 0
                ? informational.InformationalVersion
                : informational.InformationalVersion[..plus];
        }

        return assembly.GetName().Version?.ToString(3) ?? "0.0.0";
    }
}
