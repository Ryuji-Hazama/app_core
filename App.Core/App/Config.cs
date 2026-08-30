using System.Text.Json;

/*
 * App core configuration classes
 */

namespace App.Core.App
{
    public class AppConfig
    {
        public string AssemblyPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "AppData", "bin");
        public string ComponentsList { get; set; } = Path.Combine(AppContext.BaseDirectory, "AppData", "config", "components.json");
    }

    public class LoggerConfig
    {
        public string? LogConfig { get; set; }
    }

    public class Config
    {
        public AppConfig App { get; set; } = new AppConfig();
        public LoggerConfig Logger { get; set; } = new LoggerConfig();
    }

    public static class ConfigManager
    {
        private static Config? _config = null;

        private static string ResolvePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            return Path.IsPathRooted(path)
                ? path
                : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));
        }

        /// <summary>
        /// Get the configuration from the App.json file. If the file does not exist, return a new Config object with default values.
        /// </summary>
        /// <returns>The configuration object.</returns>
        public static Config GetConfig()
        {
            if (_config == null)
            {
                var configFilePath = Path.Combine(AppContext.BaseDirectory, "App.json");
                if (File.Exists(configFilePath))
                {
                    var json = File.ReadAllText(configFilePath);
                    _config = JsonSerializer.Deserialize<Config>(json);

                    if (_config != null)
                    {
                        _config.App.AssemblyPath = ResolvePath(_config.App.AssemblyPath);
                        _config.App.ComponentsList = ResolvePath(_config.App.ComponentsList);
                        _config.Logger.LogConfig = ResolvePath(_config.Logger.LogConfig);
                    }
                }
                else
                {
                    _config = new Config();
                }
            }
            return _config ?? new Config();
        }
    }
}