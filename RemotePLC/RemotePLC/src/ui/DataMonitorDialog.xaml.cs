using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Windows.Threading;
using RemotePLC.src.comm;

namespace RemotePLC.src.ui
{
    /// <summary>
    /// DataMonitorDialog.xaml 的交互逻辑
    /// </summary>
    public partial class DataMonitorDialog : Window
    {
        private bool _isPause;
        private DateTime _lasttime;

        public DataMonitorDialog()
        {
            InitializeComponent();
            _lasttime = DateTime.Parse("1/1/1970");
            setPause(false);
        }

        private void setPause(bool pause)
        {
            _isPause = pause;
            if (pause)
            {
                btnStart.IsEnabled = true;
                btnPause.IsEnabled = false;
            }
            else
            {
                btnStart.IsEnabled = false;
                btnPause.IsEnabled = true;
            }

        }

        private void Start()
        {
            setPause(false);
        }
        private void Stop()
        {
            setPause(true);
        }
        public void Add(byte[] data, MonitorDataType type)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                if (!_isPause)
                {
                    DateTime timenow = DateTime.Now;
                    if (_lasttime == DateTime.Parse("1/1/1970") || timenow < _lasttime)
                    {
                        _lasttime = timenow;
                    }
                    TimeSpan ts = timenow - _lasttime;
                    MonitorData monitordata = new MonitorData(datas.Items.Count, (int)ts.TotalMilliseconds, data, type);
                    datas.Items.Add(monitordata);
                    ScrollToEnd();
                }
            });
        }
        private void Clear()
        {
            datas.Items.Clear();
        }
        private void Save()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "文本文件|*.txt"
            };
            saveFileDialog.FileName = string.Format("{0:yyMMdd_HHmmss}.txt", DateTime.Now);
            var result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                string path = saveFileDialog.FileName;
                FileStream fs = new FileStream(path, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                //开始写入
                foreach (MonitorData data in datas.Items)
                {
                    sw.Write(data.Id);
                    sw.Write(",\t");
                    sw.Write(data.TickCount);
                    sw.Write(",\t");
                    sw.Write(data.Type);
                    sw.Write(",\t");
                    sw.Write(data.ByteCount);
                    sw.Write(",\t");
                    sw.Write(data.ASCII);
                    sw.Write(",\t");
                    sw.WriteLine(data.HEX);
                }
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() == typeof(Button))
            {
                Button btn = sender as Button;
                if (btn.Content.ToString().CompareTo("开始") == 0)
                {
                    Start();
                }
                else if (btn.Content.ToString().CompareTo("停止") == 0)
                {
                    Stop();
                }
                else if (btn.Content.ToString().CompareTo("清空") == 0)
                {
                    Clear();
                }
                else if (btn.Content.ToString().CompareTo("保存") == 0)
                {
                    Save();
                }
                else if (btn.Content.ToString().CompareTo("退出") == 0)
                {
                    Close();
                }
            }
        }

        public void ScrollToEnd()
        {
            if (datas.Items.Count > 0 && Mouse.LeftButton != MouseButtonState.Pressed)
            {
                datas.ScrollIntoView(datas.Items.GetItemAt(datas.Items.Count - 1));
            }
        }

        private void datas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (datas.SelectedItem != null)
            {
                MonitorData data = datas.SelectedItem as MonitorData;
                string infoText = String.Format("ASCII:\n{0}\nHEX:\n{1}\n", data.ASCII, data.HEX);
                infoBox.Text = infoText;
            }
            else
            {
                infoBox.Text = "";
            }
        }

    }
}
