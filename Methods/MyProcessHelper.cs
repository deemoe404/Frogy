using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace Frogy.Methods
{
    class MyProcessHelper
    {
        #region DLL导入
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);
        #endregion

        /// <summary>
        /// 获取窗口对应的进程PID
        /// </summary>
        /// <param name="hWnd">hWnd</param>
        /// <returns>进程PID</returns>
        public static Process GetWindowPID(IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, out int calcID);
            Process result = Process.GetProcessById(calcID);

            return result;
        }

        /// <summary>
        /// 获取进程名称
        /// </summary>
        /// <param name="process">进程</param>
        /// <returns>进程名称</returns>
        public static string GetProcessName(Process process)
        {
            bool isUWP = MyProcessHelper.IsProcessUWP(process);

            string applicationName = "";
            if (isUWP)
            {
                try
                {
                    List<IntPtr> allChildWindows = MyWindowHelper.GetAllChildHandles(process.MainWindowHandle);
                    foreach (IntPtr ptr in allChildWindows)
                    {
                        Process uwpProcess = MyProcessHelper.GetWindowPID(ptr);
                        if (uwpProcess.MainModule.ModuleName != "ApplicationFrameHost.exe")
                        {
                            applicationName = MyAppxPackageHelper.GetAppDisplayNameFromProcess(uwpProcess);
                            break;
                        }
                    }
                }
                catch { applicationName = null; }
            }
            else
            {
                applicationName = process.MainModule.FileVersionInfo.FileDescription;

                if (string.IsNullOrEmpty(applicationName))
                    applicationName = process.ProcessName;
            }

            return applicationName;
        }

        /// <summary>
        /// 获取进程路径
        /// </summary>
        /// <param name="process">进程</param>
        /// <returns>进程路径</returns>
        public static string GetProcessPath(Process process)
        {
            bool isUWP = MyProcessHelper.IsProcessUWP(process);

            string applicationPath = "";
            if (isUWP)
            {
                try
                {
                    List<IntPtr> allChildWindows = MyWindowHelper.GetAllChildHandles(process.MainWindowHandle);
                    foreach (IntPtr ptr in allChildWindows)
                    {
                        Process uwpProcess = MyProcessHelper.GetWindowPID(ptr);
                        if (uwpProcess.MainModule.ModuleName != "ApplicationFrameHost.exe")
                            applicationPath = uwpProcess.MainModule.FileName;
                    }
                }
                catch { applicationPath = null; }
            }
            else
            {
                applicationPath = process.MainModule.FileName;
            }

            return applicationPath;
        }

        /// <summary>
        /// 获取进程图标
        /// </summary>
        /// <param name="process">进程</param>
        /// <returns>图标</returns>
        public static Bitmap GetProcessIcon(Process process)
        {
            bool isUWP = MyProcessHelper.IsProcessUWP(process);

            Bitmap result = new Bitmap(Properties.Resources.Default.ToBitmap());
            if (isUWP)
            {
                try
                {
                    List<IntPtr> allChildWindows = MyWindowHelper.GetAllChildHandles(process.MainWindowHandle);
                    foreach (IntPtr ptr in allChildWindows)
                    {
                        Process uwpProcess = MyProcessHelper.GetWindowPID(ptr);
                        if (uwpProcess.MainModule.ModuleName != "ApplicationFrameHost.exe")
                        {
                            AppxPackage package = AppxPackage.FromProcess(uwpProcess);
                            result = new Bitmap(package.FindHighestScaleQualifiedImagePath(package.Logo));
                        }
                    }
                }
                catch { result = new Bitmap(Properties.Resources.Default.ToBitmap()); }
            }
            else
            {
                result = Icon.ExtractAssociatedIcon(GetProcessPath(process)).ToBitmap();
            }

            return result;
        }

        /// <summary>
        /// 判断进程是否为UWP
        /// </summary>
        /// <param name="process">进程</param>
        /// <returns>bool</returns>
        public static bool IsProcessUWP(Process process)
        {
            try
            {
                return process.MainModule.FileVersionInfo.FileDescription == "Application Frame Host";
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}
