using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FluxConvert.App.Converters;

/// <summary>
/// Converts a hexadecimal colour such as <c>#4C8DFF</c> into a brush.
/// </summary>
public sealed class HexToBrushConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var text = value as string ?? value?.ToString();
        if (string.IsNullOrWhiteSpace(text))
        {
            return Brushes.Transparent;
        }

        try
        {
            var converted = ColorConverter.ConvertFromString(text.Trim());
            return converted is Color color ? new SolidColorBrush(color) : Brushes.Transparent;
        }
        catch (FormatException)
        {
            return Brushes.Transparent;
        }
        catch (NotSupportedException)
        {
            return Brushes.Transparent;
        }
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException("The conversion back is not supported.");
}
