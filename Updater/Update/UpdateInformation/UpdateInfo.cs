namespace VelinoStudio.Updater.UpdateInformation
{
    public class UpdateInfo
    {
        private UpdateFileInfo[] _FileInfos = null;
        private VersionInfo[] _VersionInfos = null;
        public UpdateFileInfo[] FileInfos { get { return _FileInfos; } set { _FileInfos = value; } }
        public VersionInfo[] VersionInfos { get { return _VersionInfos; } set { _VersionInfos = value; } }
        public UpdateInfo() { }
    }
}
