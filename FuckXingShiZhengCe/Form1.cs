using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FuckXingShiZhengCe
{
    public partial class Form1 : Form
    {

        //public string kaoshiurl = @"file:///F:/Work/%E5%BD%A2%E5%8A%BF%E6%94%BF%E7%AD%96/%E5%BD%A2%E5%8A%BF%E6%94%BF%E7%AD%96-s2/2013%E5%B9%B4%E4%B8%8A%E5%8D%8A%E5%B9%B4%E5%BD%A2%E5%8A%BF%E4%B8%8E%E6%94%BF%E7%AD%96%E8%80%83%E8%AF%951.htm";
        public string kaoshiurl = @"http://xg.info.bit.edu.cn/dangke";
        private bool debugInfo = true;
        SortedDictionary<string, int> ans;
        bool ansReady = false;

        public Form1()
        {
            InitializeComponent();
        }

        struct aoption
        {
            public string id;
            public string content;
        }

        void sortlist(ref aoption[] mylist,int count)
        {
            aoption tmp;
            for (int i = count - 1; i > 0; i--)
                for (int j = 0; j < i; j++)
                {
                    if (mylist[j].content.CompareTo(mylist[j+1].content) > 0)
                    {
                        tmp = mylist[j];
                        mylist[j] = mylist[j + 1];
                        mylist[j + 1] = tmp;
                    }
                }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser1.IsBusy)
                return;
            if (e.Url.AbsolutePath.Contains("attempt") == false)
            { 
                return; 
            }

            if (!ansReady)
            {
                MessageBox.Show("题库初始化失败！请联系开发者", "错误！！！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(
                @"^.{1}\.", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            HtmlElement formDom, curQuestion, CurQuestionAwsDiv;
            formDom = webBrowser1.Document.GetElementById("responseform");

            HtmlElementCollection tmp;

            tmp = webBrowser1.Document.GetElementsByTagName("h1");
            int t = searchitem("classname", "headermain", ref tmp);
            if(t == tmp.Count )
            {
                throw new Exception ("fail to get title");
            }
            string title = tmp[t].InnerText;

            HtmlElementCollection alldiv = webBrowser1.Document.GetElementsByTagName("div");

            int lostAns = 0;
            string debugstr = "\"Title\",\"Title_md5\",\"Option_0\",\"Option_1\",\"Option_2\",\"Option_3\",\"Option_4\",\"Option_5\"\r\n";
            for (int i=0; i < alldiv.Count; i++)
            {
                if (alldiv[i].GetAttribute("id").Length > 0 && alldiv[i].GetAttribute("id")[0] != 'q')
                    continue;
                if (!alldiv[i].GetAttribute("classname").Contains("que"))
                    continue;

                //curQuestion = formDom.Document.GetElementById("q" + i.ToString());
                curQuestion = alldiv[i];
                tmp = curQuestion.GetElementsByTagName("div");

                t = searchitem("classname", "qtext", ref tmp);

                string questionTitle = tmp[t].InnerText;
                questionTitle = questionTitle.Replace("\r\n", "");
                questionTitle = questionTitle.Replace(" ", "");
                string questionTitleMd5 = GetMD5(questionTitle);

                t = searchitem("classname", "answer", ref tmp);
                if (t == tmp.Count)
                    throw new Exception("fail to get answerlist");
                CurQuestionAwsDiv = tmp[t];

                HtmlElementCollection AwsContentList
                    = CurQuestionAwsDiv.GetElementsByTagName("label");

                aoption[] tmplist = new aoption[AwsContentList.Count];

                for (int j = 0; j < AwsContentList.Count; j++)
                {
                    tmplist[j].id = AwsContentList[j].GetAttribute("htmlfor");
                    tmplist[j].content = AwsContentList[j].InnerText;
                    tmplist[j].content = reg.Replace(tmplist[j].content, "").Trim();
                }
                sortlist(ref tmplist, AwsContentList.Count);

                if (!ans.ContainsKey(questionTitleMd5))
                {
                    lostAns++;
                    curQuestion.Style = "border: 5px red solid;";
                    debugstr += '"' + questionTitle + "\",";
                    debugstr += '"' + questionTitleMd5 + "\",";

                    for (int k = 0; k < tmplist.Length; k++)
                    {
                        debugstr += '"' + tmplist[k].content + "\",";
                    }
                    debugstr += "\r\n";
                    continue;
                }

                webBrowser1.Document.GetElementById(tmplist[ans[questionTitleMd5]].id).InvokeMember("click");
            }

            if (lostAns > 0)
            {
                MessageBox.Show("共计 " + lostAns.ToString()
                    + " 道题未找到答案。题目已经用红线标出；\r\n请协助提交题库并谨慎答题提交。"
                    , "警告！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //MessageBox.Show(debugstr);
                if (debugInfo)
                {
                    Form2 debugtool = new Form2();
                    debugtool.setDebugInfo(debugstr);
                    debugtool.ShowDialog();
                    debugtool.Dispose();
                }
            }
        }

        private int searchitem(string attrname, string matchstr, ref HtmlElementCollection hecollection)
        {
            if (attrname == "class") attrname = "classname";
            if (attrname == "for") attrname = "htmlfor";
            for (int i = 0; i < hecollection.Count; i++)
            {
                if (hecollection[i].GetAttribute(attrname) == matchstr)
                    return i;
            }
            return hecollection.Count;
        }

        private int getans(int ansid, ref string[] ans)
        {
            return 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DialogResult warning = MessageBox.Show("本次形势政策更换了新的系统，所以答题软件为临时编写。" +
                                        "时间匆忙，难免出现bug。\r\n    本次答题系统仅适用于 2013年上半年 " +
                                        "\r\n    千万注意不用误用错误的版本。\r\n\r\n答案仅供参考，请谨慎提交！" +
                                        "\r\nPS：题库不一定全。" +
                                        "\r\n    如果遇到漏题或没有答题，尝试左下角的 刷新页面 。" +
                                        "\r\n    如果卡在登录or其他地方，尝试左下角的 重新载入。" +
                                        "\r\n    提交前检查是否答题完整！！！"
                                        , "警告！！！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            webBrowser1.Url = new Uri(kaoshiurl);
            int ansCount = 0;
            ans = initializationAnswer(ref ansCount);

            toolStripStatusLabel2.Text = "仅适用于：2013年上半年考试  题库总共收集：" + ansCount.ToString() + "题";
            debugInfo = false;
            toolStripStatusLabel4.Text = "调试信息：关闭";
        }

        private void toolStripStatusLabel4_Click(object sender, EventArgs e)
        {
            debugInfo = !debugInfo;
            if (debugInfo)
            {
                toolStripStatusLabel4.Text = "调试信息：打开";
            }
            else
            {
                toolStripStatusLabel4.Text = "调试信息：关闭";
            }
        }

        public static string GetMD5(string myString)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] fromData = System.Text.Encoding.UTF8  .GetBytes(myString);
            byte[] targetData = md5.ComputeHash(fromData);
            string byte2String = null;

            for (int i = 0; i < targetData.Length; i++)
            {
                byte2String += targetData[i].ToString("x2");
            }

            return byte2String;
        }

        private SortedDictionary<string, int> initializationAnswer(ref int count)
        {
            SortedDictionary<string, int> ans = new SortedDictionary<string, int>();
            int ctmp = 0;

            //ans.Add("c2d831b23f00eff0ccf858e74610a5e4", 0);

            System.IO.StringReader ra = new System.IO.StringReader(
                Properties.Resources.tiku_txt);

            for (string i; (i = ra.ReadLine()) != null; )
            {
                string[] strs = i.Split(',');
                int tmpans;
                if(!int.TryParse(strs[1], out tmpans))
                    throw new Exception();
                ans.Add(strs[0], tmpans);
                ctmp++;
            }

            //ans.Add("65630fc976dd16a0d8146418d00ba0fd", 1);
            //ans.Add("3143e85abdc19b332bbdec8d4f246063", 2);
            //ans.Add("e1613182a0e61d6c21e08a3f947326b1", 0);
            //ans.Add("5222df48ebb34c20719fd96b4f7fa904", 0);
            //ans.Add("daf1b9670e8284cbb320976976ed6e54", 0);
            count = ctmp;
            ansReady = true;
            return ans;
        }

        private void toolStripStatusLabel3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://weibo.com/mythicalh");
        }

        private void httpweibocommythicalhToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://weibo.com/mythicalh");
        }

        private void nP导航ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://nav.bitnp.net");
        }

        private void 北理百科词条ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://baike.bitnp.net/%E5%BD%A2%E5%8A%BF%E4%B8%8E%E6%94%BF%E7%AD%96");
        }

        private void 重新载入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            webBrowser1.Url = new Uri(kaoshiurl);
        }

        private void httpweibocomu1788312393ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://weibo.com/u/1788312393");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Windows.Forms.DialogResult re;
            re =
                MessageBox.Show("支持一下我们\n把NP导航设为首页或者来北理百科编辑一下词条吧！" +
                "\n\n NP导航: nav.bitnp.net\n北理百科: baike.bitnp.net" +
                "\n\n点击 是 打开【NP导航】"
                , "支持我们", MessageBoxButtons.YesNo);

            if (re == System.Windows.Forms.DialogResult.Yes)
            {
                System.Diagnostics.Process.Start("http://nav.bitnp.net");
            }
        }

        private void 刷新页面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            webBrowser1.Refresh();
        }


    }
}
