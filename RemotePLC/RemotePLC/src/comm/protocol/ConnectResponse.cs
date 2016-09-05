using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePLC.src.comm.protocol
{
    public class ConnectResponse : Payload
    {
        //10.调试连接响应帧(ID= 0x08）
        //-----------------------
        //|   1B     |   1B     |   
        //-----------------------
        //|  result  |  reason  |
        //-----------------------
        //result：0（成功）1（失败）  
        //reason：result= 1时有效   0=无效，1=DTU不存在，2=DTU未在线，3=DTU调试中，4=DTU监控中

        private byte _result;
        private byte _reason;

        public byte Result { get { return _result; } }
        public byte Reason { get { return _reason; } }

        public ConnectResponse(byte[] bytes)
        {
            if (bytes.Length < 1 + 1)
            {
                throw new ArgumentOutOfRangeException("不是有效的连接应答消息。");
            }

            MemoryStream ms = new MemoryStream(bytes);
            //result
            _result = (byte)ms.ReadByte();

            //reason
            _reason = (byte)ms.ReadByte();
        }
        public override byte[] toBytes()
        {
            MemoryStream ms = new MemoryStream();
            //result
            ms.WriteByte(_result);

            //reason
            ms.WriteByte(_reason);

            return ms.ToArray();
        }

        public override string ToString()
        {
            return String.Format("result:{0} reason:{1}", _result, _reason);
        }
    }
}
