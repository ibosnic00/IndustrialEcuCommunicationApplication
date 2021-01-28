using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.J1939.Configuration
{
    public class ConfigurationSpn
    {
        public ConfigurationSpn(string fullName, uint dataStartIndex, uint bitLength, double? offset, double? multiplier)
        {
            DataStartIndex = dataStartIndex;
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
            BitLength = bitLength;
            Offset = offset;
            Multiplier = multiplier;
        }

        public string FullName { get; }
        public uint DataStartIndex { get; }
        public uint BitLength { get; }
        public double? Offset { get; }
        public double? Multiplier { get; }
    }
}
