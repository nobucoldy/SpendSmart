using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace spendsmart.Converters;

public sealed class PercentageToGridLengthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var percentage = value switch
        {
            decimal decimalValue => (double)decimalValue,
            double doubleValue => doubleValue,
            _ => 0d
        };

        var clamped = Math.Min(Math.Max(percentage, 0), 100);

        if (string.Equals(parameter as string, "Remaining", StringComparison.OrdinalIgnoreCase))
        {
            clamped = 100 - clamped;
        }

        return new GridLength(clamped, GridUnitType.Star);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
