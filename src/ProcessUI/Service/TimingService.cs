using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ProcessUI.Control;

namespace ProcessUI.Service
{
    public class TimingService
    {
        public static List<ProcessModel> ProcessModels { set; get; }

    }


    public class ProcessConfig
    {
        public string Tag { set; get; }

        /// <summary>
        /// 组
        /// </summary>
        public string Group { set; get; }

        public string FileName { set; get; }

        public string Arguments { set; get; }

        public ProcessCloseType ProcessCloseType { set; get; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
    }

    public class ProcessModel : ProcessConfig
    {
        public Process Process { set; get; }
    }

    /// <summary>
    /// 进程关闭类型
    /// </summary>
    public enum ProcessCloseType
    {
        不关闭 = 0,
        定时关闭 = 1,
        定时关闭后启动 = 2
    }
}
