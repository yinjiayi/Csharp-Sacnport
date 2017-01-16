using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace ScanPort
{
    public partial class Form1 : Form
    {
        int ip1, ip2, ip3, ip4;

        string[] t = new string[5];
        string ss = null;
       
        Thread fThread;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Text = null;
            ss = this.textBox1.Text;

            this.richTextBox1.Text = "正在进行扫描，请稍候......\n";
            fThread = new Thread(new ThreadStart(runs));

            fThread.Start();


           
        }
        public void ipstart_get(string ff)
        {
            t =ff.Split('.');
            ip1 = Convert.ToInt16(t[0].ToString().Trim());
            ip2 = Convert.ToInt16(t[1].ToString().Trim());
            ip3 = Convert.ToInt16(t[2].ToString().Trim());
            ip4 = Convert.ToInt16(t[3].ToString().Trim());
            
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void IPAdd() 
        {
            if (++ip4 > 255)
            {
                ip3++;
                ip4 = 1;
            }

            if (ip3 > 255)
            {
                ip2++;
                ip3 = 1;
            }

            if (ip2 > 255)
            {
                ip1++;
                ip2 = 1;
            }

            if (ip1 > 255)
            {
                ip1 = 1;
            }
        }   
        public void Startscan(string yy)
        {
            Int32 port = Convert.ToInt32(comboBox1.Text.Trim());
            try
            {
                TcpClient tcp = new TcpClient(); 
                tcp.Connect(this.textBox1.Text, port);
                this.richTextBox1.AppendText(ss+"   端口：" + port.ToString() + "开放\n");
               
            }
            catch
            {
                this.richTextBox1.AppendText(ss+"  端口：" + port.ToString() + "未开放\n");
               
            }

        }

        public void runs()
        {
            while (true) 
            {
                
                if (!ss.Equals(this.textBox2.Text))
                {
                    Startscan(ss);
                    ipstart_get(ss);
                    IPAdd();
                    ss = ip1.ToString() + "." + ip2.ToString() + "." + ip3.ToString() + "." + ip4.ToString();
                   
                }
                else
                 {
                    Startscan(ss);
                     break;
                 }
            }
        }
    }
}