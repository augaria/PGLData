using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Drawing;

namespace PGLData
{
    //form used for data update 
    public partial class UpdateData : Form
    {

        public bool valid;
        public bool defaultHasNew;

        private BackgroundWorker bkWorker = new BackgroundWorker();

        System.Diagnostics.Stopwatch stopwatch;
        TimeSpan timeSpan;
        ArrayList pkIdList;
        bool errorExists;
        int[] generationTotal;
        ArrayList newTables;
        ArrayList fetchTasks;

        ArrayList gitVersions;
        ArrayList gitTasks;

        int total;


        public UpdateData(bool autoCheck)
        {
            InitializeComponent();

            if (GlobalConstants.dpiX == 120)
            {
                foreach (Control ct in this.Controls)
                    ct.Font = new System.Drawing.Font(ct.Font.FontFamily, (float)(ct.Font.Size / 1.25), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            }
            

            valid = false;
            defaultHasNew = false;
            radioButton2.Enabled = false;
            radioButton2.Visible = false;

            int checkRes = checkUpdate();
            bool gitValid  = checkGit();

            //fail to connect to PGL
            if (checkRes == -2)
            {
                radioButton1.Enabled = false;
                radioButton1.Visible = false;
                radioButton1.Checked = false;
                if (!gitValid)
                {
                    if (!autoCheck)
                    {
                        SelfDesignedMsg sdm = new SelfDesignedMsg("Sorry~ 无法连接PGL服务器, 可能这货在维护...", false);
                        sdm.ShowDialog();
                    }
                    return;
                }
            }

            //no return for PGL http post
            else if (checkRes == -1)
            {
                radioButton1.Enabled = false;
                radioButton1.Visible = false;
                radioButton1.Checked = false;
                if (!gitValid)
                {
                    if (!autoCheck)
                    {
                        SelfDesignedMsg sdm = new SelfDesignedMsg("刚刚进入新赛季,PGL服务器上暂时还找不到新的数据~", false);
                        sdm.ShowDialog();
                    }
                    return;
                }
            }

            //no new data
            else if (checkRes == 0)
            {
                if (!autoCheck)
                {
                    SelfDesignedMsg sdm = new SelfDesignedMsg("您已拥有最新各项榜单啦~", false);
                    sdm.ShowDialog();
                }
                return;
            }

            //initialize a new thread
            valid = true;
            errorExists = false;
            bkWorker.WorkerReportsProgress = true;
            bkWorker.WorkerSupportsCancellation = true;
            bkWorker.DoWork += new DoWorkEventHandler(startUpdate);
            bkWorker.ProgressChanged += new ProgressChangedEventHandler(updateProgress);
            bkWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(completeWork);

            //get user's selection for new data 
            treeView1.CheckBoxes = true;
            if (checkRes > 0 && (!gitValid))
            {
                for (int i = 0; i < fetchTasks.Count; i++)
                {
                    TreeNode tn = new TreeNode();
                    tn.Text = ((Fetch)fetchTasks[i]).generation + "-" + ((Fetch)fetchTasks[i]).curSeasonName+ "-" +((Fetch)fetchTasks[i]).updateTime.Split('-')[0];
                    if (((int[])newTables[i])[0] == 1)
                        tn.Nodes.Add("All Matches");
                    if (((int[])newTables[i])[1] == 1)
                        tn.Nodes.Add("Single");
                    if (((int[])newTables[i])[2] == 1)
                        tn.Nodes.Add("Double");
                    if (((int[])newTables[i])[3] == 1)
                        tn.Nodes.Add("Triple");
                    if (((int[])newTables[i])[4] == 1)
                        tn.Nodes.Add("Rotation");
                    if (((int[])newTables[i])[5] == 1)
                        tn.Nodes.Add("Special");
                    treeView1.Nodes.Add(tn);
                }
            }
            else
            {
                gitTasks = new ArrayList();
                newTables.Clear();
                for (int i = 0; i < GlobalConstants.GENERATION.Count; i++)
                {
                    int[] blankTable = { 1, 1, 1, 1, 1, 1 };

                    if (gitVersions[i].ToString().Equals(""))
                    {
                        int tableCount = 0;
                        AccessHandler ah = new AccessHandler(GlobalConstants.MOSTRECENTFILES[i].ToString());
                        string[] rankTables = ah.showAllTables();
                        foreach (string rankTable in rankTables)
                        {
                            blankTable[rankTable[10] - '0'] = 0;
                            tableCount++;
                        }
                        if (tableCount < 6)
                        {
                            newTables.Add(blankTable);
                            gitTasks.Add(GlobalConstants.MOSTRECENTFILES[i].ToString());
                            if (GlobalConstants.DEFAULTGENERATION.Equals(i.ToString()))
                                defaultHasNew = true;
                        }
                    }
                    else
                    {
                        newTables.Add(blankTable);
                        gitTasks.Add(gitVersions[i].ToString());
                        if (GlobalConstants.DEFAULTGENERATION.Equals(i.ToString()))
                            defaultHasNew = true;
                    }
                }

                if (gitTasks.Count == 0)
                {
                    valid = false;
                    if (!autoCheck)
                    {
                        SelfDesignedMsg sdm = new SelfDesignedMsg("您已拥有最新各项榜单啦~", false);
                        sdm.ShowDialog();
                    }
                    return;
                }


                for (int i = 0; i < gitTasks.Count; i++)
                {

                    TreeNode tn = new TreeNode();
                    string[] nameParts = gitTasks[i].ToString().Split('/');
                    tn.Text = nameParts[1]+ "-" + nameParts[2].Split('-')[1] + "-" + nameParts[2].Split('-')[2];
                    if (((int[])newTables[i])[0] == 1)
                        tn.Nodes.Add("All Matches");
                    if (((int[])newTables[i])[1] == 1)
                        tn.Nodes.Add("Single");
                    if (((int[])newTables[i])[2] == 1)
                        tn.Nodes.Add("Double");
                    if (((int[])newTables[i])[3] == 1)
                        tn.Nodes.Add("Triple");
                    if (((int[])newTables[i])[4] == 1)
                        tn.Nodes.Add("Rotation");
                    if (((int[])newTables[i])[5] == 1)
                        tn.Nodes.Add("Special");
                    treeView1.Nodes.Add(tn);
                }
            }
            
        }

        //check new data on github
        public bool checkGit()
        {
            try
            {
                HtmlParser hp = new HtmlParser();
                string newVersion;
                if (fetchTasks.Count == 0)
                    newVersion = "";
                else
                    newVersion = ((Fetch)fetchTasks[0]).mostUpdated;

                gitVersions = hp.checkDataUpdate(newVersion);
                if (gitVersions.Count>0)
                {
                    radioButton2.Enabled = true;
                    radioButton2.Visible = true;
                    radioButton2.Checked = true;
                    return true;
                }
                else
                {
                    radioButton1.Checked = true;
                    return false;
                }
            }
            catch
            {
                radioButton1.Checked = true;
                return false;
            }
        }

        //check new data on PGL
        public int checkUpdate()
        {
            newTables = new ArrayList();
            fetchTasks = new ArrayList();
            for (int i = 0; i < GlobalConstants.GENERATION.Count; i++)
            {
                int[] blankTable = { 1, 1, 1, 1, 1, 1 };
                Fetch fetch = new Fetch(i);
                if (fetch.serverStatus==-1)
                    return -2;
                if (fetch.serverStatus == 0)
                    return -1;
                if (fetch.mostUpdated.Equals(GlobalConstants.MOSTRECENTFILES[i].ToString()))
                {
                    int tableCount = 0;
                    AccessHandler ah = new AccessHandler(GlobalConstants.MOSTRECENTFILES[i].ToString());
                    string[] rankTables = ah.showAllTables();
                    foreach (string rankTable in rankTables)
                    {
                        blankTable[rankTable[10] - '0'] = 0;
                        tableCount++;
                    }
                    if (tableCount < 6)
                    {
                        newTables.Add(blankTable);
                        fetchTasks.Add(fetch);
                        if (GlobalConstants.DEFAULTGENERATION.Equals(i.ToString()))
                            defaultHasNew = true;
                    }
                }
                else
                { 
                    newTables.Add(blankTable);
                    fetchTasks.Add(fetch);
                    if (GlobalConstants.DEFAULTGENERATION.Equals(i.ToString()))
                        defaultHasNew = true;
                }
            }
            if (fetchTasks.Count == 0)
                return 0;

            
            return 1;
        }

        //thead updates
        private void updateProgress(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 1)
                textBox1.Text += (string)e.UserState;
            else
            {
                int[] progress = (int [])e.UserState;
                string ratioStr = progress[0] + "%";
                if (ratioStr != label1.Text)
                {
                    label1.Text = ratioStr;
                    if (WindowState == FormWindowState.Minimized)
                        Text = ratioStr;
                }
                if (progress[0] != progressBar1.Value)
                    progressBar1.Value = progress[0];
                label2.Text = progress[1] / 60 + "分" + progress[1] % 60 + "秒";
            }
        }

        //thread finishes
        private void completeWork(object sender, RunWorkerCompletedEventArgs e)
        {
            label2.Text = "0分0秒";
            label1.Text = "100%";
            if (WindowState == FormWindowState.Minimized)
                Text = "100%";
            progressBar1.Value = 100;
            if (errorExists)
                textBox1.Text += "下载因异常而中断\r\n";
            else
            { 
                textBox1.Text += "下载全部完成\r\n";
                SelfDesignedMsg sdm = new SelfDesignedMsg("小女子已经为您下载完全部数据啦~~~\r\n要阅览最新数据请重启软件哦~", false);
                sdm.ShowDialog();
            }
        }

        //thread starts
        private void startUpdate(object sender, DoWorkEventArgs e)
        {
            try
            {
                string fetchMessage;

                int done = 0;
                for (int i = 0; i < treeView1.Nodes.Count; i++)
                {
                    if (generationTotal[i] == 0)
                        continue;

                    int[] blankTable = (int[])newTables[i];
                    int index = 0;

                    if (radioButton2.Checked)
                    {
                        UpdateBySqls ubs = new UpdateBySqls(gitTasks[i].ToString());
                        string url = "https://raw.githubusercontent.com/DearUnknown/PGLToolUpdate/master/PGLToolUpdate/" + gitTasks[i].ToString();
                        WebClient myWebClient = new WebClient();
                        for (int j = 0; j < treeView1.Nodes[i].Nodes.Count; j++)
                        {
                            while (blankTable[index] == 0)
                                index++;
                            if (treeView1.Nodes[i].Nodes[j].Checked)
                            {
                                try
                                {
                                    myWebClient.DownloadFile(url + "/" + index + ".txt", "Update/" + index + ".txt");
                                }
                                catch (Exception e0)
                                {
                                    BugBox bb = new BugBox(url + "/" + index + ".txt\r\n"+ "Update/" + index + ".txt\r\n" + e0.ToString());
                                    bb.ShowDialog();
                                }
                                ubs.setTable(index, "Update/" + index + ".txt");
                                File.Delete("Update/" + index + ".txt");
                            }
                            index++;
                        }

                        for (int j = 0; j < GlobalConstants.GENERATION.Count; j++)
                        {
                            if(gitVersions[j].ToString().Equals(gitTasks[i].ToString()))
                            Config.setConfig("MostRecentFile" + (j+1).ToString(), gitTasks[i].ToString());
                        }

                        continue;
                    }

                    Fetch fetch = (Fetch)fetchTasks[i];
                    if (fetch.serverStatus==-1)
                    {
                        bkWorker.ReportProgress(1, "无法连接PGL服务器,请确认服务器是否在维护\r\n");
                        errorExists = true;
                        return;
                    }
                    fetch.updateInitial(done, total, bkWorker, pkIdList);
                    
                    for (int j = 0; j < treeView1.Nodes[i].Nodes.Count; j++)
                    {
                        while (blankTable[index] == 0)
                            index++;
                        if (treeView1.Nodes[i].Nodes[j].Checked)
                        {
                            stopwatch.Restart();                           
                            fetchMessage = fetch.getSeasonPokemonDetail(index);
                            stopwatch.Stop();
                            timeSpan = stopwatch.Elapsed;
                            bkWorker.ReportProgress(1, fetchMessage + "用时" + ((int)timeSpan.TotalSeconds) / 60 + "分" + ((int)timeSpan.TotalSeconds) % 60 + "秒\r\n");
                        }
                        index++;
                    }
                    done += generationTotal[i];
                    Config.setConfig("MostRecentFile" + fetch.generationId, fetch.mostUpdated);
                }
   
              }
            catch (Exception ex)
            {
                bkWorker.ReportProgress(1, ex.ToString()+"\r\n");
                errorExists = true;
            }
        }

        //start updating
        private void button1_Click(object sender, EventArgs e)
        {
            pkIdList = new ArrayList();
            try
            {

                FileStream file = new FileStream("Data/pokeIdList.dat", FileMode.Open);
                StreamReader sr = new StreamReader(file);
                string line = sr.ReadLine();
                while (line != null)
                {
                    pkIdList.Add(line);
                    line = sr.ReadLine();
                }
                sr.Close();
            }
            catch
            {
                textBox1.Text += "读取文件pokeIdList.dat失败\r\n";
                errorExists = true;
                return;
            }
            
            progressBar1.Value = 0;

            total = 0;
            generationTotal = new int[treeView1.Nodes.Count];
            for (int i = 0; i < treeView1.Nodes.Count; i++)
            {
                generationTotal[i] = 0;
                for (int j = 0; j < treeView1.Nodes[i].Nodes.Count; j++)
                {
                    if (treeView1.Nodes[i].Nodes[j].Checked)
                    {
                        total++;
                        generationTotal[i]++;
                    }
                }
                generationTotal[i] *= pkIdList.Count;
            }
            total *= pkIdList.Count;

            if (total == 0)
            {
                SelfDesignedMsg sdm = new SelfDesignedMsg("要先点选需更新的榜单,小女子才能帮您下载哦~", false);
                sdm.ShowDialog();
                return;
            }


            textBox1.Text += "下载开始\r\n";
            stopwatch = new System.Diagnostics.Stopwatch();
            bkWorker.RunWorkerAsync();
        }

        //handle size change of the form
        private void UpdateData_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                Text = label1.Text;
            else if (WindowState != FormWindowState.Maximized)
                Text = "数据更新";
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

        //select PGL as update source
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton1.Checked)
                return;
            radioButton2.Checked = false;
        }

        //select github as update source
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton2.Checked)
                return;
            radioButton1.Checked = false;
        }
    }

}