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
        private static IniFile _iniFile = null;
        public static string _serverip = "121.42.161.235";
        public static int _serverport = 6789;
        public static int _serverapiport = 6788;
        public static string _serverapiroot = "/plc/api";
        public static int _buffersize = 6144;//6K=6*1024

        public static int BufferSize { get { return _buffersize; } }
        public static string ServerIp { get { return _serverip; } set { _serverip = value; } }
        public static int ServerPort { get { return _serverport; } set { _serverport = value; } }
        public static int ServerApiPort { get { return _serverapiport; } set { _serverapiport = value; } }
        public static string ServerApiRoot { get { return _serverapiroot; } set { _serverapiroot = value; } }

        public static void Save()
        {
            try
            {
                string sectionName = "Server";

                _iniFile.Set(sectionName, "Ip", _serverip);
                _iniFile.Set(sectionName, "Port", _serverport);
                _iniFile.Set(sectionName, "ApiPort", _serverapiport);
                _iniFile.Set(sectionName, "ApiRoot", _serverapiroot);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

        }
        public static void Load()
        {
            try
            {
                string path = String.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), System.IO.Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().GetName().Name));
                string filename = String.Format("{0}\\{1}", path, "Setup.ini");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (!File.Exists(filename))
                {
                    File.Create(filename);
                }
                _iniFile = new IniFile(filename);

                string sectionName = "Server";

                if (!_iniFile.HasOption(sectionName, "Ip"))
                {
                    _iniFile.Set(sectionName, "Ip", _serverip);
                }
                if (!_iniFile.HasOption(sectionName, "Port"))
                {
                    _iniFile.Set(sectionName, "Port", _serverport);
                }
                if (!_iniFile.HasOption(sectionName, "ApiPort"))
                {
                    _iniFile.Set(sectionName, "ApiPort", _serverapiport);
                }
                if (!_iniFile.HasOption(sectionName, "ApiRoot"))
                {
                    _iniFile.Set(sectionName, "ApiRoot", _serverapiroot);
                }

                _serverip = _iniFile.Get(sectionName, "Ip");
                _serverport = _iniFile.GetInt(sectionName, "Port");
                _serverapiport = _iniFile.GetInt(sectionName, "ApiPort");
                _serverapiroot = _iniFile.Get(sectionName, "ApiRoot");
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }
    }
}
