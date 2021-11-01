using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FluentScheduler;
using Newtonsoft.Json;
using ProcessUI.SchedulerJob;
using ProcessUI.Service;
using Convert = System.Convert;

namespace ProcessUI.FormUI
{
    /// <summary>
    /// 添加界面
    /// </summary>
    public partial class ProcessAddForm : Form
    {
        private readonly ProcessConfigForm _processConfigForm;
        private readonly ProcessAddFormType _type;

        public ProcessAddForm(ProcessConfigForm processConfigForm, ProcessAddFormType type)
        {
            _processConfigForm = processConfigForm;
            _type = type;
            InitializeComponent();
        }

        private void AddProcessForm_Load(object sender, EventArgs e)
        {
            if (_type == ProcessAddFormType.添加)
            {
                Text = @"添加配置";
                button2.Text = @"添加";
            }
            else
            {
                Text = @"编辑配置";
                button2.Text = @"修改";
            }
            openFileDialog1.FileName = "";
            openFileDialog1.Multiselect = false;
            openFileDialog1.Filter = @"文件|*.exe;";
            textBox4.Text = @"23";
            textBox5.Text = @"50";
        }

        //浏览
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            textBox2.Text = openFileDialog1.FileName;
        }

        //添加配置或者启动进程
        private void button2_Click(object sender, EventArgs e)
        {


            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show(@"进程标识不能为空");
                return;
            }
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show(@"请先选择程序路径");
                return;
            }
            if (!Regex.IsMatch(textBox4.Text, @"\d+") || !Regex.IsMatch(textBox5.Text, @"\d+"))
            {
                MessageBox.Show(@"时间格式错误！");
                return;
            }
            var t = Convert.ToInt32(textBox4.Text);
            var m = Convert.ToInt32(textBox5.Text);
            if (t < 0 || t > 24 || m < 0 || m > 60)
            {
                MessageBox.Show(@"请输入正确的时间!");
                return;
            }

            if (t == 0 || t == 24)
            {
                t = 00;
            }
            if (m == 0 || m == 60)
            {
                m = 00;
            }

            var config = new ProcessConfig
            {
                Tag = textBox1.Text,
                FileName = textBox2.Text,
                Arguments = textBox3.Text,
                Hours = t,
                Minutes = m
            };

            if (radioButton1.Checked)
            {
                config.ProcessCloseType = ProcessCloseType.不关闭;
            }
            else if (radioButton2.Checked)
            {
                config.ProcessCloseType = ProcessCloseType.定时关闭;
            }
            else if (radioButton3.Checked)
            {
                config.ProcessCloseType = ProcessCloseType.定时关闭后启动;
            }
            else
            {
                MessageBox.Show(@"请先选择关闭类型！");
                return;
            }

            var json = "";
            if (File.Exists(AppConfig.ProcessConfigFilePath))
            {
                json = File.ReadAllText(AppConfig.ProcessConfigFilePath);
            }
            var processConfigs = string.IsNullOrEmpty(json) ? new List<ProcessConfig>() : JsonConvert.DeserializeObject<List<ProcessConfig>>(json);
            if (processConfigs.Exists(t => t.Tag == config.Tag && t.Arguments == config.Arguments && t.ProcessCloseType == config.ProcessCloseType && t.FileName == config.FileName))
            {
                MessageBox.Show($@"进程标识：{config.Tag}已存在");
                return;
            }
            processConfigs.Add(config);
            File.WriteAllText(AppConfig.ProcessConfigFilePath, JsonConvert.SerializeObject(processConfigs));
            MessageBox.Show($@"进程标识：{config.Tag}{button2.Text}成功！");
            _processConfigForm?.InitListView();
            JobManager.Initialize(new SchedulerFactory(Program._processMainForm));
        }

        //重置
        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            openFileDialog1.FileName = "";
            radioButton1.Checked = true;
            textBox4.Text = @"23";
            textBox5.Text = @"50";
            textBox4.Enabled = false;
            textBox5.Enabled = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            textBox4.Enabled = true;
            textBox5.Enabled = true;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            textBox4.Enabled = true;
            textBox5.Enabled = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            textBox4.Text = @"23";
            textBox5.Text = @"50";
            textBox4.Enabled = false;
            textBox5.Enabled = false;
        }
    }

    /// <summary>
    /// 添加还是启动
    /// </summary>
    public enum ProcessAddFormType
    {
        添加 = 0,
        编辑 = 1
    }

}
