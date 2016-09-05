using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RemotePLC.src.comm.protocol
{
    public class PlcConnectRequest : Payload
    {
        //18.PLC连接请求(ID= 0x12）
        //-------------------------------------------------
        //|    8B     |    1B     |    4B     |    4B     |
        //-------------------------------------------------
        //|    sn     |    path   |    ip     |    port   |
        //-------------------------------------------------
        //sn：目的设备的sn
        //path：连接途径 0 - 串口  1 - 网口
        //ip：PLC的IP地址 如 192.168.50.99 为 0xC0A83263
        //port：PLC网口调试服务端口

        private byte[] _sn;
        private byte _path;
        private byte[] _ip;
        private Int32 _port;
        public PlcConnectRequest(byte[] sn, byte path, string ip, int port)
        {
            if (sn.Length != 8)
            {
                throw new ArgumentOutOfRangeException("不是有效的sn。");
            }

            IPAddress ipaddr;
            if (!IPAddress.TryParse(ip, out ipaddr))
            {
                throw new ArgumentOutOfRangeException("不是有效的ip地址。");
            }

            _sn = sn;
            _path = path;
            _ip = ipaddr.GetAddressBytes();

            if (_ip.Length != 4)
            {
                throw new ArgumentOutOfRangeException("不是有效的IPv4地址。");
            }

            _port = port;
        }
        public override byte[] toBytes()
        {
            MemoryStream ms = new MemoryStream();
            //sn
            ms.Write(_sn, 0, 8);

            //path
            ms.WriteByte(_path);

            //ip
            ms.Write(_ip, 0, 4);

            //port
            byte[] port = BitConverter.GetBytes(_port);
            Array.Reverse(port);
            ms.Write(port, 0, 4);

            return ms.ToArray();
        }

        public override string ToString()
        {
            IPAddress ipaddr = new IPAddress(_ip);
            return String.Format("sn:{0} path:{1} ip:{2} port:{3}", BitConverter.ToString(_sn), _path, ipaddr.ToString(), _port);
        }
    }
}
