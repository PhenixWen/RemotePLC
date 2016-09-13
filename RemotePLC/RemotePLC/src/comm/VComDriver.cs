using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace RemotePLC.src.comm
{
    public static class VComDriver
    {
        public const int MaxSerialPortNum = 255;
        private static int exec(string cmd, out ArrayList output)
        {
            int ret = 0;

            output = new ArrayList();

            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = @"./setupc.exe";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = cmd;//参数以空格分隔，如果某个参数为空，可以传入””

            try
            {
                if (p.Start())
                {
                    string line;
                    while ((line = p.StandardOutput.ReadLine()) != null)
                    {
                        output.Add(line);
                    }
                    p.WaitForExit();

                    ret = p.ExitCode;

                    p.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

            return ret;
        }

        public static bool AddVCom(string name0, string name1)
        {
            string cmd = String.Format("install PortName={0} PortName={1},HiddenMode=yes", name0, name1);
            ArrayList output;
            int ret = exec(cmd, out output);
            if (ret == 0)
            {
                foreach (string line in output)
                {
                    Regex reg = new Regex("(CNCA|CNCB).+ PortName=(.+)");
                    Match match = reg.Match(line.Trim());
                    if (match.Success && match.Groups.Count > 2)
                    {
                        string value1 = match.Groups[1].Value;
                        string value2 = match.Groups[2].Value;

                        if (value1.CompareTo("CNCA") == 0 && value2.CompareTo(name0) == 0 ||
                            value1.CompareTo("CNCB") == 0 && value2.CompareTo(name1) == 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool GetVComId(string name, out int id)
        {
            id = -1;
            string cmd = String.Format("list");
            ArrayList output;
            int ret = exec(cmd, out output);
            if (ret == 0)
            {
                foreach (string line in output)
                {
                    Regex reg = new Regex("(CNCA|CNCB)(\\d+) PortName=(\\w+)");
                    Match match = reg.Match(line.Trim());
                    if (match.Success && match.Groups.Count > 3)
                    {
                        string value1 = match.Groups[1].Value;
                        string value2 = match.Groups[2].Value;
                        string value3 = match.Groups[3].Value;
                        if (value3.CompareTo(name) == 0)
                        {
                            int index = -1;
                            if (Int32.TryParse(value2, out index))
                            {
                                id = index;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static bool DelAllVCom()
        {
            string cmd = String.Format("uninstall");
            ArrayList output;
            int ret = exec(cmd, out output);
            if (ret == 0)
            {
                return true;
            }
            return false;
        }
        public static bool DelVComById(int id)
        {
            string cmd = String.Format("remove {0}", id);
            ArrayList output;
            int ret = exec(cmd, out output);
            if (ret == 0)
            {
                return true;
            }
            return false;
        }
        public static ArrayList GetIdleComId()
        {
            ArrayList ids = new ArrayList();
            for (int i = 1; i <= MaxSerialPortNum; i++)
            {
                bool isExist = false;
                foreach (string vPortName in SerialPort.GetPortNames())
                {
                    if (vPortName.CompareTo(String.Format("COM{0}", i)) == 0)
                    {
                        isExist = true;
                        break;
                    }
                }

                if (!isExist)
                {
                    ids.Add(i);
                }
            }
            return ids;
        }
        public static List<VCom> GetComList()
        {
            Dictionary<String, String> cncalist = new Dictionary<string, string>();
            List<VCom> list = new List<VCom>();

            string cmd = String.Format("list");
            ArrayList output;
            int ret = exec(cmd, out output);
            if (ret == 0)
            {
                foreach (string line in output)
                {
                    Regex reg = new Regex("(CNCA|CNCB)(\\d+) PortName=(\\w+)");
                    Match match = reg.Match(line.Trim());
                    if (match.Success && match.Groups.Count > 3)
                    {
                        string value1 = match.Groups[1].Value;
                        string value2 = match.Groups[2].Value;
                        string value3 = match.Groups[3].Value;

                        if (value1.CompareTo("CNCA") == 0 && !cncalist.ContainsKey(value2))
                        {
                            cncalist.Add(value2, value3);
                        }
                        else if (value1.CompareTo("CNCB") == 0 && cncalist.ContainsKey(value2))
                        {
                            string portname;
                            if (cncalist.TryGetValue(value2, out portname))
                            {
                                string strid = Regex.Replace(portname, @"COM", "");
                                int id = 0;
                                if (Int32.TryParse(strid, out id))
                                {
                                    VCom vcom = new VCom();
                                    vcom.Id = id;
                                    vcom.VComName = portname;
                                    vcom.VComName4Socket = value3;
                                    list.Add(vcom);
                                }
                            }
                        }
                    }
                }
            }
            list.Sort();
            return list;
        }

    }
}
