using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EDSEditorGUI2.ViewModels
{
    public partial class OdObject : ObservableObject
    {
        [ObservableProperty]
        private bool _disabled;

        [ObservableProperty]
        string _name = string.Empty;

        [ObservableProperty]
        string _alias = string.Empty;

        [ObservableProperty]
        string _description = string.Empty;

        [ObservableProperty]
        LibCanOpen.OdObject.Types.ObjectType _type;

        [ObservableProperty]
        string _countLabel = string.Empty;

        [ObservableProperty]
        string _storageGroup = string.Empty;

        [ObservableProperty]
        bool flagsPDO;

        [ObservableProperty]
        ObservableCollection<KeyValuePair<string, OdSubObject>> _subObjects = [];

    }
}
