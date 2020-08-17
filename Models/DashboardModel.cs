using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Frogy.Models
{
    class DashboardModel
    {

    }

    /// <summary>
    /// MainPage中概览的数据模型
    /// </summary>
    public class OverViewItem
    {
        public BitmapImage AppIcon { get; set; }

        public string AppName { get; set; }

        public string AppDuration { get; set; }
    }

    public class DetailsViewItem
    {
        public BitmapImage AppIcon { get; set; }

        public string AppName { get; set; }

        public string AppDuration { get; set; }

        public string StartTime { get; set; }

        public string StopTime { get; set; }

        public string WindowTitle { get; set; }

        public string SystemState { get; set; }
    }
}
