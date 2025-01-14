using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using libEDSsharp;
using System.Collections.Specialized;
using SourceGrid;

namespace ODEditor
{
    public partial class DevicePDOView2 : MyTabUserControl
    {

        libEDSsharp.PDOHelper helper;
        bool isTXPDO;

        StringCollection TXchoices = new StringCollection();
        string[] srray;

        PDOSlot selectedslot = null;

        CellBackColorAlternate viewNormal = new CellBackColorAlternate(Color.Khaki, Color.LemonChiffon);
        CellBackColorAlternate viewEmpty = new CellBackColorAlternate(Color.LightGray, Color.Gainsboro);
        CellBackColorAlternate viewCOB = new CellBackColorAlternate(Color.LightBlue, Color.LightCyan);

        Point RightClickPoint = new Point(0, 0);

        public DevicePDOView2()
        {
            InitializeComponent();

            grid1.Redim(2, 67);
            grid1.FixedRows = 2;

            grid1.SelectionMode = SourceGrid.GridSelectionMode.Row;
            grid1.VScrollBar.LargeChange = 5;

            grid1.Click += Grid1_Click;

            //1 Header Row
            grid1[0, 0] = new MyHeader("ID");
            grid1[0, 1] = new MyHeader("COB");
            grid1[0, 2] = new MyHeader("Index");

            //fixed width for info columns
            grid1.Columns[0].Width = 35;
            grid1.Columns[1].Width = 45;
            grid1.Columns[2].Width = 50;

            for (int x = 0; x < 64; x++)
            {
                grid1[0, 3 + x] = new MyHeader(string.Format("{0}", x));
            }

            for (int x = 0; x < 8; x++)
            {
                grid1[1, 3 + x * 8] = new MyHeader(string.Format("Byte {0}", x));
                grid1[1, 3 + x * 8].ColumnSpan = 8;

                grid1[1, 3 + x * 8].View.BackColor = Color.Tomato;

            }

            grid1.Rows[0].Height = 30;
            
            contextMenuStrip_removeitem.ItemClicked += ContextMenuStrip_removeitem_ItemClicked;

            Invalidated += DevicePDOView2_Invalidated;
        }

        private void DevicePDOView2_Invalidated(object sender, InvalidateEventArgs e)
        {

            UpdatePDOinfo();
        }

        private void ContextMenuStrip_removeitem_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

            int foundrow, foundcol;
            SourceGrid.Cells.ICellVirtual v = getItemAtGridPoint(RightClickPoint, out foundrow, out foundcol);
            SourceGrid.Cells.Cell c = (SourceGrid.Cells.Cell)v;

            if (c == null)
                return;

            PDOlocator location = (PDOlocator)c.Tag;

            if (location == null)
                return;


            switch (e.ClickedItem.Tag)
            {
                case "remove":
                    location.slot.Mapping.Remove(location.entry);

                    break;

                case "insert":
                    ODentry od = new ODentry();
                    location.slot.Mapping.Insert(location.ordinal, eds.dummy_ods[0x002]);
                    break;

            }

            helper.buildmappingsfromlists((ExporterFactory.Exporter)Properties.Settings.Default.ExporterType == ExporterFactory.Exporter.CANOPENNODE_V4);
            UpdatePDOinfo();


        }

        private void Vcc_ValueChangedEvent(object sender, EventArgs e)
        {

            SourceGrid.CellContext cell = (SourceGrid.CellContext)sender;

            // "0x3100/05/BUTTONS2" 
            string[] bits = cell.Value.ToString().Split('/');

            UInt16 newindex = EDSsharp.ConvertToUInt16(bits[0]);
            //warning if the subindex is still hex the converter will not know about it
            //we may need to append 0x to keep it correct
            UInt16 newsubindex = EDSsharp.ConvertToUInt16("0x" + bits[1]);

            //bits[2] is the description if we need it

            PDOlocator location = (PDOlocator)((SourceGrid.Cells.Cell)cell.Cell).Tag;
            PDOSlot slot = location.slot;

            ODentry newentry = null;

            if (eds.tryGetODEntry(newindex, out newentry))
            {
                if (newsubindex != 0)
                    newentry = newentry.subobjects[newsubindex];
            }
            else
            {
                return;
            }

            if (location.entry == null)
            {
                slot.Mapping.Add(newentry);
            }
            else
            {
                slot.Mapping[location.ordinal] = newentry;
            }

            helper.buildmappingsfromlists((ExporterFactory.Exporter)Properties.Settings.Default.ExporterType == ExporterFactory.Exporter.CANOPENNODE_V4);

            doUpdateOD();
            UpdatePDOinfo();
        }

        SourceGrid.Cells.ICellVirtual getItemAtGridPoint(Point P, out int foundrow, out int foundcol)
        {
            int y = 0;
            int y2 = 0;
            foundrow = 0;
            foreach (GridRow row in grid1.Rows)
            {
                y2 = y + row.Height;

                if (P.Y > y && P.Y < y2)
                {
                    foundrow = row.Index + grid1.VScrollBar.Value;
                }
                y = y2;
            }

            int x2 = 0;
            foundcol = 0;
            int outofview = 0;
            for (int i = 0; i < grid1.HScrollBar.Value; i++)
            {
                outofview += grid1.Columns.GetWidth(i);
            }

            x2 = -outofview;
            foreach (GridColumn col in grid1.Columns)
            {


                if (P.X > x2)
                {
                    foundcol = col.Index;
                }
                x2 += col.Width;
            }

            Console.WriteLine(string.Format("Found grid at {0}x{1}", foundcol, foundrow));
            
            SourceGrid.Cells.ICellVirtual v =  grid1.GetCell(foundrow, foundcol);

            return v;

        }

        private void Grid1_Click(object sender, EventArgs e)
        {

            MouseEventArgs ma = (MouseEventArgs)e;

            int foundrow, foundcol;
            SourceGrid.Cells.ICellVirtual v = getItemAtGridPoint(ma.Location, out foundrow, out foundcol);


            //// DEBUG code: Create ToolTip with col, row and hscroll.value
            //ToolTip toolTip1 = new ToolTip();
            //if (ma.Button == MouseButtons.Left)
            //{
            //    Point loc = new Point(0, 0);
            //    loc.X = foundcol;
            //    loc.Y = foundrow;
            //    // Force the ToolTip text to be displayed whether or not the form is active.
            //    toolTip1.ShowAlways = true;
            //    toolTip1.Show(loc.ToString() + ", " + grid1.HScrollBar.Value.ToString(), grid1, ma.Location);
            //}

            grid1.Selection.ResetSelection(false);
            grid1.Selection.SelectRow(foundrow, true);

            if (ma.Button == MouseButtons.Right)
            {
                RightClickPoint = ma.Location;
                //Show context menu
                contextMenuStrip_removeitem.Show(grid1, ma.Location);
            }
            else if (foundrow > 1) //don't select headers or bits
            {
                var obj = grid1.Rows[foundrow];
                if (obj!= null && obj.Tag != null)
                {

                    PDOSlot slot = (PDOSlot)obj.Tag;
                    selectedslot = slot;

                    updateslotdisplay();

                    if (isTXPDO)
                    {
                        textBox_inhibit.Enabled = true;
                        textBox_syncstart.Enabled = true;
                    }
                    textBox_eventtimer.Enabled = true;
                    textBox_type.Enabled = true;
                    textBox_cob.Enabled = true;

                    button_deletePDO.Enabled = true;
                    button_savepdochanges.Enabled = true;

                    //Is invalid bit set
                    checkBox_invalidpdo.Checked = slot.invalid;



                }
            }


        }

        public void Init(bool isTX)
        {
            isTXPDO = isTX;

            if (!isTXPDO)
            {
                textBox_inhibit.Enabled = false;
                textBox_eventtimer.Enabled = false;
                textBox_syncstart.Enabled = false;
            }
        }

        public libEDSsharp.EDSsharp eds;


        public void addPDOchoices()
        {

            listView_TXPDO.BeginUpdate();

            listView_TXPDO.Items.Clear();
            foreach (KeyValuePair<UInt16, ODentry> kvp in eds.ods)
            {
                ODentry od = kvp.Value;
                int index = kvp.Key;

                if (od.prop.CO_disabled == true)
                    continue;

                if (od.objecttype == ObjectType.VAR && (od.PDOtype == PDOMappingType.optional || (isTXPDO && (od.PDOtype == PDOMappingType.TPDO)) || (!isTXPDO && (od.PDOtype == PDOMappingType.RPDO))))
                {
                    AddTXPDOoption(od);
                }

                foreach (KeyValuePair<UInt16, ODentry> kvp2 in od.subobjects)
                {
                    ODentry odsub = kvp2.Value;
                    UInt16 subindex = kvp2.Key;

                    if (subindex == 0)
                        continue;

                    if (odsub.PDOtype == PDOMappingType.optional || (isTXPDO && (odsub.PDOtype == PDOMappingType.TPDO)) || (!isTXPDO && (odsub.PDOtype == PDOMappingType.RPDO)))
                    {
                        AddTXPDOoption(odsub);
                    }
                }

            }

            listView_TXPDO.EndUpdate();
        }

        private void AddTXPDOoption(ODentry od)
        {

            TXchoices.Add(String.Format("0x{0:X4}/{1:X2}/", od.Index, od.Subindex) + od.parameter_name);

            ListViewItem lvi = new ListViewItem(String.Format("0x{0:X4}", od.Index));
            lvi.SubItems.Add(String.Format("0x{0:X2}", od.Subindex));
            lvi.SubItems.Add(od.parameter_name);

            DataType dt = od.datatype;
            if (dt == DataType.UNKNOWN && od.parent != null)
                dt = od.parent.datatype;
            lvi.SubItems.Add(dt.ToString());

            lvi.SubItems.Add(od.Sizeofdatatype().ToString());

            lvi.Tag = (object)od;

            listView_TXPDO.Items.Add(lvi);

        }

        public void updateslotdisplay()
        {
            if (selectedslot == null)
                return;

            textBox_slot.Text = string.Format("0x{0:X4}", selectedslot.ConfigurationIndex);
            textBox_mapping.Text = string.Format("0x{0:X4}", selectedslot.MappingIndex);
            textBox_cob.Text = string.Format("0x{0:X4}", selectedslot.COB);
            textBox_type.Text = string.Format("{0}", selectedslot.transmissiontype);
            textBox_inhibit.Text = string.Format("{0}", selectedslot.inhibit);
            textBox_eventtimer.Text = string.Format("{0}", selectedslot.eventtimer);
            textBox_syncstart.Text = string.Format("{0}", selectedslot.syncstart);


        }

        public void UpdatePDOinfo(bool updatechoices = true)
        {
            int savVScrollValue = 0;

            if (!updatechoices)
                savVScrollValue = grid1.VScrollBar.Value;

            button_savepdochanges.Enabled = (textBox_slot.Text != "");

            updateslotdisplay();

            if (eds == null)
                return;

            if (updatechoices)
                addPDOchoices();

            if (grid1.RowsCount > 2)
                grid1.Rows.RemoveRange(2, grid1.RowsCount - 2);


            TXchoices.Clear();

            foreach (ODentry od in eds.dummy_ods.Values)
            {
                TXchoices.Add(String.Format("0x{0:X4}/{1:X2}/", od.Index, od.Subindex) + od.parameter_name);
            }

            foreach (KeyValuePair<UInt16, ODentry> kvp in eds.ods)
            {
                ODentry od = kvp.Value;
                int index = kvp.Key;

                if (od.prop.CO_disabled == true)
                    continue;

                if (od.objecttype == ObjectType.VAR && (od.PDOtype == PDOMappingType.optional || (isTXPDO && (od.PDOtype == PDOMappingType.TPDO)) || (!isTXPDO && (od.PDOtype == PDOMappingType.RPDO))))
                {
                    TXchoices.Add(String.Format("0x{0:X4}/{1:X2}/", od.Index, od.Subindex) + od.parameter_name);

                }

                foreach (KeyValuePair<UInt16, ODentry> kvp2 in od.subobjects)
                {
                    ODentry odsub = kvp2.Value;
                    UInt16 subindex = kvp2.Key;

                    if (subindex == 0)
                        continue;

                    if (odsub.PDOtype == PDOMappingType.optional || (isTXPDO && (odsub.PDOtype == PDOMappingType.TPDO)) || (!isTXPDO && (odsub.PDOtype == PDOMappingType.RPDO)))
                    {
                        TXchoices.Add(String.Format("0x{0:X4}/{1:X2}/", odsub.Index, odsub.Subindex) + odsub.parameter_name);

                    }
                }

                srray = new string[TXchoices.Count];
                TXchoices.CopyTo(srray, 0);
            }


            SourceGrid.Cells.Editors.ComboBox comboStandard = new SourceGrid.Cells.Editors.ComboBox(typeof(string), srray, false);
            #if !NETCOREAPP
            comboStandard.Control.DropDownWidth = 0x100;
            #endif
            comboStandard.Changed += ComboStandard_Changed;

            //tableLayoutPanel1.SuspendLayout();

            redrawtable();
            helper = new libEDSsharp.PDOHelper(eds);
            helper.build_PDOlists();

            srray = new string[TXchoices.Count];
            TXchoices.CopyTo(srray, 0);


            int row = 0;
            foreach (PDOSlot slot in helper.pdoslots)
            {
                if (isTXPDO != slot.isTXPDO())
                    continue;
                if (grid1.ColumnsCount > 64+3)
                    grid1.ColumnsCount = 64+3;
                grid1.Redim(grid1.RowsCount + 1, grid1.ColumnsCount);
                grid1.Rows[grid1.RowsCount - 1].Tag = slot;
                grid1.Rows[row + 2].Height = 30;

                grid1[row + 2, 0] = new SourceGrid.Cells.Cell(String.Format("{0}", row + 1), typeof(string));
                grid1[row + 2, 1] = new SourceGrid.Cells.Cell(String.Format("{0:X}", slot.COB), typeof(string));
                grid1[row + 2, 2] = new SourceGrid.Cells.Cell(String.Format("{0:X}", slot.ConfigurationIndex), typeof(string));

                grid1[grid1.RowsCount - 1, 0].View = viewCOB;
                grid1[grid1.RowsCount - 1, 1].View = viewCOB;
                grid1[grid1.RowsCount - 1, 2].View = viewCOB;

                int bitoff = 0;
                int ordinal = 0;
                foreach (ODentry entry in slot.Mapping)
                {
                    if ((bitoff + entry.Sizeofdatatype()) > 64)
                    {
                       string toDisplay = string.Join(Environment.NewLine, slot.Mapping);
                       MessageBox.Show(string.Format("Invalid TXPDO mapping parameters in 0x{0:X}!\r\nTrying to map more than the maximum lenght of a CAN message (8 bytes).\r\n\r\nMembers are:\r\n{1}", slot.ConfigurationIndex,toDisplay));
                        break;
                    }
                        string target = slot.getTargetName(entry);
                        grid1[row + 2, bitoff + 3] = new SourceGrid.Cells.Cell(target, comboStandard);
                        grid1[row + 2, bitoff + 3].ColumnSpan = entry.Sizeofdatatype();
                        grid1[row + 2, bitoff + 3].View = viewNormal;

                        PDOlocator location = new PDOlocator();
                        location.slot = slot;
                        location.ordinal = ordinal;
                        location.entry = entry;

                        Console.WriteLine(string.Format("New location at Row {0} Col {1} Loc {2}", row, bitoff, location.ToString()));
                        grid1[row + 2, bitoff + 3].Tag = location;

                        ValueChangedController vcc = new ValueChangedController();
                        vcc.ValueChangedEvent += Vcc_ValueChangedEvent;


                        grid1[row + 2, bitoff + 3].AddController(vcc);
                        bitoff += entry.Sizeofdatatype();


                    ordinal++;

                }

                //Pad out with an empty combo
                if (bitoff < 64)
                {
                    grid1[row + 2, bitoff + 3] = new SourceGrid.Cells.Cell("Empty", comboStandard);
                    int colspan = 64 - bitoff;
                    if (colspan > 8)
                        colspan = 8;
                    grid1[row + 2, bitoff + 3].ColumnSpan = colspan;
                    grid1[row + 2, bitoff + 3].View = viewEmpty;
                    ValueChangedController vcc = new ValueChangedController();
                    vcc.ValueChangedEvent += Vcc_ValueChangedEvent;
                    grid1[row + 2, bitoff + 3].AddController(vcc);

                    PDOlocator location = new PDOlocator();
                    location.slot = slot;
                    location.ordinal = ordinal;
                    location.entry = null;

                    Console.WriteLine(string.Format("New location at Row {0} Col {1} Loc {2}", row, bitoff, location.ToString()));
                    grid1[row + 2, bitoff + 3].Tag = location;


                }
                row++;
            }

            if (!updatechoices)
                grid1.VScrollBar.Value = savVScrollValue;
        }

        private void ComboStandard_Changed(object sender, EventArgs e)
        {


        }

        public void redrawtable()
        {

        }

        private class MyHeader : SourceGrid.Cells.ColumnHeader
        {
            public MyHeader(object value) : base(value)
            {
                //1 Header Row
                SourceGrid.Cells.Views.ColumnHeader view = new SourceGrid.Cells.Views.ColumnHeader();
                view.Font = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
                view.WordWrap = true;
                view.TextAlignment = DevAge.Drawing.ContentAlignment.MiddleCenter;
                view.BackColor = Color.Tomato;

                string text = value.ToString();
                if (text == "0" || text == "8" || text == "16" || text == "24" || text == "32" || text == "40" || text == "48" || text == "56")
                {
                    view.ForeColor = Color.Tomato;
                }

                View = view;

                AutomaticSortEnabled = false;
            }
        }

        private class PDOlocator
        {
            public PDOSlot slot;
            public int ordinal;
            public ODentry entry;

            public override string ToString()
            {
                string msg;
                msg = String.Format("Ordinal {0} , slot {1} entry {2}", ordinal, slot.ToString(), entry == null ? "NULL" : entry.ToString());

                return msg;
            }

        }

        private void clickEvent_Click(object sender, EventArgs e)
        {
            SourceGrid.CellContext context = (SourceGrid.CellContext)sender;
            MessageBox.Show(this, context.Position.ToString());
        }

        private void button_down_Click(object sender, EventArgs e)
        {
            int newwidth = grid1.Columns[3].Width - 10;
            if (newwidth < 18)
                newwidth = 18;

            Console.WriteLine("New Width " + newwidth.ToString());

            for (int x = 0; x < 64; x++)
            {
                grid1.Columns[x + 3].Width = newwidth;
            }

        }

        private void button_up_Click(object sender, EventArgs e)
        {
            int newwidth = grid1.Columns[3].Width + 10;
            Console.WriteLine("New Width " + newwidth.ToString());

            for (int x = 0; x < 64; x++)
            {
                grid1.Columns[x + 3].Width = newwidth;
            }

        }


        private class CellBackColorAlternate : SourceGrid.Cells.Views.Cell
        {
            public CellBackColorAlternate(Color firstColor, Color secondColor)
            {
                FirstBackground = new DevAge.Drawing.VisualElements.BackgroundSolid(firstColor);
                SecondBackground = new DevAge.Drawing.VisualElements.BackgroundSolid(secondColor);
            }

            private DevAge.Drawing.VisualElements.IVisualElement mFirstBackground;
            public DevAge.Drawing.VisualElements.IVisualElement FirstBackground
            {
                get { return mFirstBackground; }
                set { mFirstBackground = value; }
            }

            private DevAge.Drawing.VisualElements.IVisualElement mSecondBackground;
            public DevAge.Drawing.VisualElements.IVisualElement SecondBackground
            {
                get { return mSecondBackground; }
                set { mSecondBackground = value; }
            }

            protected override void PrepareView(SourceGrid.CellContext context)
            {
                base.PrepareView(context);

                if (Math.IEEERemainder(context.Position.Row, 2) == 0)
                    Background = FirstBackground;
                else
                    Background = SecondBackground;
            }
        }

        private void listView_TXPDO_ItemDrag(object sender, ItemDragEventArgs e)
        {


            List<ODentry> entries = new List<ODentry>();

            foreach (ListViewItem item in listView_TXPDO.SelectedItems)
            {
                if (item.Tag.GetType() == typeof(ODentry))
                    entries.Add((ODentry)item.Tag);
            }

            DataObject data = new DataObject(DataFormats.FileDrop, entries);
            data.SetData(entries.ToArray());
            listView_TXPDO.DoDragDrop(data, DragDropEffects.Copy);

        }



        private void grid1_DragOver(object sender, DragEventArgs e)
        {
            Point p = grid1.PointToClient(new Point(e.X, e.Y));
            int foundrow, foundcol;

            SourceGrid.Cells.Cell cell = (SourceGrid.Cells.Cell)getItemAtGridPoint(p, out foundrow, out foundcol);

            if (cell == null || cell.Tag == null)
            {
                e.Effect = DragDropEffects.None;
            }
            else
            {
                e.Effect = DragDropEffects.Copy;

            }
        }

        private void grid1_DragDrop(object sender, DragEventArgs e)
        {
            //base.OnDragDrop(sender, e);

            Point p = grid1.PointToClient(new Point(e.X, e.Y));

            int foundrow, foundcol;

            SourceGrid.Cells.Cell cell = (SourceGrid.Cells.Cell)getItemAtGridPoint(p, out foundrow, out foundcol);

            ODentry[] entries = (ODentry[])e.Data.GetData(typeof(ODentry[]));
            PDOlocator location = (PDOlocator)cell.Tag;

            if (location == null || entries == null)
                return;

            foreach (ODentry entry in entries)
            {
                location.slot.insertMapping(location.ordinal, entry);
            }

            helper.buildmappingsfromlists((ExporterFactory.Exporter)Properties.Settings.Default.ExporterType == ExporterFactory.Exporter.CANOPENNODE_V4);
            UpdatePDOinfo(false); //dont cause the list to refresh

        }

        private void grid1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        public class ValueChangedController : SourceGrid.Cells.Controllers.ControllerBase
        {

            public event EventHandler ValueChangedEvent;

            public override void OnValueChanged(CellContext sender, EventArgs e)
            {
                EventHandler handler = ValueChangedEvent;
                handler?.Invoke(sender, e);

                base.OnValueChanged(sender, e);
            }

        }

        private void button_deletePDO_Click(object sender, EventArgs e)
        {
            if (selectedslot == null)
                return;

            if (MessageBox.Show(string.Format("Are you sure you wish to delete the entire PDO 0x{0:X4}/0x{1:X4}", selectedslot.ConfigurationIndex, selectedslot.MappingIndex), "Are you sure", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {

                helper.removePDOslot(selectedslot.ConfigurationIndex);

                helper.buildmappingsfromlists((ExporterFactory.Exporter)Properties.Settings.Default.ExporterType == ExporterFactory.Exporter.CANOPENNODE_V4);
                doUpdateOD();
                UpdatePDOinfo();


                selectedslot = null;
            }

        }

        private void button_addPDO_Click(object sender, EventArgs e)
        {
            addnewPDO();
        }

        private void addnewPDO()
        {

            UInt16 slot = helper.findPDOslotgap(isTXPDO);
            helper.addPDOslot(slot);

            helper.buildmappingsfromlists((ExporterFactory.Exporter)Properties.Settings.Default.ExporterType == ExporterFactory.Exporter.CANOPENNODE_V4);
            doUpdateOD();
            UpdatePDOinfo();
        }

        private void checkBox_invalidpdo_CheckedChanged(object sender, EventArgs e)
        {
            if (selectedslot == null)
                return;

            selectedslot.invalid = checkBox_invalidpdo.Checked;

            textBox_cob.Text = string.Format("0x{0:X4}", selectedslot.COB);
        }

        private void button_savepdochanges_Click(object sender, EventArgs e)
        {

        }

        private void button_savepdochanges_Click_1(object sender, EventArgs e)
        {

            UInt16 config = libEDSsharp.EDSsharp.ConvertToUInt16(textBox_slot.Text);

            if (!isTXPDO)
            {
                if (config < 0x1400 | config >= 0x1600)
                {
                    MessageBox.Show(string.Format("Invalid TXPDO Communication parameters index 0x{0:X4}", config));
                    return;
                }
            }
            else
            {
                if (config < 0x1800 | config >= 0x1A00)
                {
                    MessageBox.Show(string.Format("Invalid RXPDO Communication parameters index 0x{0:X4}", config));
                    return;
                }
            }            

            UInt16 inhibit = libEDSsharp.EDSsharp.ConvertToUInt16(textBox_inhibit.Text);
            UInt16 eventtimer = libEDSsharp.EDSsharp.ConvertToUInt16(textBox_eventtimer.Text);
            UInt32 COB = libEDSsharp.EDSsharp.ConvertToUInt32(textBox_cob.Text);
            byte syncstart = libEDSsharp.EDSsharp.ConvertToByte(textBox_syncstart.Text);
            byte transmissiontype = libEDSsharp.EDSsharp.ConvertToByte(textBox_type.Text);

            selectedslot.ConfigurationIndex = config;
            selectedslot.COB = COB;
            selectedslot.inhibit = inhibit;
            selectedslot.eventtimer = eventtimer;
            selectedslot.syncstart = syncstart;
            selectedslot.transmissiontype = transmissiontype;

            try
            {
                helper.buildmappingsfromlists((ExporterFactory.Exporter)Properties.Settings.Default.ExporterType == ExporterFactory.Exporter.CANOPENNODE_V4);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            doUpdateOD();
            UpdatePDOinfo();
        }

        private void listView_TXPDO_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
    