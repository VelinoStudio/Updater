using System;


namespace VelinoStudio.Updater.UpdateInformation
{
    public class VersionInfo : IComparable<VersionInfo>
    {
        private string _Version = string.Empty;
        private string _UpdateDescribe = string.Empty;
        private int _Index = 0;
        private string _UpdateDate = string.Empty;
        public VersionInfo() { }
        public string Version { get { return _Version; } set { _Version = value; } }
        public string UpdateDescribe { get { return _UpdateDescribe; } set { _UpdateDescribe = value; } }
        public int Index { get { return _Index; } set { _Index = value; } }
        public string UpdateDate { get { return _UpdateDate; } set { _UpdateDate = value; } }
        public int CompareTo(VersionInfo other)
        {
            if (other == null) throw new ArgumentNullException("other");
            return this.Index.CompareTo(other.Index);
        }
    }
}
