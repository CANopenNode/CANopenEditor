using System;
using System.Drawing;
using System.Windows.Forms;
using libEDSsharp;

namespace ODEditor
{
    public partial class DeviceInfoView
    {
        private readonly TextBox textBox_revisionNumber = new TextBox { Text = "0", Dock = DockStyle.Fill };
        private readonly TextBox textBox_orderCode = new TextBox { Dock = DockStyle.Fill };
        private readonly TextBox textBox_networkName = new TextBox { Dock = DockStyle.Fill };

        private void InitializeAdditionalFields()
        {
            // Runs after InitializeComponent, i.e. after the designer layout has
            // been font-scaled: place the group below the existing groups as they
            // are laid out now instead of at fixed designer coordinates.
            int gap = groupBox3.Top - groupBox4.Bottom;
            var group = new GroupBox
            {
                Text = "Additional device information",
                Location = new Point(groupBox5.Left, Math.Max(groupBox5.Bottom, groupBox6.Bottom) + gap),
                Size = new Size(groupBox6.Right - groupBox5.Left, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, Padding = new Padding(6) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            string[] labels = { "Revision number", "Order code", "Commissioning network name" };
            TextBox[] fields = { textBox_revisionNumber, textBox_orderCode, textBox_networkName };
            for (int row = 0; row < labels.Length; row++)
            {
                layout.Controls.Add(new Label { Text = labels[row], AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
                fields[row].AccessibleName = labels[row];
                layout.Controls.Add(fields[row], 1, row);
            }
            group.Controls.Add(layout);
            int frame = group.Height - group.DisplayRectangle.Height;
            group.Height = layout.GetPreferredSize(new Size(group.DisplayRectangle.Width, 0)).Height + frame;
            panel1.Controls.Add(group);
            // Keep every setting and the existing Update button reachable at small window sizes.
            panel1.Dock = DockStyle.Fill;
            panel1.AutoScroll = true;
            panel1.AutoScrollMinSize = new Size(groupBox2.Right + gap, group.Bottom + gap);
        }

        private void PopulateAdditionalFields()
        {
            textBox_revisionNumber.Text = eds.di.RevisionNumber.ToString();
            textBox_orderCode.Text = eds.di.OrderCode;
            textBox_networkName.Text = eds.dc.NetworkName;
        }

        private uint ParseRevisionNumber()
        {
            return string.IsNullOrWhiteSpace(textBox_revisionNumber.Text) ? 0 : EDSsharp.ConvertToUInt32(textBox_revisionNumber.Text);
        }

        private void CommitAdditionalFields(uint revision)
        {
            eds.di.RevisionNumber = revision;
            eds.di.OrderCode = textBox_orderCode.Text;
            eds.dc.NetworkName = textBox_networkName.Text;
        }
    }
}
