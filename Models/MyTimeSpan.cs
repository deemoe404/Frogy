using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frogy
{
    class MyTimeSpan
    {
        /// <summary>
        /// 第一次记录到当前任务启动的时间
        /// </summary>
        public TimeSpan StartTime { get; set; } = new TimeSpan(0,0,0);

        /// <summary>
        /// 记录离开当前任务的时间
        /// </summary>
        public TimeSpan StopTime { get; set; } = new TimeSpan(0, 0, 0);

        /// <summary>
        /// 记录当前时间执行的任务
        /// </summary>
        public MyFocusTask TimeSpanTask { get; set; } = new MyFocusTask();
    }
}
