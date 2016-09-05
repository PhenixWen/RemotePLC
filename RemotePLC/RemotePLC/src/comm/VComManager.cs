using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RemotePLC.src.comm
{
    public class VCom : IComparable
    {
        public int Id { get; set; }
        public string VComName { get; set; }
        public string VComName4Socket { get; set; }

        public int CompareTo(object obj)
        {
            int res = 0;
            try
            {
                VCom vcom = (VCom)obj;
                if (this.Id > vcom.Id)
                {
                    res = 1;
                }
                else if (this.Id < vcom.Id)
                {
                    res = -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("比较异常", ex.InnerException);
            }
            return res;
        }
    }
    public class VComManager
    {
        private VComManager()
        {
        }

        public static readonly VComManager instance = new VComManager();
        private bool IsVComExist(string name)
        {
            List<VCom> list = VComDriver.GetComList();

            foreach (VCom vcom in list)
            {
                if (vcom.VComName.CompareTo(name) == 0 || vcom.VComName4Socket.CompareTo(name) == 0)
                {
                    return true;
                }
            }
            return false;
        }
        public bool AddVCom(string name0, string name1)
        {
            return VComDriver.AddVCom(name0, name1);
        }
        public bool DelVCom(string name4ide)
        {
            int id = -1;
            if (VComDriver.GetVComId(name4ide, out id))
            {
                bool ret = VComDriver.DelVComById(id);
                if (!ret)
                {
                    return false;
                }
            }
            return true;
        }
        public bool DelAllVCom()
        {
            if (VComDriver.DelAllVCom())
            {
                return true;
            }
            return false;
        }
        public List<VCom> GetComList()
        {
            return VComDriver.GetComList();
        }

    }
}
