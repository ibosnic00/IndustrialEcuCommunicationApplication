using IECA.J1939;
using IECA.J1939.Messages;
using IECA.J1939.Messages.TransportProtocol;
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
        #region PGN's

        private const uint REQUEST_PGN = 59904;
        private const uint TP_CM_PGN = 60416;
        private const uint TP_DT_PGN = 60160;
        private const uint REQUEST_2_PGN = 51456;
        private const uint ACK_PGN = 59392;

        private const byte TP_CM_DATA_SIZE = 8;
        private const byte TP_DT_DATA_SIZE = 8;

        #endregion

        #region Constants

        private const string CONFIGURATION_PATH = @"/home/pi/Desktop/ieca_configuration.json";
        private const byte GLOBAL_DESTINATION_ADDRESS = 255;

        #endregion Constants


        #region Events

        public event EventHandler<MultiFrameMessage>? MultiFrameMessageReceived;

        #endregion


        #region Fields

        Queue<KeyValuePair<uint, List<byte>>> requestQue = new Queue<KeyValuePair<uint, List<byte>>>();
        J1939ToStringConverter? J1939ToJsonConverter;
        List<MultiFrameMessage>? mfMsgsBuffer;

        #endregion Fields


        #region Application Logic

        static void Main(string[] args)
        {
            var program = new Program();
            var selectedChannel = CanChannel.can0;

            var configuration = ConfigurationDeserializer.GetConfigurationFromFile(CONFIGURATION_PATH);
            program.J1939ToJsonConverter = new J1939ToStringConverter(configuration);
            Console.WriteLine($"Configuration file has {configuration.Count} defined PGN's");

            program.mfMsgsBuffer = new List<MultiFrameMessage>();
            program.MultiFrameMessageReceived += program.OnMultiFrameMessageReceived;

            var canInterface = new SocketCanInterface(selectedChannel);
            canInterface.StartReceiverThread();
            canInterface.DataFrameReceived += program.OnCanMessageReceived;

            // main program loop
            while (true)
            {
                // if any multiframe message is fully received
                _ = Task.Run(() =>
                {
                    if (program.mfMsgsBuffer.Any(msg => msg.IsMessageComplete == true))
                    {
                        var receivedMFMessage = program.mfMsgsBuffer.SingleOrDefault(msg => msg.IsMessageComplete == true);
                        program.MultiFrameMessageReceived?.Invoke(program, receivedMFMessage);
                    }
                });
            };
        }

        #endregion Application Logic


        #region Event Handlers

        private void OnMultiFrameMessageReceived(object? sender, MultiFrameMessage msg)
        {
            if (msg.Data == null)
                return;

            var receivedPdu = msg.PDU;

            Console.WriteLine("Received J1939 message with PGN: 0x" + receivedPdu.ParameterGroupNumber.ToString("X2"));

            var receivedMessage = new J1939Message(receivedPdu, msg.Data.ToList());
            HandleReceivedCompletedJ1939Message(receivedMessage);
            RemoveCompletedMultiFrameMessageFromBuffer(msg);
        }

        private void OnCanMessageReceived(object? sender, CanMessage msg)
        {
            if (!msg.IsExtendedId)
                return;

            if (msg.Data == null)
                return;

            var receivedPdu = ProtocolDataUnit.FromCanExtIdentifierFormat(msg.ID);
            Console.WriteLine("Received J1939 message with PGN: 0x" + receivedPdu.ParameterGroupNumber.ToString("X2"));

            if (receivedPdu.ParameterGroupNumber == TP_CM_PGN)
            {
                if (msg.DLC < TP_CM_DATA_SIZE)
                    return;

                if (msg.Data[0] == (byte)ConnectionManagementMessageControlBytes.BAM)
                {
                    var broadcastAnnounceMsg = new BrodcastAnnounceMessage(msg.Data.ToList(), receivedPdu.SourceAddress);
                    HandleReceivedBroadcastAnnounceMessage(broadcastAnnounceMsg);
                }
            }
            else if (receivedPdu.ParameterGroupNumber == TP_DT_PGN)
            {
                if (mfMsgsBuffer == null || msg.DLC < TP_CM_DATA_SIZE)
                    return;

                if (mfMsgsBuffer.Exists(msg => msg.PDU.SourceAddress == receivedPdu.SourceAddress))
                {
                    var dataTransferMessage = new DataTransferMessage(msg.Data.ToList(), receivedPdu.Specific.Value, receivedPdu.SourceAddress);
                    HandleReceivedDataTransferMessage(dataTransferMessage);
                }
            }
            else if (receivedPdu.ParameterGroupNumber == REQUEST_2_PGN)
            {
                //currently not in use
            }
            else if (receivedPdu.ParameterGroupNumber == ACK_PGN)
            {
                //currently not in use
            }
            else
            {
                var receivedMessage = new J1939Message(receivedPdu, msg.Data.ToList());
                HandleReceivedCompletedJ1939Message(receivedMessage);
            }
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


        #region Private Methods


        private void HandleReceivedBroadcastAnnounceMessage(BrodcastAnnounceMessage rcvMsg)
        {
            // remove any message that in buffer with same source - only one PGN per transfer is allowed
            mfMsgsBuffer?.RemoveAll(msg => msg.PDU.SourceAddress == rcvMsg.PDU.SourceAddress);
            // add new mf message in buffer
            mfMsgsBuffer?.Add(new MultiFrameMessage(rcvMsg.Pgn, rcvMsg.PDU.SourceAddress));
        }

        private void HandleReceivedDataTransferMessage(DataTransferMessage rcvMsg)
        {
            //TODO: Erase after testing
            Console.WriteLine("Received data transfer message for pgn " + rcvMsg.PDU.ParameterGroupNumber + " sqNo: " + rcvMsg.SequenceNumber);
            mfMsgsBuffer.Single(msg => msg.PDU.SourceAddress == rcvMsg.PDU.SourceAddress).AddPacketizedData(rcvMsg.PacketizedData);
        }

        private void RemoveCompletedMultiFrameMessageFromBuffer(MultiFrameMessage rcvMsg)
        {
            if (mfMsgsBuffer == null)
                return;

            //TODO: Check will this erase correct msg
            mfMsgsBuffer.RemoveAll(mfMsg => mfMsg.PDU.SourceAddress == rcvMsg.PDU.SourceAddress
                                                                && mfMsg.PDU.Specific.Value == rcvMsg.PDU.Specific.Value);
        }

        private void HandleReceivedCompletedJ1939Message(J1939Message rcvMsg)
        {
            if (J1939ToJsonConverter != null)
                Console.WriteLine(J1939ToJsonConverter.ConvertJ1939MessageToHumanReadableFormat(rcvMsg));
        }


        #endregion
    }

    public enum CanChannel : byte
    {
        can0,
        can1,
        can2,
        can3
    }
}