using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePLC.src.comm.protocol
{
    public class PassThrough : Payload
    {
        //8.数据透传帧(ID= 0xFF）
        //-------------------------------
        //|   8B     |      nB           | 
        //--------------------------------
        //|  sn      |    data           |   
        //--------------------------------
        //sn：目的设备的sn
        //data:透传数据

        private byte[] _sn;
        private byte[] _data;
        public byte[] Data { get { return _data; } }

        public PassThrough(byte[] sn, byte[] data)
        {
            _sn = sn;
            _data = data;
        }
        public PassThrough(byte[] bytes)
        {
            if (bytes.Length < 8)
            {
                throw new ArgumentOutOfRangeException("不是有效的透传消息。");
            }

            MemoryStream ms = new MemoryStream(bytes);
            //sn
            _sn = new byte[8];
            ms.Read(_sn, 0, 8);

            //data
            _data = new byte[bytes.Length - 8];
            ms.Read(_data, 0, bytes.Length - 8);
        }
        public override byte[] toBytes()
        {
            MemoryStream ms = new MemoryStream();
            //sn
            ms.Write(_sn, 0, 8);

            //data
            ms.Write(_data, 0, _data.Length);

            return ms.ToArray();
        }

        public override string ToString()
        {
            return String.Format("sn:{0} data:{1}", BitConverter.ToString(_sn), BitConverter.ToString(_data));
        }
    }
}
