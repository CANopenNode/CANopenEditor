using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace EDSEditorGUI2.Converter;

public sealed class BrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // TODO: Get theme and use the correct brush based on that
        if (value is Boolean bValue)
        {
            if (bValue)
            {
                return new SolidColorBrush(Colors.Red);
            }
            else
            {
                var currentThemeVariant = Application.Current?.ActualThemeVariant;

                // TODO: figure out how to get the default forground from theme instead of hardcoding it
                if (currentThemeVariant == Avalonia.Styling.ThemeVariant.Dark)
                {
                    return new SolidColorBrush(Colors.White);
                }
                else
                {
                    return new SolidColorBrush(Colors.Black);
                }
            }
        }
        return new SolidColorBrush(Colors.Orange);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
