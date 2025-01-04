using CommunityToolkit.Mvvm.ComponentModel;
using LibCanOpen;
using System.ComponentModel;
using System.Xml.Linq;

namespace EDSEditorGUI2.ViewModels
{
    public partial class Device : ObservableObject
    {
        [ObservableProperty]
        CanOpenDevice _model;
        public Device(CanOpenDevice model) { Model = model;

            _DeviceInfo = new(Model.DeviceInfo);
            _DeviceInfo.PropertyChanged += (s, e) => { OnPropertyChanged(nameof(DeviceInfo)); };
            _objects = new(_model.Objects);
        }

        private DeviceInfo _DeviceInfo;
        public DeviceInfo DeviceInfo
        {
            get => _DeviceInfo;
            set
            {
                Model.DeviceInfo = value.Model;
                OnPropertyChanged(nameof(DeviceInfo));
            }
        }

        private DeviceOD _objects;
        public DeviceOD Objects
        {
            get => _objects;
            set
            {
                OnPropertyChanged(nameof(Objects));
            }
        }

        public void OnClickCommand()
        {
            // do something
        }
    }
}

