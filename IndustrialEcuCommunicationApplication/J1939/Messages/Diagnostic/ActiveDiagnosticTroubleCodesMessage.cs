using IECA.J1939.Configuration;
using IECA.J1939.Utility;
using System.Collections.Generic;
using System.Text;

namespace IECA.J1939.Messages.Diagnostic
{
    public class ActiveDiagnosticTroubleCodesMessage : J1939Message
    {
        const byte DTC_RECORD_SIZE_BYTES = 6;

        public ActiveDiagnosticTroubleCodesMessage(ProtocolDataUnit receivedPDU, List<byte> data)
            : base(receivedPDU, data)
        {
            DtcRecords = ExtractDtcRecordFromRawBytes(data);
        }

        public List<DtcRecord> DtcRecords { get; }

        List<DtcRecord> ExtractDtcRecordFromRawBytes(List<byte> rawData)
        {
            var result = new List<DtcRecord>();

            var rawDtcRecords = rawData.ChunkBy(DTC_RECORD_SIZE_BYTES);

            foreach (var rawDtcRecord in rawDtcRecords)
                if (rawDtcRecord.Count == DTC_RECORD_SIZE_BYTES)
                    result.Add(DtcRecord.FromRawBytes(rawDtcRecord));

            return result;
        }
    }
}
