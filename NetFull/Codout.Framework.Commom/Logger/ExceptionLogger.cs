using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;

namespace Codout.Framework.Commom.Logger
{
    public class ExceptionLogger
    {
        private static ExceptionLogger _instance;

        private ExceptionLogger()
        {
            
        }

        public static ExceptionLogger Get { get { return _instance ?? (_instance = new ExceptionLogger()); } }

        private string GetExceptionTypeStack(Exception e)
        {
            if (e.InnerException != null)
            {
                var message = new StringBuilder();
                message.AppendLine(GetExceptionTypeStack(e.InnerException));
                message.AppendLine("   " + e.GetType());
                return (message.ToString());
            }
            return "   " + e.GetType();
        }

        private string GetExceptionMessageStack(Exception e)
        {
            if (e.InnerException != null)
            {
                var message = new StringBuilder();
                message.AppendLine(GetExceptionMessageStack(e.InnerException));
                message.AppendLine("   " + e.Message);
                return (message.ToString());
            }
            return "   " + e.Message;
        }

        private string GetExceptionCallStack(Exception e)
        {
            if (e.InnerException != null)
            {
                var message = new StringBuilder();
                message.AppendLine(GetExceptionCallStack(e.InnerException));
                message.AppendLine("--- Next Call Stack:");
                message.AppendLine(e.StackTrace);
                return (message.ToString());
            }
            return e.StackTrace;
        }

        private static TimeSpan GetSystemUpTime()
        {
            var upTime = new PerformanceCounter("System", "System Up Time");
            upTime.NextValue();
            return TimeSpan.FromSeconds(upTime.NextValue());
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        public string GetExceptionDetails(Exception exception)
        {
            var error = new List<string>();

            error.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            var memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                error.Add(memStatus.ullTotalPhys / (1024 * 1024) + "Mb");
                error.Add(memStatus.ullAvailPhys / (1024 * 1024) + "Mb");
            }

            if (HttpContext.Current != null)
            {
                error.Add(HttpContext.Current.Request.UserAgent);
                error.Add(HttpContext.Current.Request.Browser.Version);
                error.Add(HttpContext.Current.Request.UserLanguages != null ? string.Join(";", HttpContext.Current.Request.UserLanguages) : "");
                error.Add(HttpContext.Current.Request.UserHostAddress);
                error.Add(HttpContext.Current.Request.Url.AbsoluteUri);
                error.Add(
                    $"{HttpContext.Current.Request.Browser.ScreenPixelsWidth}x{HttpContext.Current.Request.Browser.ScreenPixelsHeight}");

                var vars = new List<string>();
                foreach (string key in HttpContext.Current.Request.Form.Keys)
                    vars.Add($"{key}: {HttpContext.Current.Request.Form[key]}");

                error.Add(string.Join(", ", vars.ToArray()));
            }

            error.Add(GetExceptionTypeStack(exception));
            error.Add(GetExceptionMessageStack(exception));
            error.Add(GetExceptionCallStack(exception));
            var thisProcess = Process.GetCurrentProcess();
            foreach (ProcessModule module in thisProcess.Modules)
                error.Add(module.FileName + " " + module.FileVersionInfo.FileVersion);

            return string.Join(";", error.ToArray());

        }
    }
}
