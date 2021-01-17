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
                result += pgnFromConfig.FullName + " ";
                foreach (var spnFromConfig in pgnFromConfig.Spns)
                {
                    result += spnFromConfig.FullName + " ";

                    if (spnFromConfig.Multiplier == null && spnFromConfig.Offset == null)
                        result += GetStringValueFromByteList(j1939Message.Data, spnFromConfig.DataStartIndex);
                    else
                    {
                        var multiplier = spnFromConfig.Multiplier;
                        var offset = spnFromConfig.Offset;
                        var wantedValue = GetNumberValueFromBitArray(new BitArray(j1939Message.Data.ToArray()), spnFromConfig.DataStartIndex, spnFromConfig.BitLength);
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

        uint GetNumberValueFromBitArray(BitArray bitArray, uint startIndex, uint len)
        {
            // TODO: check bitArray.len >= (start + len)
            string result = string.Empty;
            for (int i = 0; i < len; i++)
            {
                result += Convert.ToString(bitArray[(int)(startIndex + i)]); // +len-i in case of endian changes
            }
            return Convert.ToUInt32(result, 2);
        }

        string GetStringValueFromByteList(List<byte> byteList, uint startIndex)
        {
            byte currentChar = 0;
            var stringInAscii = new List<byte>();
            byte count = 0;
            while ((currentChar = byteList[(int)startIndex / 8]) != 42 && count < 200)
            {
                count++;
                stringInAscii.Add(currentChar);
            }

            return System.Text.Encoding.ASCII.GetString(stringInAscii.ToArray());
        }

        #endregion

    }
}
