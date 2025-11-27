using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ODEditor
{
    /// <summary>
    /// A small dialog for PDO mapping width adjustments
    /// </summary>
    public partial class ChangeMappingWidth : Form
    {
        public int selected_width = 1;
        private int default_width = 1;

        public ChangeMappingWidth(int current_width, int max_width)
        {
            InitializeComponent();
            // Validate params
            if (current_width < 1)
                current_width = 1;
            if (max_width < 1)
                max_width = 1;
            if (current_width > max_width)
                current_width = max_width;
            updown_newwidth.Maximum = max_width;
            updown_newwidth.Value = current_width;
            selected_width = current_width;
            default_width = current_width;
        }

        private void button_ok_Click(object sender, EventArgs e)
        {
            selected_width = (int)updown_newwidth.Value;
            if (selected_width != default_width)
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                this.DialogResult = DialogResult.Cancel;
            }
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ChangeMappingWidth_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.button_ok_Click(null, null);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.button_cancel_Click(null, null);
            }
        }
    }
}
