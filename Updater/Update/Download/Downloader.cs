using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace VelinoStudio.Updater.Download
{
    internal class Downloader
    {
        internal event EventHandler<DownloadInfo> Downloading;
        public void OnDownloading(DownloadInfo downloadInfo)
        {
            try
            {
                Downloading?.Invoke(this, downloadInfo);
            }
            catch (Exception ex)
            {
                switch (downloadInfo.DownloadState)
                {
                    case DownloadState.BeginDownload:
                        throw Common.Exception<Exception>(ex.Message + Environment.NewLine + "错误代码：0x0fa1");
                    case DownloadState.Downloading:
                        throw Common.Exception<Exception>(ex.Message + Environment.NewLine + "错误代码：0x0fa2");
                    case DownloadState.Downloaded:
                        throw Common.Exception<Exception>(ex.Message + Environment.NewLine + "错误代码：0x0fa3");
                }
            }
        }
        internal bool HttpDownload(string url, string localFile)
        {
            string directory = Path.GetDirectoryName(localFile);
            //Console.WriteLine($"下载路径：{url}，本地路径：{directory}，文件名：{Path.GetFileName(localFile)}");
            Common.WriteLog_Information("下载路径：{0}，本地路径：{1}，文件名：{2}", url, directory, Path.GetFileName(localFile));
            bool success = false;
            FileStream writeStream = null;
            Stream readStream = null;
            string backupFile = localFile + Updater.UpdateTempFileName;
            if (File.Exists(localFile))
            {
                File.Move(localFile, backupFile);
                Common.WriteLog_Information("备份文件：{0} => {1}", localFile, backupFile);
            }
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            writeStream = new FileStream(localFile, FileMode.Create);
            DownloadFileInfo downloadingFile = null;
            try
            {
                downloadingFile = new DownloadFileInfo(localFile, backupFile, 0, 0);

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Proxy = null;
                WebResponse webResponse = webRequest.GetResponse();
                long contentLength = webResponse.ContentLength;

                downloadingFile.ContentLength = contentLength;
                OnDownloading(new DownloadInfo(DownloadState.BeginDownload, downloadingFile));

                readStream = webResponse.GetResponseStream();
                byte[] buffer = new byte[8192];
                int downloadedSize = readStream.Read(buffer, 0, buffer.Length);

                downloadingFile.DownloadedSize += downloadedSize;
                OnDownloading(new DownloadInfo(DownloadState.Downloading, downloadingFile));

                while (downloadedSize > 0)
                {
                    writeStream.Write(buffer, 0, downloadedSize);
                    downloadedSize = readStream.Read(buffer, 0, buffer.Length);

                    downloadingFile.DownloadedSize += downloadedSize;
                    OnDownloading(new DownloadInfo(DownloadState.Downloading, downloadingFile));

                    Application.DoEvents();
                }

                OnDownloading(new DownloadInfo(DownloadState.Downloaded, downloadingFile));

                success = true;
            }
            catch (Exception ex)
            {
                throw Common.Exception<Exception>(ex.Message + Environment.NewLine + "错误代码：0x0f01");
            }
            finally
            {
                if (writeStream != null) writeStream.Close();
                if (readStream != null) readStream.Close();
            }
            return success;
        }

    }
}
