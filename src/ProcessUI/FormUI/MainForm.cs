using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProcessUI.FormUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public Process[] procList, s;
        public int i = 1;
        string[] paths = new string[10];

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                i = listBox1.SelectedIndex;
                try
                {
                    procList[i].Kill();
                }
                catch (Exception)
                {
                    MessageBox.Show("拒绝访问", "error");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBox1.SelectedIndex != -1)
                {
                    i = listBox1.SelectedIndex;
                    label1.Text = "进程名:" + procList[i].ProcessName + "\n优先级:" + procList[i].BasePriority + "\n开始时间:" + procList[i].StartTime + "\nPID:" + procList[i].Id;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("无法获取该进程信息", "error");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)//获取监视目录
            {

                //textBox1.Text = folderBrowserDialog1.SelectedPath;
                //fileSystemWatcher1.Path = textBox1.Text;

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
          
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            listBox1.Items.Clear();//清除列表框
            procList = Process.GetProcesses();//获取进程
            foreach (Process s in procList)
            {
                listBox1.Items.Add(s.ProcessName);
            }
            listBox1.SetSelected(i, true);
        }
    }
}
