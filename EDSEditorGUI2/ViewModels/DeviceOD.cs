using CommunityToolkit.Mvvm.ComponentModel;
using Google.Protobuf.Collections;
using LibCanOpen;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EDSEditorGUI2.ViewModels;
public partial class DeviceOD : ObservableObject
{
    [ObservableProperty]
    private MapField<string, OdObject> _model;
    public DeviceOD(MapField<string, OdObject> model)
    {
        Model = model;
        _viewModel = new(Model);
        _viewModel.CollectionChanged += ViewModel_CollectionChanged;
    }

    private void ViewModel_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(ViewModel));
    }
    [ObservableProperty]
    ObservableCollection<KeyValuePair<string, OdObject>> _viewModel;
}
