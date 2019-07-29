using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace VelinoStudio.Updater
{
    using UpdateInformation;
    public interface IUpdateForm : IDisposable
    {
        
        event FormClosingEventHandler FormClosing;
        event EventHandler Shown;
        /// <summary>
        /// 将窗体显示为具有指定所有者的模式对话框。
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        DialogResult ShowDialog(IWin32Window owner);
        /// <summary>
        /// 获取或设置窗体的对话框结果。
        /// </summary>
        DialogResult DialogResult { get; set; }
        /// <summary>
        /// 获取更新信息
        /// </summary>
        UpdateInfo UpdateInfo { get; set; }
        /// <summary>
        /// 获取更新历史
        /// </summary>
        string VersionHistory { get; set; }
        /// <summary>
        /// 获取需要更新的文件
        /// </summary>
        UpdateFileInfo[] NeedUpdateFile { get; set; }
        /// <summary>
        /// 获取更新时的备份数据
        /// </summary>
        Func<Dictionary<string, string>> BackupFile { get; set; }
        /// <summary>
        /// 发生错误
        /// </summary>
        Action OnErrorUpdate { get; set; }
        /// <summary>
        /// 完成更新
        /// </summary>
        Action OnFinishUpdate { get; set; }
        /// <summary>
        /// 取消更新
        /// </summary>
        Action OnCancelUpdate { get; set; }
        /// <summary>
        /// 回滚
        /// </summary>
        Action OnRollbackUpdate { get; set; }
        /// <summary>
        /// 开始更新
        /// </summary>
        Action OnBeginUpdate { get; set; }
        /// <summary>
        /// 更新进度的CALLBack
        /// </summary>
        /// <param name=""></param>
        void UpdateProgressingCallBack(UpdateArgs e);
    }
}
