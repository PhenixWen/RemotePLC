using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RemotePLC.src.comm
{
    public class CommPort
    {

        public string PortName = "COM1";
        public DCB DCBlock = new DCB();
        //comm port win32 file handle 
        private int hComm = -1;

        public bool IsOpen = false;

        //win32 api constants 
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const int OPEN_EXISTING = 3;
        private const int INVALID_HANDLE_VALUE = -1;
        //private const int MAXBLOCK = 4096;
        private const uint PURGE_TXABORT = 0x0001;  // Kill the pending/current writes to the comm port.
        private const uint PURGE_RXABORT = 0x0002;  // Kill the pending/current reads to the comm port.
        private const uint PURGE_TXCLEAR = 0x0004;  // Kill the transmit queue if there.
        private const uint PURGE_RXCLEAR = 0x0008;  // Kill the typeahead buffer if there.

        private const int NOPARITY = 0;
        private const int ODDPARITY = 1;
        private const int EVENPARITY = 2;
        private const int MARKPARITY = 3;
        private const int SPACEPARITY = 4;

        private const int ONESTOPBIT = 0;
        private const int ONE5STOPBITS = 1;
        private const int TWOSTOPBITS = 2;

        [StructLayout(LayoutKind.Sequential)]
        public struct DCB
        {
            //taken from c struct in platform sdk 
            public int DCBlength; // sizeof(DCB) 
            public int BaudRate; // current baud rate 
            /* these are the c struct bit fields, bit twiddle flag to set 
            public int fBinary; // binary mode, no EOF check 
            public int fParity; // enable parity checking 
            public int fOutxCtsFlow; // CTS output flow control 
            public int fOutxDsrFlow; // DSR output flow control 
            public int fDtrControl; // DTR flow control type 
            public int fDsrSensitivity; // DSR sensitivity 
            public int fTXContinueOnXoff; // XOFF continues Tx 
            public int fOutX; // XON/XOFF out flow control 
            public int fInX; // XON/XOFF in flow control 
            public int fErrorChar; // enable error replacement 
            public int fNull; // enable null stripping 
            public int fRtsControl; // RTS flow control 
            public int fAbortOnError; // abort on error 
            public int fDummy2; // reserved 
            */
            public uint flags;
            public ushort wReserved; // not currently used 
            public ushort XonLim; // transmit XON threshold 
            public ushort XoffLim; // transmit XOFF threshold 
            public byte ByteSize; // number of bits/byte, 4-8 
            public byte Parity; // 0-4=no,odd,even,mark,space 
            public byte StopBits; // 0,1,2 = 1, 1.5, 2 
            public char XonChar; // Tx and Rx XON character 
            public char XoffChar; // Tx and Rx XOFF character 
            public char ErrorChar; // error replacement character 
            public char EofChar; // end of input character 
            public char EvtChar; // received event character 
            public ushort wReserved1; // reserved; do not use 
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct COMMTIMEOUTS
        {
            public uint ReadIntervalTimeout;
            public uint ReadTotalTimeoutMultiplier;
            public uint ReadTotalTimeoutConstant;
            public uint WriteTotalTimeoutMultiplier;
            public uint WriteTotalTimeoutConstant;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct OVERLAPPED
        {
            public int Internal;
            public int InternalHigh;
            public int Offset;
            public int OffsetHigh;
            public int hEvent;
        }

        ///
        /// ClearCommError的结构
        ///
        [StructLayout(LayoutKind.Sequential)]
        private struct COMSTAT
        {
            internal const uint fCtsHold = 0x1;
            internal const uint fDsrHold = 0x2;
            internal const uint fRlsdHold = 0x4;
            internal const uint fXoffHold = 0x8;
            internal const uint fXoffSent = 0x10;
            internal const uint fEof = 0x20;
            internal const uint fTxim = 0x40;
            internal UInt32 Flags;

            ///
            /// 输入缓存中字节个数
            ///
            public uint cbInQue;
            ///
            /// 输出缓存中字节个数
            ///
            public uint cbOutQue;

        }

        [DllImport("kernel32.dll")]
        private static extern int CreateFile(
        string lpFileName, // file name 
        uint dwDesiredAccess, // access mode 
        int dwShareMode, // share mode 
        int lpSecurityAttributes, // SD 
        int dwCreationDisposition, // how to create 
        int dwFlagsAndAttributes, // file attributes 
        int hTemplateFile // handle to template file 
        );
        [DllImport("kernel32.dll")]
        private static extern bool GetCommState(
        int hFile, // handle to communications device 
        ref DCB lpDCB // device-control block 
        );
        [DllImport("kernel32.dll")]
        private static extern bool BuildCommDCB(
        string lpDef, // device-control string 
        ref DCB lpDCB // device-control block 
        );
        [DllImport("kernel32.dll")]
        private static extern bool SetCommState(
        int hFile, // handle to communications device 
        ref DCB lpDCB // device-control block 
        );
        [DllImport("kernel32.dll")]
        private static extern bool GetCommTimeouts(
        int hFile, // handle to comm device 
        ref COMMTIMEOUTS lpCommTimeouts // time-out values 
        );
        [DllImport("kernel32.dll")]
        private static extern bool SetCommTimeouts(
        int hFile, // handle to comm device 
        ref COMMTIMEOUTS lpCommTimeouts // time-out values 
        );
        [DllImport("kernel32.dll")]
        private static extern bool ReadFile(
        int hFile, // handle to file 
        byte[] lpBuffer, // data buffer 
        int nNumberOfBytesToRead, // number of bytes to read 
        ref int lpNumberOfBytesRead, // number of bytes read 
        ref OVERLAPPED lpOverlapped // overlapped buffer 
        );
        [DllImport("kernel32.dll")]
        private static extern bool WriteFile(
        int hFile, // handle to file 
        byte[] lpBuffer, // data buffer 
        int nNumberOfBytesToWrite, // number of bytes to write 
        ref int lpNumberOfBytesWritten, // number of bytes written 
        ref OVERLAPPED lpOverlapped // overlapped buffer 
        );
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(
        int hObject // handle to object 
        );
        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();
        [DllImport("kernel32")]
        private static extern bool PurgeComm(
        int hFile,     // handle to file
        uint dwFlags
        );
        [DllImport("kernel32.dll")]
        private static extern bool ClearCommError(
        int hFile,
        ref uint lpErrors,
        ref COMSTAT lpStat
        );
        [DllImport("kernel32.dll")]
        public extern static int FormatMessage(
        int flag,
        ref IntPtr source,
        int msgid,
        int langid,
        ref string buf,
        int size,
        ref IntPtr args
        );
        public static string GetSysErrMsg(int errCode)
        {
            IntPtr tempptr = IntPtr.Zero;
            string msg = null;
            FormatMessage(0x1300, ref tempptr, errCode, 0, ref msg, 255, ref tempptr);
            return msg;
        }

        // 配置串行口
        bool ConfigConnection()
        {
            if (hComm == INVALID_HANDLE_VALUE)
            {
                return false;
            }

            if (!GetCommState(hComm, ref DCBlock))
                return false;

            //dcb.BaudRate = BaudRate; // 波特率
            //dcb.ByteSize = ByteSize; // 每字节位数

            //switch (Parity) // 校验设置
            //{
            //    case 0:
            //        dcb.Parity = NOPARITY;
            //        break;
            //    case 1:
            //        dcb.Parity = EVENPARITY;
            //        break;
            //    case 2:
            //        dcb.Parity = ODDPARITY;
            //        break;
            //}
            //switch (StopBits) // 停止位
            //{
            //    case 0:
            //        dcb.StopBits = ONESTOPBIT;
            //        break;
            //    case 1:
            //        dcb.StopBits = ONE5STOPBITS;
            //        break;
            //    case 2:
            //        dcb.StopBits = TWOSTOPBITS;
            //        break;
            //}

            bool fSuccess;
            fSuccess = SetCommState(hComm, ref DCBlock);
            if (fSuccess)
            {
                return true;
            }
            return false;
        }
        public void Open(string portname)
        {
            PortName = portname;
            //BaudRate = baudrate;
            //ByteSize = (byte)bytesize;
            //Parity = (byte)parity; // 0-4=no,odd,even,mark,space 
            //StopBits = (byte)stopbits; // 0,1,2 = 1, 1.5, 2 

            string filename = String.Format("\\\\.\\{0}", PortName);
            // OPEN THE COMM PORT. 
            hComm = CreateFile(filename, GENERIC_READ | GENERIC_WRITE, 0, 0, OPEN_EXISTING, 0, 0);
            // IF THE PORT CANNOT BE OPENED, BAIL OUT. 
            if (hComm == INVALID_HANDLE_VALUE)
            {
                throw (new ApplicationException(GetSysErrMsg((int)GetLastError())));
            }

            COMMTIMEOUTS TimeOuts;
            //设定读超时
            TimeOuts.ReadIntervalTimeout = 0xffffffff;
            TimeOuts.ReadTotalTimeoutMultiplier = 0;
            TimeOuts.ReadTotalTimeoutConstant = 0;
            //设定写超时
            TimeOuts.WriteTotalTimeoutMultiplier = 100;
            TimeOuts.WriteTotalTimeoutConstant = 500;
            SetCommTimeouts(hComm, ref TimeOuts); //设置超时

            if (ConfigConnection())
            {
                PurgeComm(hComm, PURGE_TXCLEAR | PURGE_RXCLEAR);
                IsOpen = true;
                Logger.Info("{0} opend!", PortName);
            }
            else
            {
                CloseHandle(hComm);
                IsOpen = false;
                Logger.Info("{0} not opend!", PortName);
            }
        }

        public void Close()
        {
            if (hComm != INVALID_HANDLE_VALUE)
            {
                if (CloseHandle(hComm))
                {
                    IsOpen = false;
                    hComm = INVALID_HANDLE_VALUE;

                    Logger.Info("{0} closed!", PortName);
                }
            }
        }

        public int Read(ref byte[] commRead, int NumBytes)
        {
            if (hComm != INVALID_HANDLE_VALUE)
            {
                COMSTAT ComStat = new COMSTAT();
                uint dwErrorFlags = 0;
                ClearCommError(hComm, ref dwErrorFlags, ref ComStat);
                if (ComStat.cbInQue > 0 && NumBytes > 0)
                {
                    int count = Math.Min(NumBytes, (int)(ComStat.cbInQue));
                    OVERLAPPED ovlCommPort = new OVERLAPPED();
                    int BytesRead = 0;

                    if (!GetCommState(hComm, ref DCBlock))
                    {
                        Logger.Error("GetCommState Error!");
                    }

                    bool ret = ReadFile(hComm, commRead, count, ref BytesRead, ref ovlCommPort);
                    if (ret)
                    {
                        return BytesRead;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return -1;
            }
        }

        public int Write(byte[] WriteBytes, int NumBytes)
        {
            if (hComm != INVALID_HANDLE_VALUE)
            {
                OVERLAPPED ovlCommPort = new OVERLAPPED();
                int BytesWritten = 0;
                WriteFile(hComm, WriteBytes, NumBytes, ref BytesWritten, ref ovlCommPort);
                return BytesWritten;
            }
            else
            {
                return -1;
            }
        }
    }

}
