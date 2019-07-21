using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace VelinoStudio.Updater
{
    using Download;
    using UpdateInformation;
    public partial class UpdateForm : Form, IUpdateForm
    {
        UpdateInfo _UpdateInfo = null;
        string _VersionHistory = null;
        public Exception Exception { get; protected internal set; }
        private Dictionary<string, string> UpdateBackupFile;
        protected internal UpdateFileInfo[] needUpdateFile;
        protected internal string VersionHistory => _VersionHistory;
        internal UpdateInfo UpdateInfo
        {
            get { return _UpdateInfo; }
            set
            {
                if (value != null)
                {
                    _UpdateInfo = value;
                    needUpdateFile = GetNeedUpdateFile(_UpdateInfo.FileInfos);
                    Array.Sort(_UpdateInfo.VersionInfos);
                    VersionInfo versionInfo = _UpdateInfo.VersionInfos[_UpdateInfo.VersionInfos.Length - 1];
                    StringBuilder versionSB = new StringBuilder();
                    versionSB.AppendLine($"最新更新 - 主文件版本：{versionInfo.Version}");
                    versionSB.AppendLine();
                    versionSB.AppendLine(versionInfo.UpdateDescribe);
                    versionSB.AppendLine();
                    versionSB.AppendLine("===============================");
                    Common.WriteLog("获取服务器版本信息：{0}\t{1}", versionInfo.Version, versionInfo.UpdateDescribe);
                    for (int versionIndex = _UpdateInfo.VersionInfos.Length - 2; versionIndex >= 0; versionIndex--)
                    {
                        versionSB.AppendLine();
                        versionSB.AppendLine($"历史更新 - 主文件版本：{_UpdateInfo.VersionInfos[versionIndex].Version}");
                        versionSB.AppendLine();
                        versionSB.AppendLine(_UpdateInfo.VersionInfos[versionIndex].UpdateDescribe);
                        versionSB.AppendLine();
                        versionSB.AppendLine("===============================");
                        Common.WriteLog("获取服务器版本信息：{0}\t{1}", _UpdateInfo.VersionInfos[versionIndex].Version, _UpdateInfo.VersionInfos[versionIndex].UpdateDescribe);
                    }
                    _VersionHistory = versionSB.ToString();
                    
                }
            }
        }
        public event EventHandler<UpdateArgs> UpdateProgressing;
        private void OnUpdateProgressing(UpdateArgs updateArgs)
        {
            try
            {
                UpdateProgressing?.Invoke(this, updateArgs);
            }
            catch (Exception ex)
            {
                switch (updateArgs.UpdateState)
                {
                    case UpdateState.Find:
                        throw Common.Exception<Exception>(ex.Message + Environment.NewLine + "错误代码：0x0fb1");
                    case UpdateState.Updating:
                        throw Common.Exception<Exception>(ex.Message + Environment.NewLine + "错误代码：0x0fb2");
                    case UpdateState.BeginDownload:
                        throw Common.Exception<Exception>(ex.Message + Environment.NewLine + "错误代码：0x0fb3");
                    case UpdateState.Downloading:
                        throw Common.Exception<Exception>(ex.Message + Environment.NewLine + "错误代码：0x0fb4");
                    case UpdateState.Downloaded:
                        throw Common.Exception<Exception>(ex.Message + Environment.NewLine + "错误代码：0x0fb5");
                    case UpdateState.Finish:
                        throw Common.Exception<Exception>(ex.Message + Environment.NewLine + "错误代码：0x0fb6");
                    case UpdateState.Cancel:
                        throw Common.Exception<Exception>(ex.Message + Environment.NewLine + "错误代码：0x0fb7");
                    case UpdateState.Rollback:
                        throw Common.Exception<Exception>(ex.Message + Environment.NewLine + "错误代码：0x0fb8");
                }
            }

        }
        Downloader download = new Downloader();
        bool beginUpdate = false;


        public static bool IsDesignMode()
        {
            bool returnFlag = false;

#if DEBUG
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                returnFlag = true;
            }
            else if (Process.GetCurrentProcess().ProcessName == "devenv")
            {
                returnFlag = true;
            }
#endif

            return returnFlag;
        }


        public UpdateForm()
        {
            InitializeComponent();
            this.Shown += (s, e) =>
              {
                  if (!IsDesignMode())
                  {
                      OnUpdateProgressing(new UpdateArgs(UpdateState.Find, null, needUpdateFile.Length, -1));
                  }
              };
            this.FormClosing += (s, e) => { e.Cancel = beginUpdate; };
        }

        private UpdateFileInfo[] GetNeedUpdateFile(UpdateFileInfo[] updateFileInfos)
        {
            List<UpdateFileInfo> needUpdate = new List<UpdateFileInfo>();
            foreach (UpdateFileInfo updateFileInfo in updateFileInfos)
            {
                if (Common.CheckFileIsUpdate(updateFileInfo))
                {
                    Common.WriteLog_Warning("文件 {0} 需要更新", updateFileInfo.FilePath);
                    needUpdate.Add(updateFileInfo);
                }
            }
            return needUpdate.ToArray();
        }
        /// <summary>
        /// 发生错误
        /// </summary>
        protected internal virtual void OnErrorUpdate()
        {
            beginUpdate = false;
            Common.WriteLog_Warning("更新发生错误");
            DialogResult = DialogResult.Abort;
        }
        /// <summary>
        /// 完成更新
        /// </summary>
        protected internal virtual void OnFinishUpdate()
        {
            beginUpdate = false;
            Common.WriteLog_Information("更新完成");
            DialogResult = DialogResult.OK;
        }
        /// <summary>
        /// 取消更新
        /// </summary>
        protected internal virtual void OnCancelUpdate()
        {
            beginUpdate = false;
            OnUpdateProgressing(new UpdateArgs(UpdateState.Cancel, null, needUpdateFile.Length, -1));
            DialogResult = DialogResult.Cancel;
        }
        /// <summary>
        /// 回滚
        /// </summary>
        protected internal virtual void OnRollbackUpdate()
        {
            string exceptionMessage = this.Exception.Message;
            Common.WriteLog_Warning("开始更新回滚");
            bool result = false;
            foreach (KeyValuePair<string, string> keyValue in UpdateBackupFile)
            {
                OnUpdateProgressing(new UpdateArgs(UpdateState.Rollback, new UpdateFileInfo() { FilePath = keyValue.Key }, -1, -1));
                Common.WriteLog_Warning("开始回滚文件：{0}，备份位置：{1}", keyValue.Key, keyValue.Value);
                if (File.Exists(keyValue.Value))
                {
                    File.Delete(keyValue.Key);
                    if (!File.Exists(keyValue.Key))
                    {
                        try
                        {
                            File.Move(keyValue.Value, keyValue.Key);
                            if (File.Exists(keyValue.Key))
                            {
                                Common.WriteLog_Warning("文件 {0} 回滚成功", keyValue.Key);
                                result = true;
                            }
                            else
                            {
                                Common.WriteLog_Warning("文件 {0} 回滚失败，备份文件无法移动到原位", keyValue.Key);
                                exceptionMessage += $"{Environment.NewLine}更新回滚失败！可能需要重新安装！{Environment.NewLine}错误代码：0x0f07";
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            exceptionMessage += $"{Environment.NewLine}更新回滚失败！可能需要重新安装！{Environment.NewLine}{ex.Message}{Environment.NewLine}错误代码：0x0f04";
                            break;
                        }

                    }
                    else
                    {
                        Common.WriteLog_Warning("文件 {0} 回滚失败，新文件无法删除", keyValue.Key);
                        exceptionMessage += $"{Environment.NewLine}更新回滚失败！可能需要重新安装！{Environment.NewLine}错误代码：0x0f05";
                        break;
                    }
                }
                else
                {
                    Common.WriteLog_Warning("文件 {0} 回滚失败，备份文件不存在", keyValue.Key);
                    exceptionMessage += $"{Environment.NewLine}更新回滚失败！可能需要重新安装！{Environment.NewLine}错误代码：0x0f06";
                    break;
                }
            }
            OnUpdateProgressing(new UpdateArgs(UpdateState.Rollbacked, null, -1, -1));
            if (result)
            {
                exceptionMessage += $"{Environment.NewLine}当前更新已回滚";
            }
            this.Exception = new Exception(exceptionMessage);
            OnUpdateProgressing(new UpdateArgs(UpdateState.RollbackFailed, null, -1, -1));
        }


        /// <summary>
        /// 开始更新
        /// </summary>
        protected internal virtual void OnBeginUpdate()
        {
            try
            {
                
                UpdateBackupFile = new Dictionary<string, string>();
                UpdateFileInfo currentFileInfo = null;
                beginUpdate = true;
                if (needUpdateFile != null && needUpdateFile.Length > 0)
                {
                    Common.WriteLog_Information("开始更新进程，需要更新的文件数量：{0}", needUpdateFile.Length);
                    int thisFileIndex = 1;

                    download.Downloading += (s, e) =>
                    {
                        switch (e.DownloadState)
                        {
                            case DownloadState.BeginDownload:
                                UpdateBackupFile.Add(e.DownloadingFile.DownloadFile, e.DownloadingFile.BackupFile);
                                OnUpdateProgressing(new UpdateArgs(UpdateState.BeginDownload, currentFileInfo, needUpdateFile.Length, thisFileIndex, e));
                                break;
                            case DownloadState.Downloading:
                                OnUpdateProgressing(new UpdateArgs(UpdateState.Downloading, currentFileInfo, needUpdateFile.Length, thisFileIndex, e));
                                break;
                            case DownloadState.Downloaded:
                                OnUpdateProgressing(new UpdateArgs(UpdateState.Downloaded, currentFileInfo, needUpdateFile.Length, thisFileIndex, e));
                                break;
                        }
                    };


                    foreach (UpdateFileInfo updateFileInfo in needUpdateFile)
                    {
                        string currentFile = updateFileInfo.FilePath.Replace("\\", "/");
                        currentFileInfo = updateFileInfo;
                        OnUpdateProgressing(new UpdateArgs(UpdateState.Updating, updateFileInfo, needUpdateFile.Length, thisFileIndex));
                        string file = Path.Combine(Environment.CurrentDirectory, currentFile.TrimStart('/'));
                        string url = string.Empty;
                        if (!Updater.UpdateUrl.EndsWith("/"))
                        {
                            url = Updater.UpdateUrl + currentFile;
                        }
                        else
                        {
                            url = Updater.UpdateUrl + currentFile.TrimStart('/');
                        }
                        Application.DoEvents();
                        download.HttpDownload(url, file);
                        thisFileIndex++;
                    }
                    OnUpdateProgressing(new UpdateArgs(UpdateState.Finish, null, needUpdateFile.Length, -1));
                }
            }
            catch (Exception ex)
            {
                beginUpdate = false;
                this.Exception = Common.Exception<Exception>(ex.Message + Environment.NewLine + "错误代码：0x0f02");
                OnUpdateProgressing(new UpdateArgs(UpdateState.Error, null, needUpdateFile.Length, -1));
                OnRollbackUpdate();
            }
        }
    }
}
