using IECA.J1939;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IECA
{
    public class J1939ToJsonConverter
    {
        public J1939ToJsonConverter(List<ConfigurationPgn> configuration)
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
                result += pgnFromConfig.FullName;
                var dataConvertedToBits = ConvertByteListToStringOfBits(j1939Message.Data);
                foreach (var spnFromConfig in pgnFromConfig.Spns)
                {
                    result += "\n" + spnFromConfig.FullName + ": ";

                    if (spnFromConfig.Multiplier == null && spnFromConfig.Offset == null)
                        result += GetNumberValueFromBitArray(dataConvertedToBits, spnFromConfig.DataStartIndex, spnFromConfig.BitLength);
                    else
                    {
                        var multiplier = spnFromConfig.Multiplier;
                        var offset = spnFromConfig.Offset;
                        var wantedValue = GetNumberValueFromBitArray(dataConvertedToBits, spnFromConfig.DataStartIndex, spnFromConfig.BitLength);
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

        #region Helper Methods

        string ConvertByteListToStringOfBits(List<byte> byteList)
        {
            string result = string.Empty;
            foreach (var singleByte in byteList)
            {
                result += Convert.ToString(singleByte, 2).PadLeft(8, '0');
            }
            return result;
        }

        uint GetNumberValueFromBitArray(string stringBitArray, uint startIndex, uint len)
        {
            // TODO: check bitArray.len >= (start + len)
            return Convert.ToUInt32(stringBitArray.Substring((int)startIndex,(int)len), 2);
        }

        #endregion

    }
}
