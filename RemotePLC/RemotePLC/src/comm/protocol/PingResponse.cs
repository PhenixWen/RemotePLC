using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePLC.src.comm.protocol
{
    public class PingResponse : Payload
    {
        //12.Ping响应(ID= 0x0B）
        //-----------------------------------------------------------------------------------------------------------------
        //|      8B         |   1B          |   1B            |      1B         |      1B          |         4B       |
        //-----------------------------------------------------------------------------------------------------------------
        //|    destSn       |   src         |   step          |    status       |     reason       |         tag      |
        //--------------------------------------------------------------------------------------------------------------
        //destSn：宿DTU sn
        //src:  响应来源，0=server  1=dtu
        //step：阶段，1=server回pc 2=server转发给dtu后回pc，3=dtu回pc 4=plc回pc
        //status： 状态，0=成功 1=失败
        //reason：0无效，大于0，具体原因待定1= DTU未在线 2=DTU未处于调试模式
        //tag:  标签

        private byte[] _destSn;
        private byte _src;
        private byte _step;
        private byte _status;
        private byte _reason;
        private byte[] _tag;

        public byte Step { get { return _step; } }
        public byte Status { get { return _status; } }
        public byte Reason { get { return _reason; } }

        public PingResponse(byte[] bytes)
        {
            if (bytes.Length < 8 + 1 + 1 + 1 + 1 + 4)
            {
                throw new ArgumentOutOfRangeException("不是有效的ping应答消息。");
            }

            MemoryStream ms = new MemoryStream(bytes);

            //destSn
            _destSn = new byte[8];
            ms.Read(_destSn, 0, _destSn.Length);

            //src
            _src = (byte)ms.ReadByte();

            //step
            _step = (byte)ms.ReadByte();

            //status
            _status = (byte)ms.ReadByte();

            //reason
            _reason = (byte)ms.ReadByte();

            //tag
            _tag = new byte[4];
            ms.Read(_tag, 0, _tag.Length);
        }
        public override byte[] toBytes()
        {
            MemoryStream ms = new MemoryStream();

            //destSn
            ms.Write(_destSn, 0, 8);

            //src
            ms.WriteByte(_src);

            //step
            ms.WriteByte(_step);

            //status
            ms.WriteByte(_status);

            //reason
            ms.WriteByte(_reason);

            //tag
            ms.Write(_tag, 0, 4);

            return ms.ToArray();
        }

        public override string ToString()
        {
            return String.Format(" destSn:{0}, src:{1}, step:{2}, status:{3}, reason:{4}, tag:{5}", BitConverter.ToString(_destSn), _src, _step, _status, _reason, BitConverter.ToString(_tag));
        }
    }
}
