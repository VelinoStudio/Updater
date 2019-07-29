using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
namespace VelinoStudio.Updater
{
    using Download;
    using UpdateInformation;
    public class Updater
    {
        private UpdateInfo _UpdateInfo;
        bool beginUpdate = false;
        private Exception Exception = new Exception();
        internal static string UpdateTempRandom => ".ekeYnB.";
        internal static string UpdateTempFileName => UpdateTempRandom + Guid.NewGuid().ToString();
        internal static string UpdateUrl { get; private set; }

        private string updateConfig = string.Empty;
        private Dictionary<string, string> BackupFile { get; set; }
        private UpdateFileInfo[] NeedUpdateFile { get; set; }
        private string VersionHistory { get; set; }
        private Action<UpdateArgs> UpdateProgressingCallBack;

        private UpdateInfo UpdateInfo
        {
            get { return _UpdateInfo; }
            set
            {
                if (value != null)
                {
                    _UpdateInfo = value;
                    NeedUpdateFile = GetNeedUpdateFile(_UpdateInfo.FileInfos);
                    Array.Sort(_UpdateInfo.VersionInfos);
                    VersionInfo versionInfo = _UpdateInfo.VersionInfos[_UpdateInfo.VersionInfos.Length - 1];
                    StringBuilder versionSB = new StringBuilder();
                    versionSB.AppendLine($"最新更新 - 主文件版本：{versionInfo.Version}");
                    versionSB.AppendLine();
                    versionSB.AppendLine(versionInfo.UpdateDescribe);
                    versionSB.AppendLine();
                    versionSB.AppendLine("===============================");
                    Common.WriteLog_Information("获取服务器版本信息：{0}\t{1}", versionInfo.Version, versionInfo.UpdateDescribe);
                    for (int versionIndex = _UpdateInfo.VersionInfos.Length - 2; versionIndex >= 0; versionIndex--)
                    {
                        versionSB.AppendLine();
                        versionSB.AppendLine($"历史更新 - 主文件版本：{_UpdateInfo.VersionInfos[versionIndex].Version}");
                        versionSB.AppendLine();
                        versionSB.AppendLine(_UpdateInfo.VersionInfos[versionIndex].UpdateDescribe);
                        versionSB.AppendLine();
                        versionSB.AppendLine("===============================");
                        Common.WriteLog_Information("获取服务器版本信息：{0}\t{1}", _UpdateInfo.VersionInfos[versionIndex].Version, _UpdateInfo.VersionInfos[versionIndex].UpdateDescribe);
                    }
                    VersionHistory = versionSB.ToString();

                }
            }
        }

        Downloader download = new Downloader();
        public bool Debug { get { return Common.Debug; } set { Common.Debug = value; } }

        public Updater(string updateUrl, string updateConfig)
        {
            if (string.IsNullOrWhiteSpace(updateUrl) || string.IsNullOrWhiteSpace(updateConfig))
            {
                throw Common.Exception<ArgumentNullException>("updateUrl,updateConfig", "启用更新实例错误，更新URL及更新配置文件不能为空！" + Environment.NewLine + "错误代码：0x0d2");
            }
            UpdateUrl = updateUrl;
            this.updateConfig = updateConfig;
        }

        public void ClearOldFile(string directory)
        {
            string[] files = Directory.GetFiles(directory, "*.ekeYnB.*");
            foreach (string file in files)
            {
                Common.WriteLog_Information("清理旧文件 {0}", file);
                File.Delete(file);
            }

            string[] directorys = Directory.GetDirectories(directory);
            foreach (string dir in directorys)
            {
                ClearOldFile(dir);
            }
        }
        public event EventHandler<UpdateArgs> UpdateProgressing;

        private void OnUpdateProgressing(UpdateArgs updateArgs)
        {
            try
            {
                UpdateProgressing?.Invoke(this, updateArgs);
                UpdateProgressingCallBack?.Invoke(updateArgs);
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

        public DialogResult CheckUpdate(Form parentForm, out Dictionary<string, string> configurations)
        {
            try
            {
                return CheckUpdate<Form_FindedUpdate>(parentForm, out configurations);
            }
            catch (Exception ex)
            {
                Common.WriteLog_Error(ex);
                throw ex;
            }

        }
        public DialogResult CheckUpdate<T>(Form parentForm, out Dictionary<string, string> configurations) where T : Form, IUpdateForm, new()
        {
            ClearOldFile(Environment.CurrentDirectory);
            bool result = false;
            string tempUpdateConfigFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");
            Common.WriteLog_Information("配置文件临时路径：{0}", tempUpdateConfigFile);
            try
            {
                string url = string.Empty;
                if (!UpdateUrl.EndsWith("/"))
                {
                    url = UpdateUrl + "/";
                }
                url += updateConfig;

                if (download.HttpDownload(url, tempUpdateConfigFile))
                {
                    UpdateInfo = Json.JsonDeserialize<UpdateInfo>(File.ReadAllText(tempUpdateConfigFile));
                    File.Delete(tempUpdateConfigFile);
                    if (UpdateInfo == null)
                    {
                        throw Common.Exception<Exception>("更新配置文件反序列失败！" + Environment.NewLine + "错误代码：0x0d3");
                    }
                    foreach (UpdateFileInfo updateFileInfo in UpdateInfo.FileInfos)
                    {
                        if (Common.CheckFileIsUpdate(updateFileInfo))
                        {
                            result = true;
                            break;
                        }
                    }
                }

                configurations = null;
                if (result)
                {
                    configurations = UpdateInfo.Configurations;
                    using (IUpdateForm updateForm = new T())
                    {
                        Common.WriteLog_Information("实例化更新窗体，窗体类型：{0}", typeof(T).AssemblyQualifiedName);

                        updateForm.FormClosing += (s, e) => { e.Cancel = beginUpdate; };
                        updateForm.Shown += (s, e) => { OnUpdateProgressing(new UpdateArgs(UpdateState.Find, null, NeedUpdateFile.Length, -1)); };
                        updateForm.OnErrorUpdate = new Action(delegate
                          {
                            beginUpdate = false;
                            Common.WriteLog_Warning("更新发生错误");
                            updateForm.DialogResult = DialogResult.Abort;
                          });

                        updateForm.OnFinishUpdate = new Action(delegate
                          {
                              beginUpdate = false;
                              Common.WriteLog_Information("更新完成");
                              updateForm.DialogResult = DialogResult.OK;
                          });

                        updateForm.OnCancelUpdate = new Action(delegate
                          {
                              beginUpdate = false;
                              OnUpdateProgressing(new UpdateArgs(UpdateState.Cancel, null, NeedUpdateFile.Length, -1));
                              updateForm.DialogResult = DialogResult.Cancel;
                          });

                        updateForm.OnRollbackUpdate = OnRollbackUpdate;

                        updateForm.OnBeginUpdate = OnBeginUpdate;

                        updateForm.UpdateInfo = _UpdateInfo;

                        updateForm.VersionHistory = VersionHistory;

                        updateForm.NeedUpdateFile = NeedUpdateFile;

                        updateForm.BackupFile = new Func<Dictionary<string, string>>(delegate
                          {
                            return BackupFile;
                          });

                        UpdateProgressingCallBack = updateForm.UpdateProgressingCallBack;




                        DialogResult dialogResult = (DialogResult)parentForm.Invoke(new Func<IWin32Window, DialogResult>(delegate { return updateForm.ShowDialog(parentForm); }), parentForm);
                        if (dialogResult == DialogResult.Abort)
                        {
                            Common.WriteLog_Error(Exception);
                            throw Exception;
                        }
                        return dialogResult;
                    }
                }
                return DialogResult.Cancel;
            }
            catch (Exception ex)
            {
                throw Common.Exception<Exception>(ex.Message + Environment.NewLine + "错误代码：0x0f03");
            }
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

        #region 供使用接口的窗体的委托方法，方便使用各种控件的窗体，而无需因继承System.Windows.Form的窗体无法使用其他控件漂亮的窗体,同时也减少一层委托

        private void OnRollbackUpdate()
        {
            string exceptionMessage = this.Exception.Message;
            Common.WriteLog_Warning("开始更新回滚");
            bool result = false;
            foreach (KeyValuePair<string, string> keyValue in BackupFile)
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



        private void OnBeginUpdate()
        {
            try
            {

                BackupFile = new Dictionary<string, string>();
                UpdateFileInfo currentFileInfo = null;
                beginUpdate = true;
                if (NeedUpdateFile != null && NeedUpdateFile.Length > 0)
                {
                    Common.WriteLog_Information("开始更新进程，需要更新的文件数量：{0}", NeedUpdateFile.Length);
                    int thisFileIndex = 1;

                    download.Downloading += (s, e) =>
                    {
                        switch (e.DownloadState)
                        {
                            case DownloadState.BeginDownload:
                                BackupFile.Add(e.DownloadingFile.DownloadFile, e.DownloadingFile.BackupFile);
                                OnUpdateProgressing(new UpdateArgs(UpdateState.BeginDownload, currentFileInfo, NeedUpdateFile.Length, thisFileIndex, e));
                                break;
                            case DownloadState.Downloading:
                                OnUpdateProgressing(new UpdateArgs(UpdateState.Downloading, currentFileInfo, NeedUpdateFile.Length, thisFileIndex, e));
                                break;
                            case DownloadState.Downloaded:
                                OnUpdateProgressing(new UpdateArgs(UpdateState.Downloaded, currentFileInfo, NeedUpdateFile.Length, thisFileIndex, e));
                                break;
                        }
                    };


                    foreach (UpdateFileInfo updateFileInfo in NeedUpdateFile)
                    {
                        string currentFile = updateFileInfo.FilePath.Replace("\\", "/");
                        currentFileInfo = updateFileInfo;
                        OnUpdateProgressing(new UpdateArgs(UpdateState.Updating, updateFileInfo, NeedUpdateFile.Length, thisFileIndex));
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
                    OnUpdateProgressing(new UpdateArgs(UpdateState.Finish, null, NeedUpdateFile.Length, -1));
                }
            }
            catch (Exception ex)
            {
                beginUpdate = false;
                this.Exception = Common.Exception<Exception>(ex.Message + Environment.NewLine + "错误代码：0x0f02");
                OnUpdateProgressing(new UpdateArgs(UpdateState.Error, null, NeedUpdateFile.Length, -1));
                OnRollbackUpdate();
            }
        }
        #endregion


    }
}
