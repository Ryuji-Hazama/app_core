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
        public Containers.ConsoleOutput ConsoleOutput { get; set; } = new Containers.ConsoleOutput();
        public List<Containers.FileOutput> FileOutputs { get; set; } = new List<Containers.FileOutput>();
        private readonly List<Containers.NameSpace> _nameSpaces = new List<Containers.NameSpace>();

        public void LoadLoggerConfig(string? logConfigPath = null)
        {
            App.Config config = App.ConfigManager.GetConfig();
            string log_config_json = logConfigPath ?? config.Logger.LogConfig ?? string.Empty;

            if (string.IsNullOrWhiteSpace(log_config_json) || !File.Exists(log_config_json))
                return;

            string json_content = File.ReadAllText(log_config_json);
            var logger_config = JsonSerializer.Deserialize<Containers.ConfigurationFile>(json_content);

            if (logger_config == null)
                return;

            foreach (var output in logger_config.Outputs)
            {
                OutputType? output_type = null;
                try
                {
                    if (output.Type == null)
                        throw new InvalidOperationException("Output type cannot be null.");

                    object output_type_type = output.Type.GetType();

                    if (output_type_type is Type type && type == typeof(string))
                        output_type = (OutputType)Enum.Parse(typeof(OutputType), output.Type.ToString() ?? "Unset", true);
                    else if (output_type_type is Type type2 && type2 == typeof(int))
                        output_type = (OutputType)(int)output.Type;
                    else
                        throw new InvalidOperationException($"Unsupported output type format: {output.Type}");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error processing output type: {output.Type}", ex);
                }

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

                    // Prepare a plastic cup
                    Containers.FileOutput fileOutput = new Containers.FileOutput
                    {
                        MinLogLevel = minLogLevel,
                        MaxLogLevel = maxLogLevel,
                        LogFileName = output.LogFileName,
                        LogFilePath = output.LogFilePath
                    };

                    try
                    {
                        object output_mode_type = output.Mode.GetType();

                        if (output_mode_type is Type mode_type && mode_type == typeof(string))
                            fileOutput.Mode = (LogFileMode)Enum.Parse(typeof(LogFileMode), output.Mode.ToString() ?? "Append", true);
                        else if (output_mode_type is Type mode_type2 && mode_type2 == typeof(int))
                            fileOutput.Mode = (LogFileMode)(int)output.Mode;
                        else
                            throw new InvalidOperationException($"Unsupported log file mode format: {output.Mode}");
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Error processing log file mode: {output.Mode}", ex);
                    }

                    FileOutputs.Add(fileOutput);    // Add the prepared file output to the list of file outputs
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported output type: {output.Type}");
                }
            }

            List<Containers.ConfigurationFile.NameSpace> nameSpaces = logger_config.NameSpaces;

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

            // I want to break these into separate methods...
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