using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.J1939.Messages
{
    public class MultiFrameMessage : J1939Message
    {
        public MultiFrameMessage(uint pgn, byte sourceAddress, uint numberOfBytesToRcv)
            : base(GeneratePduFromPgnAndSourceAddress(pgn, sourceAddress), new List<byte>())
        {
            NumberOfBytesToReceiveLeft = numberOfBytesToRcv;
        }

        public bool IsMessageComplete { get; set; }
        public uint NumberOfBytesToReceiveLeft { get; set; }

        public new void AddPacketizedData(List<byte> packetizedData)
        {
            var dataToAdd = packetizedData;

            if (NumberOfBytesToReceiveLeft < 7)
                dataToAdd = RemoveExcessDataFromList(dataToAdd, (byte)NumberOfBytesToReceiveLeft);

            Data.AddRange(dataToAdd);
            NumberOfBytesToReceiveLeft -= (uint)dataToAdd.Count;

            if (NumberOfBytesToReceiveLeft == 0)
                IsMessageComplete = true;
        }

        private List<byte> RemoveExcessDataFromList(List<byte> dataList, byte expectedNumberOfBytes)
        {
            var result = new List<byte>();
            for (byte i = 0; i < expectedNumberOfBytes; i++)
                result.Add(dataList[i]);

            return result;
        }

        private static ProtocolDataUnit GeneratePduFromPgnAndSourceAddress(uint pgn, byte sourceAddress)
        {
            return new ProtocolDataUnit(pgn, sourceAddress);
        }
    }
}
