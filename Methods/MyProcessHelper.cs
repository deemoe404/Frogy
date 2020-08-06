using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Frogy.Methods
{
    class MyProcessHelper
    {
        #region DLL导入
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);
        #endregion

        /// <summary>
        /// 获取焦点窗口
        /// </summary>
        /// <returns>IntPtr</returns>
        public static IntPtr GetFocueWindow()
        {
            return GetForegroundWindow();
        }


        /// <summary>
        /// 获取窗口对应的进程PID
        /// </summary>
        /// <param name="hWnd">hWnd</param>
        /// <returns>进程PID</returns>
        public static Process GetWindowPID(IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, out int calcID);
            Process result = Process.GetProcessById(calcID);
            //result.WaitForInputIdle();

            return result;
        }
    }
}
