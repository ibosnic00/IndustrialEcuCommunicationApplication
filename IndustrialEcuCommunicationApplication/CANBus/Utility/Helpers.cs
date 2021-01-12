using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace IECA
{
    public static class Helpers
    {
        public static CanMessage CandumpStringToCanMessage(string candumpLine)
        {
            Regex canDumpRegex = new Regex(@"can(?'channel'\d)\W+(?'id'\d|\w*)\W+(?'len'\d)\W+(?'data'.*)");
            var matchInLine = canDumpRegex.Match(candumpLine);

            if (matchInLine.Success)
            {
                var groupsFoundInLine = matchInLine.Groups;
                var parsedId = groupsFoundInLine["id"].Value;
                var parsedLen = groupsFoundInLine["len"].Value;
                var parsedData = groupsFoundInLine["data"].Value;

                return new CanMessage(Convert.ToUInt32(parsedId, 16),
                    Convert.ToByte(parsedLen, 16),
                    GetDataBytesFromStringValue(parsedData, Convert.ToByte(parsedLen, 16)),
                    CanMessageType.Data);

            }
            else
            {
                throw new Exception("Received invalid CANDUMP message");
            }
        }

        public static string? CanMessageToCandumpString(CanMessage canMessage)
        {
            if (canMessage.Data == null)
                return null;

            return canMessage.ID.ToString("X2") + "#" + BitConverter.ToString(canMessage.Data).Replace("-", "");
        }

        #region Private Methods

        private static byte[] GetDataBytesFromStringValue(string? value, int len)
        {
            var dataBytesList = new List<byte>();

            if (value == null)
                throw new Exception("Invalid Candump data.");

            var valueSplitted = value.Split(' ');
            foreach (var hexString in valueSplitted)
                dataBytesList.Add(Convert.ToByte(hexString, 16));

            dataBytesList.RemoveRange(len, dataBytesList.Count - len);

            if (dataBytesList.Count <= 8)
                return dataBytesList.ToArray();
            else
                throw new Exception("Candump data larger than expected");

        }
        #endregion
    }
}
