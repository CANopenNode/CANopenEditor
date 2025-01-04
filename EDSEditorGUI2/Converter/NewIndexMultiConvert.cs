using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Immutable;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EDSEditorGUI2.Converter;

public class NewIndexRequest(int index, string name, LibCanOpen.OdObject.Types.ObjectType type)
{
    public int Index { get; } = index;
    public string Name { get; } = name;
    public LibCanOpen.OdObject.Types.ObjectType Type { get; } = type;
}

public sealed class NewIndexMultiConvert : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        // Ensure all bindings are provided and attached to correct target type
        if (values?.Count != 3 || !targetType.IsAssignableFrom(typeof(ImmutableSolidColorBrush)))
            throw new NotSupportedException();

        if (values[0] is not string rawindex ||
            values[1] is not string name ||
            values[2] is not int typeIndex)
            return BindingOperations.DoNothing;

        int index = int.Parse(rawindex, NumberStyles.HexNumber);

        var typeValues = Enum.GetNames(typeof(LibCanOpen.OdObject.Types.ObjectType)).Skip(1).ToArray();
        bool parseOk = Enum.TryParse(typeValues[typeIndex], out LibCanOpen.OdObject.Types.ObjectType type);

        if (parseOk)
        {
            var indexRequest = new NewIndexRequest(index, name, type);
            return indexRequest;
        }
        else
        {
            return BindingOperations.DoNothing;
        }
    }
}
