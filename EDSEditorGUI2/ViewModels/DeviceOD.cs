using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using DialogHostAvalonia;
using Google.Protobuf.Collections;
using LibCanOpen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace EDSEditorGUI2.ViewModels;
public partial class DeviceOD : ObservableObject
{
    [ObservableProperty]
    private MapField<string, OdObject> _model;
    [ObservableProperty]
    ReadOnlyObservableCollection<KeyValuePair<string, OdObject>> _DataTypes;

    [ObservableProperty]
    OdObject _SelectedObject;

    [ObservableProperty]
    OdSubObject _SelectedSubObject;

    public DeviceOD(MapField<string, OdObject> model)
    {
        Model = model;
        _viewModel = new(Model);
        _viewModel.CollectionChanged += ViewModel_CollectionChanged;
        HackyUpdate();
    }

    private void ViewModel_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(ViewModel));
/*
        var newObj = new OdObject();
        newObj.Name = "name1";
        newObj.Type = OdObject.Types.ObjectType.Var;
        Model.Add("1000", newObj);

        var newObj2 = new OdObject();
        newObj.Name = "name2";
        newObj.Type = OdObject.Types.ObjectType.Array;
        Model.Add("1001", newObj2);
*/
        HackyUpdate();
    }

    private void HackyUpdate()
    {
        //Hack should be rewritten
        var temp = Model.Where(key => 0x0001 <= IndexStringToInt(key.Key) && IndexStringToInt(key.Key) <= 0x025F);
        DataTypes = new(new ObservableCollection<KeyValuePair<string, OdObject>>(temp));
    }

    private static int IndexStringToInt(string str)
    {
        if (str.StartsWith("0x"))
        {
            var hex = str[2..];
            return Convert.ToUInt16(hex, 16);
        }
        else
        {
            return Convert.ToUInt16(str);
        }
    }

    public void AddIndex(int index, string name, OdObject.Types.ObjectType type)  
    {
        var strIndex = index.ToString("X4");
        var newObj = new OdObject
        {
            Name = name,
            Type = type
        };

        // create OD entry
        if (type == OdObject.Types.ObjectType.Var)
        {
            var newSub = new OdSubObject()
            {
                Name = name,
                Type = OdSubObject.Types.DataType.Unsigned32,
                Sdo = OdSubObject.Types.AccessSDO.Rw,
                Pdo = OdSubObject.Types.AccessPDO.No,
                Srdo = OdSubObject.Types.AccessSRDO.No,
                DefaultValue = "0"
            };
            newObj.SubObjects.Add("0", newSub);
        }
        else
        {
            var CountSub = new OdSubObject()
            {
                Name = "Highest sub-index supported",
                Type = OdSubObject.Types.DataType.Unsigned8,
                Sdo = OdSubObject.Types.AccessSDO.Ro,
                Pdo = OdSubObject.Types.AccessPDO.No,
                Srdo = OdSubObject.Types.AccessSRDO.No,
                DefaultValue = "0x01"
            };
            var Sub1 = new OdSubObject()
            {
                Name = "Sub Object 1",
                Type = OdSubObject.Types.DataType.Unsigned32,
                Sdo = OdSubObject.Types.AccessSDO.Rw,
                Pdo = OdSubObject.Types.AccessPDO.No,
                Srdo = OdSubObject.Types.AccessSRDO.No,
                DefaultValue = "0"
            };
            newObj.SubObjects.Add("0", CountSub);
            newObj.SubObjects.Add("1", Sub1);
        }

        ViewModel.Add(new KeyValuePair<string, OdObject>(strIndex, newObj));
        Model.Add(strIndex, newObj);
    }

    public void RemoveIndex(object sender)
    {
        
    }
    [ObservableProperty]
    ObservableCollection<KeyValuePair<string, OdObject>> _viewModel;
}
