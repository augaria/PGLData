using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace PGLData
{
    public partial class DownloadOldSeasons : Form
    {

        readonly static string url = "https://raw.githubusercontent.com/DearUnknown/PGLToolUpdate/master/PGLToolUpdate/Data/history";
        WebClient myWebClient;
        private BackgroundWorker bkWorker = new BackgroundWorker();
        bool errorExists;
        int total;

        public bool hasNew;

        public DownloadOldSeasons()
        {
            InitializeComponent();

            //fit the dpi on client machines
            if (GlobalConstants.dpiX == 120)
            {
                foreach (Control ct in this.Controls)
                    ct.Font = new System.Drawing.Font(ct.Font.FontFamily, (float)(ct.Font.Size / 1.25), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            }

            //check old seasons on the github 
            hasNew = false;
            myWebClient = new WebClient();
            treeView1.CheckBoxes = true;
            for (int i = 0; i < GlobalConstants.GENERATION.Count; i++)
            {
                TreeNode tn = new TreeNode();
                tn.Text = GlobalConstants.GENERATION[i].ToString();
                myWebClient.DownloadFile(url + "/" + GlobalConstants.GENERATION[i] + "/" + GlobalConstants.GENERATION[i] + "-historyList.txt", "Update/" + GlobalConstants.GENERATION[i] + "-historyList.txt");

                FileStream aFile = new FileStream("Update/" + GlobalConstants.GENERATION[i] + "-historyList.txt", FileMode.Open);
                StreamReader sr = new StreamReader(aFile);

                string line = sr.ReadLine();
                while (line != null)
                {
                    if (!File.Exists("Data/" + GlobalConstants.GENERATION[i] + "/" + line))
                    {
                        tn.Nodes.Add(line);
                        hasNew=true;
                    }
                    line = sr.ReadLine();
                }
                sr.Close();
                aFile.Close();
                File.Delete("Update/" + GlobalConstants.GENERATION[i] + "-historyList.txt");
                treeView1.Nodes.Add(tn);
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown)
            {
                bool checkStatus = true;
                if (!e.Node.Checked)
                    checkStatus = false;
                foreach (TreeNode child in e.Node.Nodes)
                {
                    child.Checked = checkStatus;
                }
            }
        }

        //start downloading
        private void button1_Click(object sender, EventArgs e)
        {
            total = 0;
            for (int i = 0; i < GlobalConstants.GENERATION.Count; i++)
                foreach (TreeNode season in treeView1.Nodes[i].Nodes)
                    if (season.Checked)
                        total++;
            total *= 6;

            if (total == 0)
            {
                SelfDesignedMsg sdm = new SelfDesignedMsg("要先点选需要下载赛季,小女子才能帮您下载哦~", false);
                sdm.ShowDialog();
                return;
            }
            errorExists = false;
            bkWorker.WorkerReportsProgress = true;
            bkWorker.WorkerSupportsCancellation = true;
            bkWorker.DoWork += new DoWorkEventHandler(startUpdate);
            bkWorker.ProgressChanged += new ProgressChangedEventHandler(updateProgress);
            bkWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(completeWork);
            label2.Text = "已开始下载";
            bkWorker.RunWorkerAsync();

        }


        //thread updates
        private void updateProgress(object sender, ProgressChangedEventArgs e)
        {
            string ratioStr = e.ProgressPercentage + "%";
            if (ratioStr != label1.Text)
            {
                label1.Text = ratioStr;
                if (WindowState == FormWindowState.Minimized)
                    Text = ratioStr;
            }
            if (e.ProgressPercentage != progressBar1.Value)
                progressBar1.Value = e.ProgressPercentage;

        }

        //thread finishes
        private void completeWork(object sender, RunWorkerCompletedEventArgs e)
        {
            label1.Text = "100%";
            if (WindowState == FormWindowState.Minimized)
                Text = "100%";
            progressBar1.Value = 100;
            if (errorExists)
            {
                label2.Text = "下载中断";
                SelfDesignedMsg sdm = new SelfDesignedMsg("Sorry~ 下载因为异常而中断了... \r\n可能github服务器暂时无法连接", false);
                sdm.ShowDialog();
            }
            else
            {
                label2.Text = "下载完成";
                SelfDesignedMsg sdm = new SelfDesignedMsg("您需要的旧赛季数据已经都下载好啦~~~\r\n要阅览最新数据请重启软件哦~", false);
                sdm.ShowDialog();              
            }
        }


        //thread starts
        private void startUpdate(object sender, DoWorkEventArgs e)
        {
            int finished = 0;
            try
            {
                for (int i = 0; i < GlobalConstants.GENERATION.Count; i++)
                {
                    foreach (TreeNode season in treeView1.Nodes[i].Nodes)
                    {
                        if (season.Checked)
                        {
                            UpdateBySqls ubs = new UpdateBySqls("Data/" + GlobalConstants.GENERATION[i] + "/" + season.Text);
                            for (int j = 0; j < 6; j++)
                            {
                                myWebClient.DownloadFile(url + "/" + GlobalConstants.GENERATION[i] + "/" + season.Text + "/" + j + ".txt", "Update/" + j + ".txt");
                                ubs.setTable(j, "Update/" + j + ".txt");
                                File.Delete("Update/" + j + ".txt");
                                finished++;
                                bkWorker.ReportProgress(finished * 100 / total, true);
                            }
                        }
                    }
                }
            }
            catch { }
        }
        
        //handle size changing of the form  
        private void DownloadOldSeasons_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                Text = label1.Text;
            else if (WindowState != FormWindowState.Maximized)
                Text = "下载旧赛季";
        }
    }
}
