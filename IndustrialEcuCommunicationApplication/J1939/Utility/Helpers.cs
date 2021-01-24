using System;
using System.Collections.Generic;
using System.Text;

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

        public static uint? GetPgnFromList(List<byte>? inList)
        {
            if (inList == null || inList.Count != 3)
                return null;

            var highByte = inList[2];
            var midByte = inList[1];
            var lowByte = inList[0];
            return (uint)(highByte << 16 | midByte << 8 | lowByte);
        }
    }
}
