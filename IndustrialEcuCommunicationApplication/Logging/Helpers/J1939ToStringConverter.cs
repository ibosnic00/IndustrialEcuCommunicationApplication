using IECA.J1939.Configuration;
using IECA.J1939;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IECA.J1939.Utility;

namespace IECA
{
    public class J1939ToStringConverter
    {
        public J1939ToStringConverter(List<ConfigurationPgn> configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        List<ConfigurationPgn> configuration { get; }

        public string ConvertJ1939MessageToHumanReadableFormat(J1939Message j1939Message)
        {
            string result = string.Empty;

            var pgnFromConfig = configuration.SingleOrDefault(configPgn => configPgn.ParameterGroupNumber == j1939Message.PDU.ParameterGroupNumber);
            if (pgnFromConfig == null)
                return $"Undefined PGN {j1939Message.PDU.ParameterGroupNumber}. Check Your configuration.";
            try
            {
                result += "[" + j1939Message.PDU.SourceAddress + "] " + pgnFromConfig.FullName;
                var dataConvertedToBits = Helpers.ConvertByteListToStringOfBits(j1939Message.Data);
                foreach (var spnFromConfig in pgnFromConfig.Spns)
                {
                    result += " " + spnFromConfig.FullName + ": ";

                    if (spnFromConfig.Multiplier == null && spnFromConfig.Offset == null)
                        result += Helpers.GetRawValueFromBitArray(dataConvertedToBits, spnFromConfig.DataStartIndex, spnFromConfig.BitLength);
                    else
                    {
                        var multiplier = spnFromConfig.Multiplier;
                        var offset = spnFromConfig.Offset;
                        var wantedValue = Helpers.GetNumberValueFromBitArray(dataConvertedToBits, spnFromConfig.DataStartIndex, spnFromConfig.BitLength);
                        if (wantedValue == null)
                            result = "Inv.cfg";
                        else
                            result += (wantedValue * multiplier + offset);
                    }

                }
            }
            catch (Exception ex)
            {
                result = $"Problem while parsing PGN {pgnFromConfig.FullName}. Error: {ex.Message}";
            }

            return result;
        }
    }
}
