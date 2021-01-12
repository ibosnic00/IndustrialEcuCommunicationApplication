using NUnit.Framework;
using IECA;

namespace j1939.Test.Unit
{
    public class HelpersTests
    {
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
            var messageToSend = new CanMessage(id: 0x7EC, dlc: 5, data: new byte[] { 0xFF, 0x3, 0xAB, 0x7, 0xA }, CanMessageType.Data);
            var expected = "7EC#FF03AB070A";
            var result = Helpers.CanMessageToCandumpString(messageToSend);

            Assert.AreEqual(expected, result);
        }
    }
}