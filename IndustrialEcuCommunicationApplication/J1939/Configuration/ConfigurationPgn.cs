using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.J1939.Configuration
{
    public class ConfigurationPgn
    {
        public ConfigurationPgn(string fullName, uint parameterGroupNumber, List<ConfigurationSpn> spns)
        {
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
            ParameterGroupNumber = parameterGroupNumber;
            Spns = spns ?? throw new ArgumentNullException(nameof(spns));
        }

        public string FullName { get; }
        public uint ParameterGroupNumber { get; }
        public List<ConfigurationSpn> Spns { get; }
    }
}
