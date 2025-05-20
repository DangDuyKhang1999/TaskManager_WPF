using System.IO;
using System.Reflection;

namespace TaskManager.Common
{
    public static class IniConfig
    {
        private static readonly string _iniPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TaskManager.ini");

        public static bool Exists => File.Exists(_iniPath);

        public static string Mode => GetValue("AppSettings", "Mode");
        public static bool IsAdmin => GetValue("AppSettings", "IsAdmin")?.ToLower() == "true";

        private static string GetValue(string section, string key)
        {
            if (!File.Exists(_iniPath))
                return null;

            string[] lines = File.ReadAllLines(_iniPath);
            bool inSection = false;

            foreach (string line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    inSection = trimmed == $"[{section}]";

                else if (inSection && trimmed.StartsWith($"{key}="))
                    return trimmed.Substring(key.Length + 1).Trim();
            }

            return null;
        }
    }
}
