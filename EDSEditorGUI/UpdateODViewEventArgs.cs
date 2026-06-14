using libEDSsharp;
using System;

namespace ODEditor
{
    public class UpdateODViewEventArgs : EventArgs
    {
        public EDSsharp EDS { get; set; }

        public UpdateODViewEventArgs(EDSsharp eds)
        {
            EDS = eds;
        }
    }
}
