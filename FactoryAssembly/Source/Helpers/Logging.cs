using UnityEngine;

namespace FactoryAssembly
{
    public static class Logging
    {
        private const string LOG_FORMAT = "[Factory] {0}";

        public static void Log(string message)
        {
            Debug.LogFormat(LOG_FORMAT, message);
        }

        public static void Log(string format, params object[] args)
        {
            Debug.LogFormat(LOG_FORMAT, string.Format(format, args));
        }
    }
}
