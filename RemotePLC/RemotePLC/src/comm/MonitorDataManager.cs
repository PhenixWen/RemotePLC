using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;
using RemotePLC.src.ui;

namespace RemotePLC.src.comm
{
    public class MonitorDataManager
    {

        private string _dtuSn;

        DataMonitorDialog _dlg = null;
        private MonitorDataManager()
        {
            _dtuSn = "";
        }

        public static readonly MonitorDataManager instance = new MonitorDataManager();
        //从Handle中获取Window对象
        static Window GetWindowFromHwnd(IntPtr hwnd)
        {
            return (Window)HwndSource.FromHwnd(hwnd).RootVisual;
        }

        //GetForegroundWindow API
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        //调用GetForegroundWindow然后调用GetWindowFromHwnd
        static Window GetTopWindow()
        {
            var hwnd = GetForegroundWindow();
            if (hwnd == null)
                return null;

            return GetWindowFromHwnd(hwnd);
        }


        public void StartMonitoring(string dtusn)
        {
            if (dtusn == null)
            {
                return;
            }

            _dlg = null;
            _dtuSn = dtusn;

            foreach (Window win in App.Current.Windows)
            {
                if (win.GetType() == typeof(DataMonitorDialog))
                {
                    _dlg = win as DataMonitorDialog;
                    break;
                }
            }
            if (_dlg == null)
            {
                _dlg = new DataMonitorDialog();
            }
            _dlg.Owner = GetTopWindow();
            _dlg.Show();
            _dlg.Activate();
        }

        public void Add(string dtusn, byte[] data, MonitorDataType type)
        {
            if (_dlg != null && _dlg.IsVisible)
            {
                if (_dtuSn.CompareTo(dtusn) == 0)
                {
                    _dlg.Add(data, type);
                }
            }
        }
    }
}
