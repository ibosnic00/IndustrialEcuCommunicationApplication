
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
            uint expectedPgn = 50567;
            byte expectedSource = 129;

            var receivedMessage = new RequestPgnMessage(expectedPgn, expectedSource, expectedDestination);
            var receivedDestination = receivedMessage?.PDU.Specific.Value;
            var receivedPgn = Helpers.GetPgnFromList(receivedMessage?.Data);
            var receivedSource = receivedMessage?.PDU.SourceAddress;

            Assert.AreEqual(expectedDestination, receivedDestination);
            Assert.AreEqual(expectedPgn, receivedPgn);
            Assert.AreEqual(expectedSource, receivedSource);
        }
    }
}

