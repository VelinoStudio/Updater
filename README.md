# Updater
更新器



使用UpdaterUI制作更新配置文件，主要以文件版本为主，MD5为辅。将制作的配置文件与更新的程序下的所有文件原样放入HTTP服务器。
客户端引用UpdaterUI里的Updater.dll，并调用下面两句即可。
```java
//更新配置文件中记录的是相对路径，HTTP链接应该是程序的根目录，更新配置文件也需要在这个目录中
Updater updater = new Updater("HTTP链接", "更新配置文件");
//检查并更新
//这个方法唯一的参数是父窗体，使用时可以阻止父窗体被点击到。可以使用单独的线程运行，不会影响到父窗口内代码的运行，而同时又确保不会点击到父窗口
updater.CheckUpdate(Form)；
```


CheckUpdate方法有两个重载：
```java
public DialogResult CheckUpdate(Form parentForm, out Dictionary<string, string> configurations)
public DialogResult CheckUpdate<T>(Form parentForm, out Dictionary<string, string> configurations) where T : Form, IUpdateForm, new()
```
第一个重载增加了一个out参数，用户获取更新配置中保存的.exe.config结尾的配置文件中<appSettings>节点下的所有配置项。
  
第二个重载有一个泛型T参数，使用该参数，可以使用自定义的更新窗口，更新窗口是通过接口“IUpdateForm”调用的，而不是使用窗体“Form”调用，这样提高了自定义窗体的自由度。

  自定义窗口必须是实现了“IUpdateForm”接口的“Form”或“Form”的派生类，你可以使用任何继承于“Form”的第三方窗体控件，比如DevExpress的XtraForm。
  
  接口中定义了两个事件、一个属性和一个方法，这四个都是“Form”类中自带的，自定义窗口中无需自己实现。
  
  除了这四个，还定义了9个属性和1个方法。9个属性中，有三个是给自定义窗体可能用到的基础信息，另外6个是委托方法。9个属性在实例化时，就已经由调用方法全部  赋值了，只需要实现最简单的{get;set;}即可。剩下的那一个方法是更新进度的CallBack，只有一个参数，通过这个参数可以获取更新进度的每一步骤。
  
  自定义窗口的代码看起来应该是这样的：
```java
public partial class Form_FindedUpdate : XtraForm, IUpdateForm
    {
        bool finish = false;
        bool error = false;
        public UpdateInfo UpdateInfo { get; set; }
        public string VersionHistory { get; set; }
        public UpdateFileInfo[] NeedUpdateFile { get; set; }
        public Func<Dictionary<string, string>> BackupFile { get; set; }
        public Action OnErrorUpdate { get; set; }
        public Action OnFinishUpdate { get; set; }
        public Action OnCancelUpdate { get; set; }
        public Action OnRollbackUpdate { get; set; }
        public Action OnBeginUpdate { get; set; }
        public Form_FindedUpdate(){InitializeComponent();}
        private void Form_FindedUpdate_Load(object sender, EventArgs f)
        {
            richTextBox1.Text = VersionHistory;
            progressBar1.Maximum = NeedUpdateFile.Length;
            progressBar1.Step = 1;
            progressBar1.Minimum = 0;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            progressBar2.Maximum = 100;
            progressBar2.Minimum = 0;
            progressBar2.Value = 0;
            progressBar2.Visible = true;
        }
        public void UpdateProgressingCallBack(UpdateArgs e)
        {
            switch (e.UpdateState)
            {
                case UpdateState.Find:
                    this.Text = $"找到更新，更新文件数量 {e.FilesCount}";
                    break;
                case UpdateState.Updating:
                    this.Text = $"更新中，当前文件：{e.UpdateFileInfo.FilePath} ({e.CurrentIndex}/{e.FilesCount})"; 
                    break;
                case UpdateState.Downloading:
                    int progress = (int)(((e.DownloadInfo.DownloadingFile.DownloadedSize * 1.00) / (e.DownloadInfo.DownloadingFile.ContentLength * 1.00)) * 100);
                    progressBar2.Value = progress;
                    break;
                case UpdateState.Downloaded:
                    progressBar1.PerformStep();
                    break;
                case UpdateState.Rollback:
                    this.Text = $"更新失败，正在回滚：{e.UpdateFileInfo.FilePath.Replace(Environment.CurrentDirectory, "")}";
                    error = true;
                    break;
                case UpdateState.Finish:
                    this.Text = "完成更新！";
                    button3.Text = "完成更新";
                    button1.Enabled = button1.Visible = false;
                    button2.Enabled = button2.Visible = false;
                    button3.Enabled = button3.Visible = true;
                    break;
                case UpdateState.Rollbacked:
                    this.Text = "回滚完成，关闭更新界面查看错误信息！";
                    button3.Text = "关闭";
                    button1.Enabled = button1.Visible = false;
                    button2.Enabled = button2.Visible = false;
                    button3.Enabled = button3.Visible = false;
                    break;
            }
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            if (finish)
                OnFinishUpdate();
            else
            {
                button1.Enabled = false;
                button2.Enabled =  false;
                OnBeginUpdate();
            }
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            if (error)
                OnErrorUpdate();
            else
                OnFinishUpdate();
        }
        private void Button2_Click(object sender, EventArgs e)
        {
            if (finish)
                OnFinishUpdate();
            else
                OnCancelUpdate();
        }
    }
```


================================================================

UpdaterUI用于制作更新的配置文件，通过该程序的配置文件，可以指定哪些文件或哪些目录下的文件（不包含子目录）使用MD5校验的方式更新，方便更新一些不包含文件版本的文件，比如文本类型的配置文件或图片。

比较特殊的是以“.exe.config”结尾的EXE配置文件，UI会自动记录主EXE程序的配置文件中<appSettings>节点下的所有配置项，而其他的以“.exe.config”结尾的EXE配置文件会被自动忽略。
  
配置文件中设置如下，需要的请自行添加，理论上可以有无限多个。但是使用MD5校验方式更新会影响更新的效率，建议不要太多。
```xml

<appSettings>
    <add key="MD5Hash_File_1" value="文件1"/>
    <add key="MD5Hash_File_2" value="文件2"/>
    <add key="MD5Hash_File_3" value="bin\文件3"/>
    <add key="MD5Hash_File_x" value="bin\data\文件x"/>
    <add key="MD5Hash_Directory_1" value="bin\data\"/>
    <add key="MD5Hash_Directory_2" value="bin\image\"/>
    <add key="MD5Hash_Directory_3" value="bin\gif\"/>
    <add key="MD5Hash_Directory_x" value="bin\xxxxx\"/>
</appSettings>
