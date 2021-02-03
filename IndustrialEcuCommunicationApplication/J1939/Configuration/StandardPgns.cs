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

        // Diagnostic PGN's
        public const uint DM1_PGN = 65226;
        public const uint DM2_PGN = 65227;
        public const uint DM3_PGN = 65228;
        public const uint DM4_PGN = 65229;
        public const uint DM5_PGN = 65230;
        public const uint DM6_PGN = 65231;
        public const uint DM7_PGN = 58112;
        public const uint DM8_PGN = 65232;
        public const uint DM9_PGN = 65233;
        public const uint DM10_PGN = 65234;
        public const uint DM11_PGN = 65235;
        public const uint DM12_PGN = 65236;

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
