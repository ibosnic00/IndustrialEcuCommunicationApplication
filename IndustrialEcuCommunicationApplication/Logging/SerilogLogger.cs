using System;
using System.Collections.Generic;
using System.Text;
using Serilog;

namespace IECA.Logging
{
    public class SerilogLogger : ILogger
    {
        const int LOG_FILE_SIZE_LIMIT_MB = 5;

        public void Initialize()
        {
            var logFilename = "logs/" + DateTime.Now.ToString("HH-mm")+ "_ieca_.log";
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(logFilename, 
                              rollingInterval: RollingInterval.Day,
                              rollOnFileSizeLimit:true,
                              fileSizeLimitBytes: LOG_FILE_SIZE_LIMIT_MB * 1024 * 1024,
                              outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }

        public void LogInfo(string lineToAdd)
        {
            Log.Information(lineToAdd);
        }

        public void LogDebug(string lineToAdd)
        {
            Log.Debug(lineToAdd);
        }
    }
}
