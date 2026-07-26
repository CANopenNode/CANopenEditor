using mshtml;
using System;
using System.IO;
using System.Windows.Forms;

namespace ODEditor
{
    public partial class ReportView : Form
    {
        public ReportView(string pathtohtml)
        {
            InitializeComponent();



            webBrowser1.Url = new Uri(pathtohtml);

            webBrowser1.Navigated += WebBrowser1_Navigated;


        }

        private void WebBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {

            //Try to load a css override from ~/.edseditor/style.css first then fallback to installed default

            string csspath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".edseditor");
            csspath = Path.Combine(csspath, "style.css");

            if (!File.Exists(csspath))
            {
                csspath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "style.css");
            }

            if (!File.Exists(csspath))
                return;

            string text = System.IO.File.ReadAllText(csspath);

            mshtml.HTMLDocument CurrentDocument = (HTMLDocument)webBrowser1.Document.DomDocument;
            mshtml.IHTMLStyleSheet styleSheet = CurrentDocument.createStyleSheet("", 0);
            styleSheet.cssText = text;
        }

        private void toolStripButton_print_Click(object sender, EventArgs e)
        {
            webBrowser1.ShowPrintDialog();
        }

        private void toolStripButton_preview_Click(object sender, EventArgs e)
        {
            webBrowser1.ShowPrintPreviewDialog();
        }

        private void toolStripButton_save_Click(object sender, EventArgs e)
        {
            webBrowser1.ShowSaveAsDialog();
        }
    }
}
