using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePLC.src.comm
{
    public static class Config
    {
        public static int BufferSize { get { return Properties.Settings.Default.BufferSize; } }
        public static string ServerIp { get { return Properties.Settings.Default.ServerIp; } set { Properties.Settings.Default.ServerIp = value; } }
        public static int ServerPort { get { return Properties.Settings.Default.ServerPort; } set { Properties.Settings.Default.ServerPort = value; } }
        public static int ServerApiPort { get { return Properties.Settings.Default.ApiPort; } set { Properties.Settings.Default.ApiPort = value; } }
        public static string ServerApiRoot { get { return Properties.Settings.Default.ApiRoot; } set { Properties.Settings.Default.ApiRoot = value; } }
        public static string PlcIp { get { return Properties.Settings.Default.PlcIp; } set { Properties.Settings.Default.PlcIp = value; } }
        public static int PlcPort { get { return Properties.Settings.Default.PlcPort; } set { Properties.Settings.Default.PlcPort = value; } }
        public static int LocalPort { get { return Properties.Settings.Default.LocalPort; } set { Properties.Settings.Default.LocalPort = value; } }

        public static void Save()
        {
            Properties.Settings.Default.Save();
        }
    }
}
