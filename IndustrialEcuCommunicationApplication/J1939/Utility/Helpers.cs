using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IECA.CANBus;

namespace IECA.J1939.Utility
{
    public class Helpers
    {
        public static uint? GetPgnFromRequestMessageData(byte[] requestMessageData)
        {
            if (requestMessageData.Length == 3)
                return (uint)(requestMessageData[0] * 256 * 256 + requestMessageData[1] * 256 + requestMessageData[2]);

            return null;
        }

        public static byte[] GetMessageDataFromPgn(uint pgn)
        {
            var msb = Convert.ToByte((pgn & 0xFF0000) >> 16);
            var mb = Convert.ToByte((pgn & 0xFF00) >> 8);
            var lsb = Convert.ToByte(pgn & 0xFF);

            return new byte[] { msb, mb, lsb };
        }

        public static List<byte> ConvertPgnToList(uint pgn)
        {
            var highByte = (byte)((pgn & 0xFF0000) >> 16);
            var midByte = (byte)((pgn & 0xFF00) >> 8);
            var lowByte = (byte)(pgn & 0xFF);
            return new List<byte>() { lowByte, midByte, highByte };
        }

        public static List<byte> ConvertEcuNameToList(ulong ecuName)
        {
            var result = new List<byte>();
            var ecuNameParsed = ecuName;
            for (int i = 0; i < 8; i++)
            {
                result.Add((byte)(ecuNameParsed & 0xFF));
                ecuNameParsed = ecuNameParsed >> 8;
            }

            return result;
        }

        public static ulong? GetRawEcuName(List<byte> ecuNameData)
        {
            if (ecuNameData == null || ecuNameData.Count != 8)
                return null;

            ulong result = 0;

            for (int i = 7; i >= 0; i--)
                result = result << 8 | ecuNameData[i];

            return result;
        }

        public static uint? GetPgnFromList(List<byte>? inList)
        {
            if (inList == null || inList.Count != 3)
                return null;

            var highByte = inList[2];
            var midByte = inList[1];
            var lowByte = inList[0];
            return (uint)(highByte << 16 | midByte << 8 | lowByte);
        }

        public static CanMessage ConvertSingleFrameJ1939MsgToCanMsg(J1939Message j1939Message)
        {
            if (j1939Message.Data.Count > 8)
                throw new Exception("J1939 Message is not single frame.");

            return new CanMessage(id: j1939Message.PDU.ToCanExtIdentifierFormat(),
                                  idLen: 7,
                                  dlc: (byte)j1939Message.Data.Count,
                                  data: j1939Message.Data.ToArray(),
                                  CanMessageType.Data);
        }

        public static string ConvertByteListToStringOfBits(List<byte> byteList)
        {
            string result = string.Empty;
            foreach (var singleByte in byteList)
            {
                result += Convert.ToString(singleByte, 2).PadLeft(8, '0');
            }
            return result;
        }

        public static uint? GetNumberValueFromBitArray(string stringBitArray, uint startIndex, uint len, bool reverseEndian = true)
        {
            if (stringBitArray.Length < startIndex + len)
                return null;
            if (len > 8 && reverseEndian)
            {
                var reversedEndianStringitArray = string.Empty;
                uint numberOfBytes = len / 8;
                uint leftover = len % 8;

                if (leftover != 0)
                    reversedEndianStringitArray = stringBitArray.Substring((int)(startIndex + numberOfBytes * 8), (int)leftover);
                for (int i = 1; i <= numberOfBytes; i++)
                    reversedEndianStringitArray += stringBitArray.Substring((int)(startIndex + len - i * 8 + leftover), (int)8);

                return Convert.ToUInt32(reversedEndianStringitArray, 2);
            }
            if (len > 8 && !reverseEndian)
            {
                var reversedEndianStringitArray = string.Empty;
                uint numberOfBytes = len / 8;
                uint leftover = len % 8;

                for (int i = 0; i < numberOfBytes; i++)
                    reversedEndianStringitArray += stringBitArray.Substring((int)(startIndex + i * 8), (int)8);
                if (leftover != 0)
                    reversedEndianStringitArray += stringBitArray.Substring((int)(startIndex + numberOfBytes * 8), (int)leftover);

                return Convert.ToUInt32(reversedEndianStringitArray, 2);
            }
            else
                return Convert.ToUInt32(stringBitArray.Substring((int)startIndex, (int)len), 2);
        }

        public static string GetRawValueFromBitArray(string stringBitArray, uint startIndex, uint len)
        {
            if (stringBitArray.Length < startIndex + len)
                return "Inv.cfg";

            return stringBitArray.Substring((int)startIndex, (int)len);
        }
    }

    public static class ListExtensions
    {
        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}
