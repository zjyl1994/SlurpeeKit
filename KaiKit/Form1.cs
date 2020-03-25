using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KaiKit
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dilog = new FolderBrowserDialog();
            dilog.Description = "请选择文件夹";
            if (dilog.ShowDialog() == DialogResult.OK || dilog.ShowDialog() == DialogResult.Yes)
            {
                textBox1.Text = dilog.SelectedPath;
            }
        }

        public void AppendLog(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendLog), new object[] { value });
                return;
            }
            if (value != null) {
                this.textBox2.Text += value+"\r\n";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("请先选择软件包");
                return;
            }
            // KaiPack
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "请选择软件包保存位置";
            saveFileDialog.FileName = "app.zip";
            saveFileDialog.Filter = "OmniSD软件包(*.zip)|*.zip";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Process p = new Process();
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = @"kaipack.exe";
                p.StartInfo.Arguments = String.Format("-path=\"{0}\" -output=\"{1}\" --verbose",textBox1.Text, saveFileDialog.FileName.ToString());

                p.OutputDataReceived += new DataReceivedEventHandler((s, de) =>
                {
                    AppendLog(de.Data);
                });
                p.ErrorDataReceived += new DataReceivedEventHandler((s, de) =>
                {
                    AppendLog(de.Data);
                });
                p.SynchronizingObject = this;
                p.EnableRaisingEvents = true;
                p.Exited += (s, a) =>
                {
                    Process proc = (Process)s;
                    if (proc.ExitCode != 0)
                    {
                        MessageBox.Show("打包失败");
                    }
                    else
                    {
                        MessageBox.Show("打包完成");
                    }
                };
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("请先选择软件包");
                return;
            }
            // ADB connect
            {
                Process p = new Process();
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = @"adb.exe";
                p.StartInfo.Arguments = "forward tcp:6000 localfilesystem:/data/local/debugger-socket";

                p.OutputDataReceived += new DataReceivedEventHandler((s, de) =>
                {
                    AppendLog(de.Data);
                });
                p.ErrorDataReceived += new DataReceivedEventHandler((s, de) =>
                {
                    AppendLog(de.Data);
                });

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
            }
            // KaiDeploy
            {
                Process p = new Process();
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = @"kaideploy.exe";
                p.StartInfo.Arguments = String.Format("--socket=localhost:6000 --path=\"{0}\" --launch --verbose", textBox1.Text);

                p.OutputDataReceived += new DataReceivedEventHandler((s, de) =>
                {
                    AppendLog(de.Data);
                });
                p.ErrorDataReceived += new DataReceivedEventHandler((s, de) =>
                {
                    AppendLog(de.Data);
                });
                p.SynchronizingObject = this;
                p.EnableRaisingEvents = true;
                p.Exited += (s, a) =>
                {
                    Process proc = (Process)s;
                    if (proc.ExitCode != 0)
                    {
                        MessageBox.Show("部署失败，请检查是否打开手机调试模式并使用了数据电缆");
                    }
                    else
                    {
                        MessageBox.Show("部署成功");
                    }
                    Console.WriteLine("Exited");
                };
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
            }
        }
    }
}
