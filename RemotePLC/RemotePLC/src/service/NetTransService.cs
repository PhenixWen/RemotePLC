using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using RemotePLC.src.comm;
using RemotePLC.src.comm.protocol;

namespace RemotePLC.src.service
{
    public class NetTransService : BaseService
    {
        public NetTransService(ConnectionInfo info) : base(info)
        {
            _connectionInfo = info;
            _connector = new NetConnector(info);
        }


    }
}
