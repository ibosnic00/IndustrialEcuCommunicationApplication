using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.J1939.Messages
{
    class TransferPgnMessage
    {
        const uint HIGHEST_POSSIBLE_PGN_VALUE = 0xFFFFFF;

        private const int TRANSF_PRIORITY = 6;
        private const int TRANSF_DATA_PAGE = 0;
        private const int TRANSF_FORMAT = 202;
    }
}
