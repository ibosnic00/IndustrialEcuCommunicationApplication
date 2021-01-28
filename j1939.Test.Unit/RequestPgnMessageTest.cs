
using NUnit.Framework;
using IECA.J1939.Messages;
using IECA.J1939.Utility;

namespace j1939.Test.Unit
{
    public class RequestPgnMessageTest
    {
        [Test]
        public void RequestPgn_ValidPgn_ValidJ1939Message()
        {
            byte expectedDestination = 2;
            uint expectedPgn = 0x112233;
            var receivedMessage = RequestPgnMessage.RequestPgn(expectedPgn, expectedDestination);
            var receivedDestination = receivedMessage?.PDU.Specific.Value;

            var receivedPgn = Helpers.GetPgnFromList(receivedMessage?.Data);

            Assert.AreEqual(expectedDestination, receivedDestination);
            Assert.AreEqual(expectedPgn, receivedPgn);
        }

        [Test]
        public void RequestPgn_InvalidPgn_NullJ1939Message()
        {
            byte expectedDestination = 2;
            uint expectedPgn = 0x11223344;
            var receivedMessage = RequestPgnMessage.RequestPgn(expectedPgn, expectedDestination);

            Assert.AreEqual(null, receivedMessage);
        }
    }
}

