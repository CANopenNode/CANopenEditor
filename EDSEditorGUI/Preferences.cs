using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using libEDSsharp;

namespace ODEditor
{
    public partial class Preferences : Form
    {
        public Preferences()
        {
            InitializeComponent();
            AlignInputColumn();

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

            textBox_couchdburl.Text = Properties.Settings.Default.CouchDBUrl ?? "http://localhost:5984/canopen";
            textBox_couchdbuser.Text = Properties.Settings.Default.CouchDBUser ?? "";
            textBox_couchdbpassword.Text = SettingsCrypto.Unprotect(Properties.Settings.Default.CouchDBPassword);

            textBox_gitrepourl.Text = Properties.Settings.Default.GitRepoUrl ?? "";
            textBox_gituser.Text = Properties.Settings.Default.GitUser ?? "";
            textBox_gittoken.Text = SettingsCrypto.Unprotect(Properties.Settings.Default.GitToken);

            float fontSize = Properties.Settings.Default.UiFontSize;
            numericUpDown_uifontsize.Value = (decimal)Math.Max(6f, Math.Min(24f, fontSize));
        }

        /// <summary>
        /// Moves the input column right of the widest label; the labels grow
        /// with the UI font and would otherwise run under the input fields.
        /// </summary>
        private void AlignInputColumn()
        {
            Label[] labels = { label1, label_couchdb, label_couchdbuser, label_couchdbpassword,
                               label_gitrepourl, label_gituser, label_gittoken, label_uifontsize };
            Control[] inputs = { comboBox_exporter, textBox_couchdburl, textBox_couchdbuser, textBox_couchdbpassword,
                                 textBox_gitrepourl, textBox_gituser, textBox_gittoken, numericUpDown_uifontsize };

            int inputLeft = labels.Max(l => l.Left + l.PreferredWidth) + label1.Left / 3;
            if (inputLeft > comboBox_exporter.Left)
            {
                foreach (Control c in inputs)
                    c.Left = inputLeft;
            }
            label_uifonthint.Left = numericUpDown_uifontsize.Right + label1.Left / 3;
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

            Properties.Settings.Default.CouchDBUrl = textBox_couchdburl.Text;
            Properties.Settings.Default.CouchDBUser = textBox_couchdbuser.Text;
            Properties.Settings.Default.CouchDBPassword = SettingsCrypto.Protect(textBox_couchdbpassword.Text);
            Properties.Settings.Default.GitRepoUrl = textBox_gitrepourl.Text;
            Properties.Settings.Default.GitUser = textBox_gituser.Text;
            Properties.Settings.Default.GitToken = SettingsCrypto.Protect(textBox_gittoken.Text);
            Properties.Settings.Default.UiFontSize = (float)numericUpDown_uifontsize.Value;

            Properties.Settings.Default.Save();
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
