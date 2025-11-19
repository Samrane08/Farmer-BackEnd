using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper
{
    public static class ExceptionLogging
    {


        public static void LogException(string ex)
        {
            string logFolder = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            string logFilePath = Path.Combine(logFolder, $"Log_{DateTime.Now:yyyy-MM-dd}.txt");
            File.AppendAllText(logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - :\n{Convert.ToString(ex)}\n");
        }

    }
}
