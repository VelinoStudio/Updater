using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace VelinoStudio.Updater.UpdaterUI
{
    using UpdateInformation;
    public partial class Form_BuildConfig : Form
    {
        UpdateInfo newUpdateInfo = null;
        UpdateInfo oldUpdateInfo = null;
        public Form_BuildConfig(UpdateInfo updateInfo)
        {
            InitializeComponent();
            newUpdateInfo = updateInfo;
            richTextBox1.Enabled = false;
            comboBox1.Items.AddRange(newUpdateInfo.FileInfos);
            comboBox1.SelectedIndex = 0;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "升级文件配置文件|*.json";
                openFileDialog.RestoreDirectory = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Console.WriteLine(openFileDialog.FileName);
                    string json = File.ReadAllText(openFileDialog.FileName);
                    oldUpdateInfo = Json.JsonDeserialize<UpdateInfo>(json);
                    VersionInfo[] VersionInfos = oldUpdateInfo.VersionInfos;
                    Array.Sort(VersionInfos);
                    foreach (VersionInfo vi in VersionInfos)
                    {
                        richTextBox1.AppendText(vi.Version + Environment.NewLine);
                        richTextBox1.AppendText(vi.UpdateDescribe + Environment.NewLine);
                        richTextBox1.AppendText(Environment.NewLine);
                    }
                }
            }
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            richTextBox1.Enabled = true;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.FileName = "update.json";
                saveFileDialog.CheckPathExists = true;
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Console.WriteLine(saveFileDialog.FileName);
                    List<VersionInfo> versionInfos;
                    if (oldUpdateInfo != null)
                    {
                        versionInfos = new List<VersionInfo>(oldUpdateInfo.VersionInfos);
                    }
                    else
                    {
                        versionInfos = new List<VersionInfo>();
                    }
                    List<UpdateFileInfo> fileInfos = new List<UpdateFileInfo>(newUpdateInfo.FileInfos);
                    UpdateFileInfo fileInfo = fileInfos.Find(f => f.FileName == Form_UpdateConfig.MainExe);
                    if (fileInfo == null)
                    {
                        MessageBox.Show($"未包含主程序 {Form_UpdateConfig.MainExe}");
                        return;
                    }
                    versionInfos.Add(new VersionInfo()
                    {
                        Index = versionInfos.Count + 1,
                        Version = fileInfo.FileVersion,
                        UpdateDescribe = richTextBox1.Text,
                        UpdateDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
                    });
                    VersionInfo[] vis = versionInfos.ToArray();
                    Array.Sort(vis);
                    newUpdateInfo.VersionInfos = vis;
                    string json = Json.JsonSerialize(newUpdateInfo);
                    Console.WriteLine(json);
                    File.WriteAllText(saveFileDialog.FileName, json);
                }
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex >= 0)
            {
                UpdateFileInfo fileInfo = comboBox1.SelectedItem as UpdateFileInfo;
                richTextBox1.AppendText($"文件名：{fileInfo.FileName}，文件版本：{fileInfo.FileVersion}");
            }
        }
    }
}
