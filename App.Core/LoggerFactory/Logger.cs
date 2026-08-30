using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

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
            Config.LoadLoggerConfig();
        }

        /*
         * Private Methods
         */

        private void Log(LogLevel level, object message, int caller_depth = 2)
        {
            MethodBase? caller = new StackTrace().GetFrame(caller_depth)?.GetMethod();
            string logMessage = $"{DateTime.Now} [{level,-5}] {_source}.{caller} - {message}";

            LogToConsole(level, caller, logMessage);
            OutputToFile(level, caller, message);
        }

        private void LogError(LogLevel level, object message, Exception ex)
        {
            Log(level, message, 3);
            Log(level, ex.ToString(), 3);
        }

        private void LogToConsole(LogLevel level, MethodBase? caller, string logMessage)
        {
            string target_namespace = $"{_source}.{caller?.Name}";
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

        public void Trace(object message) => Log(LogLevel.TRACE, message);
        public void Trace(object message, Exception ex) => LogError(LogLevel.TRACE, message, ex);
        public void Debug(object message) => Log(LogLevel.DEBUG, message);
        public void Debug(object message, Exception ex) => LogError(LogLevel.DEBUG, message, ex);
        public void Info(object message) => Log(LogLevel.INFO, message);
        public void Info(object message, Exception ex) => LogError(LogLevel.INFO, message, ex);
        public void Warn(object message) => Log(LogLevel.WARN, message);
        public void Warn(object message, Exception ex) => LogError(LogLevel.WARN, message, ex);
        public void Error(object message) => Log(LogLevel.ERROR, message);
        public void Error(object message, Exception ex) => LogError(LogLevel.ERROR, message, ex);
        public void Fatal(object message) => Log(LogLevel.FATAL, message);
        public void Fatal(object message, Exception ex) => LogError(LogLevel.FATAL, message, ex);

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
        void Trace(object message);
        void Trace(object message, Exception ex);
        void Debug(object message);
        void Debug(object message, Exception ex);
        void Info(object message);
        void Info(object message, Exception ex);
        void Warn(object message);
        void Warn(object message, Exception ex);
        void Error(object message);
        void Error(object message, Exception ex);
        void Fatal(object message);
        void Fatal(object message, Exception ex);
    }
}