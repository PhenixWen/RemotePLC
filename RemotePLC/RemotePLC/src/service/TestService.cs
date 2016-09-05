using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RemotePLC.src.comm;
using RemotePLC.src.comm.protocol;
using RemotePLC.src.ui;

namespace RemotePLC.src.service
{
    public class TestService
    {
        private enum STEP
        {
            UNCHECKED,
            CONNECTSERVER,
            CONNECTSERVEROK,
            CONNECTSERVERFAIL,
            CHECKSERVER,
            CHECKSERVEROK,
            CHECKSERVERFAIL,
            CHECKSERVERTIMEOUT,
            CHECKDTU,
            CHECKDTUCHECKING,
            CHECKDTUOK,
            CHECKDTUFAIL,
            CHECKDTUTIMEOUT,
            CHECKPLC,
            CHECKPLCOK,
            CHECKPLCFAIL,
            CHECKPLCTIMEOUT,
            CHECKPLCNOTSUPPORT
        };

        private ConnectionTestDialog _dlg;
        private Thread testThread;
        private Socket _socket;
        private STEP _step = STEP.UNCHECKED;


        private void setStep(STEP step)
        {
            _step = step;
            Logger.Debug("Connection Test ............ step [{0}]", _step);
            switch (_step)
            {
                case STEP.UNCHECKED:
                    Logger.Info("[{0}] 尚未检测", _step);
                    break;
                case STEP.CONNECTSERVER:
                    Logger.Info("[{0}] 连接服务器", _step);
                    break;
                case STEP.CONNECTSERVEROK:
                    Logger.Info("[{0}] 服务器连接成功", _step);
                    break;
                case STEP.CONNECTSERVERFAIL:
                    Logger.Info("[{0}] 服务器连接失败", _step);
                    break;
                case STEP.CHECKSERVER:
                    Logger.Info("[{0}] 检测连接服务器", _step);
                    break;
                case STEP.CHECKSERVEROK:
                    Logger.Info("[{0}] 检测连接服务器成功", _step);
                    break;
                case STEP.CHECKSERVERFAIL:
                    Logger.Info("[{0}] 检测连接服务器失败", _step);
                    break;
                case STEP.CHECKSERVERTIMEOUT:
                    Logger.Info("[{0}] 检测连接服务器超时", _step);
                    break;
                case STEP.CHECKDTU:
                    Logger.Info("[{0}] 检测连接DTU", _step);
                    break;
                case STEP.CHECKDTUOK:
                    Logger.Info("[{0}] 检测连接DTU成功", _step);
                    break;
                case STEP.CHECKDTUFAIL:
                    Logger.Info("[{0}] 检测连接DTU失败", _step);
                    break;
                case STEP.CHECKDTUTIMEOUT:
                    Logger.Info("[{0}] 检测连接DTU超时", _step);
                    break;
                case STEP.CHECKPLC:
                    Logger.Info("[{0}] 检测连接PLC", _step);
                    break;
                case STEP.CHECKPLCOK:
                    Logger.Info("[{0}] 检测连接PLC成功", _step);
                    break;
                case STEP.CHECKPLCFAIL:
                    Logger.Info("[{0}] 检测连接PLC失败", _step);
                    break;
                case STEP.CHECKPLCTIMEOUT:
                    Logger.Info("[{0}] 检测连接PLC超时", _step);
                    break;
                case STEP.CHECKPLCNOTSUPPORT:
                    Logger.Info("[{0}] 检测连接PLC不支持", _step);
                    break;
                default:
                    break;
            }

        }

        public TestService(ConnectionTestDialog dlg)
        {
            _dlg = dlg;
        }
        public void DoTest()
        {
            if (testThread == null || (testThread != null && !testThread.IsAlive))
            {
                testThread = new Thread(TestThreadWork);
                testThread.Start();
            }
        }
        public void DoClose()
        {
            if (_socket != null && _socket.Connected)
            {
                _socket.Close();
            }
        }
        private void sendPingRequest(byte step)
        {
            byte[] sn = BitConverter.GetBytes(ulong.Parse(_dlg.VCOMInfo.DtuSn, NumberStyles.AllowHexSpecifier));
            Array.Reverse(sn);

            DateTime timestamp = DateTime.Now.ToUniversalTime();
            TimeSpan ts = timestamp.Subtract(DateTime.Parse("1/1/1970"));
            byte[] tag = BitConverter.GetBytes((uint)ts.TotalSeconds);
            Array.Reverse(tag);

            PingRequest pingRequest = new PingRequest(sn, step, tag);
            Package pkg = new Package(pingRequest);
            _socket.Send(pkg.toBytes());
        }

        private PingResponse getPingResponse()
        {
            PingResponse pingResponse = null;
            DateTime startTime = DateTime.Now;
            while (((TimeSpan)(DateTime.Now - startTime)).TotalSeconds < 10)
            {
                byte[] buffer = new byte[Config.BufferSize];
                int recv = _socket.Receive(buffer, buffer.Length, SocketFlags.None);
                if (recv > 0)
                {
                    byte[] bytes = buffer.Take(recv).ToArray();
                    Package package = new Package(bytes);
                    if (package.Id == Protocol.ID_PINGRESPONSE)
                    {
                        pingResponse = new PingResponse(package.getPayload());
                        return pingResponse;
                    }
                }
            }
            Logger.Debug("Connection Test Response Timeout!");
            return null;
        }
        private void TestThreadWork()
        {
            try
            {
                setStep(STEP.CONNECTSERVER);
                _dlg.SetCheckPointState(1, ConnectionTestCheckState.CTCS_CHECK);
                _doConnect();

                if (_socket != null && _socket.Connected)
                {
                    setStep(STEP.CONNECTSERVEROK);
                }
                else
                {
                    setStep(STEP.CONNECTSERVERFAIL);
                    _dlg.SetCheckPointState(1, ConnectionTestCheckState.CTCS_CHECKFAIL);
                    _dlg.SetReason("1.请检查PC的网络连接。\n2.请检查服务器的运行状态。");
                }

                if (_step == STEP.CONNECTSERVEROK)
                {
                    sendPingRequest(1);
                    setStep(STEP.CHECKSERVER);
                }

                if (_step == STEP.CHECKSERVER)
                {
                    PingResponse pingResponse = getPingResponse();

                    if (pingResponse != null)
                    {
                        if (pingResponse.Status == 0 && pingResponse.Step == 1)
                        {
                            setStep(STEP.CHECKSERVEROK);
                            _dlg.SetCheckPointState(1, ConnectionTestCheckState.CTCS_CHECKOK);
                        }
                        else
                        {
                            setStep(STEP.CHECKSERVERFAIL);
                            _dlg.SetCheckPointState(1, ConnectionTestCheckState.CTCS_CHECKFAIL);
                            _dlg.SetReason("请检查服务器的运行状态。");
                        }
                    }
                    else
                    {
                        setStep(STEP.CHECKSERVERTIMEOUT);
                        _dlg.SetCheckPointState(1, ConnectionTestCheckState.CTCS_CHECKFAIL);
                        _dlg.SetReason("请检查服务器的运行状态。");
                    }
                }

                if (_step == STEP.CHECKSERVEROK)
                {
                    sendPingRequest(2);
                    setStep(STEP.CHECKDTU);
                    _dlg.SetCheckPointState(2, ConnectionTestCheckState.CTCS_CHECK);
                }

                if (_step == STEP.CHECKDTU)
                {
                    PingResponse pingResponse = getPingResponse();
                    if (pingResponse != null)
                    {
                        if (pingResponse.Status == 0 && pingResponse.Step == 2)
                        {
                            setStep(STEP.CHECKDTUCHECKING);
                        }
                        else
                        {
                            setStep(STEP.CHECKDTUFAIL);
                            _dlg.SetCheckPointState(2, ConnectionTestCheckState.CTCS_CHECKFAIL);
                            if (pingResponse.Reason == 1)
                            {
                                _dlg.SetReason("DTU未在线。\n1.请检查DTU是否开机。\n2.请检查DTU的网络连接。");
                            }
                            else if (pingResponse.Reason == 2)
                            {
                                _dlg.SetReason("DTU未处于调试模式。请将DTU设置到调试模式。（拨动DTU上的切换开关到调试模式）");
                            }
                            else
                            {
                                _dlg.SetReason("请检查DTU的运行状态。");
                            }
                        }
                    }
                    else
                    {
                        setStep(STEP.CHECKDTUTIMEOUT);
                        _dlg.SetCheckPointState(2, ConnectionTestCheckState.CTCS_CHECKFAIL);
                        _dlg.SetReason("请检查服务器的运行状态。");
                    }
                }

                if (_step == STEP.CHECKDTUCHECKING)
                {
                    PingResponse pingResponse = getPingResponse();
                    if (pingResponse != null)
                    {
                        if (pingResponse.Status == 0 && pingResponse.Step == 3)
                        {
                            setStep(STEP.CHECKDTUOK);
                            _dlg.SetCheckPointState(2, ConnectionTestCheckState.CTCS_CHECKOK);
                        }
                        else
                        {
                            setStep(STEP.CHECKDTUFAIL);
                            _dlg.SetCheckPointState(2, ConnectionTestCheckState.CTCS_CHECKFAIL);
                            if (pingResponse.Reason == 1)
                            {
                                _dlg.SetReason("DTU未在线。\n1.请检查DTU是否开机。\n2.请检查DTU的网络连接。");
                            }
                            else if (pingResponse.Reason == 2)
                            {
                                _dlg.SetReason("DTU未处于调试模式。请将DTU设置到调试模式。（拨动DTU上的切换开关到调试模式）");
                            }
                            else
                            {
                                _dlg.SetReason("请检查DTU的运行状态。");
                            }
                        }
                    }
                    else
                    {
                        setStep(STEP.CHECKDTUTIMEOUT);
                        _dlg.SetCheckPointState(2, ConnectionTestCheckState.CTCS_CHECKFAIL);
                        _dlg.SetReason("请检查服务器的运行状态。");
                    }
                }

                if (_step == STEP.CHECKDTUOK)
                {
#if CHECKPLCSUPPORT
                    sendPingRequest(3);
                    setStep(STEP.CHECKPLC);
                    _dlg.SetCheckPointState(3, ConnectionTestCheckState.CTCS_CHECK);
#else
                    setStep(STEP.CHECKPLCNOTSUPPORT);
                    _dlg.SetCheckPointState(3, ConnectionTestCheckState.CTCS_CHECKNOTSUPPORT);
                    _dlg.SetReason("DTU与PLC之间的连接暂时无法检测。\n在前两项检测成功的情况下，使用PLC的IDE自带的检测功能进行PLC的连接检测。如果发生异常请检查：\n1.请检查PLC是否开机。\n2.请检查DTU到PLC的连接线是否接好。\n3.请检查检查DTU的串口设置是否正确。");
#endif
                }

                if (_step == STEP.CHECKPLC)
                {
                    PingResponse pingResponse = getPingResponse();
                    if (pingResponse != null)
                    {
                        if (pingResponse.Status == 0 && pingResponse.Step == 4)
                        {
                            setStep(STEP.CHECKPLCOK);
                            _dlg.SetCheckPointState(3, ConnectionTestCheckState.CTCS_CHECKOK);
                        }
                        else
                        {
                            setStep(STEP.CHECKPLCFAIL);
                            _dlg.SetCheckPointState(3, ConnectionTestCheckState.CTCS_CHECKFAIL);
                            _dlg.SetReason("DTU与PLC之间的连接暂时无法检测。\n在前两项检测成功的情况下，使用PLC的IDE自带的检测功能进行PLC的连接检测。如果发生异常请检查：\n1.请检查PLC是否开机。\n2.请检查DTU到PLC的连接线是否接好。\n3.请检查检查DTU的串口设置是否正确。");
                        }
                    }
                    else
                    {
                        setStep(STEP.CHECKPLCTIMEOUT);
                        _dlg.SetCheckPointState(3, ConnectionTestCheckState.CTCS_CHECKFAIL);
                        _dlg.SetReason("DTU与PLC之间的连接暂时无法检测。\n在前两项检测成功的情况下，使用PLC的IDE自带的检测功能进行PLC的连接检测。如果发生异常请检查：\n1.请检查PLC是否开机。\n2.请检查DTU到PLC的连接线是否接好。\n3.请检查检查DTU的串口设置是否正确。");
                    }
                }
                Logger.Info("[{0}] 测试结束", _step);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }
        public void _doConnect()
        {
            try
            {
                int port = Config.ServerPort;
                string host = Config.ServerIp;
                ///创建终结点EndPoint
                IPAddress ip = IPAddress.Parse(host);
                IPEndPoint ipe = new IPEndPoint(ip, port);//把ip和端口转化为IPEndpoint实例

                ///创建socket并连接到服务器
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建Socket
                Logger.Info("[{0}] Connecting…", _dlg.VCOMInfo.VComName);
                _socket.Connect(ipe);//连接到服务器
                Logger.Info("[{0}] Socket Connectted!", _dlg.VCOMInfo.VComName);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }
    }
}
