using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace EDSEditorGUI2.ViewModels;

public partial class DeviceCommissioning : ObservableObject
{
    [ObservableProperty]
    private UInt32 _nodeId;

    [ObservableProperty]
    private string _nodeName = string.Empty;

    [ObservableProperty]
    private UInt32 _baudrate;
}
