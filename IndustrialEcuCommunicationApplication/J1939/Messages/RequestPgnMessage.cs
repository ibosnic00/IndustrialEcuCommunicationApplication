using System;
using System.Collections.Generic;
using System.Text;
using IECA.J1939.Utility;

namespace IECA.J1939.Messages
{
    public class RequestPgnMessage
    {
        const uint HIGHEST_POSSIBLE_PGN_VALUE = 0xFFFFFF;

        private const int REQ_1_PRIORITY = 6;
        private const int REQ_1_DATA_PAGE = 0;
        private const int REQ_1_FORMAT = 234;
        private const int REQ_1_SOURCE_ADDRESS = 254;

        private const int REQ_2_PRIORITY = 6;
        private const int REQ_2_DATA_PAGE = 0;
        private const int REQ_2_FORMAT = 201;
        private const int REQ_2_SOURCE_ADDRESS = 254;

        private const int GLOBAL_DESTINATION_ADDRESS = 255;

        public static J1939Message? RequestPgn(uint requestedPgn, byte destinationId = GLOBAL_DESTINATION_ADDRESS)
        {
            if (requestedPgn > HIGHEST_POSSIBLE_PGN_VALUE)
                return null;

            return new J1939Message(new ProtocolDataUnit(priority: REQ_1_PRIORITY,
                                                         dataPage: REQ_1_DATA_PAGE,
                                                         format: REQ_1_FORMAT,
                                                         specific: destinationId,
                                                         sourceAddress: REQ_1_SOURCE_ADDRESS),
                                                         Helpers.ConvertPgnToList(requestedPgn));
        }

        public static J1939Message? Request2Pgn(uint requestedPgn, bool useTransferPgn, byte destinationId = GLOBAL_DESTINATION_ADDRESS)
        {
            if (requestedPgn > HIGHEST_POSSIBLE_PGN_VALUE)
                return null;

            byte transPgnParam = (byte)(Convert.ToByte(useTransferPgn) << 6);
            List<byte> dataToSend = Helpers.ConvertPgnToList(requestedPgn);
            dataToSend.Add(transPgnParam);

            return new J1939Message(new ProtocolDataUnit(priority: REQ_2_PRIORITY,
                                                         dataPage: REQ_2_DATA_PAGE,
                                                         format: REQ_2_FORMAT,
                                                         specific: destinationId,
                                                         sourceAddress: REQ_2_SOURCE_ADDRESS),
                                                         dataToSend);
        }
    }
}
