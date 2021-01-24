using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.J1939.Messages
{
    public class MultiFrameMessage : J1939Message
    {
        public MultiFrameMessage(uint pgn, byte sourceAddress) : base(GeneratePduFromPgnAndSourceAddress(pgn, sourceAddress), new List<byte>()) { }

        public bool IsMessageComplete { get; set; }
        public uint NumberOfBytesToReceiveLeft { get; set; }

        public new void AddPacketizedData(List<byte> packetizedData)
        {
            var dataToAdd = packetizedData;

            if (NumberOfBytesToReceiveLeft < 7)
                dataToAdd.RemoveAll(byteToRemove => byteToRemove == 0xFF);

            Data.AddRange(dataToAdd);
            NumberOfBytesToReceiveLeft -= (uint)dataToAdd.Count;

            if (NumberOfBytesToReceiveLeft == 0)
                IsMessageComplete = true;
        }

        private static ProtocolDataUnit GeneratePduFromPgnAndSourceAddress(uint pgn, byte sourceAddress)
        {
            return new ProtocolDataUnit(pgn, sourceAddress);
        }

    }
}
