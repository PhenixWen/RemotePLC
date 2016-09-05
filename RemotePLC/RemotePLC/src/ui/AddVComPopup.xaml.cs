using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// AddVComPopup.xaml 的交互逻辑
    /// </summary>
    public partial class AddVComPopup : Window
    {
        public class VComItem
        {
            public VComItem(int id)
            {
                _id = id;
            }

            private int _id;
            public int Id { get { return _id; } set { _id = value; } }
            public string Name { get { return String.Format("COM{0}", _id); } }
        }
        public AddVComPopup()
        {
            InitializeComponent();

            ObservableCollection<VComItem> items = new ObservableCollection<VComItem>();
            ArrayList ids = VComDriver.GetIdleComId();
            foreach (int id in ids)
            {
                items.Add(new VComItem(id));
            }
            comboBox.ItemsSource = items;
            if (ids.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }
        }
        private void doAddVCom()
        {
            if (comboBox.SelectedItem != null)
            {
                VComItem item = comboBox.SelectedItem as VComItem;

                string name4ide = String.Format("COM{0}", item.Id);
                string name4socket = String.Format("TCBVCOM{0}", item.Id);

                Cursor = Cursors.Wait;
                bool ret = VComManager.instance.AddVCom(name4ide, name4socket);
                Cursor = Cursors.Arrow;
                if (ret)
                {
                    DialogResult = true;
                    Close();
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Content.ToString().CompareTo("确定") == 0)
            {
                doAddVCom();
            }
            else if (btn.Content.ToString().CompareTo("取消") == 0)
            {
                Close();
            }
        }
    }
}
