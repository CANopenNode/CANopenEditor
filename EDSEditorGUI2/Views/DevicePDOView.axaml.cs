using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.Generic;

namespace EDSEditorGUI2.Views;

public partial class DevicePDOView : UserControl
{
    private List<ColumnDefinition> _bitColumns = [];
    public DevicePDOView()
    {
        InitializeComponent();

        CreateMappingBitsAndBytesIndication();
        Zoom.Value = 100;
    }

    void CreateMappingBitsAndBytesIndication()
    {
        //Bits
        for (int i = 0; i < 64; i++)
        {
            var indication = new TextBlock
            {
                Text = i.ToString(),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
            };
            if ((i % 8) == 0)
            {
                indication.Foreground = Brushes.Red;
            }
            AddToMappingGrid(indication, 0, 3 + i);

            var newColumn = new ColumnDefinition(new GridLength(10 * 1.0));
            _bitColumns.Add(newColumn);

            MappingGrid.ColumnDefinitions.Add(newColumn);
        }
        //Bytes
        for (int i = 0; i < 8; i++)
        {
            var indication = new TextBlock
            {
                Text = $"Byte {i}",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,

                TextWrapping = TextWrapping.Wrap,
            };
            AddToMappingGrid(indication, 1, 3 + (i*8), 8);
        }
    }
    void AddToMappingGrid(Control element, int row,int column, int columnspam = 1)
    {
        Grid.SetRow(element, row);
        Grid.SetColumn(element, column);
        Grid.SetColumnSpan(element, columnspam);
        MappingGrid.Children.Add(element);
    }

    private void Zoom_PropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "Value")
        {
            decimal newValue = Zoom.Value ?? 0;
            ChangeMappingZoom((double)newValue);
        }
    }
    /// <summary>
    /// Changes the zoom level on pdo mapping
    /// </summary>
    /// <param name="zoomLevel">zoom level in percent</param>
    private void ChangeMappingZoom(double zoomPercent)
    {
        var zoom = zoomPercent / 100;
        foreach (var column in _bitColumns)
        {
            column.Width = new GridLength(10 * zoom);
        }
    }
}
