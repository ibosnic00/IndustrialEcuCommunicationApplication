using IECA.J1939;
using IECA.J1939.Messages;
using IECA.J1939.Messages.TransportProtocol;
using IECA.J1939.Configuration;
using IECA.J1939.Procedures;
using IECA.J1939.Utility;
using IECA.CANBus;
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

        // TODO: This will be part of external configuration
        private const string CONFIGURATION_PATH = @"/home/pi/Desktop/ieca_configuration.json";
        private const byte MIN_ADDRESS = 128;
        private const byte WANTED_ADDRESS = 128;
        private const byte MAX_ADDRESS = 225;
        private const int ADDRES_CLAIM_WAIT_PERIOD_IN_MS = 1250;

        #endregion Constants

        #region EcuNameSetup

        // TODO: This will be part of external configuration
        static bool arbitraryAddressCapable = true;
        static byte industryGroup = 0;
        static byte vehicleSystemInstance = 0;
        static byte vehicalSystem = 0b0111_1111;
        static bool reserved = false;
        static byte function = 0b1111_1111;
        static byte functionInstance = 0;
        static byte ecuInstance = 1;
        static uint manufacturerCode = 0;
        static uint identityNumber = 0b1_1111_1111_1111_1111_1111;

        #endregion

        #region Events

        public event EventHandler<MultiFrameMessage>? MultiFrameMessageReceived;

        #endregion


        #region Fields

        J1939ToStringConverter? J1939ToStringConverter;
        List<MultiFrameMessage>? MfMessagesBuffer;
        ICanInterface? CanInterface;
        Dictionary<byte, EcuName>? KnownNetworkNodes;
        byte ClaimedAddress = StandardData.NULL_ADDRESS;
        bool AddressClaimSuccessfull;
        EcuName? EcuName;

        #endregion Fields


        #region Application Logic

        static void Main(string[] args)
        {
            var program = new Program();
            var selectedChannel = CanChannel.can0;

            var configuration = ConfigurationDeserializer.GetConfigurationFromFile(CONFIGURATION_PATH);
            program.J1939ToStringConverter = new J1939ToStringConverter(configuration);

            program.MfMessagesBuffer = new List<MultiFrameMessage>();
            program.KnownNetworkNodes = new Dictionary<byte, EcuName>();
            program.AddressClaimSuccessfull = false;
            program.EcuName = new EcuName(arbitraryAddressCapable, industryGroup, vehicleSystemInstance, vehicalSystem, reserved, function, functionInstance, ecuInstance, manufacturerCode, identityNumber);

            program.CanInterface = new SocketCanInterface(selectedChannel);
            program.CanInterface.Initialize();
            program.CanInterface.DataFrameReceived += program.OnCanMessageReceived;
            program.MultiFrameMessageReceived += program.OnMultiFrameMessageReceived;

            program.TryToClaimAddress();

            _ = Task.Run(() =>
            {
                // main program loop
                while (true)
                {
                    // if any multiframe message is fully received
                    if (program.MfMessagesBuffer.Count != 0
                            && program.MfMessagesBuffer.Any(msg => msg.IsMessageComplete == true))
                    {
                        var receivedMFMessage = program.MfMessagesBuffer.SingleOrDefault(msg => msg.IsMessageComplete == true);
                        program.MultiFrameMessageReceived?.Invoke(program, receivedMFMessage);
                    }
                };
            });
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
            if (!msg.IsExtendedId || msg.Data == null)
                return;

            var receivedPdu = ProtocolDataUnit.FromCanExtIdentifierFormat(msg.ID);
            Console.WriteLine("Received J1939 message with PGN: 0x" + receivedPdu.ParameterGroupNumber.ToString("X2"));

            if (receivedPdu.ParameterGroupNumber == StandardPgns.TP_CM_PGN)
            {
                if (msg.DLC < StandardData.TP_CM_DATA_SIZE)
                    return;

                if (msg.Data[0] == (byte)ConnectionManagementMessageControlBytes.BAM)
                {
                    var broadcastAnnounceMsg = new BrodcastAnnounceMessage(msg.Data.ToList(), receivedPdu.SourceAddress);
                    HandleReceivedBroadcastAnnounceMessage(broadcastAnnounceMsg);
                }
            }
            else if (receivedPdu.ParameterGroupNumber == StandardPgns.TP_DT_PGN)
            {
                if (MfMessagesBuffer == null || msg.DLC < StandardData.TP_DT_DATA_SIZE)
                    return;

                if (MfMessagesBuffer.Exists(msg => msg.PDU.SourceAddress == receivedPdu.SourceAddress))
                {
                    var dataTransferMessage = new DataTransferMessage(msg.Data.ToList(), receivedPdu.Specific.Value, receivedPdu.SourceAddress);
                    HandleReceivedDataTransferMessage(dataTransferMessage);
                }
            }
            else if (receivedPdu.ParameterGroupNumber == StandardPgns.REQUEST_PGN)
            {
                if (Helpers.GetPgnFromRequestMessageData(msg.Data) == null)
                    return;

                var requestPgnMessage = new RequestPgnMessage((uint)Helpers.GetPgnFromRequestMessageData(msg.Data)!, receivedPdu.SourceAddress);
                HandleReceivedRequestPgnMessage(requestPgnMessage);
            }
            else if (receivedPdu.ParameterGroupNumber == StandardPgns.REQUEST_2_PGN)
            {
                // currently not in use
            }
            else if (receivedPdu.ParameterGroupNumber == StandardPgns.ACK_PGN)
            {
                // currently not in use
            }
            else if (receivedPdu.ParameterGroupNumber == StandardPgns.COMMANDED_ADDR_PGN)
            {
                // currently not in use
            }
            else if (receivedPdu.ParameterGroupNumber == StandardPgns.ADDR_CLAIMED_PGN)
            {
                if (msg.DLC < StandardData.ADDR_CLAIMED_DATA_SIZE)
                    return;

                var addressClaimedMessage = new AddressClaimedMessage(receivedPdu, msg.Data.ToList());
                HandleReceivedAddressClaimedMessage(addressClaimedMessage);
            }
            else
            {
                var receivedMessage = new J1939Message(receivedPdu, msg.Data.ToList());
                HandleReceivedCompletedJ1939Message(receivedMessage);
            }
        }

        #endregion Event Handlers


        #region Private Methods

        /// <summary>
        /// Process of claiming address on vehicle's network defined by J1939-81 document
        /// This is run on separate thread so Thread.Sleep() is safe to use here
        /// </summary>
        private void TryToClaimAddress()
        {
            _ = Task.Run(() =>
            {
                CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(ConnectionProcedures.SendRequestForAddressClaimMessage()!));
                Thread.Sleep(ADDRES_CLAIM_WAIT_PERIOD_IN_MS);

                if (KnownNetworkNodes != null && KnownNetworkNodes.ContainsKey(WANTED_ADDRESS))
                {
                    for (byte i = MIN_ADDRESS; i <= MAX_ADDRESS; i++)
                    {
                        if (!KnownNetworkNodes.ContainsKey(i))
                        {
                            ClaimedAddress = i;
                            break;
                        }
                    }
                }
                else
                    ClaimedAddress = WANTED_ADDRESS;

                if (ClaimedAddress != StandardData.NULL_ADDRESS)
                {
                    CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(new AddressClaimedMessage(EcuName!.ToRawFormat(), ClaimedAddress)));
                    AddressClaimSuccessfull = true;
                }
                else
                    CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(new CannotClaimAddressMessage(EcuName!.ToRawFormat())));
            });
        }

        private void HandleReceivedRequestPgnMessage(RequestPgnMessage requestedPgnMessage)
        {
            if (requestedPgnMessage.RequestedPgn == StandardPgns.ADDR_CLAIMED_PGN && AddressClaimSuccessfull)
            {
                if (ClaimedAddress != StandardData.NULL_ADDRESS)
                {
                    CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(new AddressClaimedMessage(EcuName!.ToRawFormat(), ClaimedAddress)));
                    AddressClaimSuccessfull = true;
                }
                else
                    CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(new CannotClaimAddressMessage(EcuName!.ToRawFormat())));
            }
        }

        private void HandleReceivedAddressClaimedMessage(AddressClaimedMessage addressClaimedMessage)
        {
            if (AddressClaimSuccessfull)
            {
                // add logic for comparing addresses and if == claimed - compare names
            }
            else
            {
                KnownNetworkNodes?.TryAdd(addressClaimedMessage.PDU.SourceAddress, addressClaimedMessage.EcuName);
            }
        }

        private void HandleReceivedBroadcastAnnounceMessage(BrodcastAnnounceMessage rcvMsg)
        {
            // remove any message that in buffer with same source - only one PGN per transfer is allowed
            MfMessagesBuffer?.RemoveAll(msg => msg.PDU.SourceAddress == rcvMsg.PDU.SourceAddress);
            // add new mf message in buffer
            MfMessagesBuffer?.Add(new MultiFrameMessage(rcvMsg.Pgn, rcvMsg.PDU.SourceAddress));
        }

        private void HandleReceivedDataTransferMessage(DataTransferMessage rcvMsg)
        {
            //TODO: Erase after testing
            Console.WriteLine("Received data transfer message for pgn " + rcvMsg.PDU.ParameterGroupNumber + " sqNo: " + rcvMsg.SequenceNumber);
            MfMessagesBuffer.Single(msg => msg.PDU.SourceAddress == rcvMsg.PDU.SourceAddress).AddPacketizedData(rcvMsg.PacketizedData);
        }

        private void RemoveCompletedMultiFrameMessageFromBuffer(MultiFrameMessage rcvMsg)
        {
            if (MfMessagesBuffer == null)
                return;

            //TODO: Check will this erase correct msg
            MfMessagesBuffer.RemoveAll(mfMsg => mfMsg.PDU.SourceAddress == rcvMsg.PDU.SourceAddress
                                                                && mfMsg.PDU.Specific.Value == rcvMsg.PDU.Specific.Value);
        }

        private void HandleReceivedCompletedJ1939Message(J1939Message rcvMsg)
        {
            if (J1939ToStringConverter != null)
                Console.WriteLine(J1939ToStringConverter.ConvertJ1939MessageToHumanReadableFormat(rcvMsg));
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