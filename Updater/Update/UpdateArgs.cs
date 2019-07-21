using System;

namespace VelinoStudio.Updater
{
    using UpdateInformation;
    using Download;
    public class UpdateArgs : EventArgs
    {
        /// <summary>
        /// 当前更新的文件，相对路径
        /// </summary>
        public UpdateFileInfo UpdateFileInfo { get; private set; }
        /// <summary>
        /// 更新的状态
        /// </summary>
        public UpdateState UpdateState { get; private set; }
        /// <summary>
        /// 需要更新的文件总数
        /// </summary>
        public int FilesCount { get; private set; }
        /// <summary>
        /// 当前更新的文件的序号
        /// </summary>
        public int CurrentIndex { get; private set; }
        /// <summary>
        /// 相关的下载信息
        /// </summary>
        public DownloadInfo DownloadInfo { get; private set; }
        protected internal UpdateArgs(UpdateState updateState, UpdateFileInfo updateFileInfo, int filesCount, int currentIndex, DownloadInfo downloadInfo = null)
        {
            UpdateState = updateState;
            UpdateFileInfo = updateFileInfo;
            FilesCount = filesCount;
            CurrentIndex = currentIndex;
            DownloadInfo = downloadInfo;
        }
    }
}
