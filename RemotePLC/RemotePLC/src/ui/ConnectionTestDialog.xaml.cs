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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using RemotePLC.src.comm;
using RemotePLC.src.service;

namespace RemotePLC.src.ui
{
    public enum ConnectionTestCheckState
    {
        CTCS_UNCHECKED,
        CTCS_CHECK,
        CTCS_CHECKOK,
        CTCS_CHECKFAIL,
        CTCS_CHECKNOTSUPPORT
    }
    /// <summary>
    /// ConnectionTestDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ConnectionTestDialog : Window
    {
        private ConnectionTestCheckState _point1State = ConnectionTestCheckState.CTCS_UNCHECKED;
        private ConnectionTestCheckState _point2State = ConnectionTestCheckState.CTCS_UNCHECKED;
        private ConnectionTestCheckState _point3State = ConnectionTestCheckState.CTCS_UNCHECKED;

        private ConnectionInfo _connectionInfo;
        public ConnectionInfo VCOMInfo { get { return _connectionInfo; } }

        public ConnectionTestDialog(ConnectionInfo info)
        {
            InitializeComponent();
            _connectionInfo = info;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ServiceManager.instance.DoTest(this, _connectionInfo);
        }

        public void SetCheckPointState(int checkPoint, ConnectionTestCheckState state)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                if (checkPoint == 1)
                {
                    _point1State = state;
                    switch (_point1State)
                    {
                        case ConnectionTestCheckState.CTCS_UNCHECKED:
                            pointbar1.IsIndeterminate = false;
                            pointbar1.Background = Brushes.LightGray;
                            break;
                        case ConnectionTestCheckState.CTCS_CHECK:
                            pointbar1.IsIndeterminate = true;
                            pointbar1.Background = Brushes.LightGray;
                            break;
                        case ConnectionTestCheckState.CTCS_CHECKOK:
                            pointbar1.IsIndeterminate = false;
                            pointbar1.Background = Brushes.LawnGreen;
                            break;
                        case ConnectionTestCheckState.CTCS_CHECKFAIL:
                            pointbar1.IsIndeterminate = false;
                            pointbar1.Background = Brushes.OrangeRed;
                            break;
                        case ConnectionTestCheckState.CTCS_CHECKNOTSUPPORT:
                            pointbar1.IsIndeterminate = false;
                            pointbar1.Background = Brushes.Gold;
                            break;
                        default:
                            break;
                    }
                }
                else if (checkPoint == 2)
                {
                    _point2State = state;
                    switch (_point2State)
                    {
                        case ConnectionTestCheckState.CTCS_UNCHECKED:
                            pointbar2.IsIndeterminate = false;
                            pointbar2.Background = Brushes.LightGray;
                            break;
                        case ConnectionTestCheckState.CTCS_CHECK:
                            pointbar2.IsIndeterminate = true;
                            pointbar2.Background = Brushes.LightGray;
                            break;
                        case ConnectionTestCheckState.CTCS_CHECKOK:
                            pointbar2.IsIndeterminate = false;
                            pointbar2.Background = Brushes.LawnGreen;
                            break;
                        case ConnectionTestCheckState.CTCS_CHECKFAIL:
                            pointbar2.IsIndeterminate = false;
                            pointbar2.Background = Brushes.OrangeRed;
                            break;
                        case ConnectionTestCheckState.CTCS_CHECKNOTSUPPORT:
                            pointbar2.IsIndeterminate = false;
                            pointbar2.Background = Brushes.Gold;
                            break;
                        default:
                            break;
                    }
                }
                else if (checkPoint == 3)
                {
                    _point3State = state;
                    switch (_point3State)
                    {
                        case ConnectionTestCheckState.CTCS_UNCHECKED:
                            pointbar3.IsIndeterminate = false;
                            pointbar3.Background = Brushes.LightGray;
                            break;
                        case ConnectionTestCheckState.CTCS_CHECK:
                            pointbar3.IsIndeterminate = true;
                            pointbar3.Background = Brushes.LightGray;
                            break;
                        case ConnectionTestCheckState.CTCS_CHECKOK:
                            pointbar3.IsIndeterminate = false;
                            pointbar3.Background = Brushes.LawnGreen;
                            break;
                        case ConnectionTestCheckState.CTCS_CHECKFAIL:
                            pointbar3.IsIndeterminate = false;
                            pointbar3.Background = Brushes.OrangeRed;
                            break;
                        case ConnectionTestCheckState.CTCS_CHECKNOTSUPPORT:
                            pointbar3.IsIndeterminate = false;
                            pointbar3.Background = Brushes.Gold;
                            break;
                        default:
                            break;
                    }
                }
                UpdateUI();
            });
        }

        public void SetReason(string reason)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                reasonBlock.Text = reason;
                reasonBlock.Visibility = Visibility.Visible;
            });
        }

        private void UpdateUI()
        {
            if (_point1State == ConnectionTestCheckState.CTCS_UNCHECKED)
            {
                imgOK1.Visibility = Visibility.Hidden;
                imgERR1.Visibility = Visibility.Hidden;
                imgNTC1.Visibility = Visibility.Hidden;
            }
            else if (_point1State == ConnectionTestCheckState.CTCS_CHECKOK)
            {
                imgOK1.Visibility = Visibility.Visible;
                imgERR1.Visibility = Visibility.Hidden;
                imgNTC1.Visibility = Visibility.Hidden;
            }
            else if (_point1State == ConnectionTestCheckState.CTCS_CHECKFAIL)
            {
                imgOK1.Visibility = Visibility.Hidden;
                imgERR1.Visibility = Visibility.Visible;
                imgNTC1.Visibility = Visibility.Hidden;
            }
            else if (_point1State == ConnectionTestCheckState.CTCS_CHECKNOTSUPPORT)
            {
                imgOK1.Visibility = Visibility.Hidden;
                imgERR1.Visibility = Visibility.Hidden;
                imgNTC1.Visibility = Visibility.Visible;
            }

            if (_point2State == ConnectionTestCheckState.CTCS_UNCHECKED)
            {
                imgOK2.Visibility = Visibility.Hidden;
                imgERR2.Visibility = Visibility.Hidden;
                imgNTC2.Visibility = Visibility.Hidden;
            }
            else if (_point2State == ConnectionTestCheckState.CTCS_CHECKOK)
            {
                imgOK2.Visibility = Visibility.Visible;
                imgERR2.Visibility = Visibility.Hidden;
                imgNTC2.Visibility = Visibility.Hidden;
            }
            else if (_point2State == ConnectionTestCheckState.CTCS_CHECKFAIL)
            {
                imgOK2.Visibility = Visibility.Hidden;
                imgERR2.Visibility = Visibility.Visible;
                imgNTC2.Visibility = Visibility.Hidden;
            }
            else if (_point2State == ConnectionTestCheckState.CTCS_CHECKNOTSUPPORT)
            {
                imgOK2.Visibility = Visibility.Hidden;
                imgERR2.Visibility = Visibility.Hidden;
                imgNTC2.Visibility = Visibility.Visible;
            }

            if (_point3State == ConnectionTestCheckState.CTCS_UNCHECKED)
            {
                imgOK3.Visibility = Visibility.Hidden;
                imgERR3.Visibility = Visibility.Hidden;
                imgNTC3.Visibility = Visibility.Hidden;
            }
            else if (_point3State == ConnectionTestCheckState.CTCS_CHECKOK)
            {
                imgOK3.Visibility = Visibility.Visible;
                imgERR3.Visibility = Visibility.Hidden;
                imgNTC3.Visibility = Visibility.Hidden;
            }
            else if (_point3State == ConnectionTestCheckState.CTCS_CHECKFAIL)
            {
                imgOK3.Visibility = Visibility.Hidden;
                imgERR3.Visibility = Visibility.Visible;
                imgNTC3.Visibility = Visibility.Hidden;
            }
            else if (_point3State == ConnectionTestCheckState.CTCS_CHECKNOTSUPPORT)
            {
                imgOK3.Visibility = Visibility.Hidden;
                imgERR3.Visibility = Visibility.Hidden;
                imgNTC3.Visibility = Visibility.Visible;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ServiceManager.instance.StopTest();
        }
    }
}
