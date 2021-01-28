using System;
using System.Collections.Generic;
using System.Text;
using IECA.J1939.Configuration;
using IECA.J1939.Messages;

namespace IECA.J1939.Procedures
{
    public static class ConnectionProcedures
    {
        public static J1939Message? SendRequestForAddressClaimMessage()
        {
            return new RequestPgnMessage(StandardPgns.ADDR_CLAIMED_PGN, sourceAddress: StandardData.NULL_ADDRESS);
        }
    }
}
