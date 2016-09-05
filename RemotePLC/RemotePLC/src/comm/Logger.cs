using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotePLC.src.comm
{
    public static class Logger
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Trace(string msg, params object[] args)
        {
            logger.Trace(msg, args);
        }
        public static void Debug(string msg, params object[] args)
        {
            logger.Debug(msg, args);
        }
        public static void Info(string msg, params object[] args)
        {
            logger.Info(msg, args);
        }
        public static void Warn(string msg, params object[] args)
        {
            logger.Warn(msg, args);
        }
        public static void Error(string msg, params object[] args)
        {
            logger.Error(msg, args);
        }
        public static void Fatal(string msg, params object[] args)
        {
            logger.Fatal(msg, args);
        }
    }
}
