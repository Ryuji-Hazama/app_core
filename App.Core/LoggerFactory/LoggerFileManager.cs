using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace App.Core.LoggerFactory
{
    public partial class Logger : ILoggerFileManager
    {
        private void OutputToFile(LogLevel level, StackFrame? caller_frame, object message)
        {
            if (Config.FileOutputs == null || Config.FileOutputs.Count == 0)
                return;

            MethodBase? caller_method = caller_frame?.GetMethod();
            string target_namespace = $"{_source}.{caller_method}";
            Containers.NameSpace? ns = Config.FindNameSpace(target_namespace);

            foreach (var fileOutput in Config.FileOutputs)
            {
                LogLevel file_min_log_level = Config.FindMinLogLevel(fileOutput.MinLogLevel, target_namespace, ns);
                LogLevel file_max_log_level = Config.FindMaxLogLevel(fileOutput.MaxLogLevel, target_namespace, ns);

                if (level >= file_min_log_level && level <= file_max_log_level)
                {
                    string log_file_path = GetLogFilePath(fileOutput);
                    string log_message = $"{DateTime.Now} [{level,-5}] {_source}.{caller_method?.Name}({caller_frame?.GetFileLineNumber()}) - {message}";
                    LogToFile(log_file_path, log_message);
                    RotateLogFile(log_file_path, fileOutput.MaxFileSize, fileOutput.Mode);
                }
            }
        }

        private string GetLogFilePath(Containers.FileOutput file_output)
        {
            LogFileMode logFileMode = file_output.Mode;

            if (logFileMode == LogFileMode.Daily)
            {
                string dateSuffix = DateTime.Now.ToString("yyyy-MM-dd");
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file_output.LogFileName);
                string fileExtension = Path.GetExtension(file_output.LogFileName);
                return Path.Combine(file_output.LogFilePath, $"{fileNameWithoutExtension}_{dateSuffix}{fileExtension}");
            }
            else
            {
                return Path.Combine(file_output.LogFilePath, file_output.LogFileName);
            }
        }

        private void LogToFile(string log_file_path, string message)
        {
            try
            {
                // Append the log message to the specified log file
                using (StreamWriter writer = new StreamWriter(new FileStream(log_file_path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
                {
                    writer.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during file writing
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        /// <summary>
        /// Rotates the log file if it exceeds the specified maximum file size.
        /// </summary>
        /// <param name="log_file_path">Log file full path.</param>
        /// <param name="max_file_size">Maximum file size in bytes.</param>
        /// <param name="log_file_mode">Log file mode.</param>
        private void RotateLogFile(string log_file_path, long max_file_size, LogFileMode log_file_mode)
        {
            long file_size = new FileInfo(log_file_path).Length;
            if (file_size > max_file_size)
            {
                if (log_file_mode == LogFileMode.Overwrite)
                {
                    string backup_file_path = log_file_path + ".bak";
                    if (File.Exists(backup_file_path))
                        File.Delete(backup_file_path);
                    File.Move(log_file_path, backup_file_path);
                }
                else
                {
                    string directory = Path.GetDirectoryName(log_file_path) ?? throw new InvalidOperationException("Failed to get log file directory.");
                    string file_name_without_extension = Path.GetFileNameWithoutExtension(log_file_path);
                    string file_extension = Path.GetExtension(log_file_path);
                    string archived_file_path = Path.Combine(directory, $"{file_name_without_extension}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}{file_extension}");
                    File.Move(log_file_path, archived_file_path);
                }
            }
        }
    }

    public interface ILoggerFileManager
    {
    }
}