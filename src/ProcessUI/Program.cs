using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentScheduler;
using ProcessUI.FormUI;
using ProcessUI.SchedulerJob;

namespace ProcessUI
{
    static class Program
    {
        public static ProcessMainForm _processMainForm=new ProcessMainForm();
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(_processMainForm);
        }
    }
}
