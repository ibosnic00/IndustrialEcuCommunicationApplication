using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.J1939.Configuration
{
    public class StandardPgns
    {
        public const uint REQUEST_PGN = 59904;
        public const uint TP_CM_PGN = 60416;
        public const uint TP_DT_PGN = 60160;
        public const uint REQUEST_2_PGN = 51456;
        public const uint ACK_PGN = 59392;
        public const uint ADDR_CLAIMED_PGN = 60928;
        public const uint COMMANDED_ADDR_PGN = 60928;

    }
    public class StandardData
    {
        public const byte TP_CM_DATA_SIZE = 8;
        public const byte TP_DT_DATA_SIZE = 8;
        public const byte ADDR_CLAIMED_DATA_SIZE = 8;
        public const byte NULL_ADDRESS = 254;
        public const int GLOBAL_DESTINATION_ADDRESS = 255;
    }

}
