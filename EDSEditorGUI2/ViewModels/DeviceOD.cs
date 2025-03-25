using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EDSEditorGUI2.ViewModels;
public partial class DeviceOD : ObservableObject
{
    public ObservableCollection<KeyValuePair<string, OdObject>> Data { get; } = [];

    [ObservableProperty]
    KeyValuePair<string, OdObject> _selectedObject;

    [ObservableProperty]
    KeyValuePair<string, OdSubObject> _selectedSubObject;

    [ObservableProperty]
    ObservableCollection<KeyValuePair<string, OdSubObject>> _selectedSubObjects = [];

    public DeviceOD()
    {
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

    public void AddIndex(int index, string name, LibCanOpen.OdObject.Types.ObjectType type)
    {
        var strIndex = index.ToString("X4");
        var newObj = new OdObject
        {
            Name = name,
            ObjectType = type
        };

        // create OD entry
        if (type == LibCanOpen.OdObject.Types.ObjectType.Var)
        {
            var newSub = new OdSubObject()
            {
                Name = name,
                Type = LibCanOpen.OdSubObject.Types.DataType.Unsigned32,
                Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Rw,
                Pdo = LibCanOpen.OdSubObject.Types.AccessPDO.No,
                Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.No,
                DefaultValue = "0"
            };
            newObj.SubObjects.Add(new KeyValuePair<string, OdSubObject>("0x0", newSub));
        }
        else
        {
            var CountSub = new OdSubObject()
            {
                Name = "Highest sub-index supported",
                Type = LibCanOpen.OdSubObject.Types.DataType.Unsigned8,
                Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Ro,
                Pdo = LibCanOpen.OdSubObject.Types.AccessPDO.No,
                Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.No,
                DefaultValue = "0x01"
            };
            var Sub1 = new OdSubObject()
            {
                Name = "Sub Object 1",
                Type = LibCanOpen.OdSubObject.Types.DataType.Unsigned32,
                Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Rw,
                Pdo = LibCanOpen.OdSubObject.Types.AccessPDO.No,
                Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.No,
                DefaultValue = "0"
            };

            newObj.SubObjects.Add(new KeyValuePair<string, OdSubObject>("0x0", CountSub));
            newObj.SubObjects.Add(new KeyValuePair<string, OdSubObject>("0x1", Sub1));
        }
        Data.Add(new KeyValuePair<string, OdObject>(strIndex, newObj));
    }

    public void RemoveIndex(object sender)
    {

    }
}
