/*
    This file is part of libEDSsharp.

    libEDSsharp is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    libEDSsharp is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with libEDSsharp.  If not, see <http://www.gnu.org/licenses/>.

    Copyright(c) 2016 - 2019 Robin Cornelius <robin.cornelius@gmail.com>
*/

using System.Windows.Forms;


namespace ODEditor
{
    public class MyTabUserControl : UserControl
    {
        #region events

        public delegate void UpdatePDOs_Handler();
        public event UpdatePDOs_Handler UpdatePDOs;

        public void doUpdatePDOs()
        {
            if (UpdatePDOs != null)
                UpdatePDOs();
        }

        public delegate void UpdateDeviceInfo_Handler();
        public event UpdateDeviceInfo_Handler UpdateDeviceInfo;

        public void doUpdateDeviceInfo()
        {
            if (UpdateDeviceInfo != null)
                UpdateDeviceInfo();
        }

        public delegate void UpdateOD_Handler();
        public event UpdateOD_Handler UpdateOD;

        public void doUpdateOD()
        {
            if (UpdateOD != null)
                UpdateOD();
        }

        #endregion

        private void InitializeComponent()
        {


            this.SuspendLayout();
            // 
            // MyTabUserControl
            // 
            this.Name = "MyTabUserControl";
            this.Size = new System.Drawing.Size(357, 262);
            this.ResumeLayout(false);

        }
    }
}
