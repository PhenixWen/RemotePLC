using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemotePLC.src.comm;

namespace RemotePLC.src.service
{
    public class ComConnector : Connector
    {
        private CommPort _vcom;
        public CommPort VCom { get { return _vcom; } }
        public ComConnector(ConnectionInfo info) : base(info)
        {
            _vcom = new CommPort();
        }

        public override bool Connect()
        {
            try
            {
                if (_vcom.IsOpen)
                {
                    _vcom.Close();
                }
                _vcom.Open(_connectionInfo.VComName4Socket);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return false;
            }
            return true;
        }

        public override bool Disconnect()
        {
            try
            {
                _vcom.Close();
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return false;
            }
            return true;
        }

        public override int Receive(ref byte[] commRead, int NumBytes)
        {
            if (!_vcom.IsOpen)
            {
                return 0;
            }

            int num = 0;
            try
            {
                num = _vcom.Read(ref commRead, NumBytes);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            return num;
        }

        public override int Send(byte[] WriteBytes, int NumBytes)
        {
            if (!_vcom.IsOpen)
            {
                return 0;
            }

            int num = 0;
            try
            {
                num = _vcom.Write(WriteBytes, NumBytes);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            return num;
        }
    }
}
