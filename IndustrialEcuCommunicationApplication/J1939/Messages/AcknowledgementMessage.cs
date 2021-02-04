using IECA.J1939.Utility;
using System.Collections.Generic;
using System.Linq;

namespace IECA.J1939.Messages
{
    public class AcknowledgementMessage : J1939Message
    {
        const uint DEFAULT_PGN = 0;
        const int PGN_START_INDEX = 5;

        public AcknowledgementMessage(ProtocolDataUnit receivedPdu, List<byte> data) : base(receivedPdu, data)
        {
            Response = GetAcknowledgmentResponseFromData(data);
            PgnAcknowledged = GetAcknowledgedPgnFromData(data);
        }

        public uint PgnAcknowledged { get; }

        public AcknowledgmentResponse Response { get; }

        private AcknowledgmentResponse GetAcknowledgmentResponseFromData(List<byte> data)
        {
            return (AcknowledgmentResponse)data[0];
        }

        private uint GetAcknowledgedPgnFromData(List<byte> data)
        {
            return Helpers.GetPgnFromList(data.Skip(PGN_START_INDEX).ToList()) ?? DEFAULT_PGN;
        }
    }

    public enum AcknowledgmentResponse
    {
        PositiveAck = 0,
        NegativeAck,
        AccessDenied,
        CannotRespond
    }
}
