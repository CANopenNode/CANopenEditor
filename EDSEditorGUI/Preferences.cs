using libEDSsharp;
using System;
using System.Windows.Forms;

namespace ODEditor
{
    public partial class Preferences : Form
    {
        public Preferences()
        {
            InitializeComponent();

            comboBox_exporter.DataSource = Enum.GetValues(typeof(ExporterFactory.Exporter));

            comboBox_exporter.SelectedItem = (ExporterFactory.Exporter)Properties.Settings.Default.ExporterType;

            UInt32 mask = Properties.Settings.Default.WarningMask;

            if ((mask & 0x01) == 0x01)
                checkBox_genericwarning.Checked = true;
            if ((mask & 0x02) == 0x02)
                checkBox_renamewarning.Checked = true;
            if ((mask & 0x04) == 0x04)
                checkBox_buildwarning.Checked = true;
            if ((mask & 0x08) == 0x08)
                checkBox_stringwarning.Checked = true;
            if ((mask & 0x10) == 0x10)
                checkBox_structwarning.Checked = true;


        }

        private void button_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button_save_Click(object sender, EventArgs e)
        {

            ExporterFactory.Exporter exporter;
            Enum.TryParse<ExporterFactory.Exporter>(comboBox_exporter.SelectedValue.ToString(), out exporter);
            Properties.Settings.Default.ExporterType = (int)exporter;

            UInt32 mask = 0xFFE0;

            if (checkBox_genericwarning.Checked)
                mask |= 0x01;
            if (checkBox_renamewarning.Checked)
                mask |= 0x02;
            if (checkBox_buildwarning.Checked)
                mask |= 0x04;
            if (checkBox_stringwarning.Checked)
                mask |= 0x08;
            if (checkBox_structwarning.Checked)
                mask |= 0x10;

            Properties.Settings.Default.WarningMask = mask;

            Warnings.warning_mask = mask;


            Properties.Settings.Default.Save();
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
