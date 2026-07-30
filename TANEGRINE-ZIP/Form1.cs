using SharpCompress.Archives;
using SharpCompress.Common;
using System.ComponentModel;

namespace TANEGRINE_ZIP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        ComponentResourceManager resources = new ComponentResourceManager(typeof(Form1));
        string zippath = string.Empty;//file path of the zip file
        private void Form1_Load(object sender, EventArgs e)
        {
            statusLabel.Text = resources.GetString("readytext");
            uninstallFileToolStripMenuItem.Visible = false;
        }
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mainOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (FileDetector.IsCompressedFile(zippath))
                {
                    uninstallFileToolStripMenuItem.Visible = true;
                    zippath = mainOpenFileDialog.FileName;
                    using (var archive = ArchiveFactory.OpenArchive(zippath))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            if (!entry.IsDirectory)
                            {
                                fileBox.Items.Add(entry.Key);
                            }
                            else
                            {
                                fileBox.Items.Add(entry.Key + "/");
                            }
                        }
                    }
                }

            }
            else
            {
                return;
            }
        }
    }
}
