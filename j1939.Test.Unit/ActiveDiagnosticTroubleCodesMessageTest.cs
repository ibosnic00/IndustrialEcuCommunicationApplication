using NUnit.Framework;
using IECA.J1939;
using IECA.J1939.Messages.Diagnostic;
using System.Collections.Generic;

namespace j1939.Test.Unit
{
    class ActiveDiagnosticTroubleCodesMessageTest
    {
        [Test]
        public void Constructor_SingleDtcRecord_SuccessfulConversion()
        {
            var pdu = new ProtocolDataUnit(6, 0, 254, 202, 0x21);
            var data = new List<byte>() { 0x01, 0xFF, 0xA3, 0x55, 0xE0, 0x1, 0xFF, 0xFF};
            var msg = new ActiveDiagnosticTroubleCodesMessage(pdu, data);

            Assert.IsTrue(msg.DtcRecords.Count == 1);
        }

        [Test]
        public void Constructor_MultipleDtcRecords_SuccessfulConversion()
        {
            var pdu = new ProtocolDataUnit(6, 0, 254, 202, 0x21);
            var data = new List<byte>() { 0x01, 0xFF, 0xA3, 0x55, 0xE0, 0x1, 
                                          0x01, 0xFF, 0xA3, 0x55, 0xE0, 0x1,
                                          0x01, 0xFF, 0xA3, 0x55, 0xE0, 0x1,
                                          0x01, 0xFF, 0xA3, 0x55, 0xE0, 0x1,
                                          0x01, 0xFF, 0xA3, 0x55, 0xE0, 0x1,
                                          0x01, 0xFF, 0xA3, 0x55, 0xE0, 0x1,
                                          0xFF, 0xFF, 0xFF, 0xFF };
            var msg = new ActiveDiagnosticTroubleCodesMessage(pdu, data);

            Assert.IsTrue(msg.DtcRecords.Count == 6);
        }
    }
}
