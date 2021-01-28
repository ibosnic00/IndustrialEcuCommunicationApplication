﻿using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.J1939.Messages
{
    public class AcknowledgementMessage
    {
        const uint HIGHEST_POSSIBLE_PGN_VALUE = 0xFFFFFF;

        private const int ACK_PRIORITY = 6;
        private const int ACK_DATA_PAGE = 0;
        private const int ACK_FORMAT = 232;

    }

    enum AcknowledgmentResponse
    {
        PositiveAck = 0,
        NegativeAck,
        AccessDenied,
        CannotRespond
    }
}