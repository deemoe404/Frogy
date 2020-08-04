using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Frogy
{
    /// <summary>
    /// 时间线
    /// </summary>
    [Serializable]
    class MyDay
    {
        /// <summary>
        /// 今日任务记录
        /// </summary>
        public List<MyTimeDuration> TimeLine = new List<MyTimeDuration>();
    }

    /// <summary>
    /// 时间段
    /// </summary>
    [Serializable]
    class MyTimeDuration
    {
        /// <summary>
        /// 第一次记录到当前任务启动的时间
        /// </summary>
        public TimeSpan StartTime { get; set; } = new TimeSpan(0, 0, 0);

        /// <summary>
        /// 记录离开当前任务的时间
        /// </summary>
        public TimeSpan StopTime { get; set; } = new TimeSpan(0, 0, 0);

        /// <summary>
        /// 记录当前时间执行的任务
        /// </summary>
        public MyTask TimeDurationTask { get; set; } = new MyTask();
    }

    /// <summary>
    /// 任务
    /// </summary>
    [Serializable]
    class MyTask
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