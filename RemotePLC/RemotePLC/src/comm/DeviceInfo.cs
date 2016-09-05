using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePLC.src.comm
{
    public class DeviceInfo : INotifyPropertyChanged
    {
        private string _sn;
        private string _ip;
        private int _port;
        private bool _alive;
        private string _manufacturer;
        private string _plc;
        private string _desc;
        private int _mode;
        private string _debugSn;

        public string color { get { return !_alive ? "LightGray" : _mode == 0 ? "Red" : _debugSn.Length == 0 ? "Green" : "Orange"; } }
        public string sn { set { _sn = value; NotifyPropertyChanged("sn"); } get { return _sn; } }
        public string ip { set { _ip = value; NotifyPropertyChanged("ip"); } get { return _ip; } }
        public int port { set { _port = value; NotifyPropertyChanged("port"); } get { return _port; } }
        public bool alive { set { _alive = value; NotifyPropertyChanged("alive"); NotifyPropertyChanged("linkStatus"); NotifyPropertyChanged("color"); } get { return _alive; } }
        public string linkStatus { get { return _alive ? "在线" : "不在线"; } }
        public string manufacturer { set { _manufacturer = value; NotifyPropertyChanged("manufacturer"); } get { return _manufacturer; } }
        public string plc { set { _plc = value; NotifyPropertyChanged("plc"); } get { return _plc; } }
        public string desc { set { _desc = value; NotifyPropertyChanged("desc"); } get { return _desc; } }
        public int mode { set { _mode = value; NotifyPropertyChanged("mode"); NotifyPropertyChanged("runningMode"); NotifyPropertyChanged("color"); } get { return _mode; } }
        public string debugSn { set { _debugSn = value; NotifyPropertyChanged("debugSn"); NotifyPropertyChanged("runningMode"); NotifyPropertyChanged("color"); } get { return _debugSn; } }
        public string runningMode { get { return _mode == 0 ? "监控模式" : _debugSn.Length == 0 ? "调试模式" : "调试中"; } }

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
