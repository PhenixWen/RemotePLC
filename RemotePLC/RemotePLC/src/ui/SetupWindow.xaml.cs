using System;
using System.Collections.Generic;
using System.Linq;
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

namespace RemotePLC.src.ui
{
    /// <summary>
    /// SetupWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetupWindow : Window
    {
        public SetupWindow()
        {
            InitializeComponent();
            serverIpText.Text = Config.ServerIp;
            serverPortText.Text = Config.ServerPort.ToString();
            ApiRootText.Text = Config.ServerApiRoot;
            ApiPortText.Text = Config.ServerApiPort.ToString();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Content.ToString().CompareTo("确定") == 0)
            {
                Config.ServerIp = serverIpText.Text;
                int serverPort = 0;
                int serverApiPort = 0;
                if (int.TryParse(serverPortText.Text, out serverPort))
                {
                    Config.ServerPort = serverPort;
                }
                if (int.TryParse(ApiPortText.Text, out serverApiPort))
                {
                    Config.ServerApiPort = serverApiPort;
                }
                Config.ServerApiRoot = ApiRootText.Text;
                Config.Save();
                Close();
            }
            else if (btn.Content.ToString().CompareTo("取消") == 0)
            {
                Close();
            }
        }
    }
}
