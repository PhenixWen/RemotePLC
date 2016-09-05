using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RemotePLC.src.comm;

namespace RemotePLC.src.service
{
    class NetConnector : Connector
    {
        private Socket _socket = null;
        private Socket _listener = null;
        private Thread listenThread;
        private bool stopListen = true;
        public NetConnector(ConnectionInfo info) : base(info)
        {
            _socket = null;
            listenThread = new Thread(doListen);
        }
        private void doListen()
        {
            try
            {
                _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建Socket
                IPEndPoint ipe = new IPEndPoint(IPAddress.Any, _connectionInfo.ServicePort);//把ip和端口转化为IPEndpoint实例

                _listener.Bind(ipe);

                Logger.Info("[{0}] IDE Listening… port:[{1}]", _connectionInfo.DtuSn, _connectionInfo.ServicePort);

                _listener.Listen(10);

                while (!stopListen)
                {
                    _socket = _listener.Accept();
                    Logger.Info("[{0}] IDE Socket Connectted!", _connectionInfo.DtuSn);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }


        public override bool Connect()
        {
            try
            {
                stopListen = false;
                listenThread.Start();
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return false;
            }
            return true;
        }

        public override bool Disconnect()
        {
            stopListen = true;

            try
            {
                if (_listener != null)
                {
                    _listener.Dispose();
                }
                if (_socket != null)
                {
                    if (_socket.Connected)
                    {
                        _socket.Dispose();
                    }
                    _socket.Close();
                }
                Logger.Info("[{0}] IDE Socket Disconnectted!", _connectionInfo.DtuSn);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return false;
            }
            return true;
        }

        public override int Receive(ref byte[] commRead, int NumBytes)
        {
            int num = 0;
            if (_socket != null && _socket.Connected)
            {
                try
                {
                    num = _socket.Receive(commRead, NumBytes, SocketFlags.None);
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString());
                }
            }
            return num;
        }

        public override int Send(byte[] WriteBytes, int NumBytes)
        {
            int num = 0;
            if (_socket != null && _socket.Connected)
            {
                try
                {
                    num = _socket.Send(WriteBytes, NumBytes, SocketFlags.None);
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString());
                }
            }
            return num;
        }
    }
}
