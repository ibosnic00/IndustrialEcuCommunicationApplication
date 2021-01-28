using System;
using System.Collections.Generic;
using System.Text;
using IECA.J1939.Configuration;
using IECA.J1939.Utility;

namespace IECA.J1939.Messages
{
    public class RequestPgnMessage : J1939Message
    {
        private const int REQ_1_PRIORITY = 6;
        private const int REQ_1_DATA_PAGE = 0;
        private const int REQ_1_FORMAT = 234;

        public RequestPgnMessage(uint requestedPgn, byte sourceAddress, byte destinationId = StandardData.GLOBAL_DESTINATION_ADDRESS)
            : base(new ProtocolDataUnit(priority: REQ_1_PRIORITY,
                                                         dataPage: REQ_1_DATA_PAGE,
                                                         format: REQ_1_FORMAT,
                                                         specific: destinationId,
                                                         sourceAddress: sourceAddress),
                                                         Helpers.ConvertPgnToList(requestedPgn))
        {
            RequestedPgn = requestedPgn;
        }

        public uint RequestedPgn { get; }
    }
}
