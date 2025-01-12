using CommunityToolkit.Mvvm.ComponentModel;
using EDSEditorGUI2.Mapper;
using System.Collections.ObjectModel;

namespace EDSEditorGUI2.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    int Counter = 0;
    public void AddNewDevice(object sender)
    {
        var device = new LibCanOpen.CanOpenDevice
        {
            DeviceInfo = new()
            {
                ProductName = "New Product" + Counter.ToString()
            },
        };

        Counter++;

        //string dir = Environment.OSVersion.Platform == PlatformID.Win32NT ? "\\" : "/";
        //eds.projectFilename = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + dir + "project";

        //DeviceView device = new DeviceView(eds, network);
        //device.UpdateODViewForEDS += Device_UpdateODViewForEDS;

        //eds.OnDataDirty += Eds_onDataDirty;

        //device.Dock = DockStyle.Fill;
        //device.dispatch_updateOD();

        var DeviceView = ProtobufferViewModelMapper.MapFromProtobuffer(device);
        Network.Add(DeviceView);
    }
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
    public ObservableCollection<Device> Network { get; set; } = [];

    [ObservableProperty]
    public Device? selectedDevice;


}
