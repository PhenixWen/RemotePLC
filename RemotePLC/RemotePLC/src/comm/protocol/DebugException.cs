using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePLC.src.comm.protocol
{
    public class DebugException : Payload
    {
        //11.调试异常(ID= 0x09）
        //-------------------------
        //|   8B      |   1B      |
        //-------------------------
        //|   sn      |  reason   |
        //-------------------------
        //sn：终端的DTU sn
        //reason:异常原因 1=调试中断  2=数据转发失败，未找到远端设备

        private byte[] _sn;
        private byte _reason;
        public byte Reason { get { return _reason; } }

        public DebugException(byte[] bytes)
        {
            if (bytes.Length < 8 + 1)
            {
                throw new ArgumentOutOfRangeException("不是有效的连接应答消息。");
            }

            MemoryStream ms = new MemoryStream(bytes);

            //_sn
            _sn = new byte[8];
            ms.Read(_sn, 0, _sn.Length);

            //reason
            _reason = (byte)ms.ReadByte();
        }
        public override byte[] toBytes()
        {
            MemoryStream ms = new MemoryStream();

            //sn
            ms.Write(_sn, 0, 8);

            //reason
            ms.WriteByte(_reason);

            return ms.ToArray();
        }

        public override string ToString()
        {
            return String.Format("sn:{0} reson:{1}({2})", BitConverter.ToString(_sn), _reason, _reason == 1 ? "调试中断" : _reason == 2 ? "数据转发失败，未找到远端设备" : "null");
        }
    }
}
