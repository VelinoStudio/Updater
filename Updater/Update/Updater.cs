using System;
using System.IO;
using System.Windows.Forms;


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

        public Updater(string updateUrl, string updateConfig)
        {
            if (string.IsNullOrWhiteSpace(updateUrl) || string.IsNullOrWhiteSpace(updateConfig))
                throw new ArgumentNullException("启用更新实例错误，更新URL及更新配置文件不能为空！" + Environment.NewLine + "错误代码：0x0d2");
            UpdateUrl = updateUrl;
            this.updateConfig = updateConfig;
        }

        public void ClearOldFile(string directory)
        {
            string[] files = Directory.GetFiles(directory, "*.ekeYnB.*");
            foreach (string file in files)
            {
                File.Delete(file);
            }

            string[] directorys = Directory.GetDirectories(directory);
            foreach (string dir in directorys)
            {
                ClearOldFile(dir);
            }
        }
        public event EventHandler<UpdateArgs> UpdateProgressing;
        public DialogResult CheckUpdate(Form parentForm)
        {
            try
            {
                return CheckUpdate<Form_FindedUpdate>(parentForm);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }
        public DialogResult CheckUpdate<T>(Form parentForm) where T : UpdateForm, IUpdateForm, new()
        {
            ClearOldFile(Environment.CurrentDirectory);
            bool result = false;
            string tempUpdateConfigFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

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
                    if (updateInfo == null) throw new Exception("更新配置文件反序列失败！" + Environment.NewLine + "错误代码：0x0d3");
                    foreach (UpdateFileInfo updateFileInfo in updateInfo.FileInfos)
                    {
                        if (Common.CheckFileIsUpdate(updateFileInfo))
                        {
                            result = true;
                            break;
                        }
                    }
                }


                if (result)
                {
                    using (UpdateForm updateForm = new T())
                    {
                        updateForm.UpdateProgressing += (s, e) =>
                        {
                            try
                            {
                                UpdateProgressing?.Invoke(this, e);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message);
                            }
                        };
                        updateForm.UpdateInfo = updateInfo;

                        DialogResult dialogResult = (DialogResult)parentForm.Invoke(new Func<IWin32Window, DialogResult>(delegate { return updateForm.ShowDialog(parentForm); }), parentForm);
                        if (dialogResult == DialogResult.Abort)
                        {
                            throw updateForm.Exception;
                        }
                        return dialogResult;
                    }
                }
                return DialogResult.Cancel;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex);
                throw new Exception(ex.Message + Environment.NewLine + "错误代码：0x0f03");
            }
        }

        
    }
}
