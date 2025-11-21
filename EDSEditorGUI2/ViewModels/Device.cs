using CommunityToolkit.Mvvm.ComponentModel;

namespace EDSEditorGUI2.ViewModels
{
    public partial class Device : ObservableObject
    {
        public Device()
        {
        }

        public override string ToString()
        {
            if (DeviceInfo == null)
            {
                return "unnamed device";
            }
            else
            {
                return DeviceInfo.ProductName;
            }
        }

        [ObservableProperty]
        private FileInfo _fileInfo = new();

        [ObservableProperty]
        private DeviceInfo _deviceInfo = new();

        [ObservableProperty]
        private DeviceCommissioning _deviceCommissioning = new();

        [ObservableProperty]
        private ObjectDictionary _objects = new();

        public void OnClickCommand()
        {
            // do something
        }
    }
}

