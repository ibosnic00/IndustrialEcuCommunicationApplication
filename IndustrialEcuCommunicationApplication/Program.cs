using IECA.J1939;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IECA
{
    class Program
    {
        #region Constants

        private const string CONFIGURATION_PATH = @"/home/pi/Desktop/ieca_configuration.json";
        private const uint REQUEST_PGN = 59904;
        private const byte GLOBAL_DESTINATION_ADDRESS = 255;

        #endregion Constants


        #region Fields

        Queue<KeyValuePair<uint, List<byte>>> requestQue = new Queue<KeyValuePair<uint, List<byte>>>();
        J1939ToStringConverter? J1939ToJsonConverter;

        #endregion Fields


        #region Application Logic

        static void Main(string[] args)
        {
            var program = new Program();
            var selectedChannel = CanChannel.can0;

            var canInterface = new SocketCanInterface(selectedChannel);
            canInterface.StartReceiverThread();
            canInterface.DataFrameReceived += program.OnMessageReceivedLoggerOnly;

            var configuration = ConfigurationDeserializer.GetConfigurationFromFile(CONFIGURATION_PATH);
            program.J1939ToJsonConverter = new J1939ToStringConverter(configuration);

            Console.WriteLine($"Configuration file has {configuration.Count} defined PGN's");

            while (true)
            {
                // main program loop
            };
        }

        #endregion Application Logic


        #region Event Handlers

        private void OnMessageReceivedLoggerOnly(object? sender, CanMessage msg)
        {
            if (!msg.IsExtendedId)
                return;

            if (msg.Data == null)
                return;

            var receivedPdu = ProtocolDataUnit.FromCanExtIdentifierFormat(msg.ID);

            Console.WriteLine("Received J1939 message with PGN: 0x" + receivedPdu.ParameterGroupNumber.ToString("X2"));

            var receivedMessage = new J1939Message(receivedPdu, msg.Data.ToList());
            if (J1939ToJsonConverter != null)
                Console.WriteLine(J1939ToJsonConverter.ConvertJ1939MessageToHumanReadableFormat(receivedMessage));
        }


        private void OnMessageReceived(object? sender, CanMessage msg)
        {
            if (!msg.IsExtendedId)
                return;

            var receivedPdu = ProtocolDataUnit.FromCanExtIdentifierFormat(msg.ID);

            var receivedMessage = new J1939Message(receivedPdu, msg.Data.ToList());
            if (J1939ToJsonConverter != null)
                Console.WriteLine(J1939ToJsonConverter.ConvertJ1939MessageToHumanReadableFormat(receivedMessage));

            // specific request
            if (receivedPdu.ParameterGroupNumber == REQUEST_PGN && receivedPdu.Specific.Value != GLOBAL_DESTINATION_ADDRESS)
            {
                //TODO: key should be 4 byte ID - specific request
                requestQue.Enqueue(new KeyValuePair<uint, List<byte>>(receivedPdu.SourceAddress, receivedMessage.Data));
            }

            // global request
            if (receivedPdu.ParameterGroupNumber == REQUEST_PGN && receivedPdu.Specific.Value == GLOBAL_DESTINATION_ADDRESS)
            {
                //TODO: key should be 4 byte ID - global request
                requestQue.Enqueue(new KeyValuePair<uint, List<byte>>(receivedPdu.SourceAddress, receivedMessage.Data));
            }

            // PDU1 Format
            if (receivedPdu.Specific is DestinationAddress destinationAddress)
            {
                if (destinationAddress.Value == GLOBAL_DESTINATION_ADDRESS)
                {
                    // USE JUMP TABLE FOR PGN VALUES OF INTEREST AND
                    // IF SA = ID OF SPECIAL INTEREST
                    // THEN
                    // SAVE 8 BYTES OF DATA IN DEDICATED BUFFER
                    // ELSE
                    // SAVE 12 BYTE MESSAGE(ID AND DATA) IN CIRCULAR QUE
                }
                else
                {
                    // USE JUMP TABLE FOR PGN VALUES OF INTEREST AND
                    // IF SA = ID OF SPECIAL INTEREST VALUES
                    // THEN
                    // SAVE 8 BYTES OF DATA IN DEDICATED BUFFER
                    // ELSE
                    // SAVE 12 BYTE MESSAGE(ID AND DATA) IN CIRCULAR QUE
                }
            }

            //PDU2 Format
            if (receivedPdu.Specific is GroupExtension groupExtension)
            {
                // USE JUMP TABLE FOR PGN VALUES OF INTEREST AND
                // IF SA = ID OF SPECIAL INTEREST
                // THEN
                // SAVE 8 BYTES OF DATA IN DEDICATED BUFFER
                // ELSE
                // SAVE 12 BYTE MESSAGE (ID AND DATA) IN CIRCULAR QUE
            }
        }

        #endregion Event Handlers
    }

    public enum CanChannel : byte
    {
        can0,
        can1,
        can2,
        can3
    }
}