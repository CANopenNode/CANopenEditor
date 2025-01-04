using Avalonia.Controls;
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
        ODView_Com.grid.SelectionChanged += IndexGridSelectionChanged;
        ODView_Manufacture.grid.SelectionChanged += IndexGridSelectionChanged;
        ODView_Device.grid.SelectionChanged += IndexGridSelectionChanged;

        subindexGrid.SelectionChanged += subindexGridSelectionChanged;

        _odViews.Add(ODView_Com.grid);
        _odViews.Add(ODView_Manufacture.grid);
        _odViews.Add(ODView_Device.grid);

        foreach (var v in Enum.GetNames(typeof(OdSubObject.Types.DataType)))
        {
            combo_datatype.Items.Add(v);
        }

        foreach (var v in Enum.GetNames(typeof(OdSubObject.Types.AccessSDO)))
        {
            combo_sdo.Items.Add(v);
        }

        foreach (var v in Enum.GetNames(typeof(OdSubObject.Types.AccessPDO)))
        {
            combo_pdo.Items.Add(v);
        }

        foreach (var v in Enum.GetNames(typeof(OdSubObject.Types.AccessSRDO)))
        {
            combo_srdo.Items.Add(v);
        }
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

    private void IndexGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid s && DataContext is ViewModels.DeviceOD dc)
        {
            if (s.SelectedItem is KeyValuePair<string, OdObject> selected)
            {
                dc.SelectedObject = selected;
                foreach (var dg in _odViews)
                {
                    if (dg != s)
                    {
                        dg.SelectedItem = null;
                        subindexGrid.SelectedItem = null;
                    }
                }
            }
        }
    }
    private void subindexGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid s && DataContext is ViewModels.DeviceOD dc)
        {
            if (s.SelectedItem is KeyValuePair<string, OdSubObject> selected)
            {
                dc.SelectedSubObject = selected;
            }
        }
    }
}
