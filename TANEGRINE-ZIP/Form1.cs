using SharpCompress.Archives;
using SharpCompress.Common;
using System.ComponentModel;
using System.IO.Compression;
namespace TANEGRINE_ZIP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string zippath = string.Empty;//file path of the zip file
        bool isDoingJob = false;
        private void Form1_Load(object sender, EventArgs e)
        {
            //begining of the form load,don't write any code before this line
            //set text-started
            statusLabel.Text = LanguageManager.Get("readytext");
            OpenToolStripMenuItem.Text = LanguageManager.Get("openText");
            extractToolStripMenuItem.Text = LanguageManager.Get("extractText");
            compressToolStripMenuItem.Text = LanguageManager.Get("compressText");
            SettingsToolStripMenuItem.Text = LanguageManager.Get("settingsText");
            uninstallFileToolStripMenuItem.Text = LanguageManager.Get("uninstallFileText");
            mainTab.Text = LanguageManager.Get("mainTabText");
            extractDirectlyALLToolStripMenuItem.Text = LanguageManager.Get("extractDirectlyALLText");
            extractToFolderALLToolStripMenuItem.Text = LanguageManager.Get("extractToFolderALLText");
            extractDirectlySELECTEDToolStripMenuItem.Text = LanguageManager.Get("extractDirectlySELECTEDText");
            extractToAFolderSELECTEDToolStripMenuItem.Text = LanguageManager.Get("extractToAFolderSELECTEDText");
            //set text-completed
            //now we need to disable some menus because they are useless when no file opened yet
            //set the visiblity of some menus -started
            uninstallFileToolStripMenuItem.Visible = false;
            extractToolStripMenuItem.Visible = false;
            compressToolStripMenuItem.Visible = false;
            //set the visiblity of some menus-completed
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
                    extractToolStripMenuItem.Visible = true;
                    compressToolStripMenuItem.Visible = true;
                    switch (FileDetector.DetectFileType(zippath))
                    {
                        case FileDetector.FileType.Unknown:
                            break;
                        case FileDetector.FileType.Zip:
                            mainTab.Text = "ZIP" + LanguageManager.Get("CompressFile");
                            break;
                        case FileDetector.FileType.Rar:
                            mainTab.Text = "RAR" + LanguageManager.Get("CompressFile");
                            break;
                        case FileDetector.FileType.SevenZip:
                            mainTab.Text = "7Z" + LanguageManager.Get("CompressFile");
                            break;
                        case FileDetector.FileType.Tar:
                            mainTab.Text = "TAR" + LanguageManager.Get("CompressFile");
                            break;
                        case FileDetector.FileType.GZip:
                            mainTab.Text = "GZ" + LanguageManager.Get("CompressFile");
                            break;
                        case FileDetector.FileType.BZip2:
                            mainTab.Text = "BZ2" + LanguageManager.Get("CompressFile");
                            break;
                        case FileDetector.FileType.Xz:
                            mainTab.Text = "XZ" + LanguageManager.Get("CompressFile");
                            break;
                        case FileDetector.FileType.Lz4:
                            mainTab.Text = "LZ4" + LanguageManager.Get("CompressFile");
                            break;
                        case FileDetector.FileType.Zstd:
                            mainTab.Text = "ZSTD" + LanguageManager.Get("CompressFile");
                            break;
                        case FileDetector.FileType.Iso:
                            mainTab.Text = "ISO" + LanguageManager.Get("ImageFile");
                            break;
                        case FileDetector.FileType.Wim:
                            mainTab.Text = "WIM" + LanguageManager.Get("ImageFile");
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    MessageBox.Show(LanguageManager.Get("NotACompressedFile"));
                    uninstallFile();
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
                    string text = entry.IsDirectory ? entry.Key + "/" : entry.Key;
                    Invoke(() =>
                    {
                        fileBox.Items.Add(text);
                    });
                }
                Invoke(() =>
                {
                    statusLabel.Text = LanguageManager.Get("readytext");
                    statusProgressBar.Value = 100;
                });
            });
        }
        private async Task ExtractSelectedEntriesAsync(string zippath, List<string> selectedEntries, string destinationPath)
        {
            var fileType = FileDetector.DetectFileType(zippath);
            HashSet<string> selectedSet = new(selectedEntries);
            try
            {
                isDoingJob = true;

                await Task.Run(() =>
                {
                    using var archiveZIP = ZipFile.OpenRead(zippath);
                    using var archiveRAR = ArchiveFactory.OpenArchive(zippath);
                    bool allOverWrite = false;
                    bool allSkip = false;
                    bool isThisFileExtracted = false;
                    bool stopJob = false;
                    switch (fileType)
                    {
                        case FileDetector.FileType.Unknown:
                            MessageBox.Show(LanguageManager.Get("UnknownFileType"));
                            break;

                        case FileDetector.FileType.Zip:
                            foreach (var entry in archiveZIP.Entries)
                            {
                                if (string.IsNullOrEmpty(entry.Name))
                                {
                                    continue;
                                }
                                else
                                {
                                    if (selectedSet.Contains(entry.Name))
                                    {
                                        Directory.CreateDirectory(Path.Combine(destinationPath, entry.FullName));
                                        if (File.Exists(Path.Combine(destinationPath, entry.FullName)))
                                        {
                                            if (allOverWrite)
                                            {
                                                goto cExtract;
                                            }
                                            else
                                            {
                                                if ((!allOverWrite) && (!allSkip))
                                                {
                                                reShowOWF:
                                                    OverWriteOrNotForm owonf = new OverWriteOrNotForm();
                                                    owonf.ShowDialog();
                                                    owonf.SyncBool(ref allOverWrite);
                                                    owonf.SyncBool2(ref allSkip);
                                                    int OWFresult = -1;
                                                    owonf.GetResult(ref OWFresult);
                                                    if ((!allOverWrite) && (!allSkip))
                                                    {
                                                        switch (OWFresult)
                                                        {
                                                            default:
                                                                break;
                                                            case -100:
                                                                goto reShowOWF; break;
                                                            case 1:
                                                                goto cExtract; break;
                                                            case 2:
                                                                continue;
                                                            case 1000:
                                                                stopJob = true;
                                                                break;
                                                        }
                                                    }
                                                }
                                                if (allOverWrite)
                                                {
                                                    goto cExtract;
                                                }
                                                if (allSkip)
                                                {
                                                    continue;
                                                }
                                                if (stopJob)
                                                {
                                                    isDoingJob = false;
                                                    return;
                                                }
                                            }
                                        }
                                    cExtract:
                                        entry.ExtractToFile(Path.Combine(destinationPath, entry.FullName), true);
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }
                            break;
                        case FileDetector.FileType.Rar:
                            foreach (var entry in archiveRAR.Entries)
                            {
                                if (entry.IsDirectory)
                                {
                                    continue;
                                }
                                if (selectedSet.Contains(entry.Key))
                                {
                                    Invoke(() =>
                                    {
                                        statusLabel.Text = LanguageManager.Get("ExtractingText") + $" {entry.Key}";
                                    });
                                    entry.WriteToDirectory(destinationPath, new ExtractionOptions
                                    {
                                        ExtractFullPath = true,
                                        Overwrite = true
                                    });
                                }
                            }
                            break;
                        case FileDetector.FileType.SevenZip:
                            break;
                        case FileDetector.FileType.Tar:
                            break;
                        case FileDetector.FileType.GZip:
                            break;
                        case FileDetector.FileType.BZip2:
                            break;
                        case FileDetector.FileType.Xz:
                            break;
                        case FileDetector.FileType.Lz4:
                            break;
                        case FileDetector.FileType.Zstd:
                            break;
                        case FileDetector.FileType.Iso:
                            break;
                        case FileDetector.FileType.Wim:
                            break;
                        default:
                            break;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, LanguageManager.Get("ExtractFailedText"));
            }
            finally
            {
                isDoingJob = false;
            }
            Invoke(() =>
            {
                statusLabel.Text = LanguageManager.Get("ExtractingCompleted");
                statusProgressBar.Value = 100;
            });
        }
        private async Task ExtractAllEntriesAsync(string zippath, string destinationPath)
        {
            var fileType = FileDetector.DetectFileType(zippath);
            try
            {
                isDoingJob = true;

                await Task.Run(() =>
                {
                    using var archive = ArchiveFactory.OpenArchive(zippath);

                    switch (fileType)
                    {
                        case FileDetector.FileType.Unknown:
                            break;

                        case FileDetector.FileType.Zip:
                            var entries = archive.Entries
                                .Where(e => !e.IsDirectory)
                                .ToList();

                            int total = entries.Count;
                            int current = 0;

                            foreach (var entry in entries)
                            {
                                entry.WriteToDirectory(destinationPath, new ExtractionOptions
                                {
                                    ExtractFullPath = true,
                                    Overwrite = true
                                });

                                current++;

                                if (current % 10 == 0 || current == total)
                                {
                                    int progress = total == 0 ? 100 : current * 100 / total;

                                    Invoke(() =>
                                    {
                                        statusProgressBar.Value = progress;
                                        statusLabel.Text = LanguageManager.Get("ExtractingText") + $" {current}/{total}";
                                    });
                                }
                            }
                            break;
                        case FileDetector.FileType.Rar:
                            break;
                        case FileDetector.FileType.SevenZip:
                            break;
                        case FileDetector.FileType.Tar:
                            break;
                        case FileDetector.FileType.GZip:
                            break;
                        case FileDetector.FileType.BZip2:
                            break;
                        case FileDetector.FileType.Xz:
                            break;
                        case FileDetector.FileType.Lz4:
                            break;
                        case FileDetector.FileType.Zstd:
                            break;
                        case FileDetector.FileType.Iso:
                            break;
                        case FileDetector.FileType.Wim:
                            break;
                        default:
                            break;
                    }
                });
                Invoke(() =>
                {
                    statusProgressBar.Value = 100;
                    statusLabel.Text = LanguageManager.Get("readytext");
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(LanguageManager.Get("ExtractFailedText") + ex.Message);
            }
            finally
            {
                isDoingJob = false;
            }
        }
        private void uninstallFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                uninstallFile();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }
        void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, LanguageManager.Get("ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        void uninstallFile()
        {
            statusProgressBar.Value = 50;
            statusLabel.Text = LanguageManager.Get("UninstallingText");
            zippath = string.Empty;
            statusLabel.Text = LanguageManager.Get("readytext");
            statusProgressBar.Value = 0;
            fileBox.Items.Clear();
            extractToolStripMenuItem.Visible = false;
            compressToolStripMenuItem.Visible = false;
            uninstallFileToolStripMenuItem.Visible = false;
            mainTab.Text = LanguageManager.Get("mainTabText");
        }
        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileBox.Items.Count == 0 || zippath == string.Empty)
            {
                MessageBox.Show(LanguageManager.Get("NoOpenedFile"));
                extractToolStripMenuItem.DropDown.Close();
                return;
            }
        }
        private async void extractDirectlyALLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainFolderBrowserDialog.Description = LanguageManager.Get("SelectExtractFolderText");
            if (mainFolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                if (isDoingJob)
                {
                    MessageBox.Show(LanguageManager.Get("AlreadyDoingJob"));
                    return;
                }
                isDoingJob = true;
                if (File.Exists(zippath))
                {
                    statusProgressBar.Value = 50;
                    statusLabel.Text = LanguageManager.Get("ExtractingText");
                    await ExtractAllEntriesAsync(zippath, mainFolderBrowserDialog.SelectedPath);
                }
            }
        }
        private async void extractToFolderALLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainFolderBrowserDialog.Description = LanguageManager.Get("SelectExtractFolderText");
            if (mainFolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                if (isDoingJob)
                {
                    MessageBox.Show(LanguageManager.Get("AlreadyDoingJob"));
                    return;
                }
                isDoingJob = true;
                if (File.Exists(zippath))
                {
                    statusProgressBar.Value = 50;
                    statusLabel.Text = LanguageManager.Get("ExtractingText");
                    LoadArchiveAsync(zippath);
                    await ExtractSelectedEntriesAsync(zippath, fileBox.Items.Cast<string>().ToList(), mainFolderBrowserDialog.SelectedPath);
                }
            }
        }
        private void extractDirectlySELECTEDToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private async void extractToAFolderSELECTEDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainFolderBrowserDialog.Description = LanguageManager.Get("SelectExtractFolderText");
            if (mainFolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                if (isDoingJob)
                {
                    MessageBox.Show(LanguageManager.Get("AlreadyDoingJob"));
                    return;
                }
                isDoingJob = true;
                if (File.Exists(zippath))
                {
                    statusProgressBar.Value = 50;
                    statusLabel.Text = LanguageManager.Get("ExtractingText");
                    await ExtractSelectedEntriesAsync(zippath, fileBox.SelectedItems.Cast<string>().ToList(), mainFolderBrowserDialog.SelectedPath);
                }
            }
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(LanguageManager.Get("Unavailble1"), "TZIP");
        }
    }
}
