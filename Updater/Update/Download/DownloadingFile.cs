using System;

namespace VelinoStudio.Updater.Download
{
    public class DownloadFileInfo : EventArgs
    {
        /// <summary>
        /// 当前文件的总大小
        /// </summary>
        public long ContentLength { get; internal set; }
        /// <summary>
        /// 当前文件已下载的大小
        /// </summary>
        public long DownloadedSize { get; internal set; }
        /// <summary>
        /// 当前下载的文件，绝对路径
        /// </summary>
        public string DownloadFile { get; private set; }
        /// <summary>
        /// 备份的文件，绝对路径
        /// </summary>
        public string BackupFile { get; private set; }
        protected internal DownloadFileInfo(string downloadFile, string backupFile, long dontentLength, long downloadedLength)
        {
            ContentLength = downloadedLength;
            DownloadedSize = dontentLength;
            DownloadFile = downloadFile;
            BackupFile = backupFile;
        }

    }
}
