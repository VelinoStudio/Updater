using System;
using System.Collections.Generic;
using System.Windows.Forms;
using VelinoStudio.Updater.UpdateInformation;

namespace VelinoStudio.Updater
{

    public partial class Form_FindedUpdate : Form, IUpdateForm
    {
        bool finish = false;
        bool error = false;

        public UpdateInfo UpdateInfo { get; set; }
        public string VersionHistory { get; set; }
        public UpdateFileInfo[] NeedUpdateFile { get; set; }
        public Func<Dictionary<string, string>> BackupFile { get; set; }
        public Action OnErrorUpdate { get; set; }
        public Action OnFinishUpdate { get; set; }
        public Action OnCancelUpdate { get; set; }
        public Action OnRollbackUpdate { get; set; }
        public Action OnBeginUpdate { get; set; }

   

        public Form_FindedUpdate()
        {
            InitializeComponent();

        }
        private void Form_FindedUpdate_Load(object sender, EventArgs f)
        {
            richTextBox1.Text = VersionHistory;


            progressBar1.Maximum = NeedUpdateFile.Length;
            progressBar1.Step = 1;
            progressBar1.Minimum = 0;
            progressBar1.Value = 0;
            progressBar1.Visible = true;

            progressBar2.Maximum = 100;
            progressBar2.Minimum = 0;
            progressBar2.Value = 0;
            progressBar2.Visible = true;
        }

        public void UpdateProgressingCallBack(UpdateArgs e)
        {
            switch (e.UpdateState)
            {
                case UpdateState.Find:
                    {
                        this.Text = $"找到更新，更新文件数量 {e.FilesCount}";
                    }
                    break;
                case UpdateState.Updating:
                    {
                        this.Text = $"更新中，当前文件：{e.UpdateFileInfo.FilePath} ({e.CurrentIndex}/{e.FilesCount})"; 
                    }
                    break;
                case UpdateState.Downloading:
                    {
                        int progress = (int)(((e.DownloadInfo.DownloadingFile.DownloadedSize * 1.00) / (e.DownloadInfo.DownloadingFile.ContentLength * 1.00)) * 100);
                        progressBar2.Value = progress;
                    }
                    break;
                case UpdateState.Downloaded:
                    {
                        progressBar1.PerformStep();
                    }
                    break;
                case UpdateState.Rollback:
                    {
                        this.Text = $"更新失败，正在回滚：{e.UpdateFileInfo.FilePath.Replace(Environment.CurrentDirectory, "")}";
                        error = true;
                    }
                    break;
                case UpdateState.Finish:
                    {
                        this.Text = "完成更新！";
                        button3.Text = $"完成更新";
                        button1.Enabled = button1.Visible = false;
                        button2.Enabled = button2.Visible = false;
                        button3.Enabled = button3.Visible = true;
                    }
                    break;
                case UpdateState.Rollbacked:
                    {
                        this.Text = $"回滚完成，关闭更新界面查看错误信息！";
                        button3.Text = $"关闭";
                        button1.Enabled = button1.Visible = false;
                        button2.Enabled = button2.Visible = false;
                        button3.Enabled = button3.Visible = false;
                    }
                    break;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (finish)
            {
                OnFinishUpdate();
            }
            else
            {
                button1.Enabled = false;
                button2.Enabled =  false;
                OnBeginUpdate();
            }
            
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if (error)
            {
                OnErrorUpdate();
            }
            else
            {
                OnFinishUpdate();
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (finish)
            {
                OnFinishUpdate();
            }
            else
            {
                OnCancelUpdate();
            }
            
        }
    }
}
