namespace App.Core.LoggerFactory.Containers
{
    public class ConfigurationFile
    {
        public List<Output> Outputs { get; set; } = new List<Output>();

        public List<NameSpace> NameSpaces { get; set; } = new List<NameSpace>();

        public class Output
        {
            public object Type { get; set; } = "Unset";
            public string LogFileName { get; set; } = string.Empty;
            public string LogFilePath { get; set; } = string.Empty;
            public object Mode { get; set; } = LogFileMode.Append.ToString();
            public object MinLogLevel { get; set; } = LogLevel.TRACE.ToString();
            public object MaxLogLevel { get; set; } = LogLevel.FATAL.ToString();

            /*
             * public string Format { get; set; } = string.Empty;
             * // ${pid} -> Process ID
             * // ${timestamp} -> Current timestamp
             * // ${level} -> Log level
             * // ${class_path} -> Full class path including namespace
             * // ${caller} -> The method or function that called the logger.
             * // ${message} -> The log message content
             * public string TimestampFormat { get; set; } = string.Empty;
             * // Use standard .NET date and time format strings for the timestamp format.
             */
        }

        public class NameSpace
        {
            public string Name { get; set; } = string.Empty;
            public object MinLogLevel { get; set; } = LogLevel.TRACE.ToString();
            public object MaxLogLevel { get; set; } = LogLevel.FATAL.ToString();
        }
    }

    public class ConsoleOutput
    {
        public LogLevel MinLogLevel { get; set; } = LogLevel.TRACE;
        public LogLevel MaxLogLevel { get; set; } = LogLevel.FATAL;
    }

    public class FileOutput
    {
        public string LogFileName { get; set; } = string.Empty;
        public string LogFilePath { get; set; } = string.Empty;
        public LogFileMode Mode { get; set; } = LogFileMode.Append;
        public LogLevel MinLogLevel { get; set; } = LogLevel.TRACE;
        public LogLevel MaxLogLevel { get; set; } = LogLevel.FATAL;
    }

    public class NameSpace
    {
        public string Name { get; set; } = string.Empty;
        public LogLevel MinLogLevel { get; set; } = LogLevel.TRACE;
        public LogLevel MaxLogLevel { get; set; } = LogLevel.FATAL;
    }
}