using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using RemotePLC.src.ui;
using System.Data;
using System.Windows;

namespace RemotePLC.src.comm
{
    public class ResponseJson
    {
        public string status { get; set; }
        public string error { get; set; }
        public string msg { get; set; }
        public string data { get; set; }
    }
    public static class ServerApi
    {
        private static async Task<string> getDeviceListJson()
        {
            try
            {
                string strURL = String.Format("http://{0}:{1}{2}/dtuList", Config.ServerIp, Config.ServerApiPort, Config.ServerApiRoot);
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.GetAsync(strURL);
                if (response != null)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string responseString = await response.Content.ReadAsStringAsync();

                        ResponseJson rj = JsonConvert.DeserializeObject<ResponseJson>(responseString);
                        if (rj.status.CompareTo("1") == 0)
                        {
                            return rj.data;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return null;
            }
            return null;
        }
        public static async Task<ObservableCollection<DeviceInfo>> GetDeviceList()
        {
            string jsonstr = await getDeviceListJson();

            Logger.Debug("{0}", jsonstr);

            if (jsonstr != null)
            {
                ObservableCollection<DeviceInfo> infos = JsonConvert.DeserializeObject<ObservableCollection<DeviceInfo>>(jsonstr);
                return infos;
            }

            return new ObservableCollection<DeviceInfo>();
        }
    }
}
