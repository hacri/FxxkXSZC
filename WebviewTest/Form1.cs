using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WebviewTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            webBrowser1.DocumentText = Properties.Resources.homepage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            webBrowser1.Url = new Uri("file:///C:/Users/HacRi/Desktop/xszc2013b/%E5%BD%A2%E5%8A%BF2013%E4%B8%8B7.htm?attempt=18636&page=6");
            //webBrowser1.Url = new Uri("http://www.w3school.com.cn/html/html_forms.asp");
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void webBrowser1_NewWindow(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            System.Diagnostics.Process.Start(webBrowser1.StatusText);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            webBrowser1.DocumentText = Properties.Resources.homepage;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            HtmlElementCollection allInput = webBrowser1.Document.GetElementsByTagName("input");
            for (int i = 0; i < allInput.Count; i++)
            {
                if (allInput[i].Id == "q18659:42_choice3")
                {
                    allInput[i].InvokeMember("click");
                    break;
                }
            }
        }
    }
}
