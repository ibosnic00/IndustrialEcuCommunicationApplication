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
    }
}
