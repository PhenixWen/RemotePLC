using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RemotePLC.src.comm;
using RemotePLC.src.ui;

namespace RemotePLC.src.service
{
    public class ServiceManager
    {
        private BaseService _currentService = null;
        private TestService _testService = null;

        private ServiceManager()
        {
        }
        public static readonly ServiceManager instance = new ServiceManager();

        public bool Start(ConnectionInfo info)
        {
            if (_currentService != null)
            {
                _currentService.DoStop();
            }

            if (info.NetProtocalType == (int)NetProtocal.NP_SERIALPORT_TRANSMISSION)
            {
                _currentService = new ComTransService(info);
                return _currentService.DoStart();
            }
            else if (info.NetProtocalType == (int)NetProtocal.NP_NETPORT_TRANSMISSION)
            {
                _currentService = new NetTransService(info);
                return _currentService.DoStart();
            }
            return false;
        }
        public bool Stop()
        {
            if (_currentService != null)
            {
                _currentService.DoStop();
                _currentService = null;
            }
            
            return true;
        }
        public void DoTest(ConnectionTestDialog dlg, ConnectionInfo info)
        {
            Stop();

            if (_testService != null)
            {
                _testService.DoClose();
            }

            _testService = new TestService(dlg);

            _testService.DoTest();
        }
        public void StopTest()
        {
            if (_testService != null)
            {
                _testService.DoClose();
            }
        }
    }
}
