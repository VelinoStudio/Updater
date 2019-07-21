using System;
using System.Diagnostics;
using System.IO;


namespace VelinoStudio.Updater
{
    using UpdateInformation;
    internal class Common
    {
        internal static bool CheckFileIsUpdate(UpdateFileInfo updateFileInfo)
        {
            bool result = false;
            //Console.WriteLine($"文件名：{updateFileInfo.FileName}，路径：{updateFileInfo.FilePath}，版本：{updateFileInfo.FileVersion}，大小：{updateFileInfo.FileSize}，MD5：{updateFileInfo.MD5HashStr}，检测方式：{updateFileInfo.VerificationType}");
            string checkFile = Path.Combine(Environment.CurrentDirectory, updateFileInfo.FilePath);
            if (File.Exists(checkFile))
            {
                if (updateFileInfo.VerificationType == VerificationType.Version)
                {
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(checkFile);
                    if (fileVersionInfo.FileVersion != updateFileInfo.FileVersion)
                    {
                        //Console.WriteLine($"本地文件版本：{fileVersionInfo.FileVersion}");
                        result = true;
                    }
                }
                else
                {
                    string md5Hash = MD5Hash.GetMD5HashFromFile(checkFile);
                    if (md5Hash != updateFileInfo.MD5HashStr)
                    {
                        //Console.WriteLine($"本地文件MD5：{md5Hash}");
                        result = true;
                    }
                }
            }
            else
            {
                result = true;
            }
            return result;
        }
    }
}
