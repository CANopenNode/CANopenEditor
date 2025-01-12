using CommunityToolkit.Mvvm.ComponentModel;

namespace EDSEditorGUI2.ViewModels
{
    public partial class Device : ObservableObject
    {
        public Device()
        {
        }

        [ObservableProperty]
        private FileInfo _fileInfo = new();

        [ObservableProperty]
        private DeviceInfo _deviceInfo = new();

        [ObservableProperty]
        private DeviceCommissioning _deviceCommissioning = new();

        [ObservableProperty]
        private DeviceOD _objects = new();

        public void OnClickCommand()
        {
            // do something
        }
    }
}

