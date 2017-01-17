using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace ScanDemo
{
    public partial class Form1 : Form
    {
        #region 声明变量
        //已扫描端口数目
        double scannedCount = 0;
        //正在运行的线程数目
        int runningThreadCount = 0;
      
       
        //最大工作线程数
        static int maxThread = 100;

        //IP地址
        string host = null;
        //端口
        int port = 1234;


        int startIP=1;
        int endIP=255;
        string addresIP = "192.168.1.";
        #endregion

        #region 窗体方法
         public Form1()
        {
            InitializeComponent();
        }       

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = string.Format("扫描IP段指定端口 {0}-{1}:{2}",
                addresIP+startIP,endIP,port);
        }

#endregion

        #region 扫描方法

        public void Scan(string m_host, int m_port)
        {
            //我们直接使用比较高级的TcpClient类
            TcpClient tc = new TcpClient();
            //设置超时时间
            tc.SendTimeout = tc.ReceiveTimeout = 2000;

            try
            {
                //同步方法

                //IPAddress ip = IPAddress.Parse(host);
                //IPEndPoint IPendp = new IPEndPoint(ip, port);
                //tc.Connect(IPendp);

                //异步方法
                IAsyncResult oAsyncResult = tc.BeginConnect(m_host, m_port, null, null);
                oAsyncResult.AsyncWaitHandle.WaitOne(1000, true);//1000为超时时间 

                if (tc.Connected)
                {
                    //如果连接上，证明此端口为开放状态
                    UpdateListBox(listBox1, m_host + ":" + m_port.ToString());
                }
            }
            catch (System.Net.Sockets.SocketException e)
            {
                //容错处理
                //MessageBox.Show("Port {0} is closed", host.ToString());
                //Console.WriteLine(e.Message);
            }
            finally
            {              
                tc.Close();
                tc = null;                
                scannedCount++;
                runningThreadCount--;

            }
        }
        #endregion

        #region 按钮
        private void button1_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                listBox1.Items.Clear();
                scannedCount = 0;               
                runningThreadCount = 0;
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }


        private void button3_Click(object sender, EventArgs e)
        {
            Form2 frm = new Form2();
            frm.SearcheEvent += new Form2.SearchDeletegate(frm_SearcheEvent);
            frm.ShowDialog();
        }

        void frm_SearcheEvent(object sender, SearchEventArgs e)
        {
            startIP= Convert.ToInt32(e.SartIP.Split('.')[3]);
            endIP=Convert.ToInt32(e.EndIP.Split('.')[3]);

            addresIP = e.SartIP.Substring(0,e.SartIP.LastIndexOf('.')+1);
            port = e.Port;

            this.Text = string.Format("wk986 扫描IP段指定端口 {0}-{1}:{2}",
                addresIP + startIP, endIP, port);
        }

        #endregion

        #region 异步控件

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {          
            double total = Convert.ToDouble(endIP-startIP+1);
            for (int ip = startIP; ip <= endIP; ip++)
            {
                if (backgroundWorker1.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                //IP地址段，默认：192.168.1.
                host = addresIP + ip.ToString();

                //带参数的多线程执行
                Thread thread = new Thread(() => Scan(host, port));
                thread.IsBackground = true;
                thread.Start();
                

                UpdateLabText(labTip, string.Format("正在扫描第：{0}台，共{1}台，进度：{2}%",
                        scannedCount, total, Convert.ToInt32((scannedCount / total) * 100)));
                backgroundWorker1.ReportProgress(Convert.ToInt32((scannedCount / total) * 100));
                runningThreadCount++;

                Thread.Sleep(10);
                //循环，直到某个线程工作完毕才启动另一新线程，也可以叫做推拉窗技术
                while (runningThreadCount >= maxThread) ;     
                 
            }

            //空循环，直到所有端口扫描完毕
            do
            {
                UpdateLabText(labTip, string.Format("正在扫描第：{0}台，共{1}台，进度：{2}%",
                        scannedCount, total, Convert.ToInt32((scannedCount / total) * 100)));
                backgroundWorker1.ReportProgress(Convert.ToInt32((scannedCount / total) * 100));
              
                Thread.Sleep(10);
                
            } while (runningThreadCount>0);

            
        }   

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            labTip.Text = "扫描完成！";
        }        

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }
        #endregion

        #region 异步控件显示
        //Label
        delegate void SetLabCallback(Label lb, string text);
        public void UpdateLabText(Label lb, string text)
        {
            try
            {
                if (lb.InvokeRequired)
                {
                    SetLabCallback d = new SetLabCallback(UpdateLabText);
                    this.Invoke(d, new object[] { lb, text });
                }
                else
                {
                    lb.Text = text.Trim();
                }
            }
            catch
            {
            }
        }

        //TextBox
        delegate void SetTextCallback(TextBox txtBox, string text);

        private void UpdateTextBox(TextBox txtBox, string text)
        {
            if (txtBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(UpdateTextBox);
                this.Invoke(d, new object[] { txtBox, text });
            }
            else
            {
                txtBox.Text = text.Trim();
            }
        }

        //ListBox
        delegate void SetListCallback(ListBox lstBox, string text);
        private void UpdateListBox(ListBox lstBox, string text)
        {
            if (lstBox.InvokeRequired)
            {
                SetListCallback d = new SetListCallback(UpdateListBox);
                this.Invoke(d, new object[] { lstBox, text });
            }
            else
            {
                lstBox.Items.Add(text.Trim());
            }
        }
        #endregion

      
       
    }
}
