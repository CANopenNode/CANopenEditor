using libEDSsharp;
using System;
using System.Windows.Forms;

namespace ODEditor
{
    public partial class WarningsFrm : Form
    {
        public WarningsFrm()
        {
            InitializeComponent();

            foreach (string s in Warnings.warning_list)
            {
                textBox1.AppendText(s + "\r\n");
            }
        }

        private void button_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
