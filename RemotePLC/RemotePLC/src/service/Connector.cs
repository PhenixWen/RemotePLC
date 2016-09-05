using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemotePLC.src.comm;

namespace RemotePLC.src.service
{
    public abstract class Connector
    {

        protected ConnectionInfo _connectionInfo;
        public Connector(ConnectionInfo info)
        {
            _connectionInfo = info;
        }
        public abstract bool Connect();
        public abstract bool Disconnect();
        public abstract int Receive(ref byte[] commRead, int NumBytes);
        public abstract int Send(byte[] WriteBytes, int NumBytes);
    }
}
