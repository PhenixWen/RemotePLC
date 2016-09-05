using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePLC.src.comm.protocol
{
    public static class Protocol
    {
        public const int ID_HEARTBEAT = 0x00;
        public const int ID_PASSTHROUGH = 0xFF;
        public const int ID_CONNECTREQUEST = 0x07;
        public const int ID_CONNECTRESPONSE = 0x08;
        public const int ID_DEBUGEXCEPTION = 0x09;
        public const int ID_PINGREQUEST = 0x0A;
        public const int ID_PINGRESPONSE = 0x0B;
        public const int ID_EXCEPTION = 0x0C;
        public const int ID_VERSIONINFO = 0x0D;
        public const int ID_UPDATEINFO = 0x0E;
        public const int ID_COMPASSTHROUGH = 0x0F;
        public const int ID_DTUIPREQUEST = 0x10;
        public const int ID_DTUIPRESPONSE = 0x11;
        public const int ID_PLCCONNECTREQUEST = 0x12;
        public const int ID_PLCCONNECTRESPONSE = 0x13;
    }
}
