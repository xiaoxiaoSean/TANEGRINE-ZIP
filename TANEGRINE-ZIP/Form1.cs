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
        string zippath = string.Empty;//file path of the zip file
        private void Form1_Load(object sender, EventArgs e)
        {
            statusLabel.Text = LanguageManager.Get("readytext");
            uninstallFileToolStripMenuItem.Visible = false;
            extractToolStripMenuItem.Text = LanguageManager.Get("extractText");
            compressToolStripMenuItem.Text = LanguageManager.Get("compressText");
            SettingsToolStripMenuItem.Text = LanguageManager.Get("settingsText");
            uninstallFileToolStripMenuItem.Text = LanguageManager.Get("uninstallFileText");
        }
        private async void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mainOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                zippath = mainOpenFileDialog.FileName;
                if (FileDetector.IsCompressedFile(zippath))
                {
                    uninstallFileToolStripMenuItem.Visible = true;
                    statusLabel.Text = LanguageManager.Get("OpeningFile");
                    statusProgressBar.Value = 50;
                    await LoadArchiveAsync(zippath);                    
                }
                else {
                    MessageBox.Show(LanguageManager.Get("NotACompressedFile"));
                    return;
                 }
            }
            else
            {
                return;
            }
        }
        private async Task LoadArchiveAsync(string zippath)
        {
            fileBox.Items.Clear();

            await Task.Run(() =>
            {
                using var archive = ArchiveFactory.OpenArchive(zippath);

                foreach (var entry in archive.Entries)
                {
                    string text = entry.IsDirectory
                        ? entry.Key + "/"
                        : entry.Key;


                    Invoke(() =>
                    {
                        fileBox.Items.Add(text);
                    });
                }
                Invoke(() => {
                    statusLabel.Text = LanguageManager.Get("readytext");
                    statusProgressBar.Value = 100; });
            });
        }
        private void uninstallFileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
