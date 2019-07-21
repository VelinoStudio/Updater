using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace VelinoStudio.Updater.UpdaterUI
{
    using UpdateInformation;
    public partial class Form_UpdateConfig : Form
    {
        UpdateInfo updateInfo = new UpdateInfo();
        List<UpdateFileInfo> fileinfos;
        string selectPath = string.Empty;
        List<string> MD5Hashs = new List<string>();
        public static string MainExe { get; private set; }
        public Form_UpdateConfig()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Filter = "主程序文件|*.exe";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    MainExe = openFileDialog.SafeFileName;
                    backgroundWorker.RunWorkerAsync(Path.GetDirectoryName(openFileDialog.FileName));
                }
            }

        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            selectPath = e.Argument.ToString();
            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                if (key.StartsWith("MD5Hash"))
                {
                    if (key.StartsWith("MD5Hash_File_"))
                    {
                        string file = Path.Combine(selectPath, ConfigurationManager.AppSettings[key]);
                        if (File.Exists(file))
                        {
                            MD5Hashs.Add(file.ToLower());
                        }

                    }
                    else if (key.StartsWith("MD5Hash_Directory_"))
                    {
                        string directory = Path.Combine(selectPath, ConfigurationManager.AppSettings[key]);
                        if (Directory.Exists(directory))
                        {
                            foreach (string file in Directory.GetFiles(directory))
                            {
                                if (File.Exists(file))
                                {
                                    MD5Hashs.Add(file.ToLower());
                                }

                            }
                        }
                    }
                }
            }

            textBox1.Invoke(new Action(delegate { textBox1.Text = selectPath; }));
            Console.WriteLine(selectPath);
            treeView1.Invoke(new Action(delegate { treeView1.Nodes.Clear(); }));
            TreeNode root = new TreeNode { Text = "目录" };
            treeView1.Invoke(new Action(delegate { treeView1.Nodes.Add(root); }));
            fileinfos = new List<UpdateFileInfo>();
            GetFiles(selectPath, root);
            updateInfo.FileInfos = fileinfos.ToArray();
            treeView1.Invoke(new Action(delegate { treeView1.ExpandAll(); }));
        }

        private void GetFiles(string filePath, TreeNode node)
        {
            DirectoryInfo folder = new DirectoryInfo(filePath);
            treeView1.Invoke(new Action(delegate
            {
                node.Text = folder.Name;
                node.Tag = "Folder";
                node.Name = folder.FullName.Replace(selectPath, string.Empty);
            }));


            FileInfo[] chldFiles = folder.GetFiles();
            foreach (FileInfo chlFile in chldFiles)
            {
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(chlFile.FullName);
                UpdateFileInfo fileInfo = new UpdateFileInfo
                {
                    FileName = chlFile.Name,
                    FilePath = chlFile.FullName.Replace(selectPath, string.Empty),
                    FileSize = chlFile.Length,
                    MD5HashStr = MD5Hash.GetMD5HashFromFile(chlFile.FullName),
                    FileVersion = fvi.FileVersion,
                    VerificationType = MD5Hashs.Contains(chlFile.FullName.ToLower()) ? VerificationType.MD5Hash : VerificationType.Version
                };

                fileinfos.Add(fileInfo);
                TreeNode chldNode = new TreeNode();
                string verification = string.Empty;
                if (fileInfo.VerificationType == VerificationType.MD5Hash)
                {
                    verification = $":{fileInfo.MD5HashStr}";
                }
                chldNode.Text = $"{chlFile.Name} ({fileInfo.FileVersion})({fileInfo.VerificationType}{verification})";
                   chldNode.Tag = fileInfo;
                
                treeView1.Invoke(new Action(delegate
                {
                    node.Nodes.Add(chldNode);
                    treeView1.ExpandAll();
                }));
            }
            DirectoryInfo[] chldFolders = folder.GetDirectories();
            foreach (DirectoryInfo chldFolder in chldFolders)
            {
                TreeNode chldNode = new TreeNode
                {
                    Text = folder.Name,
                    Tag = "Folder",
                    Name = folder.FullName.Replace(selectPath, string.Empty)
                };
                treeView1.Invoke(new Action(delegate
                {
                    node.Nodes.Add(chldNode);
                    treeView1.ExpandAll();
                }));
                GetFiles(chldFolder.FullName, chldNode);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Form_BuildConfig form = new Form_BuildConfig(updateInfo);
            form.ShowDialog();
        }
    }
}
