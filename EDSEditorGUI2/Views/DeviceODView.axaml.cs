using Avalonia.Controls;
using Avalonia.Interactivity;
using DialogHostAvalonia;
using EDSEditorGUI2.Converter;
using System;
using System.Linq;

namespace EDSEditorGUI2.Views;

public partial class DeviceODView : UserControl
{
    public DeviceODView()
    {
        InitializeComponent();

        var values = Enum.GetNames(typeof(LibCanOpen.OdObject.Types.ObjectType)).Skip(1).ToArray();
        type.ItemsSource = values;
    }
    private void OnDialogClosing(object? sender, DialogClosingEventArgs e)
    {
        if(e.Parameter != null)
        {
            if(DataContext is ViewModels.DeviceOD dc && e.Parameter is NewIndexRequest param)
            {
                dc.AddIndex(param.Index, param.Name, param.Type);
            }
        }
    }
    private async void AddIndex(object? sender, RoutedEventArgs e)
    {
        await DialogHost.Show(Resources["NewIndexDialog"]!, "NoAnimationDialogHost");
    }

    private void DataGrid_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
    }
}