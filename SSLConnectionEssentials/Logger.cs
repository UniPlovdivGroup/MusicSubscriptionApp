using System;
using System.IO;
using System.Text;

namespace LoggerDebug
{
    public static class Logger
    {
        private static string errorDir { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Path.DirectorySeparatorChar + "SSLProjectErrors";
        private static string errorDirAndFilePath { get; set; }
        private static string errorFileName { get; set; }
        public static bool ForceSaveToTxtFile { get; set; }

        public static bool Suppress { get; set; }

        public static void SetErrorLogFileName(string fileName)
        {
            errorFileName = fileName;
            errorDirAndFilePath= errorDir + Path.DirectorySeparatorChar + errorFileName;
        }

        public static void Log(Exception e, bool saveToTxtFile)
        {
            if (Suppress) return;
            string LogFromException = CreateLog(e);

            if (ForceSaveToTxtFile)
            {
                LogToTxtFile(LogFromException);
                return;
            }

            if (saveToTxtFile)
            {
                LogToTxtFile(LogFromException);
            }
            else
            {
                LogOnDisplay(LogFromException);
            }
        } 
        public static void Log(string Message, bool saveToTxtFile)
        {
            if (Suppress) return;
            string LogFromMessage = CreateLog(Message);

            if (ForceSaveToTxtFile)
            {
                LogToTxtFile(LogFromMessage);
                return;
            }

            if (saveToTxtFile)
            {
                LogToTxtFile(LogFromMessage);
            }
            else
            {
                LogOnDisplay(LogFromMessage);
            }
        }
        public static void ClearDisplay()
        {
            if(!Suppress)  Console.Clear();
        }

        private static void LogToTxtFile(string CreatedLog)
        {
            if (errorFileName == null)
            {
                Console.WriteLine("Please call the SetErrorLogFileName function to set filename of the log file");
                return;
            }
            if (!(Directory.Exists(errorDir)))
            {
                Directory.CreateDirectory(errorDir);
            }
            if (!(File.Exists(errorDirAndFilePath)))
            {
                File.Create(errorDirAndFilePath);
            }

            using (StreamWriter errorWriter = new StreamWriter(errorDirAndFilePath, true))
            {
                errorWriter.WriteLine(CreatedLog);
            }
        }
        private static void LogOnDisplay(string CreatedLog)
        {
            Console.WriteLine(CreatedLog);
        }

        private static string CreateLog(Exception e)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("");
            builder.AppendLine("=============================New_Log==============================");
            builder.AppendLine($"             Log Date : {DateTime.Now}");
            builder.AppendLine("==============================START===============================");
            builder.AppendLine($"Error : {e.Message}");
            builder.AppendLine($"Stack Trace : {e.StackTrace}");
            builder.AppendLine("===============================END================================");
            builder.AppendLine("");
            return builder.ToString();
        }
        private static string CreateLog(string messageToLog)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("");
            builder.AppendLine("=============================New_Log==============================");
            builder.AppendLine($"                Log Date : {DateTime.Now}");
            builder.AppendLine("==============================START===============================");
            builder.AppendLine($"Logged string: {messageToLog}");
            builder.AppendLine("===============================END================================");
            builder.AppendLine("");
            return builder.ToString();
        }
    }

}
