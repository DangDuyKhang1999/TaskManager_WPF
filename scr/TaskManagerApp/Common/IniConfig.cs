using System.IO;
using System.Reflection;

namespace TaskManagerApp.Common
{
    /// <summary>
    /// Provides access to configuration values from the TaskManager.ini file.
    /// </summary>
    public static class IniConfig
    {
        // Full path to the INI configuration file.
        private static readonly string _iniPath =
            Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory,
                "TaskManager.ini");

        /// <summary>
        /// Indicates whether the INI file exists.
        /// </summary>
        public static bool Exists => File.Exists(_iniPath);

        /// <summary>
        /// Gets the application mode from the INI file.
        /// </summary>
        public static string? Mode => GetValue("AppSettings", "Mode");

        /// <summary>
        /// Determines whether the current user is an admin based on the INI file.
        /// </summary>
        public static bool IsAdmin => GetValue("AppSettings", "IsAdmin")?.ToLower() == "true";

        /// <summary>
        /// Reads a value from a specified section and key in the INI file.
        /// </summary>
        /// <param name="section">The section name in the INI file.</param>
        /// <param name="key">The key within the section.</param>
        /// <returns>The corresponding value, or null if not found.</returns>
        private static string? GetValue(string section, string key)
        {
            if (!File.Exists(_iniPath))
                return null;

            string[] lines = File.ReadAllLines(_iniPath);
            bool inSection = false;

            foreach (string? line in lines)
            {
                var trimmed = line.Trim();

                // Check for section header
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    inSection = trimmed == $"[{section}]";

                // Return the value if the key is found within the section
                else if (inSection && trimmed.StartsWith($"{key}="))
                    return trimmed.Substring(key.Length + 1).Trim();
            }

            return null;
        }
    }
}
