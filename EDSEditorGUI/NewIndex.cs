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

using libEDSsharp;
using System;
using System.Windows.Forms;

namespace ODEditor
{
    public partial class NewIndex : Form
    {
        readonly EDSsharp eds;
        public ODentry od = null;

        public NewIndex(EDSsharp eds, UInt16 index)
        {
            this.eds = eds;

            InitializeComponent();

            numericUpDown_index.Value = index;

            DialogResult = DialogResult.Cancel;
        }

        private void Button_cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Button_create_Click(object sender, EventArgs e)
        {
            UInt16 index = (UInt16)numericUpDown_index.Value;
            if (eds.ods.ContainsKey(index))
            {
                MessageBox.Show(String.Format("Index 0x{0:X4} already exists in OD", index));
                return;
            }

            string name = textBox_name.Text;
            if (name == "")
            {
                MessageBox.Show("Please specify a name");
                return;
            }

            ObjectType objectType;
            if (radioButton_var.Checked)
                objectType = ObjectType.VAR;
            else if (radioButton_array.Checked)
                objectType = ObjectType.ARRAY;
            else if (radioButton_record.Checked)
                objectType = ObjectType.RECORD;
            else
            {
                MessageBox.Show("Please specify the Object Type");
                return;
            }

            // create OD entry
            if (objectType == ObjectType.VAR)
            {
                od = new ODentry
                {
                    parameter_name = name,
                    Index = index,
                    objecttype = objectType,
                    datatype = DataType.UNSIGNED32,
                    accesstype = EDSsharp.AccessType.rw,
                    defaultvalue = "0"
                };
            }
            else
            {
                od = new ODentry
                {
                    parameter_name = name,
                    Index = index,
                    objecttype = objectType,
                };

                od.subobjects.Add(0, new ODentry
                {
                    parent = od,
                    parameter_name = "Highest sub-index supported",
                    objecttype = ObjectType.VAR,
                    datatype = DataType.UNSIGNED8,
                    accesstype = EDSsharp.AccessType.ro,
                    defaultvalue = "0x01"
                });

                od.subobjects.Add(1, new ODentry
                {
                    parent = od,
                    parameter_name = "Sub Object 1",
                    objecttype = ObjectType.VAR,
                    datatype = DataType.UNSIGNED32,
                    accesstype = EDSsharp.AccessType.rw,
                    defaultvalue = "0"
                });
            }

            eds.ods.Add(od.Index, od);

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
