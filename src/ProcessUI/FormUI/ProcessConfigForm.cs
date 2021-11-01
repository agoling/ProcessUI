using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using ProcessUI.Service;
using UtilsSharp;

namespace ProcessUI.FormUI
{
    /// <summary>
    /// 管理配置界面
    /// </summary>
    public partial class ProcessConfigForm : Form
    {
        private readonly ProcessMainForm _processMainForm;
        private readonly ProcessConfigFormType _type;
        public ProcessConfigForm(ProcessMainForm processMainForm, ProcessConfigFormType type)
        {
            _processMainForm = processMainForm;
            _type = type;
            InitializeComponent();
        }

        private void ProcessConfigForm_Load(object sender, EventArgs e)
        {
            listView1.MultiSelect = true;
            InitListView();
            Text =_type.ToString();
            button1.Text = _type == ProcessConfigFormType.管理配置 ? "添加配置" : "启动进程";
            button2.Visible = _type == ProcessConfigFormType.管理配置;
        }

        /// <summary>
        /// 添加配置、启动进程
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            if (_type == ProcessConfigFormType.管理配置)
            {
                //添加配置
                var processAddForm = new ProcessAddForm(this, ProcessAddFormType.添加);
                processAddForm.Show();
            }
            else
            {
                //启动进程
                SelectDataOperation(c =>
                {
                    var model = new ProcessModel()
                    {
                        Tag = c.Tag,
                        FileName = c.FileName,
                        Arguments = c.Arguments,
                        ProcessCloseType = c.ProcessCloseType,
                        Process = new Process
                        {
                            StartInfo = { FileName = c.FileName, Arguments = c.Arguments }
                        },
                        Hours = c.Hours,
                        Minutes = c.Minutes
                    };
                    model.Process.Start();
                    _processMainForm.AddData(model);
                });
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            SelectDataOperation(c =>
            {
                var json = "";
                if (File.Exists(AppConfig.ProcessConfigFilePath))
                {
                    json = File.ReadAllText(AppConfig.ProcessConfigFilePath);
                }
                var processConfigs = string.IsNullOrEmpty(json) ? new List<ProcessConfig>() : JsonConvert.DeserializeObject<List<ProcessConfig>>(json);

                var config = processConfigs.FirstOrDefault(t => t.Tag == c.Tag && t.Arguments == c.Arguments && t.ProcessCloseType == c.ProcessCloseType && t.FileName == c.FileName);
                if (config != null)
                {
                    processConfigs.Remove(config);
                }
                File.WriteAllText(AppConfig.ProcessConfigFilePath, JsonConvert.SerializeObject(processConfigs));
                MessageBox.Show($@"进程标识：{c.Tag}删除成功！");

            });
            InitListView();
        }

        /// <summary>
        /// 展示数据
        /// </summary>
        internal void InitListView()
        {
            listView1.Clear();   //从控件中移除所有项和列（包括列表头）。
            listView1.Items.Clear();   //只移除所有的项。
            listView1.GridLines = true; //显示表格线
            listView1.View = View.Details;//显示表格细节
            listView1.HeaderStyle = ColumnHeaderStyle.Clickable;//对表头进行设置
            listView1.FullRowSelect = true;//是否可以选择行

            listView1.Columns.Add("进程标识", 170, HorizontalAlignment.Center);
            listView1.Columns.Add("启动参数", 100, HorizontalAlignment.Center);
            listView1.Columns.Add("关闭类型", 110, HorizontalAlignment.Center);
            listView1.Columns.Add("关闭时间", 170, HorizontalAlignment.Center);
            listView1.Columns.Add("程序路径", 600, HorizontalAlignment.Center);
           

            listView1.BeginUpdate();

            var json = "";
            if (File.Exists(AppConfig.ProcessConfigFilePath))
            {
                json = File.ReadAllText(AppConfig.ProcessConfigFilePath);
            }
            var processConfigs = string.IsNullOrEmpty(json) ? new List<ProcessConfig>() : JsonConvert.DeserializeObject<List<ProcessConfig>>(json);

            if (processConfigs != null && processConfigs.Count > 0)
            {
                foreach (var p in processConfigs)
                {
                    var lvi = new ListViewItem { Text = $@"{p.Tag}" };
                    lvi.SubItems.Add(p.Arguments);
                    lvi.SubItems.Add(p.ProcessCloseType.ToString());
                    lvi.SubItems.Add(p.ProcessCloseType != ProcessCloseType.不关闭
                        ? $"{p.Hours}:{p.Minutes}"
                        : p.ProcessCloseType.ToString());
                    lvi.SubItems.Add(p.FileName);
                    
                    listView1.Items.Add(lvi);
                }
            }
            listView1.EndUpdate();
        }

        /// <summary>
        /// 选择数据操作
        /// </summary>
        /// <param name="action">委托处理</param>
        private void SelectDataOperation(Action<ProcessConfig> action)
        {
            var selectData = listView1.SelectedItems;
            if (selectData.Count == 0)
            {
                MessageBox.Show(@"请先选择进程配置,可多选！");
                return;
            }
            for (var i = 0; i < selectData.Count; i++)
            {
                var config = new ProcessConfig
                {
                    Tag = selectData[i].SubItems[0].Text,
                    Arguments = selectData[i].SubItems[1].Text,
                    ProcessCloseType = selectData[i].SubItems[2].Text.ToEnum<ProcessCloseType>(),
                    FileName = selectData[i].SubItems[4].Text
                   
                };
                if (selectData[i].SubItems[3].Text!=@"不关闭")
                {
                    var h = selectData[i].SubItems[3].Text.Split(':')[0];
                    var m = selectData[i].SubItems[3].Text.Split(':')[1];
                    config.Hours = Convert.ToInt32(h);
                    config.Minutes = Convert.ToInt32(m);
                }
                action.Invoke(config);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }

    /// <summary>
    /// ProcessConfigForm类型
    /// </summary>
    public enum ProcessConfigFormType
    {
        管理配置=0,
        启动进程=1
    }
}
