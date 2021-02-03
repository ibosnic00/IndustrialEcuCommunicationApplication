using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.J1939
{
    public class DiagnosticTroubleCode
    {
        public uint SuspectParameterNumber { get; }
        public byte FailureModeIdentifier { get; }
        public byte OccurenceCount { get; }
    }
}
