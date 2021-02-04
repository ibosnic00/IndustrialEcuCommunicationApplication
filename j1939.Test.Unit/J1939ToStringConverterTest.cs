using NUnit.Framework;
using IECA;
using IECA.J1939;
using IECA.J1939.Configuration;
using System.IO;
using System.Linq;

namespace j1939.Test.Unit
{
    public class J1939ToStringConverterTest
    {
        J1939ToStringConverter? J1939ToStringConverter;

        public static DirectoryInfo TryGetSolutionDirectoryInfo(string? currentPath = null)
        {
            var directory = new DirectoryInfo(
                currentPath ?? Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }
            return directory ?? throw new System.ArgumentNullException();
        }

        [SetUp]
        public void Setup()
        {
            var slnPath = TryGetSolutionDirectoryInfo();
            var configurationPath = Path.Combine(slnPath.FullName, "IndustrialEcuCommunicationApplication", "Resources", "ieca_configuration.json");
            var configuration = DataConfigurationDeserializer.GetConfigurationFromFile(configurationPath);
            J1939ToStringConverter = new J1939ToStringConverter(configuration);
        }

        [Test]
        public void ConvertJ1939MessageToHumanReadableFormat_Pdu0_SuccessfullConversion()
        {
            var pdu = new ProtocolDataUnit(0, 0, 0, 0, 0);
            var j1939Message = new J1939Message(pdu,
                new System.Collections.Generic.List<byte> { 0xFF, 0xFF, 0xFF, 0xFF });
            var parsedMessage = J1939ToStringConverter?.ConvertJ1939MessageToHumanReadableFormat(j1939Message);
            var expectedMessage = "Torque/Speed Control 1 Override Control Mode:[11] Requested Speed Control Conditions: [11] Override Control Mode Priority: [11] Requested Speed/Speed Limit: 8191.875 Requested Torque/Torque Limit: 130";

            Assert.IsTrue(string.Compare(expectedMessage, parsedMessage) == 1);
        }

        [Test]
        public void ConvertJ1939MessageToHumanReadableFormat_Pdu0xCF00_SuccessfullConversion()
        {
            var pdu = new ProtocolDataUnit(6, 0, 0xCF, 0, 0x21);
            var j1939Message = new J1939Message(pdu,
                new System.Collections.Generic.List<byte> { 0x01, 0x55, 0x12, 0xAA, 0x02, 0x55, 0x0A, 0xEE });
            var parsedMessage = J1939ToStringConverter?.ConvertJ1939MessageToHumanReadableFormat(j1939Message);
            var expectedMessage = "Continuous Torque & Speed Limit Request Minimum Continuous Engine Speed Limit Request: 32 Maximum Continuous Engine Speed Limit Request: 2720 Minimum Continuous Engine Torque Limit Request: -107 Maximum Continuous Engine Torque Limit Request: 45 Minimum Continuous Retarder Speed Limit Request: 64 Maximum Continuous Retarder Speed Limit Request: 2720 Minimum Continuous Retarder Torque Limit Request: -115 Maximum Continuous Retarder Torque Limit Request: 113";

            Assert.IsTrue(parsedMessage?.StartsWith(expectedMessage));
        }
    }
}
