using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FluxConvert.App.Converters;

/// <summary>
/// Resolves a resource by its key, so a list can refer to resources by name.
/// </summary>
public sealed class ResourceKeyToGeometryConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = value as string;
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return Application.Current?.TryFindResource(key);
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("The conversion back is not supported.");
}
