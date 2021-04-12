using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.Logging
{
    public interface ILogger
    {
        void Initialize(string engineModel, string engineType, string engineDescription);

        void LogInfo(string lineToAdd);

        void AddJ1939MessageToLogHandler(J1939.J1939Message j1939Message);

        void StartLogCreationForRequestId(long requestId);
    }
}
