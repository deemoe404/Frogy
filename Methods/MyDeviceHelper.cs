﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Frogy.Methods
{
    class MyDeviceHelper
    {
        [DllImport("Wtsapi32.dll", CharSet = CharSet.Unicode)]
        private static extern bool WTSQuerySessionInformationW(IntPtr hServer, uint SessionId, WTS_INFO_CLASS WTSInfoClass, ref IntPtr ppBuffer, ref uint pBytesReturned);
        [DllImport("Wtsapi32.dll", CharSet = CharSet.Unicode)]
        private static extern void WTSFreeMemory(IntPtr pMemory);
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern uint WTSGetActiveConsoleSessionId();

        public enum WTS_INFO_CLASS
        {
            WTSInitialProgram,
            WTSApplicationName,
            WTSWorkingDirectory,
            WTSOEMId,
            WTSSessionId,
            WTSUserName,
            WTSWinStationName,
            WTSDomainName,
            WTSConnectState,
            WTSClientBuildNumber,
            WTSClientName,
            WTSClientDirectory,
            WTSClientProductId,
            WTSClientHardwareId,
            WTSClientAddress,
            WTSClientDisplay,
            WTSClientProtocolType,
            WTSIdleTime,
            WTSLogonTime,
            WTSIncomingBytes,
            WTSOutgoingBytes,
            WTSIncomingFrames,
            WTSOutgoingFrames,
            WTSClientInfo,
            WTSSessionInfo,
            WTSSessionInfoEx,
            WTSConfigInfo,
            WTSValidationInfo,   // Info Class value used to fetch Validation Information through the WTSQuerySessionInformation
            WTSSessionAddressV4,
            WTSIsRemoteSession
        }
        private enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,              // User logged on to WinStation
            WTSConnected,           // WinStation connected to client
            WTSConnectQuery,        // In the process of connecting to client
            WTSShadow,              // Shadowing another WinStation
            WTSDisconnected,        // WinStation logged on without client
            WTSIdle,                // Waiting for client to connect
            WTSListen,              // WinStation is listening for connection
            WTSReset,               // WinStation is being reset
            WTSDown,                // WinStation is down due to error
            WTSInit,                // WinStation in initialization
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WTSINFOEXW
        {
            public int Level;
            public WTSINFOEX_LEVEL_W Data;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct WTSINFOEX_LEVEL_W
        {
            public WTSINFOEX_LEVEL1_W WTSInfoExLevel1;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WTSINFOEX_LEVEL1_W
        {
            public int SessionId;
            public WTS_CONNECTSTATE_CLASS SessionState;
            public int SessionFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string WinStationName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
            public string UserName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
            public string DomainName;
            public LARGE_INTEGER LogonTime;
            public LARGE_INTEGER ConnectTime;
            public LARGE_INTEGER DisconnectTime;
            public LARGE_INTEGER LastInputTime;
            public LARGE_INTEGER CurrentTime;
            public uint IncomingBytes;
            public uint OutgoingBytes;
            public uint IncomingFrames;
            public uint OutgoingFrames;
            public uint IncomingCompressedBytes;
            public uint OutgoingCompressedBytes;
        }
        [StructLayout(LayoutKind.Explicit)]
        private struct LARGE_INTEGER //此结构体在C++中使用的为union结构，在C#中需要使用FieldOffset设置相关的内存起始地址
        {
            [FieldOffset(0)]
            readonly uint LowPart;
            [FieldOffset(4)]
            readonly int HighPart;
            [FieldOffset(0)]
            readonly long QuadPart;
        }

        /// <summary>
        /// 读取设备状态
        /// 0 代表锁定
        /// 1 代表未锁定
        /// </summary>
        public static int DeviceState
        {
            get
            {
                uint dwBytesReturned = 0;
                int dwFlags = 0;
                IntPtr pInfo = IntPtr.Zero;

                WTSQuerySessionInformationW(IntPtr.Zero, 
                    WTSGetActiveConsoleSessionId(), 
                    WTS_INFO_CLASS.WTSSessionInfoEx, 
                    ref pInfo, 
                    ref dwBytesReturned);

                var pointsTo = Marshal.PtrToStructure<WTSINFOEXW>(pInfo);

                if (pointsTo.Level == 1)
                    dwFlags = pointsTo.Data.WTSInfoExLevel1.SessionFlags;

                return dwFlags;
            }
        }

        public static void RegisterStartup()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true) ?? 
                    Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");

                Assembly curAssembly = Assembly.GetExecutingAssembly();
                key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
            }
            catch { }
        }

        public static void DeregisterStartup()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true) ?? 
                    Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");

                Assembly curAssembly = Assembly.GetExecutingAssembly();
                key.DeleteValue(curAssembly.GetName().Name);
            }
            catch { }
        }

        public static bool GetStartupStatus()
        {
            bool result = false;
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true) ?? 
                    Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");

                Assembly curAssembly = Assembly.GetExecutingAssembly();

                string[] values = key.GetValueNames();
                foreach(string s in values) if (s == curAssembly.GetName().Name) result = true;
            }
            catch { }

            return result;
        }

        public static void PromotePermission()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            Assembly curAssembly = Assembly.GetExecutingAssembly();

            psi.FileName = curAssembly.Location;
            psi.Verb = "runas";
            psi.Arguments = "restart";

            try
            {
                Process.Start(psi);
                Environment.Exit(1);
            }
            catch { }
        }

        public static void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrames), frame);
            Dispatcher.PushFrame(frame);
        }

        public static object ExitFrames(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }
    }
}
