using System;
using System.Collections.Generic;
using System.Text;

namespace IECA.CANBus
{
    public interface ICanInterface : IDisposable
    {
        void Initialize();

        void Deinitialize();

        void SendCanMessage(CanMessage canMessage);

        event EventHandler<CanMessage>? DataFrameReceived;

        event EventHandler<CanMessage>? MessageSent;
    }
}
