using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentScheduler;
using Newtonsoft.Json;
using ProcessUI.FormUI;
using ProcessUI.Service;

namespace ProcessUI.SchedulerJob
{
    /// <summary>
    /// 定时任务业务工厂
    /// </summary>
    public class SchedulerFactory : Registry
    {
        private static List<string> _list = new List<string>();
        public SchedulerFactory(ProcessMainForm f)
        {
            //Schedule(() => new 定时关闭进程Job(f).Execute()).ToRunEvery(1).Days().At(23, 55);
            var json = "";
            if (File.Exists(AppConfig.ProcessConfigFilePath))
            {
                json = File.ReadAllText(AppConfig.ProcessConfigFilePath);
            }
            var processConfigs = string.IsNullOrEmpty(json) ? new List<ProcessConfig>() : JsonConvert.DeserializeObject<List<ProcessConfig>>(json);
            if (processConfigs != null && processConfigs.Count > 0)
            {

                foreach (var p in processConfigs.Where(w => w.ProcessCloseType != ProcessCloseType.不关闭))
                {
                    if (!_list.Contains($"{p.Hours}:{p.Minutes}"))
                    {
                        Schedule(() => new 定时关闭进程Job(f).Execute()).ToRunEvery(1).Days().At(p.Hours, p.Minutes);
                        _list.Add($"{p.Hours}:{p.Minutes}");
                    }

                }
            }
        }
    }
}
