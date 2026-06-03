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
    Copyright(c) 2020 Janez Paternoster
*/

using libEDSsharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace ODEditor
{

    public partial class DeviceODView : MyTabUserControl
    {
        EDSsharp eds = null;
        public List<EDSsharp> network;

        ODentry selectedObject;
        ODentry lastSelectedObject;
        ListView selectedList;
        bool justUpdating = false;
        bool ExporterOld = false;
        bool ExporterV4 = false;

        public event EventHandler<UpdateODViewEventArgs> UpdateODViewForEDS;

        public DeviceODView()
        {

            InitializeComponent();
            RebuildControls();

            // other elements may be added in PopulateObjectLists()
            comboBox_countLabel.Items.Add("");
            comboBox_countLabel.Items.Add("Add...");
            comboBox_countLabel.SelectedIndexChanged += new System.EventHandler(this.ComboBox_countLabel_Add);
            comboBox_storageGroup.Items.Add("Add...");
            comboBox_storageGroup.SelectedIndexChanged += new System.EventHandler(this.ComboBox_storageGroup_Add);

            listView_communication_objects.DoubleBuffering(true);
            listView_deviceProfile_objects.DoubleBuffering(true);
            listView_manufacturer_objects.DoubleBuffering(true);
            listView_subObjects.DoubleBuffering(true);
        }


        public void RebuildControls()
        {
            if (ExporterTypeV4())
            {
                comboBox_dataType.Items.Clear();
                comboBox_dataType.Items.Add(DataType.BOOLEAN.ToString());
                comboBox_dataType.Items.Add(DataType.INTEGER8.ToString());
                comboBox_dataType.Items.Add(DataType.INTEGER16.ToString());
                comboBox_dataType.Items.Add(DataType.INTEGER32.ToString());
                comboBox_dataType.Items.Add(DataType.INTEGER64.ToString());
                comboBox_dataType.Items.Add(DataType.UNSIGNED8.ToString());
                comboBox_dataType.Items.Add(DataType.UNSIGNED16.ToString());
                comboBox_dataType.Items.Add(DataType.UNSIGNED32.ToString());
                comboBox_dataType.Items.Add(DataType.UNSIGNED64.ToString());
                comboBox_dataType.Items.Add(DataType.REAL32.ToString());
                comboBox_dataType.Items.Add(DataType.REAL64.ToString());
                comboBox_dataType.Items.Add(DataType.VISIBLE_STRING.ToString());
                comboBox_dataType.Items.Add(DataType.OCTET_STRING.ToString());
                comboBox_dataType.Items.Add(DataType.UNICODE_STRING.ToString());
                comboBox_dataType.Items.Add(DataType.DOMAIN.ToString());

                comboBox_objectType.Items.Clear();
                comboBox_objectType.Items.Add(ObjectType.VAR.ToString());
                comboBox_objectType.Items.Add(ObjectType.ARRAY.ToString());
                comboBox_objectType.Items.Add(ObjectType.RECORD.ToString());

                comboBox_accessSDO.Items.Clear();
                foreach (AccessSDO foo in Enum.GetValues(typeof(AccessSDO)))
                    comboBox_accessSDO.Items.Add(foo.ToString());

                comboBox_accessPDO.Items.Clear();
                foreach (AccessPDO foo in Enum.GetValues(typeof(AccessPDO)))
                    comboBox_accessPDO.Items.Add(foo.ToString());

                label_pdoFlags.Visible = false;
                checkBox_pdoFlags.Visible = false;
            }
            else
            {
                comboBox_dataType.Items.Clear();
                foreach (DataType foo in Enum.GetValues(typeof(DataType)))
                    comboBox_dataType.Items.Add(foo.ToString());
                comboBox_objectType.Items.Clear();
                foreach (ObjectType foo in Enum.GetValues(typeof(ObjectType)))
                    comboBox_objectType.Items.Add(foo.ToString());
                comboBox_accessSDO.Items.Clear();
                foreach (EDSsharp.AccessType foo in Enum.GetValues(typeof(EDSsharp.AccessType)))
                    comboBox_accessSDO.Items.Add(foo.ToString());
                comboBox_accessPDO.Items.Clear();
                foreach (PDOMappingType foo in Enum.GetValues(typeof(PDOMappingType)))
                    comboBox_accessPDO.Items.Add(foo.ToString());

                label_pdoFlags.Visible = true;
                checkBox_pdoFlags.Visible = true;
            }

            comboBox_accessSRDO.Items.Clear();
            foreach (AccessSRDO foo in Enum.GetValues(typeof(AccessSRDO)))
                comboBox_accessSRDO.Items.Add(foo.ToString());


        }
        private bool ExporterTypeV4()
        {
            ExporterFactory.Exporter type = (ExporterFactory.Exporter)Properties.Settings.Default.ExporterType;
            return (type == ExporterFactory.Exporter.CANOPENNODE_V4);
        }

        private bool Checkdirty()
        {
            var result = false;

            if (button_saveChanges.BackColor == Color.Tomato)
            {

                var answer = checkBox_autosave.Checked
                           ? DialogResult.No
                           : MessageBox.Show(String.Format("Unsaved changes on Index 0x{0:X4}/{1:X2}.\nDo you wish to switch object and loose your changes?\n\nYes = Lose changes\nNo = Save\nCancel = Go back and stay on the object", lastSelectedObject.Index, lastSelectedObject.Subindex), "Unsaved changes", MessageBoxButtons.YesNoCancel); ;


                switch (answer)
                {
                    case DialogResult.Cancel:
                    default:
                        result = lastSelectedObject != null;
                        break;

                    case DialogResult.Yes:
                        result = false;
                        break;

                    case DialogResult.No:
                        if (lastSelectedObject != null)
                        {
                            ObjectSave();
                            result = false;
                        }
                        break;
                }

                button_saveChanges.BackColor = default;
            }

            return result;
        }

        private void ComboBoxSet(ComboBox comboBox, string item)
        {
            if (item == null)
                item = "";

            if (!comboBox.Items.Contains(item))
                comboBox.Items.Add(item);

            comboBox.SelectedItem = item;
        }

        public void PopulateObjectLists(EDSsharp eds_target)
        {
            if (eds_target == null)
                return;

            eds = eds_target;
            eds.UpdatePDOcount();
            doUpdateDeviceInfo();
            doUpdatePDOs();

            /* save scroll positions */
            int listview_communication_position = 0;
            int listview_manufacturer_position = 0;
            int listview_deviceProfile_position = 0;

            if (listView_communication_objects.TopItem != null)
                listview_communication_position = listView_communication_objects.TopItem.Index;
            if (listView_manufacturer_objects.TopItem != null)
                listview_manufacturer_position = listView_manufacturer_objects.TopItem.Index;
            if (listView_deviceProfile_objects.TopItem != null)
                listview_deviceProfile_position = listView_deviceProfile_objects.TopItem.Index;

            /* prevent flickering */
            listView_communication_objects.BeginUpdate();
            listView_manufacturer_objects.BeginUpdate();
            listView_deviceProfile_objects.BeginUpdate();

            listView_communication_objects.Items.Clear();
            listView_manufacturer_objects.Items.Clear();
            listView_deviceProfile_objects.Items.Clear();

            foreach (ODentry od in eds.ods.Values)
            {
                UInt16 index = od.Index;
                ListViewItem lvi = new ListViewItem(new string[] {
                    string.Format("0x{0:X4}", index),
                    od.parameter_name
                });

                lvi.Tag = od;
                if (selectedObject != null && index == selectedObject.Index)
                    lvi.Selected = true;
                if (od.prop.CO_disabled == true)
                    lvi.ForeColor = Color.LightGray;

                if (index >= 0x1000 && index < 0x2000)
                    listView_communication_objects.Items.Add(lvi);
                else if (index >= 0x2000 && index < 0x6000)
                    listView_manufacturer_objects.Items.Add(lvi);
                else
                    listView_deviceProfile_objects.Items.Add(lvi);

                string countLabel = od.prop.CO_countLabel;
                if (!comboBox_countLabel.Items.Contains(countLabel))
                    comboBox_countLabel.Items.Insert(comboBox_countLabel.Items.Count - 1, countLabel);

                string storageGroup = od.prop.CO_storageGroup;
                if (!comboBox_storageGroup.Items.Contains(storageGroup))
                    comboBox_storageGroup.Items.Insert(comboBox_storageGroup.Items.Count - 1, storageGroup);
            }

            listView_communication_objects.EndUpdate();
            listView_manufacturer_objects.EndUpdate();
            listView_deviceProfile_objects.EndUpdate();

            /* reset scroll position and selection */
            if (listview_communication_position != 0 && listView_communication_objects.Items.Count > 0)
                listView_communication_objects.TopItem = listView_communication_objects.Items[listview_communication_position];
            if (listview_manufacturer_position != 0 && listView_manufacturer_objects.Items.Count > 0)
                listView_manufacturer_objects.TopItem = listView_manufacturer_objects.Items[listview_manufacturer_position];
            if (listview_deviceProfile_position != 0 && listView_deviceProfile_objects.Items.Count > 0)
                listView_deviceProfile_objects.TopItem = listView_deviceProfile_objects.Items[listview_deviceProfile_position];
        }

        public void PopulateSubList()
        {
            listView_subObjects.Items.Clear();

            if (selectedObject == null)
                return;
            ODentry od = selectedObject.parent ?? selectedObject;

            if (od.objecttype == ObjectType.VAR)
            {
                ListViewItem lvi = new ListViewItem(new string[] {
                    " ", // subindex
                    od.parameter_name,
                    od.ObjectTypeString(),
                    od.datatype.ToString(),
                    od.AccessSDO().ToString(),
                    od.AccessPDO().ToString(),
                    od.prop.CO_accessSRDO.ToString(),
                    od.defaultvalue
                });
                lvi.Tag = od;
                listView_subObjects.Items.Add(lvi);
            }
            else if (od.objecttype == ObjectType.ARRAY || od.objecttype == ObjectType.RECORD)
            {
                ListViewItem lvi = new ListViewItem(new string[]{
                    " ",
                    od.parameter_name,
                    od.ObjectTypeString()
                });
                lvi.Tag = od;
                listView_subObjects.Items.Add(lvi);

                foreach (KeyValuePair<UInt16, ODentry> kvp in od.subobjects)
                {
                    ODentry subod = kvp.Value;
                    int subindex = kvp.Key;

                    ListViewItem lvi2 = new ListViewItem(new string[] {
                        string.Format("0x{0:X2}", subindex),
                        subod.parameter_name,
                        subod.ObjectTypeString(),
                        (subod.datatype != DataType.UNKNOWN) ? subod.datatype.ToString() : od.datatype.ToString(),
                        subod.AccessSDO().ToString(),
                        subod.AccessPDO().ToString(),
                        subod.prop.CO_accessSRDO.ToString(),
                        subod.defaultvalue
                    });
                    lvi2.Tag = subod;
                    listView_subObjects.Items.Add(lvi2);
                }
            }
        }

        public void PopulateObject()
        {

            ExporterV4 = ExporterTypeV4();
            if (ExporterOld != ExporterV4)
            {
                RebuildControls();
                ExporterOld = ExporterV4;
            }

            justUpdating = true;
            lastSelectedObject = selectedObject;


            if (selectedObject == null)
            {
                textBox_index.Text = "";
                textBox_subIndex.Text = "";
                textBox_name.Text = "";
                textBox_denotation.Text = "";
                textBox_description.Text = "";
                justUpdating = false;
                return;
            }

            ODentry od = selectedObject;

            textBox_index.Text = string.Format("0x{0:X4}", od.Index);
            textBox_name.Text = od.parameter_name;
            textBox_denotation.Text = od.denotation;
            textBox_description.Text = (od.Description == null) ? "" : Regex.Replace(od.Description, "(?<!\r)\n", "\r\n");

            comboBox_objectType.SelectedItem = od.ObjectTypeString();

            if (od.objecttype == ObjectType.VAR)
            {
                comboBox_dataType.Enabled = true;
                comboBox_accessSDO.Enabled = true;
                comboBox_accessPDO.Enabled = true;
                comboBox_accessSRDO.Enabled = true;

                textBox_defaultValue.Enabled = true;
                textBox_actualValue.Enabled = true;
                textBox_highLimit.Enabled = true;
                textBox_lowLimit.Enabled = true;
                textBox_stringLengthMin.Enabled = true;

                string dataType = (od.datatype == DataType.UNKNOWN && od.parent != null)
                                ? od.parent.datatype.ToString()
                                : od.datatype.ToString();
                ComboBoxSet(comboBox_dataType, dataType);

                if (ExporterV4)
                {
                    comboBox_accessSDO.SelectedItem = od.AccessSDO().ToString();
                    comboBox_accessPDO.SelectedItem = od.AccessPDO().ToString();
                }
                else
                {
                    comboBox_accessSDO.SelectedItem = od.accesstype.ToString();
                    comboBox_accessPDO.SelectedItem = od.PDOtype.ToString();
                }
                comboBox_accessSRDO.SelectedItem = od.prop.CO_accessSRDO.ToString();

                textBox_defaultValue.Text = od.defaultvalue;
                textBox_actualValue.Text = od.actualvalue;
                textBox_highLimit.Text = od.HighLimit;
                textBox_lowLimit.Text = od.LowLimit;
                textBox_stringLengthMin.Text = od.prop.CO_stringLengthMin.ToString();
            }
            else
            {
                comboBox_dataType.SelectedItem = null;
                comboBox_accessSDO.SelectedItem = null;
                comboBox_accessPDO.SelectedItem = null;
                comboBox_accessSRDO.SelectedItem = null;

                textBox_defaultValue.Text = "";
                textBox_actualValue.Text = "";
                textBox_highLimit.Text = "";
                textBox_lowLimit.Text = "";
                textBox_stringLengthMin.Text = "";

                comboBox_dataType.Enabled = false;
                comboBox_accessSDO.Enabled = false;
                comboBox_accessPDO.Enabled = false;
                comboBox_accessSRDO.Enabled = false;

                textBox_defaultValue.Enabled = false;
                textBox_actualValue.Enabled = false;
                textBox_highLimit.Enabled = false;
                textBox_lowLimit.Enabled = false;
                textBox_stringLengthMin.Enabled = false;
            }

            ODentry odBase;
            if (od.parent == null)
            {
                odBase = od;
                textBox_subIndex.Text = "";
                comboBox_countLabel.Enabled = true;
                comboBox_storageGroup.Enabled = true;
                checkBox_enabled.Enabled = true;
                checkBox_pdoFlags.Enabled = true;
            }
            else
            {
                odBase = od.parent;
                textBox_subIndex.Text = string.Format("0x{0:X2}", od.Subindex);
                comboBox_countLabel.Enabled = false;
                comboBox_storageGroup.Enabled = false;
                checkBox_enabled.Enabled = false;
                checkBox_pdoFlags.Enabled = false;
            }

            ComboBoxSet(comboBox_countLabel, odBase.prop.CO_countLabel);
            ComboBoxSet(comboBox_storageGroup, odBase.prop.CO_storageGroup);
            checkBox_enabled.Checked = !odBase.prop.CO_disabled;
            checkBox_pdoFlags.Checked = odBase.prop.CO_flagsPDO;

            justUpdating = false;
            return;
        }

        private void DataDirty(object sender, EventArgs e)
        {
            if (!justUpdating)
                button_saveChanges.BackColor = Color.Tomato;
        }

        private void Button_saveChanges_Click(object sender, EventArgs e)
        {
            ObjectSave();
        }

        private void ObjectSave()
        {
            ExporterV4 = ExporterTypeV4();
            if (ExporterOld != ExporterV4)
            {
                RebuildControls();
                ExporterOld = ExporterV4;
            }

            if (selectedObject == null)
                return;

            eds.Dirty = true;
            button_saveChanges.BackColor = default;
            ODentry od = selectedObject;

            od.parameter_name = textBox_name.Text;
            od.denotation = textBox_denotation.Text;
            od.Description = textBox_description.Text.Replace("\r\n", "\n");
            od.ObjectTypeString(od.parent == null ? comboBox_objectType.SelectedItem.ToString() : "VAR");

            if (od.objecttype == ObjectType.VAR)
            {
                // dataType
                try
                {
                    od.datatype = (DataType)Enum.Parse(typeof(DataType), comboBox_dataType.SelectedItem.ToString());
                }
                catch (Exception)
                {
                    od.datatype = DataType.UNKNOWN;
                }

                if (ExporterV4)
                {
                    AccessSDO accessSDO;
                    try
                    {
                        accessSDO = (AccessSDO)Enum.Parse(typeof(AccessSDO), comboBox_accessSDO.SelectedItem.ToString());
                    }
                    catch (Exception)
                    {
                        accessSDO = AccessSDO.ro;
                    }

                    AccessPDO accessPDO;
                    try
                    {
                        accessPDO = (AccessPDO)Enum.Parse(typeof(AccessPDO), comboBox_accessPDO.SelectedItem.ToString());
                    }
                    catch (Exception)
                    {
                        accessPDO = AccessPDO.no;
                    }

                    od.AccessSDO(accessSDO, accessPDO);
                    od.AccessPDO(accessPDO);
                }
                else
                {
                    try
                    {
                        od.accesstype = (EDSsharp.AccessType)Enum.Parse(typeof(EDSsharp.AccessType), comboBox_accessSDO.SelectedItem.ToString());
                    }
                    catch (Exception)
                    {
                        od.accesstype = EDSsharp.AccessType.ro;
                    }

                    try
                    {
                        od.PDOtype = (PDOMappingType)Enum.Parse(typeof(PDOMappingType), comboBox_accessPDO.SelectedItem.ToString());
                    }
                    catch (Exception)
                    {
                        od.PDOtype = PDOMappingType.no;
                    }
                }

                // CO_accessSRDO
                try
                {
                    if (comboBox_accessSRDO.SelectedItem != null)
                        od.prop.CO_accessSRDO = (AccessSRDO)Enum.Parse(typeof(AccessSRDO), comboBox_accessSRDO.SelectedItem.ToString());
                }
                catch (Exception)
                {
                    od.prop.CO_accessSRDO = AccessSRDO.no;
                }

                // Default value
                if (listView_subObjects.SelectedItems.Count > 1)
                {
                    for (ushort i = 0; i < listView_subObjects.SelectedItems.Count; i++)
                    {
                        od.parent.subobjects[(ushort)Convert.ToInt32(listView_subObjects.SelectedItems[i].Text, 16)].defaultvalue = textBox_defaultValue.Text;
                    }
                }


                bool setDefaultValueToAll = false;
                bool identicalDefaultValues = true;
                string lastdefaultvalue;
                if (od.parent != null && od.parent.Nosubindexes > 2)
                {
                    lastdefaultvalue = od.parent.subobjects[1].defaultvalue;
                    foreach (ODentry subod in od.parent.subobjects.Values)
                    {
                        if (subod.Subindex > 0)
                        {
                            identicalDefaultValues &= (subod.defaultvalue == lastdefaultvalue) && (subod.defaultvalue != textBox_defaultValue.Text);
                            lastdefaultvalue = subod.defaultvalue;
                        }
                    }

                    if (identicalDefaultValues)
                    {
                        DialogResult confirm = MessageBox.Show("Do you want to set all identical default values in subobjects to this default value?", "Set to all?", MessageBoxButtons.YesNo);
                        if (confirm == DialogResult.Yes)
                        {
                            setDefaultValueToAll = true;
                        }
                    }
                }
                if (setDefaultValueToAll)
                {
                    for (ushort i = 1; i < od.parent.Nosubindexes; i++)
                    {
                        od.parent.subobjects[i].defaultvalue = textBox_defaultValue.Text;
                    }
                }
                else
                {
                    od.defaultvalue = textBox_defaultValue.Text;
                }

                od.actualvalue = textBox_actualValue.Text;
                od.HighLimit = textBox_highLimit.Text;
                od.LowLimit = textBox_lowLimit.Text;

                // CO_stringLengthMin
                if (od.datatype == DataType.VISIBLE_STRING || od.datatype == DataType.UNICODE_STRING || od.datatype == DataType.OCTET_STRING)
                {
                    try
                    {
                        od.prop.CO_stringLengthMin = (uint)new System.ComponentModel.UInt32Converter().ConvertFromString(textBox_stringLengthMin.Text);
                    }
                    catch (Exception)
                    {
                        od.prop.CO_stringLengthMin = 0;
                    }
                }
                else
                {
                    od.prop.CO_stringLengthMin = 0;
                }

                // some propeties in all array sub elements (and base element) must be equal
                if (od.parent != null && od.parent.objecttype == ObjectType.ARRAY && od.Subindex > 0)
                {
                    foreach (ODentry subod in od.parent.subobjects.Values)
                    {
                        if (subod.Subindex > 0)
                        {
                            subod.datatype = od.datatype;
                            subod.accesstype = od.accesstype;
                            subod.PDOtype = od.PDOtype;
                            subod.prop.CO_accessSRDO = od.prop.CO_accessSRDO;
                        }
                    }
                    od.parent.datatype = od.datatype;
                    od.parent.accesstype = od.accesstype;
                    od.parent.PDOtype = od.PDOtype;
                    od.parent.prop.CO_accessSRDO = od.prop.CO_accessSRDO;
                }
            }

            if (od.parent == null)
            {
                od.prop.CO_countLabel = comboBox_countLabel.SelectedItem.ToString();
                od.prop.CO_storageGroup = comboBox_storageGroup.SelectedItem.ToString();
                od.prop.CO_disabled = !checkBox_enabled.Checked;
                od.prop.CO_flagsPDO = checkBox_pdoFlags.Checked;
            }

            PopulateObjectLists(eds);
            PopulateSubList();
            PopulateObject();
        }

        private void ListView_objects_MouseClick(object sender, MouseEventArgs e)
        {
            ListView listview = (ListView)sender;

            if (listview.SelectedItems.Count <= 0)
                return;

            ODentry od = (ODentry)listview.SelectedItems[0].Tag;

            if ((od != selectedObject || e.Button == MouseButtons.Right) && !Checkdirty())
            {
                selectedList = listview;
                selectedObject = od;

                if (e.Button == MouseButtons.Right)
                {
                    contextMenu_object.Show(Cursor.Position);
                }

                PopulateObject();
                PopulateSubList();
            }
            else
            {
                //selectedObject = lastSelectedObject;
                //od = selectedObject;
                //selectedList.Select();
                //string indesnew = "0x" + Convert.ToString(selectedObject.Index, 16);
                //ListViewItem itemnew = selectedList.FindItemWithText(indesnew);
                //listview.FocusedItem = itemnew;
                //listView_subObjects.Focus();

            }
            listView_communication_objects.HideSelection = true;
            listView_deviceProfile_objects.HideSelection = true;
            listView_manufacturer_objects.HideSelection = true;
        }

        private void ListView_objects_SelectedIndexChanged(object sender, EventArgs e)
        {

            ListView_objects_MouseClick(sender, new MouseEventArgs(MouseButtons.None, 0, 0, 0, 0));
        }

        private void ListView_objects_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ((ListView)sender).SelectedItems.Clear();

            contextMenu_object.Show(Cursor.Position);
            PopulateObject();
            PopulateSubList();

            ListView_objects_MouseClick(sender, new MouseEventArgs(MouseButtons.Right, 0, 0, 0, 0));
        }

        private void ListView_subObjects_MouseClick(object sender, MouseEventArgs e)
        {
            if (listView_subObjects.SelectedItems.Count == 0)
                return;

            ODentry od = (ODentry)listView_subObjects.SelectedItems[0].Tag;

            if ((od != selectedObject || e.Button == MouseButtons.Right) && !Checkdirty())
            {
                if (e.Button == MouseButtons.Right)
                {
                    ODentry parent = od.parent ?? od;

                    if (parent.objecttype == ObjectType.ARRAY || parent.objecttype == ObjectType.RECORD)
                    {
                        contextMenu_subObject_removeSubItemToolStripMenuItem.Enabled = od.Subindex > 0 && od.parent != null;
                        contextMenu_subObject_removeSubItemLeaveGapToolStripMenuItem.Enabled = parent.objecttype == ObjectType.RECORD && od.Subindex > 0 && od.parent != null;

                        if (isClickOnItem(e.Location))
                        {
                            contextMenu_subObject.Show(Cursor.Position);
                        }
                    }
                }
                selectedObject = od;
                PopulateObject();
            }
        }

        private bool isClickOnItem(Point location)
        {
            if (listView_subObjects.FocusedItem != null)
            {
                return listView_subObjects.FocusedItem.Bounds.Contains(location);
            }

            foreach (ListViewItem item in listView_subObjects.Items)
            {
                if (item.Bounds.Contains(location))
                {
                    return true;
                }
            }

            return false;
        }

        private void ListView_subObjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView_subObjects_MouseClick(sender, new MouseEventArgs(MouseButtons.None, 0, 0, 0, 0));
        }

        private void ComboBox_countLabel_Add(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            if (comboBox.SelectedItem != null && comboBox.SelectedItem.ToString() == "Add...")
            {
                NewItem dialog = new NewItem("Add Count Label");
                if (dialog.ShowDialog() == DialogResult.OK && comboBox.FindStringExact(dialog.name) == -1)
                {
                    comboBox.Items.Insert(comboBox.Items.Count - 1, dialog.name);
                    comboBox.SelectedItem = dialog.name;
                }
            }
        }

        private void ComboBox_storageGroup_Add(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            if (comboBox.SelectedItem != null && comboBox.SelectedItem.ToString() == "Add...")
            {
                NewItem dialog = new NewItem("Add Storage Group");
                if (dialog.ShowDialog() == DialogResult.OK && comboBox.FindStringExact(dialog.name) == -1)
                {
                    comboBox.Items.Insert(comboBox.Items.Count - 1, dialog.name);
                    comboBox.SelectedItem = dialog.name;
                    /* add new dialog location to eds back end */
                    eds.CO_storageGroups.Add(dialog.name);
                }
            }
        }

        private void ContextMenu_object_clone_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var srcObjects = new SortedDictionary<UInt16, ODentry>();
            foreach (ListViewItem item in selectedList.SelectedItems)
            {
                ODentry od = (ODentry)item.Tag;
                srcObjects.Add(od.Index, od);
            }

            if (srcObjects.Count > 0)
            {
                InsertObjects insObjForm = new InsertObjects(eds, network, srcObjects, "1");

                if (insObjForm.ShowDialog() == DialogResult.OK)
                {
                    selectedObject = null;
                    EDSsharp modifiedEds = insObjForm.GetModifiedEDS();
                    modifiedEds.Dirty = true;
                    if (modifiedEds == this.eds)
                    {
                        PopulateObjectLists(eds);
                        PopulateSubList();
                        PopulateObject();
                    }
                    else
                    {
                        UpdateODViewForEDS?.Invoke(this, new UpdateODViewEventArgs(modifiedEds));
                    }
                }
            }
        }

        private void ContextMenu_object_add_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewIndex ni = new NewIndex(eds, (UInt16)(selectedObject == null ? 0x2000 : selectedObject.Index + 1));

            if (ni.ShowDialog() == DialogResult.OK)
            {
                selectedObject = ni.od;
                eds.Dirty = true;
                PopulateObjectLists(eds);
                PopulateSubList();
                PopulateObject();
            }
        }

        private void ContextMenu_object_delete_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection selectedItems = selectedList.SelectedItems;
            if (selectedItems.Count > 0)
            {
                DialogResult confirmDelete = MessageBox.Show(string.Format("Do you really want to delete the selected {0} items?", selectedItems.Count), "Are you sure?", MessageBoxButtons.YesNo);

                if (confirmDelete == DialogResult.Yes)
                {
                    foreach (ListViewItem item in selectedItems)
                    {
                        ODentry od = (ODentry)item.Tag;
                        eds.ods.Remove(od.Index);
                    }

                    eds.Dirty = true;
                    selectedObject = null;
                    PopulateObjectLists(eds);
                    PopulateSubList();
                    PopulateObject();
                }
            }
        }

        private void ContextMenu_object_toggle_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection selectedItems = selectedList.SelectedItems;

            justUpdating = true;
            foreach (ListViewItem item in selectedItems)
            {
                ODentry od = (ODentry)item.Tag;

                od.prop.CO_disabled = !od.prop.CO_disabled;
            }
            justUpdating = false;
            eds.Dirty = true;
            PopulateObjectLists(eds);
            PopulateObject();
        }

        private void ContextMenu_subObject_add_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection selectedItems = listView_subObjects.SelectedItems;

            ODentry newOd = null;

            foreach (ListViewItem item in selectedItems)
            {
                ODentry od = (ODentry)item.Tag;
                newOd = od.AddSubEntry();
            }

            eds.Dirty = true;
            selectedObject = newOd;
            PopulateSubList();
            PopulateObject();
        }

        private void ContextMenu_subObject_remove_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection selectedItems = listView_subObjects.SelectedItems;
            bool renumber = sender == contextMenu_subObject_removeSubItemToolStripMenuItem;
            bool update = false;

            foreach (ListViewItem item in selectedItems)
            {
                ODentry od = (ODentry)item.Tag;
                od.RemoveSubEntry(renumber);
                update = true;
            }

            if (update)
            {
                eds.Dirty = true;
                selectedObject = selectedObject.parent;
                PopulateSubList();
                PopulateObject();
            }
        }
    }

    public static class ControlExtensions
    {
        public static void DoubleBuffering(this Control control, bool enable)
        {
            var method = typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(control, new object[] { ControlStyles.OptimizedDoubleBuffer, enable });
        }
    }
}
