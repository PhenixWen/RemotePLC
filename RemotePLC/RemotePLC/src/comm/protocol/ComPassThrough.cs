using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePLC.src.comm.protocol
{
    public class ComPassThrough : Payload
    {
        //16.带串口参数的数据透传帧(ID= 0x0F）
        //----------------------------------------------------------------------------------------------------------
        //|   8B      |     1B       |      4B        |     1B       |    1B    |     1B       |      nB           | 
        //----------------------------------------------------------------------------------------------------------
        //|   sn      |   syncflag   |    baudrate    | bytesize     | parity   | stopbits     |    data           |   
        //----------------------------------------------------------------------------------------------------------
        //sn：目的设备的sn
        //syncflag：同步波特率 1-同步 0-不同步
        //baudrate：波特率
        //bytesize：数据位
        //parity：校验码 0 - 无校验  1 - 奇校验  2 - 偶校验  3 - 标记校验  4 - 空格校验
        //stopbits：停止位 0 - 1位  1 - 1.5位  2 - 2位
        //data：透传数据

        private byte[] _sn;
        private byte _syncflag;
        private Int32 _baudrate;
        private byte _bytesize;
        private byte _parity;
        private byte _stopbits;
        private byte[] _data;

        private const int LEN_sn = 8;
        private const int LEN_syncflag = 1;
        private const int LEN_baudrate = 4;
        private const int LEN_bytesize = 1;
        private const int LEN_parity = 1;
        private const int LEN_stopbits = 1;
        public byte[] Data { get { return _data; } }

        public ComPassThrough(byte[] sn, byte syncflag, int baudrate, byte bytesize, byte parity, byte stopbits, byte[] data)
        {
            _sn = sn;
            _syncflag = syncflag;
            _baudrate = baudrate;
            _bytesize = bytesize;
            _parity = parity;
            _stopbits = stopbits;
            _data = data;
        }
        public ComPassThrough(byte[] bytes)
        {
            if (bytes.Length < LEN_sn + LEN_syncflag + LEN_baudrate + LEN_bytesize + LEN_parity + LEN_stopbits)
            {
                throw new ArgumentOutOfRangeException("不是有效的透传消息。");
            }

            MemoryStream ms = new MemoryStream(bytes);
            //sn
            _sn = new byte[LEN_sn];
            ms.Read(_sn, 0, _sn.Length);

            //syncflag
            _syncflag = (byte)ms.ReadByte();

            //baudrate
            byte[] baudrate = new byte[LEN_baudrate];
            ms.Read(baudrate, 0, baudrate.Length);
            Array.Reverse(baudrate);
            _baudrate = BitConverter.ToInt32(baudrate, 0);

            //bytesize
            _bytesize = (byte)ms.ReadByte();

            //parity
            _parity = (byte)ms.ReadByte();

            //stopbits
            _stopbits = (byte)ms.ReadByte();

            //data
            _data = new byte[bytes.Length - LEN_sn - LEN_syncflag - LEN_baudrate - LEN_bytesize - LEN_parity - LEN_stopbits];
            ms.Read(_data, 0, _data.Length);
        }
        public override byte[] toBytes()
        {
            MemoryStream ms = new MemoryStream();
            //sn
            ms.Write(_sn, 0, 8);

            //syncflag
            ms.WriteByte(_syncflag);

            //baudrate
            byte[] baudrate = BitConverter.GetBytes(_baudrate);
            Array.Reverse(baudrate);
            ms.Write(baudrate, 0, 4);

            //bytesize
            ms.WriteByte(_bytesize);

            //parity
            ms.WriteByte(_parity);

            //stopbits
            ms.WriteByte(_stopbits);

            //data
            ms.Write(_data, 0, _data.Length);

            return ms.ToArray();
        }

        public override string ToString()
        {
            return String.Format("sn:{0} baudrate:{1} bytesize:{2} parity:{3} stopbits:{4} data:{5}", BitConverter.ToString(_sn), _baudrate, _bytesize, _parity, _stopbits, BitConverter.ToString(_data));
        }
    }
}
