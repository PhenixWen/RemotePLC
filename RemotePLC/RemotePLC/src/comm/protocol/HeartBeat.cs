using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePLC.src.comm.protocol
{
    public class HeartBeat : Payload
    {
        //1.心跳帧（ID=0x00)  DTU<--> Server
        //------------------------------------
        //|   1B   |       4B     |     1B   |
        //------------------------------------
        //|  value |   timestamp  |   status | 
        //------------------------------------
        //value：DTU --> Server value = 0x01     Server --> DTU value = 0x02
        //timestamp: 时间戳，DTU可以不填
        //status: DTU[busy(0)、free(1)、availably(2)、unavailably(3)、error(4)...] Server[success(0)、failure(1)...] debug(0xFF)

        private byte _value;
        private DateTime _timestamp;
        private byte _status;
        public HeartBeat()
        {
            _value = 3;
            _timestamp = DateTime.Now.ToUniversalTime();
            _status = 0;
        }
        public HeartBeat(byte[] bytes)
        {
            if (bytes.Length < 1 + 4 + 1)
            {
                throw new ArgumentOutOfRangeException("不是有效的心跳消息。");
            }

            MemoryStream ms = new MemoryStream(bytes);
            //value
            _value = (byte)ms.ReadByte();

            //timestamp
            byte[] timestamp = new byte[4];
            ms.Read(timestamp, 0, timestamp.Length);
            Array.Reverse(timestamp);
            uint utimestamp = BitConverter.ToUInt32(timestamp, 0);
            _timestamp = DateTime.Parse("1/1/1970").AddSeconds(utimestamp);

            //status
            _status = (byte)ms.ReadByte();
        }
        public override byte[] toBytes()
        {
            MemoryStream ms = new MemoryStream();
            //value
            ms.WriteByte(_value);
            //timestamp
            TimeSpan ts = _timestamp.Subtract(DateTime.Parse("1/1/1970"));
            byte[] timestamp = BitConverter.GetBytes((uint)ts.TotalSeconds);
            Array.Reverse(timestamp);
            ms.Write(timestamp, 0, 4);
            //status
            ms.WriteByte(_status);

            return ms.ToArray();
        }

        public override string ToString()
        {
            return String.Format("value:{0} timestamp:{1} status:{2}", _value, _timestamp.ToLocalTime().ToString(), _status);
        }
    }
}
