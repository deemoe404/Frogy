using Frogy.Methods;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Frogy.Classes
{
    /// <summary>
    /// 时间线
    /// </summary>
    [Serializable]
    public class MyDay
    {
        /// <summary>
        /// 今日时间线
        /// </summary>
        public List<MyTimeDuration> TimeLine = new List<MyTimeDuration>();

        /// <summary>
        /// 今日App计时器 Key为AppName+AppPath, Value为今日剩余时间
        /// </summary>
        //public Dictionary<string, TimeSpan> AppTimers = new Dictionary<string, TimeSpan>();

        /// <summary>
        /// 获取今日小结，即为时间线内相同软件使用时间合并后的列表
        /// </summary>
        /// <returns>今日小结 - Dictionary<string, TimeSpan></returns>
        public Dictionary<string, Software> GetOverView()
        {
            List<MyTimeDuration> myTimeDurations = TimeLine;
            Dictionary<string, Software> overView = new Dictionary<string, Software>();

            foreach (MyTimeDuration item in myTimeDurations)
            {
                string nowAppName = item.TimeDurationTask.ApplicationName;
                if (string.IsNullOrEmpty(nowAppName) || nowAppName == "Frogy") continue;

                TimeSpan duration = item.Duration;
                if (overView.ContainsKey(nowAppName))
                    overView[nowAppName].Duration += duration;

                else
                {
                    Software result =
                        new Software()
                        {
                            Duration = item.Duration,
                            Name = nowAppName,
                            Icon = MyDataHelper.BitmapToBitmapImage(
                                MyDataHelper.Base64StringToImage(item.TimeDurationTask.ApplicationIcon_Base64))
                        };
                    overView.Add(nowAppName, result);
                }
            }
            return overView;
        }

        public List<MyTimeDuration> GetTimeline()
        {
            return new List<MyTimeDuration>(TimeLine.ToArray());
        }
    }

    /// <summary>
    /// 时间段
    /// </summary>
    [Serializable]
    public class MyTimeDuration
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
        /// 任务持续时间
        /// </summary>
        public TimeSpan Duration 
        { 
            get 
            { 
                return StopTime - StartTime; 
            } 
        }

        /// <summary>
        /// 记录当前时间执行的任务
        /// </summary>
        public MyTask TimeDurationTask { get; set; } = new MyTask();
    }

    /// <summary>
    /// 任务
    /// </summary>
    [Serializable]
    public class MyTask
    {
        /// <summary>
        /// 任务软件名称
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// 任务窗口名
        /// </summary>
        public string ApplicationTitle { get; set; }

        /// <summary>
        /// 程序EXE路径名
        /// </summary>
        public string ApplicationFilePath { get; set; }

        /// <summary>
        /// 程序图标
        /// </summary>
        public string ApplicationIcon_Base64 { get; set; }

        /// <summary>
        /// 电脑状态。1=活动，0=锁定。
        /// </summary>
        public int ComputerStatus { get; set; } = 1;
    }

    /// <summary>
    /// 软件
    /// </summary>
    public class Software
    {
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
        public BitmapImage Icon { get; set; }
    }
}