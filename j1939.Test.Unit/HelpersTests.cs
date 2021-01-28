using NUnit.Framework;
using IECA;
using IECA.CANBus.Utility;
using IECA.CANBus;

namespace j1939.Test.Unit
{
    public class HelpersTests
    {
        [Test]
        public void CandumpStringToCanMessage_ExtId_SuccessfullParsing()
        {
            string candumpString = "can0  0000CF00   [8]  01 55 02 88 12 AA 0A EE";
            var expectedData = new byte[] { 0x01, 0x55, 0x02, 0x88, 0x12, 0xAA, 0x0A, 0xEE };
            var result = Helpers.CandumpStringToCanMessage(candumpString);

            Assert.AreEqual(0xCF00, result.ID);
            Assert.AreEqual(8, result.DLC);
            Assert.AreEqual(result.Data, expectedData);
        }

        [Test]
        public void CandumpStringToCanMessage_DataLen8_SuccessfullParsing()
        {
            string candumpString = "can1  7DF   [8]  02 01 0C 00 00 00 00 00";
            var expectedData = new byte[] { 0x02, 0x01, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00 };
            var result = Helpers.CandumpStringToCanMessage(candumpString);

            Assert.AreEqual(0x7DF, result.ID);
            Assert.AreEqual(8, result.DLC);
            Assert.AreEqual(result.Data, expectedData);
        }

        [Test]
        public void CandumpStringToCanMessage_DataLen4_SuccessfullParsing()
        {
            string candumpString = "can0 2 [4] 11 22 33 44";
            var expectedData = new byte[] { 0x11, 0x22, 0x33, 0x44 };
            var result = Helpers.CandumpStringToCanMessage(candumpString);

            Assert.AreEqual(0x2, result.ID);
            Assert.AreEqual(4, result.DLC);
            Assert.AreEqual(expectedData, result.Data);
        }

        [Test]
        public void CandumpStringToCanMessage_InvalidChannel_RaiseException()
        {
            string candumpString = "vcan 2 [4] 11 22 33 44";
            var expectedData = new byte[] { 0x11, 0x22, 0x33, 0x44 };
            Assert.Throws<System.Exception>(() => Helpers.CandumpStringToCanMessage(candumpString));
        }

        [Test]
        public void CandumpStringToCanMessage_DataLen4ButDataIs8_Parsed4Bytes()
        {
            string candumpString = "can2 2 [4] 11 22 33 44 55 66 77 88";
            var expectedData = new byte[] { 0x11, 0x22, 0x33, 0x44 };
            var result = Helpers.CandumpStringToCanMessage(candumpString);

            Assert.AreEqual(0x2, result.ID);
            Assert.AreEqual(4, result.DLC);
            Assert.AreEqual(expectedData, result.Data);
        }
        [Test]
        public void CandumpStringToCanMessage_DataLen1_SuccessfullParsing()
        {
            string candumpString = "can2 002 [1] 11 22 33 44 55 66 77 88";
            string candumpString2 = "can2 002 [1] 11";
            var expectedData = new byte[] { 0x11 };
            var result = Helpers.CandumpStringToCanMessage(candumpString);
            var result2 = Helpers.CandumpStringToCanMessage(candumpString2);

            Assert.AreEqual(0x2, result.ID);
            Assert.AreEqual(expectedData, result.Data);
            Assert.AreEqual(expectedData, result2.Data);
        }

        [Test]
        public void CanMessageToCandumpString_ValidMessage_SuccessfullConversion()
        {
            var messageToSend = new CanMessage(id: 0x7EC, idLen: 3, dlc: 5, data: new byte[] { 0xFF, 0x3, 0xAB, 0x7, 0xA }, CanMessageType.Data);
            var expected = "7EC#FF03AB070A";
            var result = Helpers.CanMessageToCandumpString(messageToSend);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetRawEcuName_FromCooperhiltechName_SuccessfullConversion()
        {
            var expected = 0x80FEFF00FFFFFFFF;
            var received = IECA.J1939.Utility.Helpers.GetRawEcuName(new System.Collections.Generic.List<byte>() { 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFE, 0x80 });

            Assert.AreEqual(expected, received);
        }

        [Test]
        public void GetRawEcuName_ToCooperhiltechName_SuccessfullConversion()
        {
            var expected = new System.Collections.Generic.List<byte>() { 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFE, 0x80 };
            var received = IECA.J1939.Utility.Helpers.ConvertEcuNameToList(0x80FEFF00FFFFFFFF);

            for(int i=0; i< expected.Count;i++)
                Assert.AreEqual(expected[i], received[i]);
        }
    }
}