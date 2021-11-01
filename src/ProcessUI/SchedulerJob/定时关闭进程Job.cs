using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProcessUI.FormUI;
using ProcessUI.Service;

namespace ProcessUI.SchedulerJob
{
    /// <summary>
    /// 定时任务
    /// </summary>
    public class 定时关闭进程Job
    {
        private ProcessMainForm _f;
        public 定时关闭进程Job(ProcessMainForm f)
        {
            _f = f;
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        public void Execute()
        {
            if (TimingService.ProcessModels == null || TimingService.ProcessModels.Count <= 0) return;
            var currentProcessModels = new List<ProcessModel>();
            currentProcessModels.AddRange(TimingService.ProcessModels.Where(w => w.Hours == DateTime.Now.Hour && w.Minutes == DateTime.Now.Minute));
            for (var i = 0; i < currentProcessModels.Count; i++)
            {
                var item = currentProcessModels[i];
                if (item.Process.HasExited)
                {
                    TimingService.ProcessModels.Remove(item);
                    continue;
                }

                //if (DateTime.Now.Hour < item.Hours || DateTime.Now.Minute < item.Minutes) continue;
                if (item.ProcessCloseType == ProcessCloseType.定时关闭)
                {
                    item.Process.Kill();
                    TimingService.ProcessModels.Remove(item);
                    continue;
                }
                if (item.ProcessCloseType == ProcessCloseType.定时关闭后启动)
                {
                    item.Process.Kill();
                    TimingService.ProcessModels.Remove(item);
                    var model = new ProcessModel()
                    {
                        Tag = item.Tag,
                        FileName = item.FileName,
                        Arguments = item.Arguments,
                        ProcessCloseType = item.ProcessCloseType,
                        Process = new Process
                        {
                            StartInfo = { FileName = item.FileName, Arguments = item.Arguments }
                        },
                        Hours = item.Hours,
                        Minutes = item.Minutes
                    };
                    model.Process.Start();
                    _f.Invoke(_f.AddDataDelegate1, new object[] { model });
                }

            }
        }
    }
}
