﻿using NUnit.Framework;
using IECA;
using IECA.J1939;

namespace j1939.Test.Unit
{
    public class ProtocolDataUnitTest
    {
        [Test]
        public void FromCanExtIdentifierFormat_ExtendedId_Success() {
            var expectedPdu = new ProtocolDataUnit(6, 0, 0xCF, 0, 0x21);
            var receivedPdu = ProtocolDataUnit.FromCanExtIdentifierFormat(0x18CF0021);
            Assert.AreEqual(expectedPdu.ParameterGroupNumber, receivedPdu.ParameterGroupNumber);
            Assert.AreEqual(expectedPdu.DataPage, receivedPdu.DataPage);
            Assert.AreEqual(expectedPdu.SourceAddress, receivedPdu.SourceAddress);
        }

    }
}
