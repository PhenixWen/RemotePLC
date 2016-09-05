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
    public abstract class BaseService
    {
        public class StateObject
        {
            // Client   socket.
            public Socket workSocket = null;
            // Receive buffer.
            public byte[] buffer = new byte[Config.BufferSize];
        }
        private bool _waitdtuconnect = false;
        private bool _dtuconnected = false;

        private bool _waitplcconnect = false;
        private bool _plcconnected = false;

        protected ConnectionInfo _connectionInfo;

        protected Socket _socket = null;

        private Thread mainThread;

        private volatile bool _shouldStop;

        protected Connector _connector = null;

        public BaseService(ConnectionInfo info)
        {
            _connectionInfo = info;
        }

        public bool DoStart()
        {
            if (mainThread != null && mainThread.IsAlive)
            {
                MessageBox.Show("Service not stoped!", "RemotePLC", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else
            {
                _shouldStop = false;
                _dtuconnected = false;
                _plcconnected = false;
                mainThread = new Thread(doMainWork);
                mainThread.Start();
                return true;
            }
        }
        public bool DoStop()
        {
            if (mainThread != null)
            {
                _shouldStop = true;
            }

            try
            {
                if (_socket != null && _socket.Connected)
                {
                    _socket.Disconnect(true);
                }

            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

            try
            {
                _connector.Disconnect();
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

            _connectionInfo.Status = (int)RunningStatus.RS_UNCONNECTED;

            return true;
        }

        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.     
                StateObject state = new StateObject();
                state.workSocket = client;
                // Begin receiving the data from the remote device.     
                client.BeginReceive(state.buffer, 0, Config.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket     
                // from the asynchronous state object.     
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                // Read data from the remote device.     
                if (client != null && client.Connected)
                {
                    int bytesRead = client.EndReceive(ar);
                    if (bytesRead > 0)
                    {
                        // There might be more data, so store the data received so far.     
                        _doLocalSend(state.buffer, bytesRead);

                        // Get the rest of the data.     
                        client.BeginReceive(state.buffer, 0, Config.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }
        private void _writeSocket(byte[] buffer, int length)
        {
            if (_socket != null && _socket.Connected && _dtuconnected)
            {
                try
                {
                    byte[] sn = BitConverter.GetBytes(ulong.Parse(_connectionInfo.DtuSn, NumberStyles.AllowHexSpecifier));
                    Array.Reverse(sn);

                    byte[] bytes = buffer.Take(length).ToArray();

                    if (GetType() == typeof(NetTransService))
                    {
                        MonitorDataManager.instance.Add(_connectionInfo.DtuSn, bytes, MonitorDataType.MDT_LOC);

                        PassThrough passthrough = new PassThrough(sn, bytes);
                        Package pkg = new Package(passthrough);
                        Logger.Trace("[{0}] ===> {1}", _connectionInfo.DtuSn, pkg.ToString());
                        _socket.Send(pkg.toBytes());
                    }
                    else if (GetType() == typeof(ComTransService))
                    {
                        if (_connector.GetType() == typeof(ComConnector))
                        {
                            ComConnector cc = _connector as ComConnector;
                            MonitorDataManager.instance.Add(_connectionInfo.DtuSn, bytes, MonitorDataType.MDT_COM);
                            byte syncflag = 1;  //同步波特率
                            ComPassThrough compassthrough = new ComPassThrough(sn, syncflag, cc.VCom.DCBlock.BaudRate, cc.VCom.DCBlock.ByteSize, cc.VCom.DCBlock.Parity, cc.VCom.DCBlock.StopBits, bytes);
                            Package pkg = new Package(compassthrough);
                            Logger.Trace("[{0}] ===> {1}", _connectionInfo.DtuSn, pkg.ToString());
                            _socket.Send(pkg.toBytes());
                        }
                    }

                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString());
                }
            }
        }
        private void doMainWork()
        {
            Logger.Info("[{0}] MainThread started!", _connectionInfo.DtuSn);
            while (!_shouldStop)
            {
                if (_socket == null || (_socket != null && !_socket.Connected))
                {
                    _dtuconnected = false;
                    #region 连接服务器
                    _connectionInfo.Status = (int)RunningStatus.RS_CONNECTSERVER;
                    Logger.Info("connecting server!", _connectionInfo.DtuSn);

                    _doConnectServer();

                    if (_socket == null || (_socket != null && !_socket.Connected))
                    {
                        _connectionInfo.Status = (int)RunningStatus.RS_SERVERCONNECTFAIL;
                        Logger.Info("[{0}] server connect fail!", _connectionInfo.DtuSn);
                        if (!_shouldStop)
                        {
                            Thread.Sleep(3000);
                        }
                        continue;
                    }
                    else
                    {
                        //_connectionInfo.Status = (int)RunningStatus.UNDEFINED;
                        Logger.Info("[{0}] connect server OK!", _connectionInfo.DtuSn);
                    }
                    #endregion //连接服务器
                }

                //收socket消息
                Receive(_socket);

                if (!_dtuconnected)
                {
                    //_connectionInfo.Status = (int)RunningStatus.UNDEFINED;
                    Logger.Info("[{0}] connect dtu!", _connectionInfo.DtuSn);
                    #region 连接DTU
                    bool dtuConnectResult = _connectDtuRequest();

                    if (!dtuConnectResult)
                    {
                        //_connectionInfo.Status = (int)RunningStatus.UNDEFINED;
                        Logger.Info("[{0}] dtu connect fail!", _connectionInfo.DtuSn);
                        if (!_shouldStop)
                        {
                            Thread.Sleep(3000);
                        }
                        continue;
                    }
                    #endregion //连接DTU
                }

                _connectionInfo.Status = (int)RunningStatus.RS_CONNECTPLC;
                Logger.Info("[{0}] connecting plc!", _connectionInfo.DtuSn);
                #region 连接PLC
                bool plcConnectResult = _connectPlcRequest();

                if (!plcConnectResult)
                {
                    _connectionInfo.Status = (int)RunningStatus.RS_PLCCONNECTFAIL;
                    Logger.Info("[{0}] plc connect fail!", _connectionInfo.DtuSn);
                    if (!_shouldStop)
                    {
                        Thread.Sleep(3000);
                    }
                    continue;
                }
                #endregion //连接PLC

                #region 等待IDE连接
                if (_connector.Connect())
                {
                    _connectionInfo.Status = (int)RunningStatus.RS_CONNECTED;
                    Logger.Info("[{0}] ide connected!", _connectionInfo.DtuSn);
                }
                else
                {
                    _connectionInfo.Status = (int)RunningStatus.RS_IDECONNECTFAIL;
                    Logger.Info("[{0}] ide connect fail!", _connectionInfo.DtuSn);
                    if (!_shouldStop)
                    {
                        Thread.Sleep(3000);
                    }
                    continue;
                }
                #endregion //等待IDE连接

                doLocalReceive();

                if (!_shouldStop)
                {
                    Thread.Sleep(3000);
                }
            }

            if (_socket != null)
            {
                if (_socket.Connected)
                {
                    try
                    {
                        _socket.Disconnect(false);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.ToString());
                    }
                }
                _socket.Close();
                _socket = null;
            }
            _connectionInfo.Status = (int)RunningStatus.RS_UNCONNECTED;
            Logger.Info("[{0}] unconnected!", _connectionInfo.DtuSn);

            Logger.Info("[{0}] MainThread stoped!", _connectionInfo.DtuSn);
        }
        public void _doConnectServer()
        {
            try
            {
                _dtuconnected = false;
                int port = Config.ServerPort;
                string host = Config.ServerIp;
                ///创建终结点EndPoint
                IPAddress ip = IPAddress.Parse(host);
                //IPAddress ipp = new IPAddress("127.0.0.1");
                IPEndPoint ipe = new IPEndPoint(ip, port);//把ip和端口转化为IPEndpoint实例

                if (_socket != null)
                {
                    _socket.Close();
                }
                ///创建socket并连接到服务器
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建Socket
                Logger.Info("[{0}] Connecting…", _connectionInfo.DtuSn);
                _socket.Connect(ipe);//连接到服务器
                Logger.Info("[{0}] Socket Connectted!", _connectionInfo.DtuSn);

            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }
        private bool _connectDtuRequest()
        {
            try
            {
                //发送连接请求
                byte[] sn = BitConverter.GetBytes(ulong.Parse(_connectionInfo.DtuSn, NumberStyles.AllowHexSpecifier));
                Array.Reverse(sn);
                ConnectRequest connectrequest = new ConnectRequest(sn);
                Package pkg = new Package(connectrequest);
                _socket.Send(pkg.toBytes());
                _waitdtuconnect = true;

                Logger.Trace("[{0}] ===> {1}", _connectionInfo.DtuSn, pkg.ToString());

                DateTime connectReqTime = DateTime.Now;
                while (_socket != null && _socket.Connected && _waitdtuconnect)
                {
                    TimeSpan ts = DateTime.Now - connectReqTime;
                    if (ts.TotalSeconds > 5)
                    {
                        _connectionInfo.Status = (int)RunningStatus.RS_CONNECTTIMEOUT;
                        Logger.Info("[{0}] connect timeout!", _connectionInfo.DtuSn);

                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return false;
            }
            if (_dtuconnected)
            {
                return true;
            }

            return false;
        }
        private bool _connectPlcRequest()
        {
            try
            {
                //发送连接请求
                byte[] sn = BitConverter.GetBytes(ulong.Parse(_connectionInfo.DtuSn, NumberStyles.AllowHexSpecifier));
                Array.Reverse(sn);
                byte path = 0; //path：连接途径 0 - 串口  1 - 网口
                if (GetType() == typeof(NetTransService))
                {
                    path = 1;
                }
                PlcConnectRequest connectrequest = new PlcConnectRequest(sn, path, _connectionInfo.PlcIp, _connectionInfo.PlcPort);
                Package pkg = new Package(connectrequest);
                _socket.Send(pkg.toBytes());
                _waitplcconnect = true;

                Logger.Trace("===> {0}", pkg.ToString());

                DateTime connectReqTime = DateTime.Now;
                while (_socket != null && _socket.Connected && _waitplcconnect)
                {
                    TimeSpan ts = DateTime.Now - connectReqTime;
                    if (ts.TotalSeconds > 15)
                    {
                        _connectionInfo.Status = (int)RunningStatus.RS_CONNECTTIMEOUT;
                        Logger.Info("[{0}] connect timeout!", _connectionInfo.DtuSn);

                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return false;
            }
            if (_plcconnected)
            {
                return true;
            }

            return false;
        }

        private void doLocalReceive()
        {
            while (_socket != null && _socket.Connected && _dtuconnected && !_shouldStop)
            {
                try
                {
                    byte[] buffer = new byte[Config.BufferSize];
                    int recv = _connector.Receive(ref buffer, buffer.Length);
                    if (recv > 0)
                    {
                        _writeSocket(buffer, recv);
                    }

                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString());
                    return;
                }
            }
        }

        public void _doLocalSend(byte[] buffer, int length)
        {
            try
            {
                byte[] bytes = buffer.Take(length).ToArray();
                Package package = new Package(bytes);

                Logger.Trace("[{0}] <=== {1}", _connectionInfo.DtuSn, package.ToString());

                if (package.Id == Protocol.ID_HEARTBEAT)
                {
                    HeartBeat heartbeat = new HeartBeat(package.getPayload());
                    HeartBeat myheartbeat = new HeartBeat();
                    Package pkg = new Package(myheartbeat);

                    Logger.Trace("[{0}] ===> {1}", _connectionInfo.DtuSn, pkg.ToString());
                    _socket.Send(pkg.toBytes());
                }
                else if (package.Id == Protocol.ID_CONNECTRESPONSE && !_dtuconnected)
                {
                    ConnectResponse response = new ConnectResponse(package.getPayload());
                    if (response.Result == 0)
                    {
                        //连接成功
                        _dtuconnected = true;
                    }
                    else if (response.Result == 1)
                    {
                        //连接失败
                        if (response.Reason == 1)
                        {
                            //DTU不存在
                            _connectionInfo.Status = (int)RunningStatus.RS_DTUNOTFOUND;
                            Logger.Info("[{0}] dtu:[{1}] not found!", _connectionInfo.DtuSn, _connectionInfo.DtuSn);
                        }
                        else if (response.Reason == 2)
                        {
                            //DTU未在线
                            _connectionInfo.Status = (int)RunningStatus.RS_DTUNOTCONNECTED;
                            Logger.Info("[{0}] dtu:[{1}] not online!", _connectionInfo.DtuSn, _connectionInfo.DtuSn);

                        }
                        else if (response.Reason == 3)
                        {
                            //DTU调试中
                            _connectionInfo.Status = (int)RunningStatus.RS_DTUALREADYDEBUGING;
                            Logger.Info("[{0}] dtu:[{1}] is debugging!", _connectionInfo.DtuSn, _connectionInfo.DtuSn);

                        }
                        else if (response.Reason == 4)
                        {
                            //DTU监控中
                            _connectionInfo.Status = (int)RunningStatus.RS_DTUNOTDEBUGMODE;
                            Logger.Info("[{0}] dtu:[{1}] is monitoring!", _connectionInfo.DtuSn, _connectionInfo.DtuSn);

                        }
                    }
                    _waitdtuconnect = false;
                }
                else if (_dtuconnected)
                {
                    if (package.Id == Protocol.ID_PLCCONNECTRESPONSE && !_plcconnected)
                    {
                        PlcConnectResponse response = new PlcConnectResponse(package.getPayload());
                        if (response.Result == 0)
                        {
                            //连接成功
                            _plcconnected = true;
                        }
                        else if (response.Result == 1)
                        {
                        }
                        _waitplcconnect = false;
                    }
                    else if (_plcconnected)
                    {
                        if (package.Id == Protocol.ID_PASSTHROUGH)
                        {
                            PassThrough passthrough = new PassThrough(package.getPayload());

                            MonitorDataManager.instance.Add(_connectionInfo.DtuSn, passthrough.Data, MonitorDataType.MDT_NET);

                            _connector.Send(passthrough.Data, passthrough.Data.Length);

                            Logger.Debug("passthrough: {0}", passthrough.ToString());
                        }
                        if (package.Id == Protocol.ID_COMPASSTHROUGH)
                        {
                            ComPassThrough compassthrough = new ComPassThrough(package.getPayload());

                            MonitorDataManager.instance.Add(_connectionInfo.DtuSn, compassthrough.Data, MonitorDataType.MDT_NET);

                            _connector.Send(compassthrough.Data, compassthrough.Data.Length);

                            Logger.Debug("passthrough: {0}", compassthrough.ToString());
                        }
                        else if (package.Id == Protocol.ID_DEBUGEXCEPTION)
                        {
                            DebugException exception = new DebugException(package.getPayload());
                            if (exception.Reason == 1)
                            {
                                _dtuconnected = false;
                                //DTU监控中
                                _connectionInfo.Status = (int)RunningStatus.RS_CONNECTBREAK;
                                Logger.Info("[{0}] connection break!", _connectionInfo.DtuSn);
                            }


                            Logger.Debug("DebugException: {0}", exception.ToString());

                        }
                    }


                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

        }

    }
}
