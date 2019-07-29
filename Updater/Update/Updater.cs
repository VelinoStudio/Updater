using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

namespace VelinoStudio.Updater
{
    using Download;
    using UpdateInformation;
    public class Updater
    {
        internal static string UpdateTempRandom => ".ekeYnB.";
        internal static string UpdateTempFileName => UpdateTempRandom + Guid.NewGuid().ToString();
        internal static string UpdateUrl { get; private set; }
        private string updateConfig = string.Empty;
        private UpdateInfo updateInfo;
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
        public DialogResult CheckUpdate<T>(Form parentForm, out Dictionary<string, string> configurations) where T : UpdateForm, IUpdateForm, new()
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
                    updateInfo = Json.JsonDeserialize<UpdateInfo>(File.ReadAllText(tempUpdateConfigFile));
                    File.Delete(tempUpdateConfigFile);
                    if (updateInfo == null)
                    {
                        throw Common.Exception<Exception>("更新配置文件反序列失败！" + Environment.NewLine + "错误代码：0x0d3");
                    }
                    foreach (UpdateFileInfo updateFileInfo in updateInfo.FileInfos)
                    {
                        if (Common.CheckFileIsUpdate(updateFileInfo))
                        {
                            result = true;
                            break;
                        }
                    }
                }

                configurations = updateInfo.Configurations;
                if (result)
                {
                    
                    using (UpdateForm updateForm = new T())
                    {
                        Common.WriteLog_Information("实例化更新窗体，窗体类型：{0}", typeof(T).AssemblyQualifiedName);
                        updateForm.UpdateProgressing += (s, e) =>
                        {
                            try
                            {
                                UpdateProgressing?.Invoke(this, e);
                            }
                            catch (Exception ex)
                            {
                                Common.WriteLog_Error(ex);
                                throw ex;
                            }
                        };
                        updateForm.UpdateInfo = updateInfo;

                        DialogResult dialogResult = (DialogResult)parentForm.Invoke(new Func<IWin32Window, DialogResult>(delegate { return updateForm.ShowDialog(parentForm); }), parentForm);
                        if (dialogResult == DialogResult.Abort)
                        {
                            Common.WriteLog_Error(updateForm.Exception);
                            throw updateForm.Exception;
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


    }
}
