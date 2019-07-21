namespace VelinoStudio.Updater.UpdateInformation
{
    public class UpdateFileInfo
    {
        private string _FileName = string.Empty;
        private string _FilePath = string.Empty;
        private string _FileVersion = string.Empty;
        private long _FileSize = 0;
        private string _MD5Hash = string.Empty;
        private VerificationType _VerificationType = VerificationType.Version;
        public UpdateFileInfo() { }
        public string FileName { get { return _FileName; } set { _FileName = value; } }
        public string FilePath { get { return _FilePath; } set { _FilePath = value; } }
        public long FileSize { get { return _FileSize; } set { _FileSize = value; } }
        public string FileVersion { get { return _FileVersion; } set { _FileVersion = value; } }
        public string MD5HashStr { get { return _MD5Hash; }set { _MD5Hash = value; } }
        public VerificationType VerificationType { get { return _VerificationType; } set { _VerificationType = value; } }
        public override string ToString()
        {
            return _FileName;
        }
    }
}
