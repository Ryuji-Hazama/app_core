using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace App.Core.LoggerFactory
{
    /// <summary>
    /// Represents the severity level of a log message.
    /// TRACE, DEBUG, INFO, WARN, ERROR, FATAL, *ALL*, *NONE*
    /// </summary>
    public enum LogLevel
    {
        NONE,
        TRACE,
        DEBUG,
        INFO,
        WARN,
        ERROR,
        FATAL,
        ALL
    }

    public partial class Logger : ILogger
    {
        private readonly string _source;
        public ILoggerConfig Config { get; set; } = new LoggerConfig();

        public Logger(string source)
        {
            _source = source;
        }

        /*
         * Private Methods
         */

        private void Log(LogLevel level, object message, int caller_depth = 2, int caller_line_number = 0)
        {
            StackFrame? caller_frame = new StackTrace().GetFrame(caller_depth);
            string logMessage = $"{DateTime.Now} [{level,-5}] {_source}.{caller_frame?.GetMethod()?.Name} - {message}";

            LogToConsole(level, caller_frame, logMessage);
            OutputToFile(level, caller_frame, caller_line_number, message);
        }

        private void LogError(LogLevel level, object message, Exception ex, int caller_line_number)
        {
            Log(level, message, 3, caller_line_number);
            Log(level, ex.ToString(), 3, caller_line_number);
        }

        private void LogToConsole(LogLevel level, StackFrame? caller_frame, string logMessage)
        {
            string target_namespace = $"{_source}.{caller_frame?.GetMethod()?.Name}";
            LogLevel console_min_log_level = Config.FindMinLogLevel(Config.ConsoleOutput.MinLogLevel, target_namespace);
            LogLevel console_max_log_level = Config.FindMaxLogLevel(Config.ConsoleOutput.MaxLogLevel, target_namespace);

            if (level >= console_min_log_level && level <= console_max_log_level)
            {
                Console.WriteLine(logMessage);
            }
        }

        /*
         * Public Methods
         */

        public void Trace(object message, [CallerLineNumber] int line = 0) => Log(LogLevel.TRACE, message, caller_line_number: line);
        public void Trace(object message, Exception ex, [CallerLineNumber] int line = 0) => LogError(LogLevel.TRACE, message, ex, line);
        public void Debug(object message, [CallerLineNumber] int line = 0) => Log(LogLevel.DEBUG, message, caller_line_number: line);
        public void Debug(object message, Exception ex, [CallerLineNumber] int line = 0) => LogError(LogLevel.DEBUG, message, ex, line);
        public void Info(object message, [CallerLineNumber] int line = 0) => Log(LogLevel.INFO, message, caller_line_number: line);
        public void Info(object message, Exception ex, [CallerLineNumber] int line = 0) => LogError(LogLevel.INFO, message, ex, line);
        public void Warn(object message, [CallerLineNumber] int line = 0) => Log(LogLevel.WARN, message, caller_line_number: line);
        public void Warn(object message, Exception ex, [CallerLineNumber] int line = 0) => LogError(LogLevel.WARN, message, ex, line);
        public void Error(object message, [CallerLineNumber] int line = 0) => Log(LogLevel.ERROR, message, caller_line_number: line);
        public void Error(object message, Exception ex, [CallerLineNumber] int line = 0) => LogError(LogLevel.ERROR, message, ex, line);
        public void Fatal(object message, [CallerLineNumber] int line = 0) => Log(LogLevel.FATAL, message, caller_line_number: line);
        public void Fatal(object message, Exception ex, [CallerLineNumber] int line = 0) => LogError(LogLevel.FATAL, message, ex, line);

        // Static methods for managing logger instances
        public static Dictionary<string, ILogger> Loggers { get; } = new Dictionary<string, ILogger>();
        public static ILogger GetLogger(string name)
        {
            if (!Loggers.ContainsKey(name))
            {
                Loggers[name] = new Logger(name);
            }
            return Loggers[name];
        }
    }

    /// <summary>
    /// Interface for a logger that supports different log levels and exception logging.
    /// </summary>
    public interface ILogger
    {
        void Trace(object message, [CallerLineNumber] int line = 0);
        void Trace(object message, Exception ex, [CallerLineNumber] int line = 0);
        void Debug(object message, [CallerLineNumber] int line = 0);
        void Debug(object message, Exception ex, [CallerLineNumber] int line = 0);
        void Info(object message, [CallerLineNumber] int line = 0);
        void Info(object message, Exception ex, [CallerLineNumber] int line = 0);
        void Warn(object message, [CallerLineNumber] int line = 0);
        void Warn(object message, Exception ex, [CallerLineNumber] int line = 0);
        void Error(object message, [CallerLineNumber] int line = 0);
        void Error(object message, Exception ex, [CallerLineNumber] int line = 0);
        void Fatal(object message, [CallerLineNumber] int line = 0);
        void Fatal(object message, Exception ex, [CallerLineNumber] int line = 0);
    }
}