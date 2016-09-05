using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePLC.src.comm.protocol
{
    public class ConnectRequest : Payload
    {
        //9.调试连接请求帧(ID= 0x07）
        //------------
        //|   8B     |   
        //------------
        //|  sn      |   
        //------------
        //sn：连接设备的sn

        private byte[] _sn;
        public ConnectRequest(byte[] sn)
        {
            if (sn.Length != 8)
            {
                throw new ArgumentOutOfRangeException("不是有效的sn。");
            }
            _sn = sn;
        }
        public override byte[] toBytes()
        {
            MemoryStream ms = new MemoryStream();
            //sn
            ms.Write(_sn, 0, 8);

            return ms.ToArray();
        }

        public override string ToString()
        {
            return String.Format("sn:{0}", BitConverter.ToString(_sn));
        }
    }
}
