using System;
using System.Collections.Generic;
using IECA.J1939.Configuration;
using IECA.J1939.Utility;

namespace IECA.J1939.Messages
{
    public class RequestPgn2Message : J1939Message
    {
        private const int REQ_2_PRIORITY = 6;
        private const int REQ_2_DATA_PAGE = 0;
        private const int REQ_2_FORMAT = 201;

        public RequestPgn2Message(uint requestedPgn, bool useTransferPgn, byte sourceAddress, byte destinationId = StandardData.GLOBAL_DESTINATION_ADDRESS)
            : base(new ProtocolDataUnit(priority: REQ_2_PRIORITY,
                                                         dataPage: REQ_2_DATA_PAGE,
                                                         format: REQ_2_FORMAT,
                                                         specific: destinationId,
                                                         sourceAddress: sourceAddress),
                                                         AppendUseTransferPgnFlagAndPackToData(useTransferPgn, requestedPgn))
        {
            RequestedPgn = requestedPgn;
        }

        public uint RequestedPgn { get; }

        private static List<byte> AppendUseTransferPgnFlagAndPackToData(bool useTransferPgn, uint requestedPgn)
        {
            byte transPgnParam = (byte)(Convert.ToByte(useTransferPgn) << 6);
            List<byte> dataToSend = Helpers.ConvertPgnToList(requestedPgn);
            dataToSend.Add(transPgnParam);
            return dataToSend;
        }
    }
}
