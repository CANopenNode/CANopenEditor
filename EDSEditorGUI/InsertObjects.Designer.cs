namespace ODEditor
{
    partial class InsertObjects
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InsertObjects));
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_offsets = new System.Windows.Forms.TextBox();
            this.button_insert = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.button_uncheck = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cbox_Target = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(290, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Index Offset (single or space separated list for multiple insert)";
            // 
            // textBox_offsets
            // 
            this.textBox_offsets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_offsets.Location = new System.Drawing.Point(12, 70);
            this.textBox_offsets.Name = "textBox_offsets";
            this.textBox_offsets.Size = new System.Drawing.Size(432, 20);
            this.textBox_offsets.TabIndex = 2;
            this.textBox_offsets.Text = "1";
            this.textBox_offsets.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_offsets_KeyPress);
            this.textBox_offsets.Leave += new System.EventHandler(this.TextBox_offsets_Leave);
            // 
            // button_insert
            // 
            this.button_insert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_insert.Location = new System.Drawing.Point(154, 401);
            this.button_insert.Name = "button_insert";
            this.button_insert.Size = new System.Drawing.Size(130, 37);
            this.button_insert.TabIndex = 6;
            this.button_insert.Text = "Insert";
            this.button_insert.UseVisualStyleBackColor = true;
            this.button_insert.Click += new System.EventHandler(this.Button_create_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.Location = new System.Drawing.Point(314, 401);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(130, 37);
            this.button_cancel.TabIndex = 7;
            this.button_cancel.Text = "Cancel";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.Button_cancel_Click);
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnKeystroke;
            this.dataGridView.Location = new System.Drawing.Point(12, 96);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.Size = new System.Drawing.Size(432, 294);
            this.dataGridView.TabIndex = 10;
            this.dataGridView.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGridView_ColumnHeaderMouseClick);
            this.dataGridView.Leave += new System.EventHandler(this.DataGridView_Leave);
            // 
            // button_uncheck
            // 
            this.button_uncheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_uncheck.Location = new System.Drawing.Point(12, 401);
            this.button_uncheck.Name = "button_uncheck";
            this.button_uncheck.Size = new System.Drawing.Size(130, 37);
            this.button_uncheck.TabIndex = 11;
            this.button_uncheck.Text = "Uncheck all";
            this.button_uncheck.UseVisualStyleBackColor = true;
            this.button_uncheck.Click += new System.EventHandler(this.button_uncheck_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Target";
            // 
            // cbox_Target
            // 
            this.cbox_Target.FormattingEnabled = true;
            this.cbox_Target.Location = new System.Drawing.Point(12, 25);
            this.cbox_Target.Name = "cbox_Target";
            this.cbox_Target.Size = new System.Drawing.Size(432, 21);
            this.cbox_Target.TabIndex = 13;
            this.cbox_Target.SelectedIndexChanged += new System.EventHandler(this.cbox_Target_SelectedIndexChanged);
            // 
            // InsertObjects
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(456, 450);
            this.Controls.Add(this.cbox_Target);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_uncheck);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_insert);
            this.Controls.Add(this.textBox_offsets);
            this.Controls.Add(this.label2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "InsertObjects";
            this.Text = "Insert OD Objects";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_offsets;
        private System.Windows.Forms.Button button_insert;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.Button button_uncheck;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbox_Target;
    }
}