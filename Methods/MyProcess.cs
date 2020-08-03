using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Frogy.Methods
{
    class MyProcess
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

        [DllImport("user32.dll", EntryPoint = "GetWindowText")]
        public static extern int GetWindowText(
            int hWnd,
            StringBuilder lpString,
            int nMaxCount
        );

        /// <summary>
        /// 获取当前焦点窗口进程PID
        /// </summary>
        /// <returns>进程PID</returns>
        public static Process GetFocusTaskInfo()
        {
            GetWindowThreadProcessId(GetForegroundWindow(), out int calcID);

            return Process.GetProcessById(calcID);
        }

        /// <summary>
        /// 获取当前焦点窗口标题
        /// </summary>
        /// <returns>窗口标题</returns>
        public static string GetFocusTaskWindowTitle()
        {
            StringBuilder title = new StringBuilder(256);
            GetWindowText((int)GetForegroundWindow(), title, 256);

            return title.ToString();
        }
    }
}
