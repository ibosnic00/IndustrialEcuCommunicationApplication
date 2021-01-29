﻿using IECA.J1939;
using IECA.J1939.Messages;
using IECA.J1939.Messages.TransportProtocol;
using IECA.J1939.Configuration;
using IECA.J1939.Services;
using IECA.J1939.Utility;
using IECA.CANBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace IECA.Application
{
    public class IndustrialEcuCommunciationApp
    {
        #region Events

        public event EventHandler<MultiFrameMessage>? MultiFrameMessageReceived;

        #endregion


        #region Fields

        public ICanInterface CanInterface;
        public bool AddressClaimSuccessfull;
        public byte ClaimedAddress = StandardData.NULL_ADDRESS;

        List<MultiFrameMessage>? _mfMessagesBuffer;
        J1939ToStringConverter? _j1939ToStringConverter;
        ApplicationConfiguration _appConfig;
        Dictionary<byte, EcuName>? _knownNetworkNodes;
        EcuName? _ecuName;

        #endregion Fields


        #region Constructors

        public IndustrialEcuCommunciationApp(ICanInterface canInterface, string appConfigurationPath)
        {
            var appConfig = ApplicationConfigurationDeserializer.GetConfigurationFromFile(appConfigurationPath);
            _appConfig = appConfig;
            CanInterface = canInterface;
            MultiFrameMessageReceived += OnMultiFrameMessageReceived;
        }

        #endregion Constructors


        #region Public Methods

        public void Initialize()
        {
            var dataConfig = DataConfigurationDeserializer.GetConfigurationFromFile(_appConfig.DataConfigurationPath);
            _j1939ToStringConverter = new J1939ToStringConverter(dataConfig);

            _mfMessagesBuffer = new List<MultiFrameMessage>();
            _knownNetworkNodes = new Dictionary<byte, EcuName>();
            AddressClaimSuccessfull = false;
            _ecuName = new EcuName(_appConfig.ArbitraryAddressCapable, _appConfig.IndustryGroup,
                _appConfig.VehicleSystemInstance, _appConfig.VehicalSystem, reserved: false,
                _appConfig.Function, _appConfig.FunctionInstance, _appConfig.EcuInstance,
                _appConfig.ManufacturerCode, _appConfig.IdentityNumber);

            CanInterface.Initialize();
            CanInterface.DataFrameReceived += OnCanMessageReceived;
            MultiFrameMessageReceived += OnMultiFrameMessageReceived;

            TryToClaimAddress();
            StartSendingRequestPgns();
        }

        public void InvokeEventIfAnyMultiframeMessageIsReceivedCompletely()
        {
            if (_mfMessagesBuffer == null)
                return;

            if (_mfMessagesBuffer.Count != 0 && _mfMessagesBuffer.Any(msg => msg.IsMessageComplete == true))
            {
                var receivedMFMessage = _mfMessagesBuffer.SingleOrDefault(msg => msg.IsMessageComplete == true);
                MultiFrameMessageReceived?.Invoke(this, receivedMFMessage);
            }
        }

        #endregion Public Methods


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
                if (_mfMessagesBuffer == null || msg.DLC < StandardData.TP_DT_DATA_SIZE)
                    return;

                if (_mfMessagesBuffer.Exists(msg => msg.PDU.SourceAddress == receivedPdu.SourceAddress))
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

        private IHostBuilder BackgroundRequestSender() =>
               Host.CreateDefaultBuilder().ConfigureServices(services =>
               {
                   services.AddHostedService(x =>
                   new BackgroundRequestSendService(_appConfig!.PgnsForRequesting, this));
               });

        private void StartSendingRequestPgns()
        {
            BackgroundRequestSender().Build().Run();
        }

        private void TryToClaimAddress()
        {
            _ = Task.Run(() =>
            {
                CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(ConnectionProcedures.SendRequestForAddressClaimMessage()!));

                if (_knownNetworkNodes != null && _knownNetworkNodes.ContainsKey(_appConfig!.WantedAddress))
                {
                    for (byte i = _appConfig!.MinAddress; i <= _appConfig!.MaxAddress; i++)
                    {
                        if (!_knownNetworkNodes.ContainsKey(i))
                        {
                            ClaimedAddress = i;
                            break;
                        }
                    }
                }
                else
                    ClaimedAddress = _appConfig!.WantedAddress;

                if (ClaimedAddress != StandardData.NULL_ADDRESS)
                {
                    CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(new AddressClaimedMessage(_ecuName!.ToRawFormat(), ClaimedAddress)));
                    AddressClaimSuccessfull = true;
                }
                else
                    CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(new CannotClaimAddressMessage(_ecuName!.ToRawFormat())));
            });
            Thread.Sleep(_appConfig!.AddressClaimWaitPeriodMs);
            Thread.Sleep(_appConfig!.AddressClaimWaitPeriodMs);
            Thread.Sleep(_appConfig!.AddressClaimWaitPeriodMs);
            Thread.Sleep(_appConfig!.AddressClaimWaitPeriodMs);
        }

        private void HandleReceivedRequestPgnMessage(RequestPgnMessage requestedPgnMessage)
        {
            if (requestedPgnMessage.RequestedPgn == StandardPgns.ADDR_CLAIMED_PGN && AddressClaimSuccessfull)
            {
                if (ClaimedAddress != StandardData.NULL_ADDRESS)
                {
                    CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(new AddressClaimedMessage(_ecuName!.ToRawFormat(), ClaimedAddress)));
                    AddressClaimSuccessfull = true;
                }
                else
                    CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(new CannotClaimAddressMessage(_ecuName!.ToRawFormat())));
            }
        }

        private void HandleReceivedAddressClaimedMessage(AddressClaimedMessage addressClaimedMessage)
        {
            if (AddressClaimSuccessfull)
            {
                if (addressClaimedMessage.PDU.SourceAddress == ClaimedAddress)
                {
                    if (addressClaimedMessage.EcuName.ToRawFormat() < _ecuName!.ToRawFormat())
                    {
                        AddressClaimSuccessfull = false;
                        TryToClaimAddress();
                    }
                    else
                        CanInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(new AddressClaimedMessage(_ecuName!.ToRawFormat(), ClaimedAddress)));
                }
            }
            else
            {
                _knownNetworkNodes?.TryAdd(addressClaimedMessage.PDU.SourceAddress, addressClaimedMessage.EcuName);
            }
        }

        private void HandleReceivedBroadcastAnnounceMessage(BrodcastAnnounceMessage rcvMsg)
        {
            // remove any message that in buffer with same source - only one PGN per transfer is allowed
            _mfMessagesBuffer?.RemoveAll(msg => msg.PDU.SourceAddress == rcvMsg.PDU.SourceAddress);
            // add new mf message in buffer
            _mfMessagesBuffer?.Add(new MultiFrameMessage(rcvMsg.Pgn, rcvMsg.PDU.SourceAddress));
        }

        private void HandleReceivedDataTransferMessage(DataTransferMessage rcvMsg)
        {
            //TODO: Erase after testing
            Console.WriteLine("Received data transfer message for pgn " + rcvMsg.PDU.ParameterGroupNumber + " sqNo: " + rcvMsg.SequenceNumber);
            _mfMessagesBuffer.Single(msg => msg.PDU.SourceAddress == rcvMsg.PDU.SourceAddress).AddPacketizedData(rcvMsg.PacketizedData);
        }

        private void RemoveCompletedMultiFrameMessageFromBuffer(MultiFrameMessage rcvMsg)
        {
            if (_mfMessagesBuffer == null)
                return;

            //TODO: Check will this erase correct msg
            _mfMessagesBuffer.RemoveAll(mfMsg => mfMsg.PDU.SourceAddress == rcvMsg.PDU.SourceAddress
                                                                && mfMsg.PDU.Specific.Value == rcvMsg.PDU.Specific.Value);
        }

        private void HandleReceivedCompletedJ1939Message(J1939Message rcvMsg)
        {
            if (_j1939ToStringConverter != null)
                Console.WriteLine(_j1939ToStringConverter.ConvertJ1939MessageToHumanReadableFormat(rcvMsg));
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