﻿using IECA.J1939;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace IECA
{
    class Program
    {
        #region Constants

        private const uint REQUEST_PGN = 59904;
        private const byte GLOBAL_DESTINATION_ADDRESS = 255;

        #endregion Constants


        #region Fields

        Queue<KeyValuePair<uint, List<byte>>> requestQue = new Queue<KeyValuePair<uint, List<byte>>>();

        #endregion Fields


        #region Application Logic

        static void Main(string[] args)
        {
            var program = new Program();
            var selectedChannel = CanChannel.can0;

            var canInterface = new SocketCanInterface(selectedChannel);
            canInterface.StartReceiverThread();
            canInterface.DataFrameReceived += program.OnMessageReceivedLoggerOnly;

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

            var receivedPdu = ProtocolDataUnit.FromCanExtIdentifierFormat(msg.ID);

            if (msg.Data != null)
                foreach (var dataByte in msg.Data)
                    receivedPdu.DataField.Add(dataByte);

            Console.WriteLine("Received J1939 message with:");
            Console.WriteLine("Priority : " + receivedPdu.Priority);
            Console.WriteLine("Reserved : " + receivedPdu.Reserved);
            Console.WriteLine("DataPage : " + receivedPdu.DataPage);
            Console.WriteLine("PDU Format : " + receivedPdu.Format);
            Console.WriteLine("PDU Specific : " + receivedPdu.Specific.Value);
            Console.WriteLine("SourceAddress : " + receivedPdu.SourceAddress);
            Console.WriteLine("PGN: 0x" + receivedPdu.ParameterGroupNumber.ToString("X2"));
        }


        private void OnMessageReceived(object? sender, CanMessage msg)
        {
            if (!msg.IsExtendedId)
                return;

            var receivedPdu = ProtocolDataUnit.FromCanExtIdentifierFormat(msg.ID);

            if (msg.Data != null)
                foreach (var dataByte in msg.Data)
                    receivedPdu.DataField.Add(dataByte);


            // specific request
            if (receivedPdu.ParameterGroupNumber == REQUEST_PGN && receivedPdu.Specific.Value != GLOBAL_DESTINATION_ADDRESS)
            {
                //TODO: key should be 4 byte ID - specific request
                requestQue.Enqueue(new KeyValuePair<uint, List<byte>>(receivedPdu.SourceAddress, receivedPdu.DataField));
            }

            // global request
            if (receivedPdu.ParameterGroupNumber == REQUEST_PGN && receivedPdu.Specific.Value == GLOBAL_DESTINATION_ADDRESS)
            {
                //TODO: key should be 4 byte ID - global request
                requestQue.Enqueue(new KeyValuePair<uint, List<byte>>(receivedPdu.SourceAddress, receivedPdu.DataField));
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