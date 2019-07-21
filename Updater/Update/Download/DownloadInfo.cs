using System;

namespace VelinoStudio.Updater.Download
{
    public class DownloadInfo : EventArgs
    {

        /// <summary>
        /// 下载的状态
        /// </summary>
        public DownloadState DownloadState { get; private set; }
        /// <summary>
        /// 当前正在下载的文件信息
        /// </summary>
        public DownloadFileInfo DownloadingFile { get; private set; }
        /// <summary>
        /// 当下载的状态为错误时，可由此获得相关的错误信息
        /// </summary>
        internal DownloadInfo(DownloadState downloadState, DownloadFileInfo downloadingFile)
        {
            DownloadState = downloadState;
            DownloadingFile = downloadingFile;
        }
    }
}
