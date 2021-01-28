using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using IECA.CANBus.Utility;

namespace IECA
{
    public class SocketCanInterface : IDisposable
    {
        #region Constants

        const string CANDUMP_FILE_NAME = "candump";
        const string CANSEND_FILE_NAME = "cansend";

        #endregion


        #region Fields

        bool shouldReceiveThreadBeAlive;
        Process? canDumpProcess;
        Process? canSendProcess;

        #endregion


        #region Constructors

        public SocketCanInterface(CanChannel channel)
        {
            selectedChannel = channel;
        }

        #endregion


        #region Events

        public event EventHandler<CanMessage>? DataFrameReceived;
        public event EventHandler<CanMessage>? MessageSent;

        #endregion


        #region Properties

        CanChannel selectedChannel { get; }

        #endregion


        #region Public Methods

        public void StartReceiverThread()
        {
            shouldReceiveThreadBeAlive = true;
            canDumpProcess = new Process();
            canDumpProcess.StartInfo.FileName = CANDUMP_FILE_NAME;
            canDumpProcess.StartInfo.UseShellExecute = false;
            canDumpProcess.StartInfo.Arguments = selectedChannel.ToString();
            canDumpProcess.StartInfo.RedirectStandardOutput = true;
            canDumpProcess.StartInfo.CreateNoWindow = true;
            _ = canDumpProcess.Start();

            _ = Task.Run(() =>
            {
                StreamReader reader = canDumpProcess.StandardOutput;
                string? lineInOutput;
                while ((lineInOutput = reader.ReadLine()) != null && shouldReceiveThreadBeAlive)
                {
                    var recCanMsg = Helpers.CandumpStringToCanMessage(lineInOutput);

                    if (recCanMsg.MessageType == CanMessageType.Data)
                        DataFrameReceived?.Invoke(this, recCanMsg);
                }
            });
        }

        public void StopReceiverThread()
        {
            shouldReceiveThreadBeAlive = false;
            canDumpProcess?.Dispose();
        }

        public void SendCanMessage(CanMessage canMessage)
        {
            var canDumpMessageToSend = Helpers.CanMessageToCandumpString(canMessage);
            if (canDumpMessageToSend == null)
                return;

            canSendProcess = new Process();
            canSendProcess.StartInfo.FileName = CANSEND_FILE_NAME;
            canSendProcess.StartInfo.UseShellExecute = false;
            canSendProcess.StartInfo.Arguments = selectedChannel.ToString() + " " + canDumpMessageToSend;
            canSendProcess.StartInfo.RedirectStandardOutput = true;
            canSendProcess.StartInfo.CreateNoWindow = true;

            _ = canSendProcess.Start();
            MessageSent?.Invoke(this, canMessage);
        }

        #endregion


        #region IDisposable implementation

        bool _isDisposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                canSendProcess?.Kill();
                canSendProcess?.Dispose();
                canDumpProcess?.Kill();
                canDumpProcess?.Dispose();
            }

            _isDisposed = true;
        }

        #endregion
    }
}
