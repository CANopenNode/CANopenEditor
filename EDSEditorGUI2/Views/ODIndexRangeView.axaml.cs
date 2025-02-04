using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DialogHostAvalonia;
using EDSEditorGUI2.Converter;
using LibCanOpen;
using System;
using System.Linq;

namespace EDSEditorGUI2.Views;

public partial class ODIndexRangeView : UserControl
{
    public ODIndexRangeView()
    {
        InitializeComponent();
        var values = Enum.GetNames(typeof(OdObject.Types.ObjectType)).Skip(1).ToArray();
        type.ItemsSource = values;

        grid.LoadingRow += GridLoadingRow;
    }

    /// <summary>
    /// Hides rows with indexes that is not in min&max range
    /// </summary>
    /// <param name="sender">sender object</param>
    /// <param name="e">event param</param>
    private void GridLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if(e.Row.DataContext != null)
        {
            var dc = (System.Collections.Generic.KeyValuePair<string, ViewModels.OdObject>)e.Row.DataContext;
            int index = int.Parse(dc.Key, System.Globalization.NumberStyles.HexNumber);
            int min = Convert.ToInt32(MinIndex, 16);
            int max = Convert.ToInt32(MaxIndex, 16);
            e.Row.IsVisible = (min <= index && index <= max);
        }
    }

    public static readonly StyledProperty<string> HeadingProperty =
        AvaloniaProperty.Register<ODIndexRangeView, string>(nameof(HeadingProperty));
    public string Heading
    {
        get { return GetValue(HeadingProperty); }
        set { SetValue(HeadingProperty, value); HeadingText.Text = value; }
    }

    public static readonly StyledProperty<string> MinIndexProperty =
        AvaloniaProperty.Register<ODIndexRangeView, string>(nameof(MinIndexProperty));
    public string MinIndex
    {
        get { return GetValue(MinIndexProperty); }
        set { SetValue(MinIndexProperty, value); }
    }

    public static readonly StyledProperty<string> MaxIndexProperty =
        AvaloniaProperty.Register<ODIndexRangeView, string>(nameof(MaxIndexProperty));
    public string MaxIndex
    {
        get { return GetValue(MaxIndexProperty); }
        set { SetValue(MaxIndexProperty, value); }
    }

    private async void AddIndex(object? sender, RoutedEventArgs e)
    {
        await DialogHost.Show(Resources["NewIndexDialog"]!, "NoAnimationDialogHost", OnDialogClosing);
    }

    private void OnDialogClosing(object? sender, DialogClosingEventArgs e)
    {
        if (e.Parameter != null)
        {
            if (DataContext is ViewModels.DeviceOD dc && e.Parameter is NewIndexRequest param)
            {
                dc.AddIndex(param.Index, param.Name, param.Type);
            }
        }
    }

    private async void RemoveIndex(object? sender, RoutedEventArgs e)
    {
        await DialogHost.Show(Resources["NewIndexDialog"]!, "NoAnimationDialogHost");
    }

    private void DataGrid_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
    }
}