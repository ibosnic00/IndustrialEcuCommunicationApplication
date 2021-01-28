using IECA.J1939.Configuration;
using IECA.J1939;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                result += pgnFromConfig.FullName;
                var dataConvertedToBits = ConvertByteListToStringOfBits(j1939Message.Data);
                foreach (var spnFromConfig in pgnFromConfig.Spns)
                {
                    result += "\n" + spnFromConfig.FullName + ": ";

                    if (spnFromConfig.Multiplier == null && spnFromConfig.Offset == null)
                        result += GetRawValueFromBitArray(dataConvertedToBits, spnFromConfig.DataStartIndex, spnFromConfig.BitLength);
                    else
                    {
                        var multiplier = spnFromConfig.Multiplier;
                        var offset = spnFromConfig.Offset;
                        var wantedValue = GetNumberValueFromBitArray(dataConvertedToBits, spnFromConfig.DataStartIndex, spnFromConfig.BitLength);
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

        #region Helper Methods

        static string ConvertByteListToStringOfBits(List<byte> byteList)
        {
            string result = string.Empty;
            foreach (var singleByte in byteList)
            {
                result += Convert.ToString(singleByte, 2).PadLeft(8, '0');
            }
            return result;
        }

        static uint? GetNumberValueFromBitArray(string stringBitArray, uint startIndex, uint len)
        {
            if (stringBitArray.Length < startIndex + len)
                return null;
            if (len > 8)
            {
                var reversedEndianStringitArray = string.Empty;
                uint numberOfBytes = len / 8;
                uint leftover = len % 8;

                if (leftover != 0)
                    reversedEndianStringitArray = stringBitArray.Substring((int)(startIndex + numberOfBytes * 8), (int)leftover);
                for (int i = 1; i <= numberOfBytes; i++)
                    reversedEndianStringitArray += stringBitArray.Substring((int)(startIndex + len - i * 8 + leftover), (int)8);

                return Convert.ToUInt32(reversedEndianStringitArray.Substring((int)startIndex, (int)len), 2); ;
            }
            else
                return Convert.ToUInt32(stringBitArray.Substring((int)startIndex, (int)len), 2);
        }

        static string GetRawValueFromBitArray(string stringBitArray, uint startIndex, uint len)
        {
            if (stringBitArray.Length < startIndex + len)
                return "Inv.cfg";

            return stringBitArray.Substring((int)startIndex, (int)len);
        }

        #endregion

    }
}
