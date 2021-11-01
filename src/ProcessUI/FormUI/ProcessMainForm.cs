using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FluentScheduler;
using ProcessUI.SchedulerJob;
using ProcessUI.Service;
using UtilsSharp;

namespace ProcessUI.FormUI
{
    /// <summary>
    /// 进程主界面
    /// </summary>
    public partial class ProcessMainForm : Form
    {

        //委托添加数据
        public delegate void AddDataDelegate(ProcessModel p);
        //委托添加数据对象
        public AddDataDelegate AddDataDelegate1; 

        public ProcessMainForm()
        {
            InitializeComponent();
            toolStripDropDownButton1.ToolTipText = "";
            toolStripDropDownButton2.ToolTipText = "";

            //委托初始化
            AddDataDelegate1 = new AddDataDelegate(AddData);

            //初始化
            TimingService.ProcessModels=new List<ProcessModel>();
            JobManager.Initialize(new SchedulerFactory(this));

            //导航菜单
            toolStripMenuItem1.Click += ToolStripMenuItem1_Click;
            toolStripMenuItem2.Click += ToolStripMenuItem2_Click;

            //右键菜单
            doubleBufferListView1.MouseClick += DoubleBufferListView1_MouseClick;
            toolStripMenuItem3.Click += ToolStripMenuItem3_Click;

            //初始化列表展示组件
            InitDoubleBufferListView();

        }

        /// <summary>
        /// 结束进程
        /// </summary>
        private void ToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (doubleBufferListView1.SelectedItems.Count == 0)
            {
                MessageBox.Show(@"请先选择进程配置,可多选！");
                return;
            }
            while (doubleBufferListView1.SelectedItems.Count > 0)
            {
                var item = doubleBufferListView1.SelectedItems[0];
                RemoveData(item);
            }
        }

        /// <summary>
        /// doubleBufferListView点击事件
        /// </summary>
        private void DoubleBufferListView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(doubleBufferListView1, e.Location);//鼠标右键按下弹出菜单
            }
        }

        /// <summary>
        /// 添加配置
        /// </summary>
        private void ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var processConfigForm = new ProcessConfigForm(this,ProcessConfigFormType.管理配置);
            processConfigForm.Show();
            timer1.Start();
        }

        /// <summary>
        /// 启动进程
        /// </summary>
        private void ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            var processConfigForm = new ProcessConfigForm(this, ProcessConfigFormType.启动进程);
            processConfigForm.Show();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitDoubleBufferListView()
        {
            doubleBufferListView1.Clear();   //从控件中移除所有项和列（包括列表头）。
            doubleBufferListView1.Items.Clear();   //只移除所有的项。
            doubleBufferListView1.GridLines = true; //显示表格线
            doubleBufferListView1.View = View.Details;//显示表格细节
            doubleBufferListView1.HeaderStyle = ColumnHeaderStyle.Clickable;//对表头进行设置
            doubleBufferListView1.FullRowSelect = true;//是否可以选择行

            doubleBufferListView1.Columns.Add("进程Id", 70, HorizontalAlignment.Center);
            doubleBufferListView1.Columns.Add("进程标识", 170, HorizontalAlignment.Center);
            doubleBufferListView1.Columns.Add("进程名称", 120, HorizontalAlignment.Center);
            doubleBufferListView1.Columns.Add("启动参数", 100, HorizontalAlignment.Center);
            doubleBufferListView1.Columns.Add("关闭类型", 110, HorizontalAlignment.Center);
            doubleBufferListView1.Columns.Add("关闭时间", 110, HorizontalAlignment.Center);
            doubleBufferListView1.Columns.Add("启动时间", 130, HorizontalAlignment.Center);
            doubleBufferListView1.Columns.Add("已运行时间", 130, HorizontalAlignment.Center);
            doubleBufferListView1.Columns.Add("程序路径", 600, HorizontalAlignment.Center);
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="p">p</param>
        internal void AddData(ProcessModel p)
        {
            doubleBufferListView1.BeginUpdate();
            var item = new ListViewItem { Text = $@"{p.Process.Id}" };
            item.SubItems.Add(p.Tag);
            item.SubItems.Add(p.Process.ProcessName);
            item.SubItems.Add(p.Process.StartInfo.Arguments);
            item.SubItems.Add(p.ProcessCloseType.ToString());
            item.SubItems.Add(p.ProcessCloseType != ProcessCloseType.不关闭
                ? $"{p.Hours}:{p.Minutes}"
                : p.ProcessCloseType.ToString());
            item.SubItems.Add(p.Process.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
            var totalRunTime = TimeHelper.GetTimeSpan(p.Process.StartTime, DateTime.Now);
            item.SubItems.Add($"{totalRunTime.Days}天{totalRunTime.Hours}时{totalRunTime.Minutes}分{totalRunTime.Seconds}秒");
            item.SubItems.Add(p.FileName);
            doubleBufferListView1.Items.Add(item);
            doubleBufferListView1.EndUpdate();
            TimingService.ProcessModels.Add(p);
        }

        /// <summary>
        /// 进程是否存在
        /// </summary>
        /// <param name="item">item</param>
        /// <returns></returns>
        private bool IsExist(ListViewItem item)
        {
            var processId = Convert.ToInt32(item.SubItems[0].Text);
            var model= TimingService.ProcessModels.FirstOrDefault(t => t.Process.Id == processId);
            if (model==null) return false;
            return !model.Process.HasExited;
        }


        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="item">项</param>
        internal void RemoveData(ListViewItem item)
        {
            var processId = Convert.ToInt32(item.SubItems[0].Text);
            var model = TimingService.ProcessModels.FirstOrDefault(t => t.Process.Id == processId);
            if (model == null)
            {
                doubleBufferListView1.Items.Remove(item);
                return;
            }
            if (model.Process.HasExited)
            {
                doubleBufferListView1.Items.Remove(item);
                TimingService.ProcessModels.Remove(model);
                return;
            }
            model.Process.Kill();
            doubleBufferListView1.Items.Remove(item);
            TimingService.ProcessModels.Remove(model);
        }

        /// <summary>
        /// 定时刷新
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {
            //刷新列表
            for (var i = 0; i < doubleBufferListView1.Items.Count; i++)
            {
                try
                {
                    var item = doubleBufferListView1.Items[i];
                    var isExist = IsExist(item);
                    if (!isExist)
                    {
                        RemoveData(doubleBufferListView1.Items[i]);
                        return;
                    }
                    var startTime = Convert.ToDateTime(doubleBufferListView1.Items[i].SubItems[5].Text);
                    var totalRunTime = TimeHelper.GetTimeSpan(startTime, DateTime.Now);
                    doubleBufferListView1.Items[i].SubItems[6].Text = $@"{totalRunTime.Days}天{totalRunTime.Hours}时{totalRunTime.Minutes}分{totalRunTime.Seconds}秒";
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("is not running")) continue;
                    RemoveData(doubleBufferListView1.Items[i]);
                    return;
                }
            }
        }

        /// <summary>
        /// 关闭窗口事件
        /// </summary>
        private void ProcessMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show(@"关闭主窗口将关闭所有从这里开启的进程，确定要关闭？", @"关闭提醒", MessageBoxButtons.YesNo,MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (TimingService.ProcessModels == null ||TimingService.ProcessModels.Count <= 0) return;
                foreach (var item in TimingService.ProcessModels.Where(item => !item.Process.HasExited))
                {
                    item.Process.Kill();
                }
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
