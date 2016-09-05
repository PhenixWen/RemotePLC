using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace RemotePLC.src.comm.protocol
{
    //帧协议格式：
    //--------------------------------------------------------------------------
    //|  2B |   2B   |  1B |      8B     |             n B             |  2B   |
    //--------------------------------------------------------------------------
    //| stx |   len  |  id  |     sn     |         payload             |  end  |
    //--------------------------------------------------------------------------
    public class Package
    {
        private readonly byte[] _stx = { 0xEF, 0xEF };
        private readonly byte[] _end = { 0xEE, 0xEE };

        private byte _id;
        private byte[] _sn;
        private byte[] _payload;

        public byte Id { get { return _id; } }

        public Package(Payload payload)
        {
            if (payload.GetType() == typeof(HeartBeat))
            {
                _id = Protocol.ID_HEARTBEAT;
            }
            else if (payload.GetType() == typeof(PassThrough))
            {
                _id = Protocol.ID_PASSTHROUGH;
            }
            else if (payload.GetType() == typeof(ConnectRequest))
            {
                _id = Protocol.ID_CONNECTREQUEST;
            }
            else if (payload.GetType() == typeof(ConnectResponse))
            {
                _id = Protocol.ID_CONNECTRESPONSE;
            }
            else if (payload.GetType() == typeof(DebugException))
            {
                _id = Protocol.ID_DEBUGEXCEPTION;
            }
            else if (payload.GetType() == typeof(PingRequest))
            {
                _id = Protocol.ID_PINGREQUEST;
            }
            else if (payload.GetType() == typeof(PingResponse))
            {
                _id = Protocol.ID_PINGRESPONSE;
            }
            else if (payload.GetType() == typeof(ComPassThrough))
            {
                _id = Protocol.ID_COMPASSTHROUGH;
            }
            //else if (payload.GetType() == typeof(DtuIpRequest))
            //{
            //    _id = Protocol.ID_DTUIPREQUEST;
            //}
            //else if (payload.GetType() == typeof(DtuIpResponse))
            //{
            //    _id = Protocol.ID_DTUIPRESPONSE;
            //}
            else if (payload.GetType() == typeof(PlcConnectRequest))
            {
                _id = Protocol.ID_PLCCONNECTREQUEST;
            }
            else if (payload.GetType() == typeof(PlcConnectResponse))
            {
                _id = Protocol.ID_PLCCONNECTRESPONSE;
            }
            else
            {
                throw new ArgumentOutOfRangeException("不是有效的数据包。");
            }
            string mac = Computer.instance.MacAddress;

            Regex reg = new Regex("(.+):(.+):(.+):(.+):(.+):(.+)");
            Match match = reg.Match(mac);
            if (match.Success && match.Groups.Count > 6)
            {
                int value1 = 0, value2 = 0, value3 = 0, value4 = 0, value5 = 0, value6 = 0;
                if (int.TryParse(match.Groups[1].Value, NumberStyles.AllowHexSpecifier, null, out value1) &&
                    int.TryParse(match.Groups[2].Value, NumberStyles.AllowHexSpecifier, null, out value2) &&
                    int.TryParse(match.Groups[3].Value, NumberStyles.AllowHexSpecifier, null, out value3) &&
                    int.TryParse(match.Groups[4].Value, NumberStyles.AllowHexSpecifier, null, out value4) &&
                    int.TryParse(match.Groups[5].Value, NumberStyles.AllowHexSpecifier, null, out value5) &&
                    int.TryParse(match.Groups[6].Value, NumberStyles.AllowHexSpecifier, null, out value6))
                {
                    _sn = new byte[] { 0x00, 0x00, (byte)value1, (byte)value2, (byte)value3, (byte)value4, (byte)value5, (byte)value6 };
                }
                else
                {
                    _sn = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, };
                }

            }
            else
            {
                _sn = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, };
            }

            _payload = payload.toBytes();
        }

        public Package(byte[] bytes)
        {
            if (bytes.Length < 2 + 2 + 1 + 8 + 2)
            {
                throw new ArgumentOutOfRangeException("不是有效的数据包。");
            }

            MemoryStream ms = new MemoryStream(bytes);
            byte[] stx = new byte[2];
            ms.Read(stx, 0, stx.Length);

            if (BitConverter.ToUInt16(stx, 0) != BitConverter.ToUInt16(_stx, 0))
            {
                throw new ArgumentOutOfRangeException("不是有效的数据包。");
            }

            byte[] len = new byte[2];
            ms.Read(len, 0, len.Length);
            Array.Reverse(len);
            ushort ulen = BitConverter.ToUInt16(len, 0);

            byte id = (byte)ms.ReadByte();

            byte[] sn = new byte[8];
            ms.Read(sn, 0, sn.Length);

            int payloadlen = ulen - 1 - 8 - 2;
            byte[] payload = new byte[payloadlen];
            ms.Read(payload, 0, payload.Length);

            byte[] end = new byte[2];
            ms.Read(end, 0, end.Length);

            if (BitConverter.ToUInt16(end, 0) != BitConverter.ToUInt16(_end, 0))
            {
                Logger.Error("bytes Error!\n[{0}]:{1}", bytes.Length, BitConverter.ToString(bytes));
                throw new ArgumentOutOfRangeException("不是有效的数据包。");
            }
            _id = id;
            _payload = payload;
            _sn = sn;
        }

        public byte[] getPayload()
        {
            return _payload;
        }

        public byte[] toBytes()
        {
            MemoryStream ms = new MemoryStream();
            //stx
            ms.Write(_stx, 0, _stx.Length);

            //len
            //len = id + sn + payload + end
            byte[] len = BitConverter.GetBytes((ushort)(1 + 8 + _payload.ToArray().Length + _end.Length));
            Array.Reverse(len);
            ms.Write(len, 0, len.Length);

            //id
            ms.WriteByte(_id);

            //sn
            ms.Write(_sn, 0, 8);

            //payload
            byte[] payload = _payload.ToArray();
            ms.Write(payload, 0, payload.Length);

            //end
            ms.Write(_end, 0, _end.Length);

            return ms.ToArray();
        }

        public override string ToString()
        {
            return BitConverter.ToString(toBytes());
        }
    }
}
