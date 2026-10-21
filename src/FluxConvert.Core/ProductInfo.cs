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
}
