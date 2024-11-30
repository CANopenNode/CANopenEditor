using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using libEDSsharp;

namespace ODEditor
{
    public partial class ModuleInfo : MyTabUserControl
    {
        public EDSsharp eds = null;

        public ModuleInfo()
        {
            InitializeComponent();

            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                // Design time logic
                return;
            }

            Invalidated += ModuleInfo_Invalidated;

        }

        private void ModuleInfo_Invalidated(object sender, InvalidateEventArgs e)
        {
           
            

        }

        public void populatemoduleinfo()
        {
            if (eds == null)
                return;

            textBox_nrsupportedmodules.Text = eds.sm.NrOfEntries.ToString();

            this.listView_modules.Items.Clear();


            foreach(KeyValuePair<UInt16,Module> kvp  in eds.modules)
            {
                Module m = kvp.Value;
                UInt16 modindex = kvp.Key;

                libEDSsharp.ModuleInfo mi = m.mi;
                ListViewItem lvi2 = new ListViewItem(String.Format("{0}", kvp.Key));
                lvi2.SubItems.Add(string.Format("{0}", mi.ProductName));
                lvi2.SubItems.Add(string.Format("{0}", mi.ProductVersion));
                lvi2.SubItems.Add(string.Format("{0}", mi.ProductRevision));
                lvi2.SubItems.Add(string.Format("{0}", mi.OrderCode));


                if (eds.cm.connectedmodulelist.Count > 0)
                {
                    if (eds.cm.connectedmodulelist.ContainsKey(kvp.Key))
                    {
                        lvi2.BackColor = Color.Green;
                    }
                    else
                    {
                        lvi2.BackColor = Color.Tomato;
                    }
                }


                listView_modules.Items.Add(lvi2);
            
            }         

        }

        private void listView_modules_MouseClick(object sender, MouseEventArgs e)
        {
            if (listView_modules.SelectedItems.Count != 1)
                return;

            listView_extends.Items.Clear();

            UInt16 index = Convert.ToUInt16(listView_modules.SelectedItems[0].Text, 10);

            ModuleSubExtends mse = eds.modules[index].mse;

            foreach (UInt16 sindex in mse.objectlist.Values)
            {
                ListViewItem lvi = null;

                if (eds.ods.ContainsKey(sindex))
                {
                    lvi = new ListViewItem(string.Format("0x{0:x4}",sindex));
                    lvi.SubItems.Add(eds.ods[sindex].parameter_name);
                }
                else
                {
                    if (eds.modules[index].modulesubext.ContainsKey(sindex))
                    {
                        lvi = new ListViewItem(string.Format("0x{0:x4}", sindex));
                        lvi.SubItems.Add(eds.modules[index].modulesubext[sindex].parameter_name);

                    }
                    else
                    {
                        lvi = new ListViewItem(string.Format("0x{0:x4}", sindex));
                        lvi.SubItems.Add("** FAULT OD NOT FOUND **");
                    }
                }

                listView_extends.Items.Add(lvi);


            }


            textBox_modulecomments.Clear();

            if (eds.modules.ContainsKey(index))
            {
                ModuleComments mc = eds.modules[index].mc;

                foreach (string s in mc.comments)
                {
                    textBox_modulecomments.AppendText(s + "\r\n");
                }
            }

        }
    }
}
