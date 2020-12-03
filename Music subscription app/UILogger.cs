using System;
using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Music_subscription_app
{
    public static class UILogger
    {
        private static string errorDir { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Path.DirectorySeparatorChar + "SSLProjectErrors";
        private static string errorDirAndFilePath { get; set; } = errorDir + Path.DirectorySeparatorChar + errorFileName;
        private static string errorFileName { get; set; }
        private static bool ForceSaveToTxtFile { get; set; }
        public static TextBox ConsoleBox { get; set; }

        public static void SetErrorLogFileName(string fileName)
        {
            errorFileName = fileName;
        }

        public static void Log(Exception e, bool saveToTxtFile)
        {
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
            ConsoleBox.Text = "";
        }

        private static void LogToTxtFile(string CreatedLog)
        {
            if (errorFileName == null)
            {
                ConsoleBox.Text += "Please call the SetErrorLogFileName function to set filename of the log file" + Environment.NewLine;
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
            Dispatcher.UIThread.InvokeAsync(new Action(() => { ConsoleBox.Text += CreatedLog + Environment.NewLine; }));
            //ConsoleBox.Text += CreatedLog + Environment.NewLine;
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
            builder.Append($"[{DateTime.Now}]: {messageToLog}");
            return builder.ToString();
        }
    }
}
