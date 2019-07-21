namespace VelinoStudio.Updater
{
    public enum UpdateState
    {
        /// <summary>
        /// 更新完成
        /// </summary>
        Finish = 1,
        /// <summary>
        /// 取消更新
        /// </summary>
        Cancel = 2,
        /// <summary>
        /// 正在更新
        /// </summary>
        Updating = 4,
        /// <summary>
        /// 找到更新
        /// </summary>
        Find = 8,
        /// <summary>
        /// 即将下载当前文件
        /// </summary>
        BeginDownload = 16,
        /// <summary>
        /// 正在下载当前文件
        /// </summary>
        Downloading = 32,
        /// <summary>
        /// 当前文件以下载完成
        /// </summary>
        Downloaded = 64,
        /// <summary>
        /// 当更新发生错误时进行回滚操作
        /// </summary>
        Rollback = 128,
        /// <summary>
        /// 已经回滚完成
        /// </summary>
        Rollbacked = 256,
        /// <summary>
        /// 更新发生错误
        /// </summary>
        Error = 512,
        /// <summary>
        /// 回滚失败
        /// </summary>
        RollbackFailed = 1024
    }
   
}
