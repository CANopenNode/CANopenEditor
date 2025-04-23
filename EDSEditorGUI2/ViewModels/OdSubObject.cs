using CommunityToolkit.Mvvm.ComponentModel;
using static LibCanOpen.OdSubObject.Types;

namespace EDSEditorGUI2.ViewModels;

public partial class OdSubObject : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _alias = string.Empty;

    [ObservableProperty]
    private DataType _type;

    [ObservableProperty]
    private AccessSDO _sdo;

    [ObservableProperty]
    private AccessPDO _pdo;

    [ObservableProperty]
    private AccessSRDO _srdo;

    [ObservableProperty]
    private string _defaultValue = string.Empty;

    [ObservableProperty]
    private string _actualValue = string.Empty;

    [ObservableProperty]
    private string _lowLimit = string.Empty;

    [ObservableProperty]
    string _highLimit = string.Empty;

    [ObservableProperty]
    private uint _stringLengthMin;
}
