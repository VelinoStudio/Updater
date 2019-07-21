namespace VelinoStudio.Updater.Download
{
    public enum DownloadState
    {
        /// <summary>
        /// 即将开始下载
        /// </summary>
        BeginDownload = 1,
        /// <summary>
        /// 正在下载中
        /// </summary>
        Downloading = 2,
        /// <summary>
        /// 下载完成
        /// </summary>
        Downloaded = 4,
    }
}
