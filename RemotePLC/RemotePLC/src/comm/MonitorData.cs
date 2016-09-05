using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePLC.src.comm
{
    public enum MonitorDataType
    {
        MDT_NONE = 0,
        MDT_COM,
        MDT_NET,
        MDT_LOC,
    }
    public class MonitorData : INotifyPropertyChanged
    {
        private int _id;
        private int _tickcount;
        private MonitorDataType _type;
        private byte[] _data;

        public MonitorData(int id, int tickcount, byte[] data, MonitorDataType type)
        {
            _id = id;
            _tickcount = tickcount;
            _type = type;
            _data = data;
        }
        public string color
        {
            get
            {
                if (_type == MonitorDataType.MDT_NET)
                {
                    return "Blue";
                }
                else
                {
                    return "Black";
                }
            }
        }
        public int Id { get { return _id; } }
        public int TickCount { get { return _tickcount; } }

        public string Type
        {
            get
            {
                if (_type == MonitorDataType.MDT_COM)
                {
                    return "COM";
                }
                else if (_type == MonitorDataType.MDT_NET)
                {
                    return "NET";
                }
                else if (_type == MonitorDataType.MDT_LOC)
                {
                    return "LOC";
                }
                else
                {
                    return "";
                }
            }
        }
        public int ByteCount { get { return _data.Count(); } }

        public string ASCII { get { return System.Text.Encoding.Default.GetString(_data); } }
        public string HEX { get { return BitConverter.ToString(_data).Replace("-", " "); } }


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
