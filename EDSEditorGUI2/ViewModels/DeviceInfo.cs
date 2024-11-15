using CommunityToolkit.Mvvm.ComponentModel;
using LibCanOpen;

namespace EDSEditorGUI2.ViewModels
{
    public partial class DeviceInfo : ObservableObject
    {
        [ObservableProperty]
        private CanOpen_DeviceInfo _model;
        public DeviceInfo(CanOpen_DeviceInfo model) { Model = model; }

        public string VendorName
        {
            get => Model.VendorName;
            set
            {
                Model.VendorName = value;
                OnPropertyChanged(nameof(VendorName));
            }
        }
        public string ProductName
        {
            get => Model.ProductName;
            set
            {
                SetProperty(Model.ProductName, value, Model, (u, n) => u.ProductName = n);
            }
        }
        public bool BaudRate10
        {
            get => Model.BaudRate10;
            set
            {
                SetProperty(Model.BaudRate10, value, Model, (u, n) => u.BaudRate10 = n);
            }
        }
        public bool BaudRate20
        {
            get => Model.BaudRate20;
            set
            {
                SetProperty(Model.BaudRate20, value, Model, (u, n) => u.BaudRate20 = n);
            }
        }
        public bool BaudRate50
        {
            get => Model.BaudRate50;
            set
            {
                SetProperty(Model.BaudRate50, value, Model, (u, n) => u.BaudRate50 = n);
            }
        }
        public bool BaudRate125
        {
            get => Model.BaudRate125;
            set
            {
                SetProperty(Model.BaudRate125, value, Model, (u, n) => u.BaudRate125 = n);
            }
        }
        public bool BaudRate250
        {
            get => Model.BaudRate250;
            set
            {
                SetProperty(Model.BaudRate250, value, Model, (u, n) => u.BaudRate250 = n);
            }
        }
        public bool BaudRate500
        {
            get => Model.BaudRate500;
            set
            {
                SetProperty(Model.BaudRate500, value, Model, (u, n) => u.BaudRate500 = n);
            }
        }
        public bool BaudRate800
        {
            get => Model.BaudRate800;
            set
            {
                SetProperty(Model.BaudRate800, value, Model, (u, n) => u.BaudRate800 = n);
            }
        }
        public bool BaudRate1000
        {
            get => Model.BaudRate1000;
            set
            {
                SetProperty(Model.BaudRate1000, value, Model, (u, n) => u.BaudRate1000 = n);
            }
        }
        public bool BaudRateAuto
        {
            get => Model.BaudRateAuto;
            set
            {
                SetProperty(Model.BaudRateAuto, value, Model, (u, n) => u.BaudRateAuto = n);
            }
        }
        public bool LssSlave
        {
            get => Model.LssSlave;
            set
            {
                SetProperty(Model.LssSlave, value, Model, (u, n) => u.LssSlave = n);
            }
        }
        public bool LssMaster
        {
            get => Model.LssMaster;
            set
            {
                SetProperty(Model.LssMaster, value, Model, (u, n) => u.LssMaster = n);
            }
        }
    }
}
