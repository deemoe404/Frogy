using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frogy
{
    class MyFocusTask
    {
        /// <summary>
        /// 任务软件名称
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// 任务窗口名
        /// </summary>
        public string FormName { get; set; }

        /// <summary>
        /// 程序EXE路径名
        /// </summary>
        public string ApplicationFilePath { get; set; }

        /// <summary>
        /// 程序是否为UWP
        /// </summary>
        public bool IsApplicationUWP { get; set; } = false;
    }
}
