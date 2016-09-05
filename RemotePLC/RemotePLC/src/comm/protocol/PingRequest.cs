using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePLC.src.comm.protocol
{
    public class PingRequest : Payload
    {
        //11.Ping(ID= 0x0A）
        //-------------------------------------------------------
        //|      8B         |      1B          |       4B       |
        //-------------------------------------------------------
        //|    destSn       |      step        |       tag      |
        //-------------------------------------------------------
        //destSn：宿DTU sn
        //step:  阶段， 1=pc->Server   2=server->dtu   3=dtu->plc
        //tag:  标签

        private byte[] _destSn;
        private byte _step;
        private byte[] _tag;
        public PingRequest(byte[] destSn, byte step, byte[] tag)
        {
            if (destSn.Length != 8)
            {
                throw new ArgumentOutOfRangeException("不是有效的 destSn。");
            }
            if (tag.Length != 4)
            {
                throw new ArgumentOutOfRangeException("不是有效的tag。");
            }
            _destSn = destSn;
            _step = step;
            _tag = tag;
        }
        public override byte[] toBytes()
        {
            MemoryStream ms = new MemoryStream();
            //destSn
            ms.Write(_destSn, 0, 8);
            //step
            ms.WriteByte(_step);
            //tag
            ms.Write(_tag, 0, 4);

            return ms.ToArray();
        }

        public override string ToString()
        {
            return String.Format("destSn:{0} step:{1} tag:{2}", BitConverter.ToString(_destSn), _step, BitConverter.ToString(_tag));
        }
    }
}
