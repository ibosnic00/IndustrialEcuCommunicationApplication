using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.J1939.Configuration
{
    public class ConfigurationRequestPgn
    {
        public ConfigurationRequestPgn(uint parameterGroupNumber, int requestRateMilliseconds)
        {
            ParameterGroupNumber = parameterGroupNumber;
            RequestRateMilliseconds = requestRateMilliseconds;
        }

        public uint ParameterGroupNumber { get; }
        public int RequestRateMilliseconds { get; }
    }
}
