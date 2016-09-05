using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePLC.src.comm
{
    public enum NetProtocal
    {
        NP_NETPORT_TRANSMISSION,
        NP_SERIALPORT_TRANSMISSION
    }

    public enum RunningStatus
    {
        RS_UNCONNECTED,
        RS_CONNECTED,
        RS_LISTENING,
        RS_CONNECTING,
        RS_CONNECTFAIL,
        RS_CONNECTBREAK,
        RS_CONNECTTIMEOUT,
        RS_DTUNOTFOUND,
        RS_DTUNOTCONNECTED,
        RS_DTUALREADYDEBUGING,
        RS_DTUNOTDEBUGMODE,
        RS_CONNECTSERVER,
        RS_CONNECTDTU,
        RS_CONNECTPLC,
        RS_SERVERCONNECTFAIL,
        RS_PLCCONNECTFAIL,
        RS_WAITIDECONNECT,
        RS_IDECONNECTFAIL,
        //UNDEFINED,
    }
    public class ConnectionInfo : INotifyPropertyChanged
    {
        private string _desc;
        private string _dtuSn;
        private int _netProtocalType;
        private string _destIp;
        private int _destPort;
        private int _localPort;
        private int _status;

        #region 虚拟串口相关参数
        private string _vcomName;
        private string _vcomName4Socket;
        #endregion


        #region 网口透传相关参数
        private string _plcip;
        private int _plcport;
        private int _serviceport;
        #endregion

        public string Desc { get { return _desc; } set { _desc = value; NotifyPropertyChanged("Desc"); } }
        public string VComName { get { return _vcomName; } set { _vcomName = value; NotifyPropertyChanged("VComName"); } }
        public string VComName4Socket { get { return _vcomName4Socket; } set { _vcomName4Socket = value; NotifyPropertyChanged("VComName4Socket"); } }
        public string DtuSn { get { return _dtuSn; } set { _dtuSn = value; NotifyPropertyChanged("DtuSn"); } }
        public int NetProtocalType { get { return _netProtocalType; } set { _netProtocalType = value; NotifyPropertyChanged("NetProtocalType"); } }
        public string DestIp { get { return _destIp; } set { _destIp = value; NotifyPropertyChanged("DestIp"); } }
        public int DestPort { get { return _destPort; } set { _destPort = value; NotifyPropertyChanged("DestPort"); } }
        public int LocalPort { get { return _localPort; } set { _localPort = value; NotifyPropertyChanged("LocalPort"); } }
        public int Status { get { return _status; } set { _status = value; NotifyPropertyChanged("StatusText"); } }
        public string PlcIp { get { return _plcip; } set { _plcip = value; NotifyPropertyChanged("PlcIp"); } }
        public int PlcPort { get { return _plcport; } set { _plcport = value; NotifyPropertyChanged("PlcPort"); } }
        public int ServicePort { get { return _serviceport; } set { _serviceport = value; NotifyPropertyChanged("ServicePort"); } }
        public String StatusText
        {
            get
            {
                if (_status == (int)RunningStatus.RS_UNCONNECTED) { return "未连接"; }
                else if (_status == (int)RunningStatus.RS_CONNECTED) { return "已连接"; }
                else if (_status == (int)RunningStatus.RS_LISTENING) { return "监听中"; }
                else if (_status == (int)RunningStatus.RS_CONNECTING) { return "连接中"; }
                else if (_status == (int)RunningStatus.RS_CONNECTFAIL) { return "连接失败"; }
                else if (_status == (int)RunningStatus.RS_CONNECTBREAK) { return "连接断开"; }
                else if (_status == (int)RunningStatus.RS_CONNECTTIMEOUT) { return "连接超时"; }
                else if (_status == (int)RunningStatus.RS_DTUNOTFOUND) { return "DTU不存在"; }
                else if (_status == (int)RunningStatus.RS_DTUNOTCONNECTED) { return "DTU未在线"; }
                else if (_status == (int)RunningStatus.RS_DTUALREADYDEBUGING) { return "DTU调试中"; }
                else if (_status == (int)RunningStatus.RS_DTUNOTDEBUGMODE) { return "DTU不在调试状态"; }
                else if (_status == (int)RunningStatus.RS_CONNECTSERVER) { return "正在连接服务器"; }
                else if (_status == (int)RunningStatus.RS_CONNECTDTU) { return "正在连接DTU"; }
                else if (_status == (int)RunningStatus.RS_CONNECTPLC) { return "正在连接PLC"; }
                else if (_status == (int)RunningStatus.RS_SERVERCONNECTFAIL) { return "服务器连接失败"; }
                else if (_status == (int)RunningStatus.RS_PLCCONNECTFAIL) { return "PLC连接失败"; }
                else if (_status == (int)RunningStatus.RS_WAITIDECONNECT) { return "等待IDE连接"; }
                else if (_status == (int)RunningStatus.RS_IDECONNECTFAIL) { return "IDE连接失败"; }
                return "";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
