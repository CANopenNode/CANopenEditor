namespace ODEditor
{
    partial class ChangeMappingWidth
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangeMappingWidth));
            this.label_enterwidth = new System.Windows.Forms.Label();
            this.updown_newwidth = new System.Windows.Forms.NumericUpDown();
            this.button_ok = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.updown_newwidth)).BeginInit();
            this.SuspendLayout();
            // 
            // label_enterwidth
            // 
            this.label_enterwidth.AutoSize = true;
            this.label_enterwidth.Location = new System.Drawing.Point(33, 24);
            this.label_enterwidth.Name = "label_enterwidth";
            this.label_enterwidth.Size = new System.Drawing.Size(126, 13);
            this.label_enterwidth.TabIndex = 0;
            this.label_enterwidth.Text = "Enter the new width (bits)";
            // 
            // updown_newwidth
            // 
            this.updown_newwidth.Location = new System.Drawing.Point(165, 22);
            this.updown_newwidth.Maximum = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.updown_newwidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.updown_newwidth.Name = "updown_newwidth";
            this.updown_newwidth.Size = new System.Drawing.Size(61, 20);
            this.updown_newwidth.TabIndex = 1;
            this.updown_newwidth.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // button_ok
            // 
            this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_ok.Location = new System.Drawing.Point(12, 57);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(130, 37);
            this.button_ok.TabIndex = 2;
            this.button_ok.Text = "OK";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(183, 57);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(130, 37);
            this.button_cancel.TabIndex = 3;
            this.button_cancel.Text = "Cancel";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // ChangeMappingWidth
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(325, 111);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.updown_newwidth);
            this.Controls.Add(this.label_enterwidth);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "ChangeMappingWidth";
            this.Text = "Change mapping width";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ChangeMappingWidth_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.updown_newwidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_enterwidth;
        private System.Windows.Forms.NumericUpDown updown_newwidth;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_cancel;
    }
}