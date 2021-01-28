using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.J1939.Messages.TransportProtocol
{
    public class DataTransferMessage : J1939Message
    {
        private const byte TP_DT_PRIORITY = 7;
        private const byte TP_DT_DATA_PAGE = 0;
        private const byte TP_DT_FORMAT = 235;

        public DataTransferMessage(List<byte> data, byte destinationAddress, byte sourceAddress) :
            base(new ProtocolDataUnit(TP_DT_PRIORITY, TP_DT_DATA_PAGE, TP_DT_FORMAT, destinationAddress, sourceAddress), data)
        {
            if (data.Count != 8)
                throw new Exception("Invalid Brodcast Announce Message");

            SequenceNumber = data[0];
            PacketizedData = data.GetRange(1, 7);
        }

        public byte SequenceNumber { get; }
        public List<byte> PacketizedData { get; }
    }
}
