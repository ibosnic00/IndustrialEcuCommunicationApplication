using IECA.J1939.Configuration;
using IECA.J1939.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.J1939.Messages.TransportProtocol
{
    public class ConnectionManagementMessage : J1939Message
    {
        private const byte TP_CM_PRIORITY = 7;
        private const byte TP_CM_DATA_PAGE = 0;
        private const byte TP_CM_FORMAT = 236;

        public ConnectionManagementMessage(byte controlByte, List<byte> data, byte destinationAddress, byte sourceAddress)
            : base(new ProtocolDataUnit(TP_CM_PRIORITY, TP_CM_DATA_PAGE, TP_CM_FORMAT, destinationAddress, sourceAddress), data)
        {
            ControlByte = controlByte;
        }

        public byte ControlByte { get; set; }
    }

    public class BrodcastAnnounceMessage : ConnectionManagementMessage
    {
        public BrodcastAnnounceMessage(List<byte> data, byte sourceAddress)
            : base((byte)ConnectionManagementMessageControlBytes.BAM, data, StandardData.GLOBAL_DESTINATION_ADDRESS, sourceAddress)
        {
            if (data.Count != 8)
                throw new Exception("Invalid Brodcast Announce Message");

            TotalMessageSize = (UInt16)(data[2] << 8 | data[1]);
            TotalNumberOfPackets = data[3];
            Pgn = Helpers.GetPgnFromList(data.GetRange(5, 3)) ?? throw new Exception("Invalid PGN in BAM Data");
        }

        public UInt16 TotalMessageSize { get; }
        public byte TotalNumberOfPackets { get; }
        public uint Pgn { get; }
    }

    public class ConnectionAbort : ConnectionManagementMessage
    {
        public ConnectionAbort(List<byte> data, byte destinationAddress, byte sourceAddress)
            : base((byte)ConnectionManagementMessageControlBytes.CA, data, destinationAddress, sourceAddress)
        {
            if (data.Count != 8)
                throw new Exception("Invalid Connection Abort Message");

            ConnectionAbortReason = data[1];
            Pgn = Helpers.GetPgnFromList(data.GetRange(5, 3)) ?? throw new Exception("Invalid PGN in CA Data");
        }

        public byte ConnectionAbortReason { get; }
        public uint Pgn { get; }
    }

    public class EndOfMessageAcknowledgment : ConnectionManagementMessage
    {
        public EndOfMessageAcknowledgment(List<byte> data, byte destinationAddress, byte sourceAddress)
            : base((byte)ConnectionManagementMessageControlBytes.EOMA, data, destinationAddress, sourceAddress)
        {
            if (data.Count != 8)
                throw new Exception("Invalid End Of Message Acknowledgment");

            TotalMessageSize = (UInt16)(data[1] << 8 | data[2]);
            TotalNumberOfPackets = data[3];
            Pgn = Helpers.GetPgnFromList(data.GetRange(5, 3)) ?? throw new Exception("Invalid PGN in EoMA Data");
        }

        public UInt16 TotalMessageSize { get; }
        public byte TotalNumberOfPackets { get; }
        public uint Pgn { get; }
    }

    public class ClearToSend : ConnectionManagementMessage
    {
        public ClearToSend(List<byte> data, byte destinationAddress, byte sourceAddress)
            : base((byte)ConnectionManagementMessageControlBytes.CTS, data, destinationAddress, sourceAddress)
        {
            if (data.Count != 8)
                throw new Exception("Invalid Clear To Send");

            NumberOfPacketsThatCanBeSent = data[1];
            NextPacketNumberToBeSent = data[2];
            Pgn = Helpers.GetPgnFromList(data.GetRange(5, 3)) ?? throw new Exception("Invalid PGN in CTS Data");
        }

        public byte NumberOfPacketsThatCanBeSent { get; }
        public byte NextPacketNumberToBeSent { get; }
        public uint Pgn { get; }
    }

    public class RequestToSend : ConnectionManagementMessage
    {
        public RequestToSend(List<byte> data, byte destinationAddress, byte sourceAddress)
            : base((byte)ConnectionManagementMessageControlBytes.RTS, data, destinationAddress, sourceAddress)
        {
            if (data.Count != 8)
                throw new Exception("Invalid Request To Send");

            TotalMessageSize = (UInt16)(data[1] << 8 | data[2]);
            TotalNumberOfPackets = data[3];
            NumberOfPacketsThatCanBeSent = data[4];
            Pgn = Helpers.GetPgnFromList(data.GetRange(5, 3)) ?? throw new Exception("Invalid PGN in RTS Data");
        }

        public UInt16 TotalMessageSize { get; }
        public byte TotalNumberOfPackets { get; }
        public byte NumberOfPacketsThatCanBeSent { get; }
        public uint Pgn { get; }
    }

    public enum ConnectionManagementMessageControlBytes : byte
    {
        RTS = 16,
        CTS = 17,
        EOMA = 19,
        BAM = 32,
        CA = 255
    }
}
