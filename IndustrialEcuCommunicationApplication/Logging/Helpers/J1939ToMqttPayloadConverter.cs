using IECA.J1939;
using IECA.J1939.Configuration;
using IECA.J1939.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IECA
{
    class J1939ToMqttPayloadConverter
    {
        public static string ConvertJ1939MessageToJsonFormat(J1939Message j1939Message)
        {
            string result = string.Empty;

            var dataConvertedToBits = Helpers.ConvertByteListToStringOfBits(j1939Message.Data);

            switch (j1939Message.PDU.ParameterGroupNumber)
            {
                case 61443:
                    result += $"\"load\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 16, bitLength: 8, offset: 0, multiplier: 1.0), dataConvertedToBits)},";
                    result += $"\"primaryThrottlePosition\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 8, bitLength: 8, offset: 0, multiplier: 0.4), dataConvertedToBits)},";
                    result += $"\"secondaryThrottlePosition\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 24, bitLength: 8, offset: 0, multiplier: 0.4), dataConvertedToBits)},";
                    break;
                case 61444:
                    result = $"\"engineSpeed\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 24, bitLength: 16, offset: 0, multiplier: 0.125), dataConvertedToBits)},";
                    break;
                case 64988:
                    var spn2615 = GetSpnValue(new ConfigurationSpn("", dataStartIndex: 0, bitLength: 4, offset: null, multiplier: null), dataConvertedToBits);
                    var spn2616 = GetSpnValue(new ConfigurationSpn("", dataStartIndex: 4, bitLength: 2, offset: null, multiplier: null), dataConvertedToBits);
                    var spn2617 = GetSpnValue(new ConfigurationSpn("", dataStartIndex: 6, bitLength: 2, offset: null, multiplier: null), dataConvertedToBits);
                    result += $"\"synchronizationStatus\" : {GetSpn2615Value(spn2615)},";
                    result += $"\"slowVesselMode\" : {GetSpn2616Value(spn2616)},";
                    result += $"\"trollingModeStatus\" : {GetSpn2617Value(spn2617)},";
                    break;
                case 65101:
                    result = $"\"averageFuelConsumption\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 16, bitLength: 16, offset: 0, multiplier: 0.001953125), dataConvertedToBits)},";
                    break;
                case 65132:
                    result = $"\"engineOverspeedVerify\" : null,";
                    break;
                case 65207:
                    result = $"\"maxEngineSpeed\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 0, bitLength: 16, offset: 0, multiplier: 0.125), dataConvertedToBits)},";
                    break;
                case 65244:
                    result += $"\"idleFuel\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 0, bitLength: 32, offset: 0, multiplier: 0.05), dataConvertedToBits)},";
                    result += $"\"idleHours\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 32, bitLength: 32, offset: 0, multiplier: 0.05), dataConvertedToBits)},";
                    break;
                case 65247:
                    result = $"\"desiredEngineSpeed\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 0, bitLength: 8, offset: -125, multiplier: 1.0), dataConvertedToBits)},";
                    break;
                case 65253:
                    result = $"\"engineHours\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 0, bitLength: 32, offset: 0, multiplier: 0.05), dataConvertedToBits)},";
                    break;
                case 65257:
                    result = $"\"fuelBurned\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 32, bitLength: 32, offset: 0, multiplier: 0.05), dataConvertedToBits)},";
                    break;
                case 65262:
                    result += $"\"coolantTemperature\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 0, bitLength: 8, offset: -40, multiplier: 1), dataConvertedToBits)},";
                    result += $"\"fuelTemperature\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 8, bitLength: 8, offset: -40, multiplier: 1), dataConvertedToBits)},";
                    result += $"\"oilTemperature\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 16, bitLength: 16, offset: -273, multiplier: 0.03125), dataConvertedToBits)},";
                    break;
                case 65263:
                    result += $"\"fuelPressure\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 0, bitLength: 8, offset: 0, multiplier: 4), dataConvertedToBits)},";
                    result += $"\"coolantLevel\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 48, bitLength: 8, offset: 0, multiplier: 2), dataConvertedToBits)},";
                    result += $"\"oilPressure\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 24, bitLength: 8, offset: 0, multiplier: 4), dataConvertedToBits)},";
                    break;
                case 65266:
                    result = $"\"fuelRate\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 0, bitLength: 16, offset: 0, multiplier: 0.05), dataConvertedToBits)},";
                    break;
                case 65269:
                    result = $"\"airInletTemperature\" : null,";
                    break;
                case 65178:
                    // currently inactive
                    result = $"\"turbochargerAirInletTemperature\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 0, bitLength: 16, offset: -273, multiplier: 0.3125), dataConvertedToBits)},";
                    break;
                case 65270:
                    result += $"\"airInletPressure\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 24, bitLength: 8, offset: 0, multiplier: 2), dataConvertedToBits)},";
                    result += $"\"boostPresure\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 8, bitLength: 8, offset: 0, multiplier: 2), dataConvertedToBits)},";
                    result += $"\"intakeManifold1Temperature\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 16, bitLength: 8, offset: -40, multiplier: 1), dataConvertedToBits)},";
                    break;
                case 65271:
                    result = $"\"batteryVoltage\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 48, bitLength: 16, offset: 0, multiplier: 0.05), dataConvertedToBits)},";
                    break;
                case 65272:
                    result += $"\"transmissionPressure\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 24, bitLength: 8, offset: 0, multiplier: 16), dataConvertedToBits)},";
                    result += $"\"transmissionTemperature\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 32, bitLength: 16, offset: -273, multiplier: 0.03125), dataConvertedToBits)},";
                    break;
                case 65276:
                    result += $"\"fuelFilterDifferentialPressure\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 16, bitLength: 8, offset: 0, multiplier: 2), dataConvertedToBits)},";
                    result += $"\"fuelLevel\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 8, bitLength: 8, offset: 0, multiplier: 0.4), dataConvertedToBits)},";
                    result += $"\"oilFilterDifferentialPressure\" : {GetSpnValue(new ConfigurationSpn("", dataStartIndex: 24, bitLength: 8, offset: 0, multiplier: 0.5), dataConvertedToBits)},";
                    break;
                default:
                    break;
            }

            return result;

        }

        private static string GetSpnValue(ConfigurationSpn spnFromConfig, string dataConvertedToBits)
        {
            if (dataConvertedToBits == string.Empty)
                return "-1";

            if (spnFromConfig.Multiplier == null && spnFromConfig.Offset == null)
                return Helpers.GetRawValueFromBitArray(dataConvertedToBits, spnFromConfig.DataStartIndex, spnFromConfig.BitLength);
            else
            {
                var multiplier = spnFromConfig.Multiplier;
                var offset = spnFromConfig.Offset;
                var wantedValue = Helpers.GetNumberValueFromBitArray(dataConvertedToBits, spnFromConfig.DataStartIndex, spnFromConfig.BitLength);
                if (wantedValue == null)
                    return "-2";
                else
                    return (wantedValue * multiplier + offset).ToString() ?? "-3";
            }
        }

        private static string GetSpn2615Value(string rawValue)
        {
            return rawValue switch
            {
                "0000" => "\"Not Synchronized\"",
                "0001" => "\"Synchronized Center\"",
                "0010" => "\"Synchronized Port\"",
                "0011" => "\"Synchronized Starboard\"",
                "0100" => "\"Synchronized Master\"",
                "1111" => "\"Take no action\"",
                _ => "\"Reserved\"",
            };
        }

        private static string GetSpn2616Value(string rawValue)
        {
            return rawValue switch
            {
                "00" => "\"Trolling mode OFF\"",
                "01" => "\"Trolling mode ACTIVE\"",
                "11" => "\"Take no action\"",
                _ => "\"Reserved\"",
            };
        }

        private static string GetSpn2617Value(string rawValue)
        {
            return rawValue switch
            {
                "00" => "\"Slow vessel mode OFF\"",
                "01" => "\"Slow vessel mode ACTIVE\"",
                "11" => "\"Take no action\"",
                _ => "\"Reserved\"",
            };
        }
    }
}
