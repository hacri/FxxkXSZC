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
        // 取消翻页和判断
        bool debugmode = false;

        //public string kaoshiurl = @"file:///C:/Users/HacRi/Desktop/xszc2013b/%E5%BD%A2%E5%8A%BF2013%E4%B8%8B2.htm?attempt=0&page=44";
        public string kaoshiurl = @"http://xg.info.bit.edu.cn/dangke";
        private bool debugInfo = true;
        SortedDictionary<string, que_ans> ans;
        bool ansReady = false;
        string debugstr = "\"Title_md5\",\"Question_Type\",\"Title\",\"Option_0\",\"Option_1\",\"Option_2\",\"Option_3\",\"Option_4\",\"Option_5\"\r\n";
        bool debugupdate = false;
        int[] auto_Ans_Status = new int[45];
        int auto_Ans = 0;
        int current_attempt = 0;

        public Form1()
        {
            InitializeComponent();
        }

        struct aoption
        {
            public string id;
            public string content;
        }

        struct que_ans
        {
            public int num;
            public int[] anslist;
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

        void init_attempt(int num)
        {
            for (int i = 0; i < 45; i++)
                auto_Ans_Status[i] = 0;
            current_attempt = num;
            auto_Ans = 0;
            if (debugmode == false)
            {
                MessageBox.Show("提示：软件会自动翻页，所以答题完成提示前请不要操作。但是如果遇到问题中断（如1分钟保持不动），可以尝试刷新页面或者手动点“向后”按钮",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser1.IsBusy)
                return;
            if (e.Url.AbsolutePath == "blank")
                return;
            if (!debugmode && e.Url.AbsolutePath.Contains("attempt") == false)
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

            System.Text.RegularExpressions.Regex reg_page = new System.Text.RegularExpressions.Regex(
                @"page=(?<page>\d*)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            System.Text.RegularExpressions.Regex reg_attempt = new System.Text.RegularExpressions.Regex(
                @"attempt=(?<attempt>\d*)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            string attemptOri = reg_attempt.Match(e.Url.AbsoluteUri).Groups["attempt"].Value;
            int attempt;
            int.TryParse(attemptOri, out attempt);
            string pageOri = reg_page.Match(e.Url.AbsoluteUri).Groups["page"].Value;
            int page;
            int.TryParse(pageOri, out page);

            bool info = false;

            if (attempt != current_attempt)
            {
                init_attempt(attempt);
            }
            else if (auto_Ans == 0 && page == 44)
            {
                auto_Ans = 1;
                info = true;
            }

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

            int lostAns = 0;
            HtmlElementCollection alldiv = webBrowser1.Document.GetElementsByTagName("div");
            HtmlElementCollection allInput = webBrowser1.Document.GetElementsByTagName("input");
            
            for (int i=0; i < alldiv.Count; i++)
            {
                if (alldiv[i].GetAttribute("id").Length > 0 && alldiv[i].GetAttribute("id")[0] != 'q')
                    continue;
                if (!alldiv[i].GetAttribute("classname").Contains("que"))
                    continue;

                //curQuestion = formDom.Document.GetElementById("q" + i.ToString());
                curQuestion = alldiv[i];
                tmp = curQuestion.GetElementsByTagName("div");

                // 题目
                t = searchitem("classname", "qtext", ref tmp);
                string questionTitle = tmp[t].InnerText;
                questionTitle = questionTitle.Replace("\r\n", "");
                questionTitle = questionTitle.Replace(" ", "");
                string questionTitleMd5 = GetMD5(questionTitle);

                // 分数
                t = searchitem("classname", "grade", ref tmp);
                string questionTypeOri = tmp[t].InnerText;
                string questionType = "";
                if (questionTypeOri == "满分1.00") { questionType = "1"; }
                else { questionType = "2"; }
                
                // 答案列
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
                    debugstr += '"' + questionTitleMd5 + "\",";
                    debugstr += '"' + questionType + "\",";
                    debugstr += '"' + questionTitle + "\",";
                    
                    for (int k = 0; k < tmplist.Length; k++)
                    {
                        debugstr += '"' + tmplist[k].content + "\",";
                    }
                    debugstr += "\r\n";
                    debugupdate = true;
                    continue;
                }

                for (int j = 0; j < ans[questionTitleMd5].num; j++)
                {
                    for (int k = 0; k < allInput.Count; k++)
                    {
                        if (allInput[k].Id == tmplist[ans[questionTitleMd5].anslist[j]].id)
                        {
                            if (allInput[k].GetAttribute("checked").ToLower() == "true")
                            {
                            }
                            else
                            {
                                allInput[k].InvokeMember("click");
                                break;
                            }
                        }
                    }
                }

            }

            if (lostAns > 0)
            {
                /*
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
                 * */
                auto_Ans_Status[page] = 0;
            }
            else
            {
                auto_Ans_Status[page] = 1;
            }

            // 翻页
            if (auto_Ans == 0 && debugmode == false)
            {
                for (int i = 0; i < alldiv.Count; i++)
                {
                    if (alldiv[i].GetAttribute("classname").Contains("submitbtns"))
                    {
                        alldiv[i].GetElementsByTagName("input")[0].InvokeMember("click");
                        break;
                    }
                }
            }

            // 提示
            if (info == true)
            {
                string info_str = "";
                int errpage = 0;
                for (int i = 0; i < 45; i++)
                {
                    if (auto_Ans_Status[i] == 0)
                    {
                        info_str += (i+1).ToString() + ",";
                        errpage++;
                    }
                }
                if (errpage > 0)
                {
                    MessageBox.Show("以下页面未找到答案或者答题发生意外： \r\n" + info_str
                        + "\r\n请协助提交题库并谨慎答题提交。"
                        , "警告！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (debugInfo && debugupdate)
                    {
                        Form2 debugtool = new Form2();
                        debugtool.setDebugInfo(debugstr);
                        debugtool.ShowDialog();
                        debugtool.Dispose();
                    }
                }
                else
                {
                    MessageBox.Show("辅助答题已经完成，请仔细检查是否有错误并谨慎提交。"
                        , "警告！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            DialogResult warning = MessageBox.Show("本次形势政策系统又做了改版，所以答题软件改动不小。" +
                                        "时间匆忙，难免出现bug。\r\n    本次答题系统仅适用于 2013年下半年 " +
                                        "\r\n    千万注意不用误用错误的版本。\r\n\r\n答案仅供参考，请谨慎提交！" +
                                        "\r\n PS：题库不一定全。" +
                                        "\r\n    如果遇到漏题或没有答题，尝试左下角的 刷新页面 。" +
                                        "\r\n    如果卡在登录or其他地方，尝试左下角的 重新载入。" +
                                        "\r\n    提交前检查是否答题完整！！！"
                                        , "警告！！！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            
            
            int ansCount = 0;
            ans = initializationAnswer(ref ansCount);

            toolStripStatusLabel2.Text = "仅适用于：2013年下半年考试  题库总共 " + ansCount.ToString() + " 题";
            debugInfo = true;
            toolStripStatusLabel4.Text = "调试信息：关闭";
            toolStripStatusLabel4.Text = "调试信息：打开";

            webBrowser1.DocumentText = Properties.Resources.homepage;
            //webBrowser1.Url = new Uri(kaoshiurl);
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
            byte[] fromData = System.Text.Encoding.UTF8.GetBytes(myString);
            byte[] targetData = md5.ComputeHash(fromData);
            string byte2String = null;

            for (int i = 0; i < targetData.Length; i++)
            {
                byte2String += targetData[i].ToString("x2");
            }

            return byte2String;
        }

        private SortedDictionary<string, que_ans> initializationAnswer(ref int count)
        {
            SortedDictionary<string, que_ans> ans = new SortedDictionary<string, que_ans>();
            int ctmp = 0;

            System.IO.StringReader ra = new System.IO.StringReader(
                Properties.Resources.tiku_txt);

            for (string i; (i = ra.ReadLine()) != null; )
            {
                string[] strs = i.Split(',');
                int nums;
                if(!int.TryParse(strs[1], out nums))
                    throw new Exception();

                que_ans tmpans = new que_ans();
                tmpans.num=nums;
                tmpans.anslist = new int[nums];
                for (int j = 0; j < nums; j++)
                {
                    if (!int.TryParse(strs[2+j], out tmpans.anslist[j]))
                        throw new Exception();
                }
                ans.Add(strs[0], tmpans);
                ctmp++;
            }
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
            //webBrowser1.Url = new Uri(kaoshiurl);
            webBrowser1.DocumentText = Properties.Resources.homepage;
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

        private void toolStripStatusLabel5_Click(object sender, EventArgs e)
        {
            Form2 debugtool = new Form2();
            debugtool.setDebugInfo(debugstr);
            debugtool.ShowDialog();
            debugtool.Dispose();
        }

        private void webBrowser1_NewWindow(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            System.Diagnostics.Process.Start(webBrowser1.StatusText);
        }


    }
}
