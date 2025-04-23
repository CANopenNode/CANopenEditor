using Avalonia.Controls;
using Avalonia.Interactivity;
using LibCanOpen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EDSEditorGUI2.Views;

public partial class DeviceODView : UserControl
{
    private List<DataGrid> _odViews = [];
    public DeviceODView()
    {
        InitializeComponent();
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

    private void IndexGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid s && DataContext is ViewModels.DeviceOD dc)
        {
            if (s.SelectedItem is KeyValuePair<string, ViewModels.OdObject> selected)
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
            if (s.SelectedItem is KeyValuePair<string, ViewModels.OdSubObject> selected)
            {
                dc.SelectedSubObject = selected;
                dc.SelectedSubObjects.Clear();
                foreach (var row in s.SelectedItems)
                {
                    if (row is KeyValuePair<string, ViewModels.OdSubObject> subObj)
                    {
                        dc.SelectedSubObjects.Add(subObj);
                    }
                }
            }
        }
    }
    private void ContextMenuSubObjectAddClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.DeviceOD dc)
        {
            var selectedObj = dc.SelectedObject.Value;
            ObservableCollection<KeyValuePair<string, ViewModels.OdSubObject>> selection = [];

            foreach (var row in dc.SelectedSubObjects)
            {
                selectedObj.AddSubEntry(row);
            }
        }
    }
    private void ContextMenuSubObjectRemoveClick(object? sender, RoutedEventArgs e)
    {
        bool renumber = sender == contextMenu_subObject_removeSubItemToolStripMenuItem;

        if (DataContext is ViewModels.DeviceOD dc)
        {
            var selectedObject = dc.SelectedObject.Value;

            //Clone the list because we cant modify the list we iterate on 
            var selectedObj = dc.SelectedSubObjects.ToList();
            foreach (var item in selectedObj)
            {
                selectedObject.RemoveSubEntry(item, renumber);
            }
        }
    }
}
