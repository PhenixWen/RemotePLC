using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
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
using RemotePLC.src.service;

namespace RemotePLC.src.ui
{
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                if (value.ToString().CompareTo("LightGray") == 0)
                {
                    return Brushes.LightGray;
                }
                else if (value.ToString().CompareTo("Green") == 0)
                {
                    return Brushes.Green;
                }
                else if (value.ToString().CompareTo("Red") == 0)
                {
                    return Brushes.Red;
                }
                else if (value.ToString().CompareTo("Orange") == 0)
                {
                    return Brushes.Orange;
                }
                else if (value.ToString().CompareTo("Blue") == 0)
                {
                    return Brushes.Blue;
                }
                else if (value.ToString().CompareTo("LightBlue") == 0)
                {
                    return Brushes.LightBlue;
                }
                else if (value.ToString().CompareTo("White") == 0)
                {
                    return Brushes.White;
                }
                else if (value.ToString().CompareTo("LightYellow") == 0)
                {
                    return Brushes.LightYellow;
                }
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    /// <summary>
    /// GuideWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GuideWindow : Window
    {
        private ConnectionInfo _connectionInfo = new ConnectionInfo();
        private LogWindow _logWin = null;
        private System.Windows.Forms.NotifyIcon _notifyIcon = null;

        //[DllImport("TestDll.dll", CallingConvention = CallingConvention.Cdecl)]
        //private static extern int TestDll(int a, int b);

        public GuideWindow()
        {
            InitializeComponent();

            InitialTray();
        }
        private void InitialTray()
        {

            //设置托盘的各个属性
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.BalloonTipText = "程序开始运行";
            _notifyIcon.Text = "PLC远程调试助手1.0";
            _notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            _notifyIcon.Visible = true;
            _notifyIcon.ShowBalloonTip(2000);
            _notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(notifyIcon_MouseClick);

            //退出菜单项
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("退出");
            exit.Click += new EventHandler(exit_Click);

            //关联托盘控件
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { exit };
            _notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);

            //窗体状态改变时候触发
            this.StateChanged += new EventHandler(SysTray_StateChanged);
        }
        private void SysTray_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Visibility = Visibility.Hidden;
            }
        }
        private void notifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (this.Visibility == Visibility.Visible)
                {
                    this.Visibility = Visibility.Hidden;
                }
                else
                {
                    this.Visibility = Visibility.Visible;
                    this.Activate();
                }
            }
        }
        private void exit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                Button btn = sender as Button;
                if (btn.Name.CompareTo("btnVComAdd") == 0)
                {
                    AddVComPopup popup = new AddVComPopup();
                    popup.Owner = this;
                    popup.ShowInTaskbar = false;
                    if (popup.ShowDialog() == true)
                    {
                        vcomlist.ItemsSource = VComManager.instance.GetComList();
                    }
                }
                else if (btn.Name.CompareTo("btnVComDel") == 0)
                {
                    if (vcomlist.SelectedItem is VCom)
                    {
                        VCom vcom = vcomlist.SelectedItem as VCom;
                        MessageBoxResult result = MessageBox.Show("确定删除虚拟串口吗？", "RemotePLC", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                        if (result == MessageBoxResult.OK)
                        {
                            Cursor = Cursors.Wait;
                            bool ret = VComManager.instance.DelVCom(vcom.VComName);
                            Cursor = Cursors.Arrow;
                            if (ret)
                            {
                                vcomlist.ItemsSource = VComManager.instance.GetComList();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("请选择一个虚拟串口！", "RemotePLC", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else if (btn.Name.CompareTo("btnVComDelAll") == 0)
                {
                    MessageBoxResult result = MessageBox.Show("确定删除全部虚拟串口吗？", "RemotePLC", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    if (result == MessageBoxResult.OK)
                    {
                        Cursor = Cursors.Wait;
                        bool ret = VComManager.instance.DelAllVCom();
                        Cursor = Cursors.Arrow;
                        if (ret)
                        {
                            vcomlist.ItemsSource = VComManager.instance.GetComList();
                        }
                    }
                }
                else if (btn.Name.CompareTo("btnVComLastStep") == 0)
                {
                    tabControl.SelectedIndex = 0;
                }
                else if (btn.Name.CompareTo("btnVComNextStep") == 0)
                {
                    int tabIndex = connectionTab.SelectedIndex;

                    if (tabIndex == 0)
                    {
                        int port = 0;
                        if (int.TryParse(plcporttext.Text, out port))
                        {
                            _connectionInfo.NetProtocalType = (int)NetProtocal.NP_NETPORT_TRANSMISSION;
                            _connectionInfo.PlcIp = plciptext.Text;
                            _connectionInfo.PlcPort = port;

                            Config.PlcIp = _connectionInfo.PlcIp;
                            Config.PlcPort = _connectionInfo.PlcPort;
                            Config.Save();

                            tabControl.SelectedIndex = 2;
                        }
                        else
                        {
                            MessageBox.Show("请输入PLC服务端口！", "RemotePLC", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }

                    }
                    else if (tabIndex == 1)
                    {
                        VCom vcom = vcomlist.SelectedItem as VCom;
                        if (vcom == null)
                        {
                            MessageBox.Show("请选择一个虚拟串口！", "RemotePLC", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            _connectionInfo.NetProtocalType = (int)NetProtocal.NP_SERIALPORT_TRANSMISSION;
                            _connectionInfo.VComName = vcom.VComName;
                            _connectionInfo.VComName4Socket = vcom.VComName4Socket;
                            tabControl.SelectedIndex = 2;
                        }
                    }

                }
                else if (btn.Name.CompareTo("btnDeviceRefresh") == 0)
                {
                    UpdateDeviceList();
                }
                else if (btn.Name.CompareTo("btnDeviceNextStep") == 0)
                {
                    DeviceInfo device = deviceList.SelectedItem as DeviceInfo;
                    if (device == null)
                    {
                        MessageBox.Show("请选择一个设备！", "RemotePLC", MessageBoxButton.OK, MessageBoxImage.Warning);

                        UpdateDeviceList();
                    }
                    else
                    {
                        if (!device.alive)
                        {
                            MessageBox.Show("当前设备不在线！", "RemotePLC", MessageBoxButton.OK, MessageBoxImage.Warning);

                            UpdateDeviceList();
                        }
                        else if (device.mode == 0)
                        {
                            MessageBox.Show("当前设备不在调试模式！", "RemotePLC", MessageBoxButton.OK, MessageBoxImage.Warning);

                            UpdateDeviceList();
                        }
                        else if (device.mode == 1 && device.debugSn.Length != 0)
                        {
                            MessageBox.Show("当前设备正在调试中！", "RemotePLC", MessageBoxButton.OK, MessageBoxImage.Warning);

                            UpdateDeviceList();
                        }
                        else
                        {
                            dtuiptext.Text = device.ip;
                            //_connectionInfo.Desc = "";
                            _connectionInfo.DtuSn = device.sn;
                            //_connectionInfo.NetProtocalType = (int)NetProtocal.NP_NETPORT_TRANSMISSION;
                            //_connectionInfo.DestIp = "";
                            //_connectionInfo.DestPort = -1;
                            //_connectionInfo.LocalPort = -1;
                            //_connectionInfo.Status = (int)RunningStatus.RS_UNCONNECTED;
                            //_connectionInfo.PlcIp = Properties.Settings.Default.PlcIp;
                            //_connectionInfo.PlcPort = Properties.Settings.Default.PlcPort;
                            //_connectionInfo.ServicePort = 0;

                            tabControl.SelectedIndex = 1;
                        }
                    }
                }
                else if (btn.Name.CompareTo("btnConnectionLastStep") == 0)
                {
                    //if (_currentService != null)
                    {
                        if (btnStopService.IsEnabled)
                        {
                            if (MessageBox.Show("返回将会断开连接？",
                                                               "RemotePLC",
                                                                MessageBoxButton.YesNo,
                                                                MessageBoxImage.Question,
                                                                MessageBoxResult.No) == MessageBoxResult.No)
                            {
                                return;
                            }
                        }

                        if (ServiceManager.instance.Stop())
                        {
                            btnConnectionTest.IsEnabled = true;
                            btnStartService.IsEnabled = true;
                            btnStopService.IsEnabled = false;
                        }

                        tabControl.SelectedIndex = 1;
                    }
                }
                else if (btn.Name.CompareTo("btnConnectionTest") == 0)
                {
                    ConnectionTestDialog dlg = new ConnectionTestDialog(_connectionInfo);
                    dlg.Owner = this;
                    dlg.ShowInTaskbar = false;
                    dlg.ShowDialog();
                }
                else if (btn.Name.CompareTo("btnStartService") == 0)
                {
                    if (_connectionInfo.NetProtocalType == (int)NetProtocal.NP_NETPORT_TRANSMISSION)
                    {
                        int serviceport = 0;
                        if (int.TryParse(textServicePort.Text, out serviceport))
                        {
                            _connectionInfo.ServicePort = serviceport;

                            if (ServiceManager.instance.Start(_connectionInfo))
                            {
                                btnConnectionTest.IsEnabled = false;
                                btnStartService.IsEnabled = false;
                                btnStopService.IsEnabled = true;
                            }
                        }
                        else
                        {
                            MessageBox.Show("服务端口错误！", "RemotePLC", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else if (_connectionInfo.NetProtocalType == (int)NetProtocal.NP_SERIALPORT_TRANSMISSION)
                    {
                        if (ServiceManager.instance.Start(_connectionInfo))
                        {
                            btnConnectionTest.IsEnabled = false;
                            btnStartService.IsEnabled = false;
                            btnStopService.IsEnabled = true;
                        }
                    }
                }
                else if (btn.Name.CompareTo("btnStopService") == 0)
                {
                    //if (_currentService != null)
                    {
                        if (ServiceManager.instance.Stop())
                        {
                            btnConnectionTest.IsEnabled = true;
                            btnStartService.IsEnabled = true;
                            btnStopService.IsEnabled = false;
                        }
                    }
                }
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                int tabIndex = tabControl.SelectedIndex;

                if (tabIndex == 0)
                {
                    Init_DeviceTab();
                }
                else if (tabIndex == 1)
                {
                    Init_ConnectionTab();
                }
                else if (tabIndex == 2)
                {
                    Init_RunningTab();
                }

            }
        }

        private void Init_ConnectionTab()
        {
            vcomlist.ItemsSource = VComManager.instance.GetComList();

            _connectionInfo.PlcIp = Config.PlcIp;
            _connectionInfo.PlcPort = Config.PlcPort;

            plciptext.Text = _connectionInfo.PlcIp;
            plcporttext.Text = _connectionInfo.PlcPort.ToString();
        }

        private async void UpdateDeviceList()
        {
            ObservableCollection<DeviceInfo> list = await ServerApi.GetDeviceList();
            if (list != null)
            {
                deviceList.ItemsSource = list;

                ICollectionView dataView = CollectionViewSource.GetDefaultView(deviceList.ItemsSource);//获取数据源视图
                dataView.SortDescriptions.Clear();//清空默认排序描述
                SortDescription sd = new SortDescription("alive", ListSortDirection.Descending);
                dataView.SortDescriptions.Add(sd);//加入新的排序描述
                dataView.Refresh();
            }
        }
        private void Init_DeviceTab()
        {
            UpdateDeviceList();
        }

        //获取内网IP
        private string GetInternalIP()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
        private void Init_RunningTab()
        {
            if (_connectionInfo.NetProtocalType == (int)NetProtocal.NP_NETPORT_TRANSMISSION)
            {
                netInfoGrid.Visibility = Visibility.Visible;
                vcomInfoGrid.Visibility = Visibility.Collapsed;

                textLocalIp.Text = GetInternalIP();

                _connectionInfo.ServicePort = Config.LocalPort;
                textServicePort.Text = _connectionInfo.ServicePort.ToString();
            }
            else if (_connectionInfo.NetProtocalType == (int)NetProtocal.NP_SERIALPORT_TRANSMISSION)
            {
                netInfoGrid.Visibility = Visibility.Collapsed;
                vcomInfoGrid.Visibility = Visibility.Visible;

                textVComName.Text = _connectionInfo.VComName;
            }

            textDtuSn.Text = _connectionInfo.DtuSn;
            Binding bindingstatus = new Binding { Source = _connectionInfo, Path = new PropertyPath("StatusText") };
            textConnectionStatus.SetBinding(TextBlock.TextProperty, bindingstatus);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() == typeof(MenuItem))
            {
                MenuItem menuitem = sender as MenuItem;

                if (menuitem.Header.ToString().CompareTo("设置") == 0)
                {
                    showSetup();
                }
                else if (menuitem.Header.ToString().CompareTo("使用手册") == 0)
                {
                    showManual();
                }
                else if (menuitem.Header.ToString().CompareTo("关于") == 0)
                {
                    showAbout();
                }
            }
        }
        private void showSetup()
        {
            SetupWindow dlg = new SetupWindow();
            dlg.Owner = this;
            dlg.ShowInTaskbar = false;
            dlg.ShowDialog();
        }
        private void showManual()
        {
            try
            {
                String path = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                System.Diagnostics.Process.Start(path + "\\manual.pdf");
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }
        private void showAbout()
        {
            Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "http://iem.dtvlab.cn/";
            proc.Start();
        }
        private void showMonitor()
        {
            if (_connectionInfo != null)
            {
                MonitorDataManager.instance.StartMonitoring(_connectionInfo.DtuSn);
            }
        }
        private void logswitch()
        {
            if (_logWin != null && _logWin.IsVisible)
            {
                _logWin.Close();
                _logWin = null;
            }
            else
            {
                _logWin = new LogWindow();
                _logWin.Owner = this;
                _logWin.Show();
                Logger.Info("Release: {0}", System.IO.File.GetLastWriteTime(this.GetType().Assembly.Location));
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && e.KeyboardDevice.IsKeyDown(Key.D))
            {
                logswitch();
            }
            else if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && e.KeyboardDevice.IsKeyDown(Key.M))
            {
                showMonitor();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (System.Windows.MessageBox.Show("确定要关闭吗?",
                                               "退出",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Question,
                                                MessageBoxResult.No) == MessageBoxResult.Yes)
            {

                _notifyIcon.Dispose();

                Environment.Exit(0);
            }
            else
            {
                e.Cancel = true;
            }

        }

        private void connectionTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                int tabIndex = connectionTab.SelectedIndex;

                if (tabIndex == 0)
                {
                    prompt02.Text = Properties.Resources.IDS_PROMPT02NET;
                }
                else if (tabIndex == 1)
                {
                    prompt02.Text = Properties.Resources.IDS_PROMPT02COM;
                }
            }
        }
    }
}
