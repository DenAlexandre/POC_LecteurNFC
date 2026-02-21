using System.Globalization;

namespace NfcSecurePoc;

/// <summary>
/// Inverts a boolean value.
/// </summary>
public class InvertBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b ? !b : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b ? !b : value;
}

/// <summary>
/// Returns Red when active (true), Primary color when inactive (false).
/// </summary>
public class ActiveColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Colors.Red : Color.FromArgb("#512BD4");

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// Formats a boolean with a "true=X|false=Y" pattern.
/// Usage: StringFormat='{0:true=Stop|false=Start}'
/// Note: MAUI StringFormat doesn't support this natively, so we use this converter.
/// </summary>
public class BoolToStringConverter : IValueConverter
{
    public string TrueText { get; set; } = "True";
    public string FalseText { get; set; } = "False";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? TrueText : FalseText;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
