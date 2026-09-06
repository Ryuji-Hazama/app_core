using System.Text.Json;

namespace App.Core.LoggerFactory
{
    public enum OutputType
    {
        Console,
        File,
        Unset
    }
    public enum LogFileMode
    {
        Append,
        Overwrite,
        Daily
    }

    public class LoggerConfig : ILoggerConfig
    {
        /*
         * Class members
         */

        public Containers.ConsoleOutput ConsoleOutput { get; set; } = new Containers.ConsoleOutput();
        public List<Containers.FileOutput> FileOutputs { get; set; } = new List<Containers.FileOutput>();
        private readonly List<Containers.NameSpace> _nameSpaces = new List<Containers.NameSpace>();

        /*
         * Private methods
         */
        private Containers.ConfigurationFile LoadConfigFromJson(string log_config_json)
        {
            if (string.IsNullOrWhiteSpace(log_config_json) || !File.Exists(log_config_json))
                return new Containers.ConfigurationFile(); // Return an empty configuration if the file path is invalid or the file does not exist

            string json_content = File.ReadAllText(log_config_json);
            var logger_config = JsonSerializer.Deserialize<Containers.ConfigurationFile>(json_content);

            if (logger_config == null)
                throw new InvalidOperationException($"Failed to deserialize logger configuration from file: {log_config_json}");

            return logger_config;
        }

        private void LoadOutputSettings(List<Containers.ConfigurationFile.Output> outputs)
        {
            foreach (var output in outputs)
            {
                OutputType output_type = ObjectToOutputType(output.Type);
                LogLevel minLogLevel = ObjectToLogLevel(output.MinLogLevel);
                LogLevel maxLogLevel = ObjectToLogLevel(output.MaxLogLevel);

                if (output_type == OutputType.Console)
                {
                    ConsoleOutput.MinLogLevel = minLogLevel;
                    ConsoleOutput.MaxLogLevel = maxLogLevel;
                }
                else if (output_type == OutputType.File)
                {
                    if (string.IsNullOrWhiteSpace(output.LogFileName) || string.IsNullOrWhiteSpace(output.LogFilePath))
                        throw new InvalidOperationException("Log file name and path must be specified for file outputs.");

                    if (!Directory.Exists(output.LogFilePath))
                        Directory.CreateDirectory(output.LogFilePath);

                    // Convert values to appropriate types
                    LogFileMode logFileMode = ObjectToLogFileMode(output.Mode);
                    long maxFileSize = ObjectToMaxFileSize(output.MaxFileSize);

                    FileOutputs.Add(new Containers.FileOutput
                    {
                        MinLogLevel = minLogLevel,
                        MaxLogLevel = maxLogLevel,
                        LogFileName = output.LogFileName,
                        LogFilePath = output.LogFilePath,
                        Mode = logFileMode,
                        MaxFileSize = maxFileSize
                    });
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported output type: {output.Type}");
                }
            }
        }

        private OutputType ObjectToOutputType(object output_type)
        {
            try
            {
                object output_type_type = output_type.GetType();

                if (output_type_type is Type t && t == typeof(OutputType))
                    return (OutputType)output_type;
                if (output_type_type is Type t2 && t2 == typeof(string))
                    return (OutputType)Enum.Parse(typeof(OutputType), (string)output_type);
                if (output_type_type is Type t3 && t3 == typeof(int))
                    return (OutputType)(int)output_type;
                else
                    throw new InvalidOperationException($"Unsupported output type format: {output_type}");
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to convert output type: {output_type}", ex);
            }
        }

        private LogFileMode ObjectToLogFileMode(object log_file_mode)
        {
            try
            {
                object log_file_mode_type = log_file_mode.GetType();

                if (log_file_mode_type is Type t && t == typeof(LogFileMode))
                    return (LogFileMode)log_file_mode;
                if (log_file_mode_type is Type t2 && t2 == typeof(string))
                    return (LogFileMode)Enum.Parse(typeof(LogFileMode), (string)log_file_mode);
                if (log_file_mode_type is Type t3 && t3 == typeof(int))
                    return (LogFileMode)(int)log_file_mode;
                else
                    throw new InvalidOperationException($"Unsupported log file mode format: {log_file_mode}");
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to convert log file mode: {log_file_mode}", ex);
            }
        }

        private int ObjectToMaxFileSize(object max_file_size)
        {
            try
            {
                object max_file_size_type = max_file_size.GetType();

                if (max_file_size_type is Type t && t == typeof(int))
                    return (int)max_file_size;
                else if (max_file_size_type is Type t2 && t2 == typeof(string))
                {
                    string size_str = max_file_size.ToString()?.ToUpper() ?? throw new InvalidOperationException("Max file size string is null or empty.");
                    if (size_str.EndsWith("KB") || size_str.EndsWith("K")){
                        double size_value = double.Parse(size_str.TrimEnd('K', 'B')) * 1024;
                        return (int)size_value;
                    }
                    else if (size_str.EndsWith("MB") || size_str.EndsWith("M"))
                    {
                        double size_value = double.Parse(size_str.TrimEnd('M', 'B')) * 1024 * 1024;
                        return (int)size_value;
                    }
                    else if (size_str.EndsWith("GB") || size_str.EndsWith("G"))
                    {
                        double size_value = double.Parse(size_str.TrimEnd('G', 'B')) * 1024 * 1024 * 1024;
                        return (int)size_value;
                    }
                    else
                    {
                        return int.Parse(size_str); // Assume it's in bytes if no unit is specified
                    }
                }
                else
                    throw new InvalidOperationException($"Unsupported max file size format: {max_file_size}");
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to convert max file size: {max_file_size}", ex);
            }
        }

        private void LoadNameSpaceSettings(List<Containers.ConfigurationFile.NameSpace> nameSpaces)
        {
            if (nameSpaces != null && nameSpaces.Count > 0)
            {
                foreach (var nameSpace in nameSpaces)
                {
                    Containers.NameSpace ns = new Containers.NameSpace
                    {
                        Name = nameSpace.Name,
                        MinLogLevel = ObjectToLogLevel(nameSpace.MinLogLevel),
                        MaxLogLevel = ObjectToLogLevel(nameSpace.MaxLogLevel)
                    };

                    _nameSpaces.Add(ns);
                }
            }
        }

        /*
         * Public methods
         */
        public void LoadLoggerConfig(string? logConfigPath = null)
        {
            App.Config config = App.ConfigManager.GetConfig();
            string log_config_json = logConfigPath ?? config.Logger.LogConfig ?? string.Empty;
            Containers.ConfigurationFile logger_config = LoadConfigFromJson(log_config_json);
            LoadOutputSettings(logger_config.Outputs);
            LoadNameSpaceSettings(logger_config.NameSpaces);
        }

        /// <summary>
        /// Finds the minimum log level for the specified output and target namespace, considering both the output's configured minimum log level and any namespace-specific overrides.
        /// </summary>
        /// <param name="output">The output configuration for which to determine the minimum log level.</param>
        /// <param name="target_namespace">The target namespace for which to determine the minimum log level.</param>
        /// <returns>The effective minimum log level for the specified output and target namespace.</returns>
        public LogLevel FindMinLogLevel(LogLevel output_log_level, string target_namespace, Containers.NameSpace? ns = null)
        {
            ns ??= FindNameSpace(target_namespace);
            if (ns != null)
                return ns.MinLogLevel > output_log_level ? ns.MinLogLevel : output_log_level;
            else return output_log_level;
        }

        /// <summary>
        /// Finds the maximum log level for the specified output log level and target namespace, considering both the output's configured maximum log level and any namespace-specific overrides.
        /// </summary>
        /// <param name="output_log_level">The output log level for which to determine the maximum log level.</param>
        /// <param name="target_namespace">The target namespace for which to determine the maximum log level.</param>
        /// <returns>The effective maximum log level for the specified output and target namespace.</returns>
        public LogLevel FindMaxLogLevel(LogLevel output_log_level, string target_namespace, Containers.NameSpace? ns = null)
        {
            ns ??= FindNameSpace(target_namespace);
            if (ns != null)
                return ns.MaxLogLevel < output_log_level ? ns.MaxLogLevel : output_log_level;
            else return output_log_level;
        }

        public Containers.NameSpace? FindNameSpace(string target_namespace)
        {
            foreach (var nameSpace in _nameSpaces)
                if (target_namespace.StartsWith(nameSpace.Name))
                    return nameSpace;

            return null;
        }

        public LogLevel ObjectToLogLevel(object log_level)
        {
            try
            {
                object log_level_type = log_level.GetType();

                if (log_level_type is Type t && t == typeof(LogLevel))
                    return (LogLevel)log_level;
                if (log_level_type is Type t2 && t2 == typeof(string))
                    return (LogLevel)Enum.Parse(typeof(LogLevel), (string)log_level);
                if (log_level_type is Type t3 && t3 == typeof(int))
                    return (LogLevel)(int)log_level;
                else
                    throw new InvalidOperationException($"Unsupported log level type: {log_level_type}");
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to convert log level: {log_level}", ex);
            }
        }
    }

    public interface ILoggerConfig
    {
        Containers.ConsoleOutput ConsoleOutput { get; set; }
        List<Containers.FileOutput> FileOutputs { get; set; }
        void LoadLoggerConfig(string? logger_config_path = null);
        LogLevel FindMinLogLevel(LogLevel output_log_level, string target_namespace, Containers.NameSpace? ns = null);
        LogLevel FindMaxLogLevel(LogLevel output_log_level, string target_namespace, Containers.NameSpace? ns = null);
        Containers.NameSpace? FindNameSpace(string target_namespace);
        LogLevel ObjectToLogLevel(object log_level);
    }
}