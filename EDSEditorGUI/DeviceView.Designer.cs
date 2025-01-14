namespace ODEditor
{
    partial class DeviceView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeviceView));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.devicePDOView2 = new ODEditor.DevicePDOView2();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.devicePDOView1 = new ODEditor.DevicePDOView2();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.deviceODView1 = new ODEditor.DeviceODView();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.deviceInfoView = new ODEditor.DeviceInfoView();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.moduleInfo1 = new ODEditor.ModuleInfo();
            this.tabPage4.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "ListBox_686.png");
            this.imageList1.Images.SetKeyName(1, "notebook_16xLG.png");
            this.imageList1.Images.SetKeyName(2, "Output_16xLG.png");
            this.imageList1.Images.SetKeyName(3, "SingleInput_8170_16x.png");
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.devicePDOView2);
            this.tabPage4.ImageIndex = 3;
            this.tabPage4.Location = new System.Drawing.Point(4, 25);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(1114, 746);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "RX PDO Mapping";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // devicePDOView2
            // 
            this.devicePDOView2.AutoScroll = true;
            this.devicePDOView2.AutoSize = true;
            this.devicePDOView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.devicePDOView2.Location = new System.Drawing.Point(0, 0);
            this.devicePDOView2.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.devicePDOView2.Name = "devicePDOView2";
            this.devicePDOView2.Size = new System.Drawing.Size(1114, 746);
            this.devicePDOView2.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.devicePDOView1);
            this.tabPage3.ImageIndex = 2;
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1030, 746);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "TX PDO Mapping";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // devicePDOView1
            // 
            this.devicePDOView1.AutoScroll = true;
            this.devicePDOView1.AutoSize = true;
            this.devicePDOView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.devicePDOView1.Location = new System.Drawing.Point(0, 0);
            this.devicePDOView1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.devicePDOView1.Name = "devicePDOView1";
            this.devicePDOView1.Size = new System.Drawing.Size(1030, 746);
            this.devicePDOView1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.deviceODView1);
            this.tabPage2.ImageIndex = 0;
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage2.Size = new System.Drawing.Size(1114, 746);
            this.tabPage2.TabIndex = 0;
            this.tabPage2.Text = "Object Dictionary";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // deviceODView1
            // 
            this.deviceODView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deviceODView1.Location = new System.Drawing.Point(4, 4);
            this.deviceODView1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.deviceODView1.Name = "deviceODView1";
            this.deviceODView1.Size = new System.Drawing.Size(1108, 740);
            this.deviceODView1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.deviceInfoView);
            this.tabPage1.ImageIndex = 1;
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage1.Size = new System.Drawing.Size(1114, 746);
            this.tabPage1.TabIndex = 1;
            this.tabPage1.Text = "Device Info";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // deviceInfoView
            // 
            this.deviceInfoView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deviceInfoView.Location = new System.Drawing.Point(4, 4);
            this.deviceInfoView.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.deviceInfoView.Name = "deviceInfoView";
            this.deviceInfoView.Size = new System.Drawing.Size(1108, 740);
            this.deviceInfoView.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.ImageList = this.imageList1;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1038, 773);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.moduleInfo1);
            this.tabPage5.Location = new System.Drawing.Point(4, 23);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(1030, 746);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Modules";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // moduleInfo1
            // 
            this.moduleInfo1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.moduleInfo1.Location = new System.Drawing.Point(3, 3);
            this.moduleInfo1.Margin = new System.Windows.Forms.Padding(4);
            this.moduleInfo1.Name = "moduleInfo1";
            this.moduleInfo1.Size = new System.Drawing.Size(1024, 740);
            this.moduleInfo1.TabIndex = 0;
            // 
            // DeviceView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "DeviceView";
            this.Size = new System.Drawing.Size(1038, 773);
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.TabPage tabPage4;
        private DevicePDOView2 devicePDOView2;
        private System.Windows.Forms.TabPage tabPage3;
        private DevicePDOView2 devicePDOView1;
        private System.Windows.Forms.TabPage tabPage2;
        private DeviceODView deviceODView1;
        private System.Windows.Forms.TabPage tabPage1;
        private DeviceInfoView deviceInfoView;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage5;
        private ModuleInfo moduleInfo1;
    }
}
