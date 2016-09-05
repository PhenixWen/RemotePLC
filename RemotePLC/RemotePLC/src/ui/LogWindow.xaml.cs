using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RemotePLC.src.comm;

namespace RemotePLC.src.ui
{
    /// <summary>
    /// LogWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LogWindow : Window
    {
        bool _ifNeedRefresh = true;
        public LogWindow()
        {
            InitializeComponent();
        }

        private void logCtrl_LayoutUpdated(object sender, EventArgs e)
        {
            if (_ifNeedRefresh)
            {
                if (logScroll.ScrollableHeight == logScroll.VerticalOffset)
                {
                    logScroll.ScrollToEnd();
                    _ifNeedRefresh = false;
                    return;
                }
            }
            _ifNeedRefresh = true;
        }
    }
}
