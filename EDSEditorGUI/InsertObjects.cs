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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using libEDSsharp;

namespace ODEditor
{
    public partial class InsertObjects : Form
    {
        private EDSsharp eds;
        readonly SortedDictionary<UInt16, ODentry> srcObjects;
        readonly bool[] enabled;
        readonly int dataGridView_InitialColumnCount;
        List<EDSsharp> network;
        List<int> offsets;

        /// <summary>
        /// Display form to: specify offset(s), insert clone of selected srcObjects into existing eds
        /// </summary>
        /// <param name="eds"></param>
        /// <param name="srcObjects"></param>
        public InsertObjects(EDSsharp eds, List<EDSsharp> network, SortedDictionary<UInt16, ODentry> srcObjects, string initialOffset)
        {
            this.eds = eds;
            this.srcObjects = srcObjects;
            this.network = network;

            InitializeComponent();

            cbox_Target.Items.Clear();
            foreach(var item in network)
            {
                cbox_Target.Items.Add(item.di.ProductName);
            }
            cbox_Target.SelectedItem = eds.di.ProductName;

            textBox_offsets.Text = initialOffset;

            dataGridView.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = "Insert",
                FalseValue = false,
                TrueValue = true,
                Visible = true
            });
            int idx = dataGridView.Columns.Add("Original Object", "Original Object");
            dataGridView.Columns[idx].ReadOnly = true;
            dataGridView_InitialColumnCount = dataGridView.ColumnCount;

            enabled = new bool[srcObjects.Count];
            for(int i = 0; i < enabled.Length; i++)
                enabled[i] = true;

            Verify(initialOffset == "0");

            DialogResult = DialogResult.Cancel;
        }

        private void Button_cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private bool Verify(bool InitiallyDisableIfError = false)
        {
            String pattern = @"(\d+)\s*([-])\s*(\d+)";
            /* get offests */
            offsets = new List<int>();

            if (textBox_offsets.Text.Contains("-"))
            {
                MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(textBox_offsets.Text, pattern);
                if (matches.Count != 0)
                {
                    foreach (System.Text.RegularExpressions.Match m in matches)
                    {
                        int value1 = Int32.Parse(m.Groups[1].Value);
                        int value2 = Int32.Parse(m.Groups[3].Value);

                        if ((value2 > value1) && (m.Groups[2].Value == "-"))
                        {
                            for (int i = value1; i <= value2; i++)
                            {
                                try
                                {
                                    offsets.Add(i);
                                }
                                catch (Exception)
                                {
                                    MessageBox.Show("Syntax error in Index Offset!\n\nValid value is single signed number or space separated list of multiple signed numbers or a range seprated by '-'.");
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                string[] str = textBox_offsets.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in str)
                {
                    try
                    {
                        int value = (int)new System.ComponentModel.Int32Converter().ConvertFromString(s);
                        offsets.Add(value);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Syntax error in Index Offset!\n\nValid value is single signed number or space separated list of multiple signed numbers or a range seprated by '-'.");
                        return false;
                    }
                }
            }
            // write column headers
            int colIdx = dataGridView_InitialColumnCount;
            dataGridView.ColumnCount = colIdx + offsets.Count;
            foreach (int o in offsets)
            {
                dataGridView.Columns[colIdx].Name = $"Offset {o}";
                dataGridView.Columns[colIdx++].ReadOnly = true;
            }

            // write rows and verify errors
            bool newIndexesValid = true;
            var newIndexes = new List<int>();
            DataGridViewCellStyle styleErr = new DataGridViewCellStyle
            {
                Font = new Font(dataGridView.Font, FontStyle.Bold),
                ForeColor = Color.Tomato
            };

            dataGridView.Rows.Clear();

            int odIdx = 0;
            int j = 1;
            foreach (ODentry od in srcObjects.Values)
            {
                String numpattern = @"(\w*\d+\Z)";
                string newname;

                string[] words = Regex.Split(od.parameter_name, numpattern);
                // MatchCollection nummatches = System.Text.RegularExpressions.Regex.Matches(od.parameter_name, numpattern);
                if (words.Length > 1)
                {

                    // value1 = Int32.Parse(m.Groups[1].Value);
                    int nameidx = Int32.Parse(words[1]) + j++;

                    newname = words[0] + nameidx;// nameidx.ToString  ;

                }
                else {
                     newname = od.parameter_name;
                }
                    int rowIdx = dataGridView.Rows.Add(enabled[odIdx], $"0x{od.Index:X4} - {newname}");
                int cellIdx = dataGridView_InitialColumnCount;

                foreach (int o in offsets)
                {
                    int newIndex = (int)od.Index + o;
                    bool err = newIndex <= 0 || newIndex > 0xFFFF;

                    dataGridView.Rows[rowIdx].Cells[cellIdx].Value = err ? $"{newIndex}" : $"0x{newIndex:X4}";

                    if (!err)
                        err = newIndexes.Contains(newIndex);

                    if (!err)
                    {
                        
                        newIndexes.Add(newIndex);
                        err = eds.ods.ContainsKey((UInt16)newIndex);
                    }

                    if (err)
                    {
                        if (enabled[odIdx] && InitiallyDisableIfError)
                        {
                            dataGridView.Rows[rowIdx].Cells[0].Value = false;
                            enabled[odIdx] = false;
                        }
                        else if (enabled[odIdx])
                        {
                            newIndexesValid = false;
                        }
                        dataGridView.Rows[rowIdx].Cells[cellIdx].Style = styleErr;
                    }
                    cellIdx++;
                }
                odIdx++;
            }

            dataGridView.ClearSelection();

            return newIndexesValid;
        }

        private void Button_create_Click(object sender, EventArgs e)
        {
            if (Verify())
            {
                // clone OD objects
                int odIdx = 0;
                int i = 1;
                foreach (ODentry od in srcObjects.Values)
                {
                    if (enabled[odIdx])
                    {
                        foreach (int o in offsets)
                        {
                            UInt16 newIndex = (UInt16)(od.Index + o);
                            ODentry newObject = od.Clone();

                            String pattern = @"(\w*\d+\Z)";

                            string[] words = Regex.Split(od.parameter_name, pattern);
                            // MatchCollection nummatches = System.Text.RegularExpressions.Regex.Matches(od.parameter_name, numpattern);
                            if (words.Length > 1)
                            {
                                int nameidx = Int32.Parse(words[1]) + i++;
                                newObject.parameter_name = words[0] + nameidx;// nameidx.ToString  ;

                            }

                            newObject.Index = newIndex;

                            eds.ods.Add(newIndex, newObject);
                        }
                    }
                    odIdx++;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void TextBox_offsets_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ' ' || e.KeyChar == '\r')
            {
                Verify();
            }
        }

        private void TextBox_offsets_Leave(object sender, EventArgs e)
        {
            Verify();
        }

        private void DataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // toggle selection on click on first column
            if (e.ColumnIndex == 0)
            {
                dataGridView.ClearSelection();
                dataGridView.CurrentCell = null;
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    row.Cells[0].Value = !((bool)row.Cells[0].Value);
                }
            }
        }

        private void DataGridView_Leave(object sender, EventArgs e)
        {
            // recalculate enabled[]
            dataGridView.ClearSelection();
            dataGridView.CurrentCell = null;
            int odIdx = 0;
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                enabled[odIdx++] = (bool)row.Cells[0].Value;
            }
        }

        private void button_uncheck_Click(object sender, EventArgs e)
        {
            int odIdx = 0;
            foreach (DataGridViewRow item in dataGridView.Rows)
            {
                DataGridViewCheckBoxCell cell = (DataGridViewCheckBoxCell)item.Cells[0];

                cell.Value = false;
                enabled[odIdx++] = false;
            }
        }

        private void cbox_Target_SelectedIndexChanged(object sender, EventArgs e)
        {
            EDSsharp edsBefore = eds;
            foreach (var item in network)
            {
                if((string)cbox_Target.SelectedItem == item.di.ProductName)
                {
                    this.eds = item;
                    break;
                }
            }
            if(this.eds != null && this.eds != edsBefore)
            {
                Verify(false);
            }
        }

        public EDSsharp GetModifiedEDS()
        {
            return this.eds;
        }
    }
}
