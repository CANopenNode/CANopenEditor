namespace ODEditor
{
    partial class ODEditor_MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ODEditor_MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openCanOpenNodeXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.loadNetworkXmlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveNetworkXmlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuRecentlyUsed = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exportDeviceFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportCanOpenNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveExportAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.closeFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reportsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.documentationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.networkPDOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.preferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.insertToolStripMenuItem,
            this.reportsToolStripMenuItem,
            this.toolsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1599, 28);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openCanOpenNodeXMLToolStripMenuItem,
            this.saveProjectToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator3,
            this.loadNetworkXmlToolStripMenuItem,
            this.saveNetworkXmlToolStripMenuItem,
            this.toolStripSeparator6,
            this.mnuRecentlyUsed,
            this.toolStripSeparator1,
            this.exportDeviceFileToolStripMenuItem,
            this.exportCanOpenNodeToolStripMenuItem,
            this.saveExportAllToolStripMenuItem,
            this.toolStripSeparator2,
            this.closeFileToolStripMenuItem,
            this.toolStripSeparator4,
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Image = global::ODEditor.Properties.Resources.NewFile_6276;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(359, 26);
            this.newToolStripMenuItem.Text = "&New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openCanOpenNodeXMLToolStripMenuItem
            // 
            this.openCanOpenNodeXMLToolStripMenuItem.Image = global::ODEditor.Properties.Resources.Open_6529;
            this.openCanOpenNodeXMLToolStripMenuItem.Name = "openCanOpenNodeXMLToolStripMenuItem";
            this.openCanOpenNodeXMLToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openCanOpenNodeXMLToolStripMenuItem.Size = new System.Drawing.Size(359, 26);
            this.openCanOpenNodeXMLToolStripMenuItem.Text = "&Open";
            this.openCanOpenNodeXMLToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveProjectToolStripMenuItem
            // 
            this.saveProjectToolStripMenuItem.Enabled = false;
            this.saveProjectToolStripMenuItem.Image = global::ODEditor.Properties.Resources.Save_6530;
            this.saveProjectToolStripMenuItem.Name = "saveProjectToolStripMenuItem";
            this.saveProjectToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveProjectToolStripMenuItem.Size = new System.Drawing.Size(359, 26);
            this.saveProjectToolStripMenuItem.Text = "&Save Project";
            this.saveProjectToolStripMenuItem.Click += new System.EventHandler(this.saveProjectToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Enabled = false;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(359, 26);
            this.saveAsToolStripMenuItem.Text = "Save &Project As...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(356, 6);
            // 
            // loadNetworkXmlToolStripMenuItem
            // 
            this.loadNetworkXmlToolStripMenuItem.Image = global::ODEditor.Properties.Resources.Open_6529;
            this.loadNetworkXmlToolStripMenuItem.Name = "loadNetworkXmlToolStripMenuItem";
            this.loadNetworkXmlToolStripMenuItem.Size = new System.Drawing.Size(359, 26);
            this.loadNetworkXmlToolStripMenuItem.Text = "Open &Network XML";
            this.loadNetworkXmlToolStripMenuItem.Click += new System.EventHandler(this.loadNetworkXmlToolStripMenuItem_Click);
            // 
            // saveNetworkXmlToolStripMenuItem
            // 
            this.saveNetworkXmlToolStripMenuItem.Enabled = false;
            this.saveNetworkXmlToolStripMenuItem.Image = global::ODEditor.Properties.Resources.Save_6530;
            this.saveNetworkXmlToolStripMenuItem.Name = "saveNetworkXmlToolStripMenuItem";
            this.saveNetworkXmlToolStripMenuItem.Size = new System.Drawing.Size(359, 26);
            this.saveNetworkXmlToolStripMenuItem.Text = "Save Networ&k XML";
            this.saveNetworkXmlToolStripMenuItem.Click += new System.EventHandler(this.saveNetworkXmlToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(356, 6);
            // 
            // mnuRecentlyUsed
            // 
            this.mnuRecentlyUsed.Name = "mnuRecentlyUsed";
            this.mnuRecentlyUsed.Size = new System.Drawing.Size(359, 26);
            this.mnuRecentlyUsed.Text = "&Recent Files";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(356, 6);
            // 
            // exportDeviceFileToolStripMenuItem
            // 
            this.exportDeviceFileToolStripMenuItem.Enabled = false;
            this.exportDeviceFileToolStripMenuItem.Name = "exportDeviceFileToolStripMenuItem";
            this.exportDeviceFileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.exportDeviceFileToolStripMenuItem.Size = new System.Drawing.Size(359, 26);
            this.exportDeviceFileToolStripMenuItem.Text = "&Export...";
            this.exportDeviceFileToolStripMenuItem.Click += new System.EventHandler(this.exportDeviceFileToolStripMenuItem_Click);
            // 
            // exportCanOpenNodeToolStripMenuItem
            // 
            this.exportCanOpenNodeToolStripMenuItem.Enabled = false;
            this.exportCanOpenNodeToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("exportCanOpenNodeToolStripMenuItem.Image")));
            this.exportCanOpenNodeToolStripMenuItem.Name = "exportCanOpenNodeToolStripMenuItem";
            this.exportCanOpenNodeToolStripMenuItem.Size = new System.Drawing.Size(359, 26);
            this.exportCanOpenNodeToolStripMenuItem.Text = "E&xport CanOpenNode...";
            this.exportCanOpenNodeToolStripMenuItem.Click += new System.EventHandler(this.exportCanOpenNodeToolStripMenuItem_Click);
            // 
            // saveExportAllToolStripMenuItem
            // 
            this.saveExportAllToolStripMenuItem.Enabled = false;
            this.saveExportAllToolStripMenuItem.Name = "saveExportAllToolStripMenuItem";
            this.saveExportAllToolStripMenuItem.Size = new System.Drawing.Size(359, 26);
            this.saveExportAllToolStripMenuItem.Text = "Save &All";
            this.saveExportAllToolStripMenuItem.Click += new System.EventHandler(this.saveExportAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(356, 6);
            // 
            // closeFileToolStripMenuItem
            // 
            this.closeFileToolStripMenuItem.Enabled = false;
            this.closeFileToolStripMenuItem.Image = global::ODEditor.Properties.Resources.Close_6519;
            this.closeFileToolStripMenuItem.Name = "closeFileToolStripMenuItem";
            this.closeFileToolStripMenuItem.Size = new System.Drawing.Size(359, 26);
            this.closeFileToolStripMenuItem.Text = "&Close file";
            this.closeFileToolStripMenuItem.Click += new System.EventHandler(this.closeFileToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(356, 6);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Image = global::ODEditor.Properties.Resources._305_Close_16x16_72;
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(359, 26);
            this.quitToolStripMenuItem.Text = "&Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // insertToolStripMenuItem
            // 
            this.insertToolStripMenuItem.Name = "insertToolStripMenuItem";
            this.insertToolStripMenuItem.Size = new System.Drawing.Size(106, 24);
            this.insertToolStripMenuItem.Text = "&Insert Profile";
            // 
            // reportsToolStripMenuItem
            // 
            this.reportsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.documentationToolStripMenuItem,
            this.networkPDOToolStripMenuItem});
            this.reportsToolStripMenuItem.Name = "reportsToolStripMenuItem";
            this.reportsToolStripMenuItem.Size = new System.Drawing.Size(74, 24);
            this.reportsToolStripMenuItem.Text = "&Reports";
            // 
            // documentationToolStripMenuItem
            // 
            this.documentationToolStripMenuItem.Enabled = false;
            this.documentationToolStripMenuItem.Image = global::ODEditor.Properties.Resources.ExporttoScript_9881;
            this.documentationToolStripMenuItem.Name = "documentationToolStripMenuItem";
            this.documentationToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.documentationToolStripMenuItem.Size = new System.Drawing.Size(339, 26);
            this.documentationToolStripMenuItem.Text = "&Documentation";
            this.documentationToolStripMenuItem.Click += new System.EventHandler(this.documentationToolStripMenuItem_Click);
            // 
            // networkPDOToolStripMenuItem
            // 
            this.networkPDOToolStripMenuItem.Enabled = false;
            this.networkPDOToolStripMenuItem.Image = global::ODEditor.Properties.Resources.ExporttoScript_9881;
            this.networkPDOToolStripMenuItem.Name = "networkPDOToolStripMenuItem";
            this.networkPDOToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.P)));
            this.networkPDOToolStripMenuItem.Size = new System.Drawing.Size(339, 26);
            this.networkPDOToolStripMenuItem.Text = "&Network PDO";
            this.networkPDOToolStripMenuItem.Click += new System.EventHandler(this.networkPDOToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.preferencesToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(58, 24);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // preferencesToolStripMenuItem
            // 
            this.preferencesToolStripMenuItem.Name = "preferencesToolStripMenuItem";
            this.preferencesToolStripMenuItem.Size = new System.Drawing.Size(168, 26);
            this.preferencesToolStripMenuItem.Text = "Preferences";
            this.preferencesToolStripMenuItem.Click += new System.EventHandler(this.preferencesToolStripMenuItem_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.tabControl1.AllowDrop = true;
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControl1.ItemSize = new System.Drawing.Size(24, 120);
            this.tabControl1.Location = new System.Drawing.Point(0, 28);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1599, 909);
            this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl1.TabIndex = 2;
            this.tabControl1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabControl1_DrawItem);
            this.tabControl1.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.tabControl1_ControlsChanged);
            this.tabControl1.ControlRemoved += new System.Windows.Forms.ControlEventHandler(this.tabControl1_Controlsremoved);
            this.tabControl1.DragDrop += new System.Windows.Forms.DragEventHandler(this.ODEditor_MainForm_DragDrop);
            this.tabControl1.DragEnter += new System.Windows.Forms.DragEventHandler(this.ODEditor_MainForm_DragEnter);
            this.tabControl1.DragOver += new System.Windows.Forms.DragEventHandler(this.ODEditor_MainForm_DragOver);
            this.tabControl1.DragLeave += new System.EventHandler(this.ODEditor_MainForm_DragLeave);
            this.tabControl1.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(this.ODEditor_MainForm_QueryContinueDrag);
            // 
            // ODEditor_MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1599, 937);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1560, 750);
            this.Name = "ODEditor_MainForm";
            this.Text = "Object Dictionary Editor ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ODEditor_MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ODEditor_MainForm_FormClosed);
            this.Load += new System.EventHandler(this.ODEditor_MainForm_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.ODEditor_MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.ODEditor_MainForm_DragEnter);
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.ODEditor_MainForm_DragOver);
            this.DragLeave += new System.EventHandler(this.ODEditor_MainForm_DragLeave);
            this.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(this.ODEditor_MainForm_QueryContinueDrag);
            this.MouseCaptureChanged += new System.EventHandler(this.ODEditor_MainForm_Leave);
            this.MouseLeave += new System.EventHandler(this.ODEditor_MainForm_Leave);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openCanOpenNodeXMLToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem closeFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem saveProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuRecentlyUsed;
        private System.Windows.Forms.ToolStripMenuItem saveNetworkXmlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadNetworkXmlToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem exportCanOpenNodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reportsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem documentationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem networkPDOToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveExportAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportDeviceFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem preferencesToolStripMenuItem;
    }
}

