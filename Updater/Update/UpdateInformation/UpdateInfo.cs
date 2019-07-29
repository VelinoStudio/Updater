using System.Collections.Generic;

namespace VelinoStudio.Updater.UpdateInformation
{
    public class UpdateInfo
    {
        private UpdateFileInfo[] _FileInfos = null;
        private VersionInfo[] _VersionInfos = null;
        private Dictionary<string, string> _Configuration = null;
        public UpdateFileInfo[] FileInfos { get { return _FileInfos; } set { _FileInfos = value; } }
        public VersionInfo[] VersionInfos { get { return _VersionInfos; } set { _VersionInfos = value; } }
        public Dictionary<string,string> Configurations { get { return _Configuration; } set { _Configuration = value; } }
        public UpdateInfo() { }
    }
}
