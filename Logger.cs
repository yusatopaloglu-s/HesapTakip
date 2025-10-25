using System.Diagnostics;

namespace HesapTakip
{
    public static class Logger
    {
        [Conditional("DEBUG")]
        public static void Log(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
