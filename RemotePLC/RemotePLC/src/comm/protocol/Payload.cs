using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePLC.src.comm.protocol
{

    public abstract class Payload
    {
        public override abstract string ToString();
        public abstract byte[] toBytes();
    }
}
