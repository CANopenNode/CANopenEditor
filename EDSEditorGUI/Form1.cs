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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using libEDSsharp;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ODEditor
{
    public partial class ODEditor_MainForm : Form
    {

        private List<string> _mru = new List<string>();
        private string appdatafolder;

        private string networkfilename;

        private string gitVersion;

        private string toolTipsString;   //used for holding the tooltip message for file drag and drop events

        public static Dictionary<UInt32, EDSsharp> TXCobMap = new Dictionary<UInt32, EDSsharp>();
        List<EDSsharp> network = new List<EDSsharp>();

        public ODEditor_MainForm()
        {
            InitializeComponent();
            loadprofiles();
            insertToolStripMenuItem.Enabled = false;
            Warnings.warning_mask = Properties.Settings.Default.WarningMask;
        }

        private void loadprofiles()
        {

            // load default profiles from the install directory
            // load user profiles from the My Documents\.edseditor\profiles\ folder
            // Personal is my documents in windows and ~ in mono

            try
            {

                List<string> profilelist = Directory.GetFiles(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "Profiles").ToList();
                string homepath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".edseditor");
                homepath = Path.Combine(homepath, "profiles");

                if (Directory.Exists(homepath))
                {
                    profilelist.AddRange(Directory.GetFiles(homepath).ToList());
                }

                var items = new List<ToolStripMenuItem>();

                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Name = "///openFile";
                item.Text = "Open Profile File...";
                item.Click += ProfileAddClick;
                item.Image = Properties.Resources.InsertColumn_5626;
                items.Add(item);

                foreach (string file in profilelist)
                {
                    string ext = Path.GetExtension(file).ToLower();
                    if (ext == ".xpd" || ext == ".xdd")
                    {
                        item = new ToolStripMenuItem();
                        item.Name = Path.GetFileName(file);
                        item.Text = Path.GetFileName(file);
                        item.Click += ProfileAddClick;
                        item.Image = Properties.Resources.InsertColumn_5626;
                        items.Add(item);
                    }
                }

                insertToolStripMenuItem.DropDownItems.AddRange(items.ToArray());
            }
            catch (Exception e)
            {
                MessageBox.Show("Loading profiles has failed for the following reason :\n" + e.ToString());
            }
        
        }

        void ProfileAddClick(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null)
            {
                ToolStripMenuItem item = (ToolStripMenuItem)sender;
                DeviceView dv = (DeviceView)tabControl1.SelectedTab.Controls[0];
                string filename;
                EDSsharp eds = null;

                if (item.Name == "///openFile")
                {
                    OpenFileDialog odf = new OpenFileDialog();
                    odf.Filter = "All supported files (*.xpd;*.xdd;*.xdc)|*.xpd;*.xdd;*.xdc|"
                               + "CANopen XPD 1.1 (*.xpd)|*.xpd|"
                               + "CANopen XDD 1.1 (*.xdd)|*.xdd|"
                               + "CANopen XDC 1.1 (*.xdc)|*.xdc";

                    if (odf.ShowDialog() != DialogResult.OK)
                        return;

                    filename = odf.FileName;
                }
                else
                {
                    filename = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "Profiles" + Path.DirectorySeparatorChar + item.Name;
                }

                switch (Path.GetExtension(filename).ToLower())
                {
                    case ".xdd":
                    case ".xdc":
                    case ".xpd":
                        CanOpenXDD_1_1 coxml_1_1 = new CanOpenXDD_1_1();
                        eds = coxml_1_1.ReadXML(filename);
                        break;
                }

                if (eds == null)
                {
                    MessageBox.Show("Problem with loading the file.");
                    return;
                }

                InsertObjects insObjForm = new InsertObjects(dv.eds, network, eds.ods, "0");

                if (insObjForm.ShowDialog() == DialogResult.OK)
                {
                    EDSsharp modifiedEds = insObjForm.GetModifiedEDS();
                    modifiedEds.Dirty = true;

                    if(modifiedEds == dv.eds)
                    {
                    dv.dispatch_updateOD();
                    dv.dispatch_updatePDOinfo();

                    dv.eds.UpdatePDOcount();
                    dv.dispatch_updatedevice();
                }
                    else
                    {
                        foreach(TabPage page in tabControl1.TabPages)
                        {
                            DeviceView devView = (DeviceView)page.Controls[0];
                            if(devView.eds == modifiedEds)
                            {
                                devView.dispatch_updateOD();
                                devView.dispatch_updatePDOinfo();

                                devView.eds.UpdatePDOcount();
                                devView.dispatch_updatedevice();
                            }
                        }
                    }
                }
            }
        }

        private void openEDSfile(string path,InfoSection.Filetype ft)
        {
            Warnings.warning_list.Clear();

            try
            {
                EDSsharp eds = new EDSsharp();

                eds.Loadfile(path);

                DeviceView device = new DeviceView(eds, network);

                device.UpdateODViewForEDS += Device_UpdateODViewForEDS;
                eds.OnDataDirty += Eds_onDataDirty;

                tabControl1.TabPages.Add(eds.di.ProductName);
                tabControl1.TabPages[tabControl1.TabPages.Count - 1].Controls.Add(device);

                device.Dock = DockStyle.Fill;
                device.dispatch_updateOD();

                network.Add(eds);
            }
            catch (Exception ex)
            {
                Warnings.warning_list.Add(ex.ToString());
            }

            if (Warnings.warning_list.Count != 0)
            {
                WarningsFrm frm = new WarningsFrm();
                frm.Show();
            }
        }

        private void exportCanOpenNode(DeviceView dv, string FileName, ExporterFactory.Exporter exporterType)
        {
            bool saveDirty = dv.eds.Dirty; // dispatch update will set it to dirty. Save and restore axtual dirty status
            Warnings.warning_list.Clear();

            IExporter exporter = ExporterFactory.getExporter(exporterType);

            try
            {
                string savePath = Path.GetDirectoryName(FileName);
                string baseFileName = Path.GetFileNameWithoutExtension(FileName);
                var filepath = $"{savePath}/{baseFileName}";
                exporter.export(filepath, dv.eds);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed see detailed reason below :-\n" + ex.ToString());
            }

            if (Warnings.warning_list.Count != 0)
            {
                WarningsFrm frm = new WarningsFrm();
                frm.Show();
            }

            dv.dispatch_updateOD();
           dv.eds.Dirty = saveDirty; // dispatch update will set it to dirty. Restore saved dirty status
        }

        private void exportCanOpenNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null)
            {
                DeviceView dv = (DeviceView)tabControl1.SelectedTab.Controls[0];
                ExporterFactory.Exporter type = (ExporterFactory.Exporter)Properties.Settings.Default.ExporterType;

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.CheckFileExists = false;

                sfd.InitialDirectory = (dv.eds.ODfilename != null && dv.eds.ODfilename != "")
                                        ? Path.GetDirectoryName(dv.eds.ODfilename)
                                        : Path.GetDirectoryName(dv.eds.projectFilename);
                sfd.RestoreDirectory = true;
                sfd.FileName = (type == ExporterFactory.Exporter.CANOPENNODE_V4) ? "OD.h" : "CO_OD.c";
                sfd.Filter = "CANopenNode (*.h, *.c)|*.h";

                DialogResult result = sfd.ShowDialog();

                if (result == DialogResult.OK)
                {
                    dv.eds.ODfilename = sfd.FileName;
                    dv.eds.ODfileVersion = (type == ExporterFactory.Exporter.CANOPENNODE_V4) ? "V4" : "V1";

                    exportCanOpenNode(dv, sfd.FileName, type);
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

            Warnings.warning_list.Clear();

            OpenFileDialog odf = new OpenFileDialog();
            odf.Filter = "All supported files (*.xdd;*.xdc;*.xpd;*.eds;*.dcf;*.binpb;*.json)|*.xdd;*.xdc;*.xpd;*.eds;*.dcf;*.binpb;*.json|"
                       + "CANopen XDD (*.xdd)|*.xdd|"
                       + "CANopen XDC (*.xdc)|*.xdc|"
                       + "CANopen XPD (*.xpd)|*.xpd|"
                       + "Electronic Data Sheet (*.eds)|*.eds|"
                       + "Device Configuration File (*.dcf)|*.dcf|"
                       + "CANopen Protobuffer (*.binpb;*.json)|*.binpb;*.json";

            if (odf.ShowDialog() == DialogResult.OK)
            {

                switch(Path.GetExtension(odf.FileName).ToLower())
                {
                    case ".xdd":
                    case ".xdc":
                    case ".xpd":
                        openXDDfile(odf.FileName);
                        break;

                    case ".eds":
                        openEDSfile(odf.FileName, InfoSection.Filetype.File_EDS);
                        break;

                    case ".dcf":
                        openEDSfile(odf.FileName, InfoSection.Filetype.File_DCF);
                        break;

                    case ".binpb":
                        OpenProtobufferfile(odf.FileName, false);
                        break;

                    case ".json":
                        OpenProtobufferfile(odf.FileName, true);
                        break;

                    default:
                        return;

                }
              
                addtoMRU(odf.FileName);
            }

        }

        private void openXDDfile(string path)
        {
            try
            {
                EDSsharp eds;

                string xddfileVersion = "1.1";
                CanOpenXDD_1_1 coxml_1_1 = new CanOpenXDD_1_1();
                eds = coxml_1_1.ReadXML(path);

                if (eds == null)
                {
                    xddfileVersion = "1.0";
                    CanOpenXDD coxml = new CanOpenXDD();
                    eds = coxml.readXML(path);

                    if (eds == null)
                        return;
                }

                eds.projectFilename = path;
                if (xddfileVersion == "1.0")
                    eds.xddfilename_1_0 = path;
                else
                    eds.xddfilename_1_1 = path;

                tabControl1.TabPages.Add(eds.di.ProductName);

                DeviceView device = new DeviceView(eds, network);

                device.UpdateODViewForEDS += Device_UpdateODViewForEDS;
                eds.OnDataDirty += Eds_onDataDirty;

                tabControl1.TabPages[tabControl1.TabPages.Count - 1].Controls.Add(device);

                device.Dock = DockStyle.Fill;
                device.dispatch_updateOD();

                network.Add(eds);
            }
            catch (Exception ex)
            {
                Warnings.warning_list.Add(ex.ToString());
            }

            if (Warnings.warning_list.Count != 0)
            {
                WarningsFrm frm = new WarningsFrm();
                frm.Show();
            }



        }

        private void OpenProtobufferfile(string path, bool json)
        {
            Warnings.warning_list.Clear();

            try
            {
                EDSsharp eds;

                CanOpenXDD_1_1 coxml_1_1 = new CanOpenXDD_1_1();
                eds = coxml_1_1.ReadProtobuf(path, json);

                if (eds == null)
                {
                    return;
                }

                eds.projectFilename = path;

                DeviceView device = new DeviceView(eds, network);

                device.UpdateODViewForEDS += Device_UpdateODViewForEDS;
                eds.OnDataDirty += Eds_onDataDirty;

                tabControl1.TabPages.Add(eds.di.ProductName);
                tabControl1.TabPages[tabControl1.TabPages.Count - 1].Controls.Add(device);

                device.Dock = DockStyle.Fill;
                device.dispatch_updateOD();

                network.Add(eds);
            }
            catch (Exception ex)
            {
                Warnings.warning_list.Add(ex.ToString());
            }

            if (Warnings.warning_list.Count != 0)
            {
                WarningsFrm frm = new WarningsFrm();
                frm.Show();
            }
        }

        private void Device_UpdateODViewForEDS(object sender, UpdateODViewEventArgs e)
        {
            foreach (TabPage page in tabControl1.TabPages)
            {
                foreach (Control c in page.Controls)
                {
                    if (c.GetType() == typeof(DeviceView))
                    {
                        DeviceView d = (DeviceView)c;
                        if (d.eds == e.EDS)
                        {
                            d.dispatch_updateOD();
                        }
                    }

                }
            }
        }


        private void Eds_onDataDirty(bool dirty, EDSsharp sender)
        {
            foreach(TabPage page in tabControl1.TabPages)
            {
                foreach(Control c in page.Controls)
                {
                    if(c.GetType() == typeof(DeviceView))
                    {
                        DeviceView d = (DeviceView)c;
                        if (d.eds.Dirty == true)
                        {
                            page.BackColor = Color.Tomato;
                        }
                        else
                        {
                            page.BackColor = default(Color);
                        }
                    }

                }

            }

        }

        private void tabControl1_DrawItem(Object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush _textBrush;

            // Get the item from the collection.
            TabPage _tabPage = tabControl1.TabPages[e.Index];

            // Get the real bounds for the tab rectangle.
            Rectangle _tabBounds = tabControl1.GetTabRect(e.Index);

            if (e.State == DrawItemState.Selected)
            {

                // Draw a different background color, and don't paint a focus rectangle.
                _textBrush = new SolidBrush(Color.GhostWhite);
                g.FillRectangle(Brushes.Gray, e.Bounds);
            }
            else
            {
                _textBrush = new System.Drawing.SolidBrush(e.ForeColor);
                e.DrawBackground();
            }

            // Use our own font.
            Font _tabFont = new Font("Arial", (float)10.0, FontStyle.Bold, GraphicsUnit.Pixel);

            // Draw string. Center the text.
            StringFormat _stringFlags = new StringFormat();
            _stringFlags.Alignment = StringAlignment.Center;
            _stringFlags.LineAlignment = StringAlignment.Center;
            g.DrawString(_tabPage.Text, _tabFont, _textBrush, _tabBounds, new StringFormat(_stringFlags));
        }

        private void closeFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null)
            {
                // tabControl1.TabPages[tabControl1.TabPages.Count - 1].Controls.Add(device);

                DeviceView device = (DeviceView)tabControl1.SelectedTab.Controls[0];

                if(device.eds.Dirty==true)
                {
                    if (MessageBox.Show( "All unsaved changes will be lost\n continue?", "Unsaved changes", MessageBoxButtons.YesNo) == DialogResult.No)
                        return;
                }

                network.Remove(device.eds);

                tabControl1.TabPages.Remove(tabControl1.SelectedTab);
            }

        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
             Close();
        }

        private void exportDeviceFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null)
            {
                DeviceView dv = (DeviceView)tabControl1.SelectedTab.Controls[0];
                SaveFileDialog sfd = new SaveFileDialog();

                sfd.Filter = "CANopen XDD v1.1 stripped (*.xdd)|*.xdd|"  //must be first or change condition below
                           + "Electronic Data Sheet (*.eds)|*.eds|"
                           + "Device Configuration File (*.dcf)|*.dcf|"
                           + "Protobuffer binary, experimental (*.binpb)|*.binpb|"
                           + "Protobuffer JSON, experimental (*.json)|*.json|"
                           + "Documentation (*.md)|*.md|"
                           + "CANopen XDD v1.0, old (*.xdd)|*.xdd";

                sfd.InitialDirectory = Path.GetDirectoryName(dv.eds.projectFilename);
                sfd.RestoreDirectory = true;
                sfd.FileName = Path.GetFileNameWithoutExtension(dv.eds.projectFilename);

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (sfd.FilterIndex == 1)
                        dosave(dv, sfd.FileName, true, true);
                    else
                        dosave(dv, sfd.FileName, false);
                }
            }
        }

        private void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null)
            {
                DeviceView dv = (DeviceView)tabControl1.SelectedTab.Controls[0];

                if (dv.eds.xddfilename_1_1 != "")
                {
                    dosave(dv, dv.eds.xddfilename_1_1, true);
                }
                else
                {
                    saveAsToolStripMenuItem_Click(sender, e);
                }
                dv.eds.Dirty = false;
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null)
            {
                DeviceView dv = (DeviceView)tabControl1.SelectedTab.Controls[0];
                SaveFileDialog sfd = new SaveFileDialog();

                sfd.Filter = "CANopen XDD v1.1 (*.xdd)|*.xdd|"
                           + "CANopen XDC v1.1 (*.xdc)|*.xdc|"
                           + "CANopen XPD v1.1 (*.xpd)|*.xpd";

                if (dv.eds.projectFilename == "")
                {
                    sfd.FileName = Path.GetFileNameWithoutExtension(networkfilename);
                }
                else
                {
                    sfd.InitialDirectory = Path.GetDirectoryName(dv.eds.projectFilename);
                    sfd.RestoreDirectory = true;
                    sfd.FileName = Path.GetFileNameWithoutExtension(dv.eds.projectFilename);
                }


                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    dosave(dv, sfd.FileName, true);
                }
            }

        }


        void dosave(DeviceView dv, string FileName, bool xddfileVersion_1_1, bool stripped = false)
        {

            switch (Path.GetExtension(FileName))
            {
                case ".eds":
                    dv.eds.Savefile(FileName, InfoSection.Filetype.File_EDS);
                    dv.eds.edsfilename = FileName;
                    break;

                case ".dcf":
                    dv.eds.Savefile(FileName, InfoSection.Filetype.File_DCF);
                    dv.eds.dcffilename = FileName;
                    break;

                case ".md":
                    DocumentationGenMarkup docgen = new DocumentationGenMarkup();
                    docgen.genmddoc(FileName, dv.eds);
                    dv.eds.mdfilename = FileName;
                    break;

                case ".xdd":
                case ".xdc":
                case ".xpd":
                    if (xddfileVersion_1_1)
                    {
                        if (stripped)
                        {
                            dv.eds.xddfilenameStripped = FileName;
                        }
                        else
                        {
                            dv.eds.projectFilename = FileName;
                            dv.eds.xddfilename_1_1 = FileName;
                        }
                        Warnings.warning_list.Clear();

                        CanOpenXDD_1_1 coxdd = new CanOpenXDD_1_1();
                        coxdd.WriteXML(FileName, dv.eds, Path.GetExtension(FileName) == ".xdc", stripped);
                        dv.eds.Dirty = false;

                        if (Warnings.warning_list.Count != 0)
                        {
                            WarningsFrm frm = new WarningsFrm();
                            frm.Show();
                        }
                    }
                    else
                    {
                        CanOpenXDD coxdd = new CanOpenXDD();
                        coxdd.writeXML(FileName, dv.eds);
                        dv.eds.xddfilename_1_0 = FileName;
                    }
                    break;

                case ".binpb":
                case ".json":
                    Warnings.warning_list.Clear();

                    CanOpenXDD_1_1 copb = new CanOpenXDD_1_1();
                    copb.WriteProtobuf(FileName, dv.eds, Path.GetExtension(FileName) == ".json");

                    if (Warnings.warning_list.Count != 0)
                    {
                        WarningsFrm frm = new WarningsFrm();
                        frm.Show();
                    }
                    break;
            }

            dv.dispatch_updateOD();

        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EDSsharp eds = new EDSsharp();
            eds.di.ProductName = "New Product";

            string dir = Environment.OSVersion.Platform == PlatformID.Win32NT ? "\\" : "/";
            eds.projectFilename = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + dir + "project";

            tabControl1.TabPages.Add(eds.di.ProductName);

            DeviceView device = new DeviceView(eds, network);
            device.UpdateODViewForEDS += Device_UpdateODViewForEDS;

            eds.OnDataDirty += Eds_onDataDirty;

            tabControl1.TabPages[tabControl1.TabPages.Count - 1].Controls.Add(device);

            device.Dock = DockStyle.Fill;
            device.dispatch_updateOD();

            network.Add(eds);
        }
        private void TabControl1_Selected(Object sender, TabControlEventArgs e)
        {
            if(tabControl1.SelectedIndex == 0)
            {
                DeviceView dv = (DeviceView)tabControl1.SelectedTab.Controls[0];
                dv.dispatch_updateOD();
                dv.dispatch_updatePDOinfo();
                dv.eds.UpdatePDOcount();
                dv.dispatch_updatedevice();
            }
        }
        private void tabControl1_ControlsChanged(object sender, ControlEventArgs e)
        {
            enablesavemenus(tabControl1.TabCount > 0);  
        }

        private void tabControl1_Controlsremoved(object sender, ControlEventArgs e)
        {
            //Because
            enablesavemenus(tabControl1.TabCount > 1);
        }


        private void enablesavemenus(bool enable)
        {
            insertToolStripMenuItem.Enabled = enable;
            saveProjectToolStripMenuItem.Enabled = enable;
            exportCanOpenNodeToolStripMenuItem.Enabled = enable;
            closeFileToolStripMenuItem.Enabled = enable;
            saveNetworkXmlToolStripMenuItem.Enabled = enable;
            documentationToolStripMenuItem.Enabled = enable;
            networkPDOToolStripMenuItem.Enabled = enable;
            saveExportAllToolStripMenuItem.Enabled = enable;
            exportDeviceFileToolStripMenuItem.Enabled = enable;
            saveAsToolStripMenuItem.Enabled = true;

        }

        void OpenRecentFile(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            var filepath = (string)menuItem.Tag;

            string ext = Path.GetExtension(filepath);

            if (ext != null)
                ext = ext.ToLower();

            if (ext == ".xdd" || ext == ".xdc" || ext == ".xpd")
                openXDDfile(filepath);

            else if (ext == ".binpb")
                OpenProtobufferfile(filepath, false);
            else if (ext == ".json")
                OpenProtobufferfile(filepath, true);

            if ( ext == ".eds" )
                openEDSfile(filepath, InfoSection.Filetype.File_EDS);
            if (ext == ".dcf")
                openEDSfile(filepath, InfoSection.Filetype.File_DCF);

            addtoMRU(filepath);

        }

        private void ODEditor_MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            var mruFilePath = Path.Combine(appdatafolder, "MRU.txt");
            System.IO.File.WriteAllLines(mruFilePath, _mru);
        }

        private void ODEditor_MainForm_Load(object sender, EventArgs e)
        {
            //read git version string, show in title bar 
            //(https://stackoverflow.com/a/15145121)
            string gitVersion = String.Empty;
            using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("ODEditor." + "version.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                gitVersion = reader.ReadToEnd().TrimEnd('\n');
            }
            if (gitVersion == "")
            {
                gitVersion = "Unknown";
            }
            this.Text += gitVersion;
            this.gitVersion = gitVersion;

            //First lets create an appdata folder

            // The folder for the roaming current user 
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Combine the base folder with your specific folder....
            appdatafolder = Path.Combine(folder, "EDSEditor");

            // Check if folder exists and if not, create it
            if (!Directory.Exists(appdatafolder))
                Directory.CreateDirectory(appdatafolder);

            var mruFilePath = Path.Combine(appdatafolder, "MRU.txt");
            if (System.IO.File.Exists(mruFilePath))
                _mru.AddRange(System.IO.File.ReadAllLines(mruFilePath));

            populateMRU();
        }

        private void addtoMRU(string path)
        {
            // if it already exists remove it then let it re-add itself
            // so it will be promoted to the top of the list
            if (_mru.Contains(path))
                _mru.Remove(path);

            _mru.Insert(0, path);

            if (_mru.Count > 20)
                _mru.RemoveAt(20);

            populateMRU();

        }

        private void populateMRU()
        {

            mnuRecentlyUsed.DropDownItems.Clear();

            foreach (var path in _mru)
            {
                var item = new ToolStripMenuItem(path);
                item.Tag = path;
                item.Click += OpenRecentFile;
                switch(Path.GetExtension(path))
                {
                    case ".xml":
                        item.Image = Properties.Resources.GenericVSEditor_9905;
                        break;
                    case ".eds":
                        item.Image = Properties.Resources.EventLog_5735;
                        break;
                    case ".nxml":
                        item.Image = Properties.Resources.Index_8287_16x;
                        break;
                }

                mnuRecentlyUsed.DropDownItems.Add(item);
            }
        }

        private void saveNetworkXmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "CANopen Network XDD v1.1 (*.nxdd)|*.nxdd|"
                       + "CANopen Network XDC v1.1 (*.nxdc)|*.nxdc|"
                       + "CANopen Network XDD v1.0, old (*.nxdd)|*.nxdd";

            sfd.InitialDirectory = Path.GetDirectoryName(networkfilename);
            sfd.RestoreDirectory = true;
            sfd.FileName = Path.GetFileNameWithoutExtension(networkfilename);

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                switch (sfd.FilterIndex)
                {
                    case 3: // .nxdd V1.0
                        CanOpenXDD xdd = new CanOpenXDD();
                        xdd.writeMultiXML(sfd.FileName, network);
                        break;

                    case 2: // .nxdc V1.1 with actual value, denotation and deviceCommissioning info
                        CanOpenXDD_1_1 xdc_1_1 = new CanOpenXDD_1_1();
                        xdc_1_1.WriteMultiXML(sfd.FileName, network, true);
                        break;

                    case 1: // .nxdd V1.1
                        CanOpenXDD_1_1 xdd_1_1 = new CanOpenXDD_1_1();
                        xdd_1_1.WriteMultiXML(sfd.FileName, network, false);
                        break;
                }
            }
        }

        private void loadNetworkXmlToolStripMenuItem_Click(object sender, EventArgs e)
        {

            OpenFileDialog odf = new OpenFileDialog();
            odf.Filter = "All supported files (*.nxdd;*.nxdc)|*.nxdd;*.nxdc|"
                       + "CANopen Network XDD (*.nxdd)|*.nxdd|"
                       + "CANopen Network XDC (*.nxdc)|*.nxdc";

            if (odf.ShowDialog() == DialogResult.OK)
            {
                switch (Path.GetExtension(odf.FileName).ToLower())
                {
                    case ".nxdd":
                    case ".nxdc":
                        openXDDNetworkfile(odf.FileName);

                        break;
                }
            }
        }

        private void openXDDNetworkfile(string file)
        {
            CanOpenXDD_1_1 xdd_1_1 = new CanOpenXDD_1_1();
            List<EDSsharp> edss = xdd_1_1.ReadMultiXML(file);

            if (edss == null)
            {
                CanOpenXDD xdd = new CanOpenXDD();
                edss = xdd.readMultiXML(file);

                if (edss == null)
                    return;
            }

            foreach (EDSsharp eds in edss)
            {

                tabControl1.TabPages.Add(eds.di.ProductName);

                DeviceView device = new DeviceView(eds, network);

                tabControl1.TabPages[tabControl1.TabPages.Count - 1].Controls.Add(device);
                device.Dock = DockStyle.Fill;

                network.Add(eds);
                device.UpdateODViewForEDS += Device_UpdateODViewForEDS;
                eds.OnDataDirty += Eds_onDataDirty;

                device.dispatch_updateOD();
            }

            addtoMRU(file);
            networkfilename = file;

        }

        private void networkPDOToolStripMenuItem_Click(object sender, EventArgs e)
        {


            string dir = GetTemporaryDirectory();

            string csspath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".edseditor");
            csspath = Path.Combine(csspath, "style.css");

            if (!System.IO.File.Exists(csspath))
            {
                csspath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "style.css");
            }

            if (System.IO.File.Exists(csspath))
            {
                System.IO.File.Copy(csspath, dir + Path.DirectorySeparatorChar + "style.css");
            }

            string temp = dir + Path.DirectorySeparatorChar + "network.html";

            NetworkPDOreport npr = new NetworkPDOreport();
            npr.gennetpdodoc(temp, network);

            if (IsRunningOnMono())
            {
                System.Diagnostics.Process.Start("file://"+temp);
            }
            else
            {
                ReportView rv = new ReportView(temp);
                rv.Show();
            }
        }

        public string GetTemporaryDirectory()
        {
            string tempDirectory;

            do
            {
                tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            } while (Directory.Exists(tempDirectory));

            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        private void documentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {

                Warnings.warning_list.Clear();

                if (tabControl1.SelectedTab != null)
                {
                    DeviceView dv = (DeviceView)tabControl1.SelectedTab.Controls[0];
                    SaveFileDialog sfd = new SaveFileDialog();

                    string dir = GetTemporaryDirectory();


                    string csspath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".edseditor");
                    csspath = Path.Combine(csspath, "style.css");

                    if (!System.IO.File.Exists(csspath))
                    {
                        csspath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "style.css");
                    }

                    if (System.IO.File.Exists(csspath))
                    {
                        System.IO.File.Copy(csspath, dir + Path.DirectorySeparatorChar + "style.css");
                    }

                    string temp = dir + Path.DirectorySeparatorChar + "documentation.html";
                    string temp2 = dir + Path.DirectorySeparatorChar + "documentation.md";

                    this.UseWaitCursor = true;

                    DocumentationGenHtml docgenHtml = new DocumentationGenHtml();
                    docgenHtml.genhtmldoc(temp, dv.eds);
                    DocumentationGenMarkup docgenMarkup = new DocumentationGenMarkup();
                    docgenMarkup.genmddoc(temp2, dv.eds);
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd", $"/c start {temp2}"));
                    }
                    if (IsRunningOnMono())
                    {
                        System.Diagnostics.Process.Start("file://" + temp);
                    }
                    else
                    {
                        ReportView rv = new ReportView(temp);
                        rv.Show();
                    }

                    this.UseWaitCursor = false;

                }
            }
            catch (Exception ex)
            {
                Warnings.warning_list.Add(ex.ToString());
            }

            if (Warnings.warning_list.Count != 0)
            {
                WarningsFrm frm = new WarningsFrm();
                frm.Show();
            }
        }

        private void saveExportAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null)
            {
                DeviceView dv = (DeviceView)tabControl1.SelectedTab.Controls[0];
                int cnt = 0;

                if (dv.eds.xddfilename_1_1 != "" && dv.eds.xddfilename_1_1 == dv.eds.projectFilename)
                {
                    dosave(dv, dv.eds.xddfilename_1_1, true);
                }
                else
                {
                    MessageBox.Show("Please save project file at least once!");
                    return;
                }

                if (dv.eds.edsfilename != null && dv.eds.edsfilename != "")
                {
                    dv.eds.Savefile(dv.eds.edsfilename, InfoSection.Filetype.File_EDS);
                    cnt++;
                }

                if (dv.eds.dcffilename != null && dv.eds.dcffilename != "")
                {
                    dv.eds.Savefile(dv.eds.dcffilename, InfoSection.Filetype.File_DCF);
                    cnt++;
                }

                if (dv.eds.mdfilename != null && dv.eds.mdfilename != "")
                {
                    DocumentationGenMarkup docgen = new DocumentationGenMarkup();
                    docgen.genmddoc(dv.eds.mdfilename, dv.eds);
                    cnt++;
                }

                if (dv.eds.ODfilename != null && dv.eds.ODfilename != "")
                {
                    ExporterFactory.Exporter type = (ExporterFactory.Exporter)Properties.Settings.Default.ExporterType;
                    string version = (type == ExporterFactory.Exporter.CANOPENNODE_V4) ? "V4" : "V1";

                    if (dv.eds.ODfileVersion == version)
                    {
                        exportCanOpenNode(dv, dv.eds.ODfilename, type);
                    }
                    else
                    {
                        // exporter type changed in preferences
                        dv.eds.ODfilename = "";
                        dv.eds.ODfileVersion = "";
                        dv.dispatch_updatedevice();
                        MessageBox.Show("CANopenNode exporter changed. Please export manually at least once!");
                    }
                    cnt++;
                }

                if (cnt == 0)
                {
                    MessageBox.Show("Nothing exported. Please use 'Export...' at least once!");
                }

                dv.eds.Dirty = false;
            }
        }

        private void ODEditor_MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            foreach (TabPage page in tabControl1.TabPages)
            {
                foreach (Control c in page.Controls)
                {
                    if (c.GetType() == typeof(DeviceView))
                    {
                        DeviceView d = (DeviceView)c;
                        if (d.eds.Dirty == true)
                        {
                           if(MessageBox.Show("Warning you have unsaved changes\n Do you wish to continue","Unsaved changes",MessageBoxButtons.YesNo)==DialogResult.No)
                            {
                                e.Cancel = true;
                                return;
                            }
                        }
                       
                    }

                }

            }
        }

        private bool fileTypeSupported(string fileName)
        {
            bool typeSupported = false;
            if (Path.HasExtension(fileName))
            {
                switch (Path.GetExtension(fileName).ToLower())
                {
                    case ".xdd":
                    case ".xdc":
                    case ".xpd":
                    case ".xml":
                    case ".eds":
                    case ".dcf":
                    case ".nxdd":
                    case ".nxdc":
                    case ".nxml":
                        typeSupported = true;
                        break;
                }
            }

            return typeSupported;

        }

        private void ODEditor_MainForm_DragEnter(object sender, DragEventArgs e)
        {
            this.Activate();
            bool unsupportedFile = false;
            string[] data = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (data != null) 
            {                
                var rawFileNames = data as string[];
                if (rawFileNames.Length > 0)
                {
                    var fileNames = rawFileNames.Distinct();
                    foreach (string fileName in fileNames)
                    {
                        if(fileTypeSupported(fileName) == false)
                        {
                            unsupportedFile = true;
                            break;
                        }
                    }

                }

                toolTipsString = (unsupportedFile ? "1 or more files not supported" : "Drop files here to open");

                if (unsupportedFile)
                {
                    e.Effect = DragDropEffects.None;
                }
                else
                {
                    e.Effect = DragDropEffects.All;
                }

                enableDragDropTooltip();

            }
                
            else
            {
               e.Effect = DragDropEffects.None;
                //disableDragDropTooltip();
                enableDragDropTooltip();
            }
                
        }

        private void enableDragDropTooltip()
        {
            toolTip1.Active = true;
            toolTip1.ReshowDelay = 0;
            toolTip1.InitialDelay = 0;
            toolTip1.UseAnimation = false;
            toolTip1.UseFading = false;
            toolTip1.Show(toolTipsString, this, this.PointToClient(Cursor.Position).X, this.PointToClient(Cursor.Position).Y);
        }

        private void disableDragDropTooltip()
        {
            toolTip1.Active = false;
        }

        private void ODEditor_MainForm_DragLeave(object sender, EventArgs e)
        {
            disableDragDropTooltip();
        }


        private void ODEditor_MainForm_DragDrop(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(DataFormats.FileDrop);
            if (data != null)
            {
                var rawFileNames = data as string[];

                if (rawFileNames.Length > 0)
                {
                    var fileNames = rawFileNames.Distinct();
                    foreach (string fileName in fileNames)
                    {
                        switch (Path.GetExtension(fileName).ToLower())
                        {
                            case ".xpd":
                            case ".xdc":
                            case ".xdd":
                                openXDDfile(fileName);
                                break;

                            case ".eds":
                                openEDSfile(fileName, InfoSection.Filetype.File_EDS);
                                break;

                            case ".dcf":
                                openEDSfile(fileName, InfoSection.Filetype.File_DCF);
                                break;

                            case ".nxdc":
                            case ".nxdd":
                                openXDDNetworkfile(fileName);
                                break;

                            case ".binpb":
                                OpenProtobufferfile(fileName, false);
                                break;

                            case ".json":
                                OpenProtobufferfile(fileName, true);
                                break;

                            default:
                                break;

                        }

                        addtoMRU(fileName);
                    }
                }
            }
            disableDragDropTooltip();
        }

        private void ODEditor_MainForm_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if(e.EscapePressed)
            {
                e.Action = DragAction.Cancel;
                disableDragDropTooltip();
            }
        }

        private void ODEditor_MainForm_Leave(object sender, EventArgs e)
        {
            disableDragDropTooltip();
        }

        private void ODEditor_MainForm_DragOver(object sender, DragEventArgs e)
        {
            enableDragDropTooltip();
        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Preferences p = new Preferences();
            p.ShowDialog();
        }

        private void tabControl1_MouseClick(object sender, MouseEventArgs e)
        {
            TabPage tp;
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i <= tabControl1.TabCount - 1; i++)
                {
                    if (tabControl1.GetTabRect(i).Contains(e.Location))
                    {
                        tp = tabControl1.TabPages[i];

                        DialogResult dialogResult = MessageBox.Show(tabControl1.TabPages[i].Text, "Close file?", MessageBoxButtons.YesNo);
                        if (dialogResult == DialogResult.Yes)
                        {

                                DeviceView device = (DeviceView)tabControl1.TabPages[i].Controls[0];

                                if (device.eds.Dirty == true)
                                {
                                    if (MessageBox.Show("All unsaved changes will be lost\n continue?", "Unsaved changes", MessageBoxButtons.YesNo) == DialogResult.No)
                                        return;
                                }

                                network.Remove(device.eds);

                                tabControl1.TabPages.Remove(tabControl1.TabPages[i]);
                        }                        
                    }
                }
            }
        }
    }
}
