using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Frogy.Methods
{
    class MyWindowHelper
    {
        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowText")]
        private static extern int GetWindowText(int hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

        //private readonly IntPtr _MainHandle;
        //public MyWindowHelper(IntPtr handle)
        //{
        //    this._MainHandle = handle;
        //}


        /// <summary>
        /// 获取窗口标题
        /// </summary>
        /// <param name="hWnd">窗口hWnd</param>
        /// <returns>窗口标题</returns>
        public static string GetWindowTitle(IntPtr hWnd)
        {
            StringBuilder title = new StringBuilder(256);
            GetWindowText((int)hWnd, title, 256);

            return title.ToString();
        }

        /// <summary>
        /// 获取焦点窗口
        /// </summary>
        /// <returns>IntPtr</returns>
        public static IntPtr GetFocueWindow()
        {
            return GetForegroundWindow();
        }

        /// <summary>
        /// 获取当窗口的所有子窗口
        /// </summary>
        /// <returns></returns>
        public static List<IntPtr> GetAllChildHandles(IntPtr mainHandle)
        {
            List<IntPtr> childHandles = new List<IntPtr>();

            GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
            IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
                EnumChildWindows(mainHandle, childProc, pointerChildHandlesList);
            }
            finally
            {
                gcChildhandlesList.Free();
            }

            return childHandles;
        }

        private static bool EnumWindow(IntPtr hWnd, IntPtr lParam)
        {
            GCHandle gcChildhandlesList = GCHandle.FromIntPtr(lParam);

            if (gcChildhandlesList == null || gcChildhandlesList.Target == null)
            {
                return false;
            }

            List<IntPtr> childHandles = gcChildhandlesList.Target as List<IntPtr>;
            childHandles.Add(hWnd);

            return true;
        }
    }
}
