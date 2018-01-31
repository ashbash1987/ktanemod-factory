using UnityEngine;

namespace FactoryAssembly
{
    internal static class Logging
    {
        private const string LOG_FORMAT = "[Factory] {0}";

        internal static void Log(string message)
        {
            Debug.LogFormat(LOG_FORMAT, message);
        }

        internal static void Log(string format, params object[] args)
        {
            Debug.LogFormat(LOG_FORMAT, string.Format(format, args));
        }
    }
}
