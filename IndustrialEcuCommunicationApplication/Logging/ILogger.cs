using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.Logging
{
    public interface ILogger
    {
        void Initialize();

        void LogInfo(string lineToAdd);

        void LogDebug(string lineToAdd);
    }
}
