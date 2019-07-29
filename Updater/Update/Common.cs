using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Reflection;


namespace VelinoStudio.Updater
{
    using UpdateInformation;
    internal class Common
    {
        internal static bool Debug { get; set; }
        internal static bool CheckFileIsUpdate(UpdateFileInfo updateFileInfo)
        {
            bool result = false;
            //Console.WriteLine($"文件名：{updateFileInfo.FileName}，路径：{updateFileInfo.FilePath}，版本：{updateFileInfo.FileVersion}，大小：{updateFileInfo.FileSize}，MD5：{updateFileInfo.MD5HashStr}，检测方式：{updateFileInfo.VerificationType}");
            string fileName = updateFileInfo.FilePath;
            if(fileName.StartsWith(@"\")) fileName= fileName.Trim('\\');
            string checkFile = Path.Combine(Environment.CurrentDirectory, fileName);
            if (File.Exists(checkFile))
            {
                if (updateFileInfo.VerificationType == VerificationType.Version)
                {
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(checkFile);
                    WriteLog_Information("检查文件更新，检测方式：{0}，文件：{1}，服务器版本：{2}，本地版本：{3}", updateFileInfo.VerificationType, checkFile, updateFileInfo.FileVersion, fileVersionInfo.FileVersion);
                    if (fileVersionInfo.FileVersion != updateFileInfo.FileVersion)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    string md5Hash = MD5Hash.GetMD5HashFromFile(checkFile);
                    WriteLog_Information("检查文件更新，检测方式：{0}，文件：{1}，服务器MD5Hash：{2}，本地MD5Hash：{3}", updateFileInfo.VerificationType, checkFile, updateFileInfo.MD5HashStr, md5Hash);
                    if (md5Hash != updateFileInfo.MD5HashStr)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            else
            {
                WriteLog_Information("检查文件更新，文件：{0} 本地文件不存在", checkFile);
                result = true;
            }
            if (result)
            {
                WriteLog_Information("\t\t，文件：{0} 信息与服务器不一致，需要更新", checkFile);
            }
            else
            {
                WriteLog_Information("\t\t，文件：{0} 信息与服务器一致，不需要更新", checkFile);
            }
            return result;
        }

        static void WriteLog(string format, params object[] args)
        {
            if (!Debug) return;
            string logDir = AppDomain.CurrentDomain.BaseDirectory + @"UpdateLog\";
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            FileStream fs = new FileStream(string.Format("{0}{1:yyyyMMdd}.log", logDir, DateTime.Now), FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\t{string.Format(format, args)}{Environment.NewLine}";
            byte[] Logbyte = Encoding.UTF8.GetBytes(logMessage);
            fs.Write(Logbyte, 0, Logbyte.Length);
            fs.Close();
        }
        internal static void WriteLog_Error(Exception ex)
        {
            WriteLog_Error(ex, null, null);
        }
        internal static void WriteLog_Error(Exception ex, string format)
        {
            WriteLog_Error(ex, format, null);
        }
        internal static void WriteLog_Error(Exception ex, string format, params object[] args)
        {
            StringBuilder exceptionMessage = new StringBuilder();
            string[] sms = ex.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            exceptionMessage.AppendLine(sms[0]);
            for (int i = 1; i < sms.Length; i++)
            {
                exceptionMessage.Append(' ', 24);
                exceptionMessage.AppendLine($"\t{sms[i]}");
            }
            if (!string.IsNullOrWhiteSpace(format) && args != null)
            {
                WriteLog($"ERRO\t{string.Format(format, args)}{Environment.NewLine}{exceptionMessage}");
            }
            else if (!string.IsNullOrWhiteSpace(format))
            {
                WriteLog($"ERRO\t{format}{Environment.NewLine}{exceptionMessage}");
            }
            else
            {
                WriteLog($"ERRO\t{exceptionMessage}");
            }
        }
        internal static void WriteLog_Warning(string format)
        {
            WriteLog_Warning(format, null);
        }
        internal static void WriteLog_Warning(string format, params object[] args)
        {
            if (!string.IsNullOrWhiteSpace(format) && args != null)
            {
                WriteLog($"WARN\t{string.Format(format, args)}");
            }
            else if (!string.IsNullOrWhiteSpace(format))
            {
                WriteLog($"WARN\t{format}");
            }
            else
            {
                return;
            }
            
        }
        internal static void WriteLog_Information(string format)
        {
            WriteLog_Information(format, null);
        }
        internal static void WriteLog_Information(string format, params object[] args)
        {
            if (!string.IsNullOrWhiteSpace(format) && args != null)
            {
                WriteLog($"INFO\t{string.Format(format, args)}");
            }
            else if (!string.IsNullOrWhiteSpace(format))
            {
                WriteLog($"INFO\t{format}");
            }
            else
            {
                return;
            }
        }
        internal static T Exception<T>(params string[] args) where T : Exception
        {
            T ex = Activator.CreateInstance(typeof(T), args) as T;
            WriteLog_Error(ex);
            return ex;
        }
    }
}
