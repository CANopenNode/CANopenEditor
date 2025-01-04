using Avalonia.Controls;
using Avalonia.Interactivity;
using DialogHostAvalonia;
using EDSEditorGUI2.Converter;
using LibCanOpen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDSEditorGUI2.Views;

public partial class DeviceODView : UserControl
{
    private List<DataGrid> _odViews = [];
    public DeviceODView()
    {
        InitializeComponent();

        var values = Enum.GetNames(typeof(OdObject.Types.ObjectType)).Skip(1).ToArray();
        type.ItemsSource = values;
        ODView_Com.grid.SelectionChanged += GridSelectionChanged;
        ODView_Manufacture.grid.SelectionChanged += GridSelectionChanged;
        ODView_Device.grid.SelectionChanged += GridSelectionChanged;

        _odViews.Add(ODView_Com.grid);
        _odViews.Add(ODView_Manufacture.grid);
        _odViews.Add(ODView_Device.grid);
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

    private void GridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if(sender is DataGrid s && DataContext is ViewModels.DeviceOD dc)
        {
            if(s.SelectedItem is KeyValuePair<string, OdObject> selected)
            {
                dc.SelectedObject = selected.Value;
                foreach (var dg in _odViews)
                {
                    if (dg != s)
                    {
                        dg.SelectedItem = null;
                    }
                }
            }
        }
    }
}
