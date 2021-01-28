using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IECA.CANBus;
using IECA.J1939.Configuration;
using IECA.J1939.Utility;
using Microsoft.Extensions.Hosting;

namespace IECA.J1939.Services
{
    internal class BackgroundRequestSendService : IHostedService, IDisposable
    {
        private List<Timer>? _timers;
        private bool _disposed;
        private readonly List<ConfigurationRequestPgn> _requestPgns;
        private readonly byte _sourceAddress;
        private ICanInterface _canInterface;

        public BackgroundRequestSendService(List<ConfigurationRequestPgn> requestPgns, ICanInterface canInterface, byte sourceAddress)
        {
            _requestPgns = requestPgns;
            _canInterface = canInterface;
            _sourceAddress = sourceAddress;
            _timers = new List<Timer>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var requestPgn in _requestPgns)
                _timers!.Add(new Timer(x =>
               {
                   var j1939MessageForSending = ConnectionProcedures.SendRequestForPgnMessage(requestPgn.ParameterGroupNumber, _sourceAddress);

                   if (j1939MessageForSending?.Data != null)
                       _canInterface?.SendCanMessage(Helpers.ConvertSingleFrameJ1939MsgToCanMsg(j1939MessageForSending));

               }, null, dueTime: 0, requestPgn.RequestRateMilliseconds));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                if (_timers != null)
                    foreach (var _timer in _timers)
                        _timer?.Dispose();
            }
            _disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
