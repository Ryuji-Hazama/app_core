using System.Reflection;

namespace App.Core.LoggerFactory
{
    public partial class Logger : ILoggerFileManager
    {
        private void OutputToFile(LogLevel level, MethodBase? caller, object message)
        {
            if (Config.FileOutputs == null || Config.FileOutputs.Count == 0)
                return;

            string caller_method = caller?.Name ?? string.Empty;
            string target_namespace = $"{_source}.{caller_method}";
            Containers.NameSpace? ns = Config.FindNameSpace(target_namespace);

            foreach(var fileOutput in Config.FileOutputs)
            {
                LogLevel file_min_log_level = Config.FindMinLogLevel(fileOutput.MinLogLevel, target_namespace, ns);
                LogLevel file_max_log_level = Config.FindMaxLogLevel(fileOutput.MaxLogLevel, target_namespace, ns);

                if (level >= file_min_log_level && level <= file_max_log_level)
                {
                    // Implement the logic to write the log message to the file specified by fileOutput
                }
            }
        }
    }

    public interface ILoggerFileManager
    {
    }
}