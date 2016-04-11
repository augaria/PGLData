using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PGLData
{
    public partial class MainPanel : Form
    {
        AccessHandler ah;
        AccessHandler ahDetail;
        AccessHandler ahOld;
        string curDir;

        DataTable detailData1;
        DataTable detailData2;
        DataTable dt;
        DataTable raceData;
        Graphics g;
        Color[] racialColors = { Color.FromArgb(255, 0, 0), Color.FromArgb(240, 128, 48), Color.FromArgb(248, 208, 48), Color.FromArgb(104, 144, 240), Color.FromArgb(120, 200, 80), Color.FromArgb(248, 88, 136) };
        Image[] typePics = { Properties.Resources.normal, Properties.Resources.fighting, Properties.Resources.fire, Properties.Resources.ice, Properties.Resources.electric, Properties.Resources.flying, Properties.Resources.grass, Properties.Resources.ground, Properties.Resources.poison, Properties.Resources.bug, Properties.Resources.dark, Properties.Resources.water, Properties.Resources.psychic, Properties.Resources.dragon, Properties.Resources.rock, Properties.Resources.ghost, Properties.Resources.steel, Properties.Resources.fairy };
        
        ArrayList shownTables;

        Hashtable specialAbilities;

        string[] rankTables;
        int battleIndex;

        FileInfo[] files;
        string oldFile;
        string newFile;
        bool compareMode;
        string generationSelected;

        string proTarget;

        Hashtable oldDetails;

        Font defaultMenuFont;
        Font selectedMenuFont;
        string curSingleFile;

        string currentFocusId;

        UpdateApp ua;
        UpdateData ud;

        private BackgroundWorker bkWorker = new BackgroundWorker();

        public class myCompare : IComparer<string>
        {

            public int Compare(string x, string y)
            {
                int a = int.Parse(x.Split('-')[1]);
                int b = int.Parse(y.Split('-')[1]);

                if (a < b)
                    return 1;
                else if (a > b)
                    return -1;

                a = int.Parse(x.Split('-')[2]);
                b = int.Parse(y.Split('-')[2]);
                if (a < b)
                    return 1;
                else if (a > b)
                    return -1;
                else
                    return 0;
            }
        }

        public MainPanel()
        {
            try
            {
                InitializeComponent();

                currentFocusId = "";
                Graphics graphics = this.CreateGraphics();
                GlobalConstants.dpiX = graphics.DpiX;
                if (GlobalConstants.dpiX == 120)
                {
                    foreach (Control ct in this.Controls)
                        ct.Font = new System.Drawing.Font(ct.Font.FontFamily, (float)(ct.Font.Size / 1.25), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                }

                proTarget = "";
                Config config = new Config();

                if (GlobalConstants.CHECKUPDATE.Equals("1"))
                {
                    bkWorker.WorkerSupportsCancellation = true;
                    bkWorker.DoWork += new DoWorkEventHandler(startUpdate);
                    bkWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(completeWork);
                    bkWorker.RunWorkerAsync();
                }

                ahDetail = new AccessHandler("Data/PokeData.mdb");
                for (int i = 0; i < 18; i++)
                    GlobalConstants.TYPEPAIR.Add(GlobalConstants.TYPENAME[i], i);
                specialAbilities = new Hashtable();
                specialAbilities.Add("Thick Fat", new double[2] { -1, 0.5 });
                specialAbilities.Add("Heatproof", new double[2] { 2, 0.5 });
                specialAbilities.Add("Filter", new double[2] { -2, 0.75 });
                specialAbilities.Add("Solid Rock", new double[2] { -2, 0.75 });
                specialAbilities.Add("Volt Absorb", new double[2] { 4, 0 });
                specialAbilities.Add("Water Absorb", new double[2] { 11, 0 });
                specialAbilities.Add("Flash Fire", new double[2] { 2, 0 });
                specialAbilities.Add("Lightning rod", new double[2] { 4, 0 });
                specialAbilities.Add("Storm Drain", new double[2] { 11, 0 });
                specialAbilities.Add("Sap Sipper", new double[2] { 6, 0 });
                specialAbilities.Add("Levitate", new double[2] { 7, 0 });
                specialAbilities.Add("Wonder Guard", new double[2] { -3, 0 });
                specialAbilities.Add("Dry Skin", new double[2] { 11, 0 });

                shownTables = new ArrayList();

                //设置抗性列表颜色
                for (int i = 0; i < 18; i++)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.SubItems.Add(GlobalConstants.TYPENAME[i]);
                    lvi.SubItems.Add("");
                    lvi.SubItems[1].BackColor = GlobalConstants.TYPECOLORS[(int)GlobalConstants.TYPEPAIR[GlobalConstants.TYPENAME[i]]];
                    lvi.SubItems[1].ForeColor = Color.White;
                    listView7.Items.Add(lvi);
                }

                //if no most updated data found fron config, show nothing, no "lable"s
                if (GlobalConstants.dpiX == 120)
                {
                    defaultMenuFont = new Font("Segoe UI", 7.2F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                    selectedMenuFont = new Font("Segoe UI", 7.2F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                }
                else
                {
                    defaultMenuFont = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                    selectedMenuFont = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                }
                //设置默认榜单
                for (int i = 0; i < 6; i++)
                {
                    ToolStripMenuItem tsmi = new ToolStripMenuItem();
                    tsmi.Click += new System.EventHandler(defautBattleTypeToolStripMenuItem_Click);

                    tsmi.Text = GlobalConstants.TABLENAMES[i];
                    if (i == GlobalConstants.DEFAULTBATTLETYPE[0] - '0')
                    {
                        tsmi.Text += " √";
                        tsmi.ForeColor = SystemColors.MenuHighlight;
                        tsmi.Font = selectedMenuFont;
                    }
                    defaultTableToolStripMenuItem.DropDownItems.Add(tsmi);
                }

                //设置默认游戏
                for (int i = 0; i < GlobalConstants.GENERATION.Count; i++)
                {
                    ToolStripMenuItem tsmi = new ToolStripMenuItem();
                    tsmi.Click += new System.EventHandler(generationToolStripMenuItem_Click);

                    tsmi.Text = GlobalConstants.GENERATION[i].ToString();
                    if (i == GlobalConstants.DEFAULTGENERATION[0] - '0')
                    {
                        generationSelected = tsmi.Text;
                        tsmi.Text += " √";
                        tsmi.ForeColor = SystemColors.MenuHighlight;
                        tsmi.Font = selectedMenuFont;
                    }
                    generationToolStripMenuItem.DropDownItems.Add(tsmi);
                }

                //设置是否自动更新
                if (GlobalConstants.CHECKUPDATE.Equals("1"))
                {
                    toolStripMenuItem7.Text += " √";
                    toolStripMenuItem7.ForeColor = SystemColors.MenuHighlight;
                    toolStripMenuItem7.Font = selectedMenuFont;
                }
                else
                {
                    toolStripMenuItem8.Text += " √";
                    toolStripMenuItem8.ForeColor = SystemColors.MenuHighlight;
                    toolStripMenuItem8.Font = selectedMenuFont;
                }

                //设置时区
                if (GlobalConstants.TIMEZONE.Equals("GMT"))
                {
                    gMTToolStripMenuItem.Text = "GMT √";
                    gMTToolStripMenuItem.ForeColor = SystemColors.MenuHighlight;
                    gMTToolStripMenuItem.Font = selectedMenuFont;
                }
                else if (GlobalConstants.TIMEZONE.Equals("JST"))
                {
                    jSTToolStripMenuItem.Text = "JST √";
                    jSTToolStripMenuItem.ForeColor = SystemColors.MenuHighlight;
                    jSTToolStripMenuItem.Font = selectedMenuFont;
                }
                else
                {
                    eSTToolStripMenuItem.Text = "EST √";
                    eSTToolStripMenuItem.ForeColor = SystemColors.MenuHighlight;
                    eSTToolStripMenuItem.Font = selectedMenuFont;
                }

                //设置单期或比较
                reloadByViewMode();

            }
            catch (Exception e)
            {
                BugBox bb = new BugBox(e.ToString());
                bb.ShowDialog();
                this.Close();
            }

        }

        //询问是否要进行更新
        private void completeWork(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (ua.hasNew)
                {
                    SelfDesignedMsg sdm = new SelfDesignedMsg("主人发布新版本软件了哦~ 是否查看更新信息?",true);
                    DialogResult dr = sdm.ShowDialog();
                    if (dr == DialogResult.Yes)
                        ua.ShowDialog();
                }
                if (ud.valid&&ud.defaultHasNew)
                {
                    SelfDesignedMsg sdm = new SelfDesignedMsg("PGL发布新的数据了~ 是否查看更新信息?", true);
                    DialogResult dr = sdm.ShowDialog();
                    if (dr == DialogResult.Yes)
                        ud.Show();
                }
            }
            catch(Exception e1)
            {
                textBox1.Text += e1.ToString();
            }
        }

        //自动检查是否有更新
        private void startUpdate(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < GlobalConstants.GENERATION.Count; i++)
            {
                DirectoryInfo TheFolder = new DirectoryInfo("Data/" + GlobalConstants.GENERATION[i].ToString());
                FileInfo[] genefiles = TheFolder.GetFiles("*.mdb");
                myCompare mc = new myCompare();
                var sortedFiles = genefiles.OrderBy(a => a.Name, mc);
                genefiles = sortedFiles.ToArray();
                if (!GlobalConstants.MOSTRECENTFILES[i].Equals("Data/" + GlobalConstants.GENERATION[i].ToString() + "/" + genefiles[0].Name))
                {
                    GlobalConstants.MOSTRECENTFILES[i] = "Data/" + GlobalConstants.GENERATION[i].ToString() + "/" + genefiles[0].Name;
                    Config.setConfig("MostRecentFile" + (i + 1).ToString(), "Data/" + GlobalConstants.GENERATION[i].ToString() + "/" + genefiles[0].Name);
                }
            }
            try
            {
                ua = new UpdateApp(true);
                ud = new UpdateData(true);
            }
            catch { }
        }


        //根据单期或比较重设界面
        private void reloadByViewMode()
        {
            curSingleFile = GlobalConstants.MOSTRECENTFILES[GlobalConstants.DEFAULTGENERATION[0] - '0'].ToString();

            if (GlobalConstants.VIEWOPTIONS.Equals("single"))
                singleModeView();
            else
                compareModeView();
        }

        //选择榜单
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex < 0)
                return;
            listView1.Items.Clear();
            battleIndex = (int)shownTables[comboBox1.SelectedIndex];
            curDir = Environment.CurrentDirectory;
            if (raceData != null)
                raceData.Clear();
            raceData = ah.SelectToDataTable("select pokemon.chi,pokemon.eng," + rankTables[battleIndex] + ".ranking,pokemon.poke_id,pokemon.form from " + rankTables[battleIndex] + " right join [;database=" + curDir + "/Data/PokeData.mdb].pokemon on pokemon.poke_id = " + rankTables[battleIndex] + ".poke_id order by " + rankTables[battleIndex] + ".ranking,pokemon.poke_id");

            string[] ranking_old = new string[raceData.Rows.Count];
            if (compareMode)
            {
                string oldDb = oldFile.Replace(".mdb", "");
                string newDb = newFile.Replace(".mdb", "");
                DataTable oldData = ahOld.SelectToDataTable("select ranking,poke_id from " + rankTables[battleIndex]  );

                EnumerableRowCollection<DataRow> hehe = raceData.AsEnumerable();
                var res = from table1 in raceData.AsEnumerable()
                          join table2 in oldData.AsEnumerable() on table1["poke_id"] equals table2["poke_id"] into joined
                          from subCompEmp in joined.DefaultIfEmpty()
                          select new
                          {
                              ranking_old = (subCompEmp != null) ? subCompEmp["ranking"] : "-"
                          };
                
                for (int i = 0; i < res.Count(); i++)
                    ranking_old[i] = res.ElementAt(i).ranking_old.ToString();
            }
            for (int i= 0; i < raceData.Rows.Count; i++)
            {
                if (raceData.Rows[i][2].ToString().Equals(""))
                    continue;
                ListViewItem lvi = new ListViewItem();
                lvi.Text = raceData.Rows[i][2].ToString();
                if (compareMode)
                {
                    int diff;
                    if (!ranking_old[i].Equals("-"))
                    {
                        diff = int.Parse(ranking_old[i]) - int.Parse(lvi.Text);
                        if (diff > 0)
                        {
                            lvi.SubItems.Add("↑" + diff);
                            if (diff < GlobalConstants.APPARENTRANKINGCHANGE)
                                lvi.BackColor = Color.PaleGreen;
                            else
                            {
                                lvi.ForeColor = Color.White;
                                lvi.BackColor = Color.Green;
                            }
                        }
                        else if (diff < 0)
                        {
                            lvi.SubItems.Add("↓" + (-diff).ToString());
                            if (diff > -GlobalConstants.APPARENTRANKINGCHANGE)
                                lvi.BackColor = Color.LightPink;
                            else
                            {
                                lvi.BackColor = Color.Red;
                                lvi.ForeColor = Color.White;
                            }
                        }
                        else
                            lvi.SubItems.Add("");
                    }
                    else
                    {
                        lvi.SubItems.Add("↑新");
                        lvi.ForeColor = Color.White;
                        lvi.BackColor = Color.Green;
                    }
                }
                lvi.SubItems.Add((string)raceData.Rows[i][0]);
                lvi.SubItems.Add((string)raceData.Rows[i][1]);
                lvi.SubItems.Add(raceData.Rows[i][3].ToString());
                listView1.Items.Add(lvi);
            }

            for (int i=0;i< raceData.Rows.Count;i++)
            {
                ListViewItem lvi = new ListViewItem();
                if (!raceData.Rows[i][2].ToString().Equals(""))
                    break;
                if (raceData.Rows[i][4].ToString().Equals("mega") || raceData.Rows[i][4].ToString().Equals("舞步") || raceData.Rows[i][4].ToString().Equals("不倒翁") || raceData.Rows[i][4].ToString().Equals("剑"))
                    continue;
                lvi.Text = "-";
                if (compareMode)
                {
                    if (!ranking_old[i].Equals("-"))
                    {
                        lvi.SubItems.Add("↓落");
                        lvi.BackColor = Color.Red;
                        lvi.ForeColor = Color.White;
                    }
                    else
                        lvi.SubItems.Add("");
                }
                lvi.SubItems.Add((string)raceData.Rows[i][0]);
                lvi.SubItems.Add((string)raceData.Rows[i][1]);
                lvi.SubItems.Add(raceData.Rows[i][3].ToString());
                listView1.Items.Add(lvi);
            }


            if(currentFocusId.Equals(""))
                listView1.Items[0].Selected = true;
            else
            {
                if (compareMode)
                {
                    foreach (ListViewItem lvi in listView1.Items)
                    {
                        if (lvi.SubItems[4].Text.Equals(currentFocusId))
                        {
                            lvi.Selected = true;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (ListViewItem lvi in listView1.Items)
                    {
                        if (lvi.SubItems[3].Text.Equals(currentFocusId))
                        {
                            lvi.Selected = true;
                            break;
                        }
                    }
                }
            }
            textBox2.Text = "";
            textBox2.Focus();
        }


        //更换图片和基础数据
        private void basicInfo()
        {
            string pkId = dt.Rows[0][0].ToString();
            label1.Text = "No." + pkId;
            label3.Text = dt.Rows[0][9].ToString();
            label5.Text = dt.Rows[0][10].ToString();
            label7.Text = dt.Rows[0][11].ToString();
            label9.Text = dt.Rows[0][12].ToString();
            label11.Text = dt.Rows[0][13].ToString();
            label13.Text = dt.Rows[0][14].ToString();
            label15.Text = dt.Rows[0][15].ToString();
            if(compareMode)
                label17.Text = listView1.SelectedItems[0].SubItems[0].Text+"  "+listView1.SelectedItems[0].SubItems[1].Text;
            else
                label17.Text = listView1.SelectedItems[0].SubItems[0].Text;
            label18.Text = dt.Rows[0][1].ToString();
            label19.Text = dt.Rows[0][3].ToString();

            string picPath = "Resources/" + pkId + ".png";
            if (File.Exists(picPath))
                pictureBox1.Image = Image.FromFile(picPath);
            else
            {
                picPath = "Resources/" + pkId.Substring(0, pkId.IndexOf('.')) + ".png";
                if (File.Exists(picPath))
                    pictureBox1.Image = Image.FromFile(picPath);
            }

            pictureBox4.Image = typePics[(int)(GlobalConstants.TYPEPAIR[dt.Rows[0][4].ToString()])];
            if (dt.Rows[0][4].ToString().Equals(dt.Rows[0][5].ToString()))
                pictureBox5.Visible = false;
            else
            {
                pictureBox5.Visible = true;
                pictureBox5.Image = typePics[(int)(GlobalConstants.TYPEPAIR[dt.Rows[0][5].ToString()])];
            }

            if(comboBox4.Items.Count==0)
                setResistance(-1);

            pictureBox2.Refresh();
        }

        //设置抗性列表
        private void setResistance(int index)
        {
            double[] resistance = new double[18];            

            if (dt.Rows[0][4].ToString().Equals(dt.Rows[0][5].ToString()))
            {
                int typeId = (int)(GlobalConstants.TYPEPAIR[dt.Rows[0][4].ToString()]);
                for (int i = 0; i < 18; i++)
                    resistance[i] = GlobalConstants.TYPERELATIONS[i, typeId];
            }
            else
            {
                int typeId1 = (int)(GlobalConstants.TYPEPAIR[dt.Rows[0][4].ToString()]);
                int typeId2 = (int)(GlobalConstants.TYPEPAIR[dt.Rows[0][5].ToString()]);
                for (int i = 0; i < 18; i++)
                    resistance[i] = GlobalConstants.TYPERELATIONS[i, typeId1] * GlobalConstants.TYPERELATIONS[i, typeId2];
            }

            if (index >= 0 && specialAbilities.Contains(comboBox4.Items[index].ToString()))
            {
                int sign = (int)(((double[])(specialAbilities[comboBox4.Items[index].ToString()]))[0]);
                if (sign >= 0)
                    resistance[sign] *= ((double[])(specialAbilities[comboBox4.Items[index].ToString()]))[1];
                else if (sign == -1)
                {
                    resistance[2] *= 0.5;
                    resistance[3] *= 0.5;
                }

                else if (sign == -2)
                {
                    for (int i = 0; i < 18; i++)
                        if (resistance[i] > 1)
                            resistance[i] *= 0.75;
                }
                else if (sign == -3)
                {
                    for (int i = 0; i < 18; i++)
                        if (resistance[i] < 2)
                            resistance[i] = 0;
                }
            }

            double avgResistance = Math.Round(resistance.Average(), 2);
            label33.Text = "平均抗性 "+ avgResistance;
            if (avgResistance > 1)
                label33.ForeColor = Color.Red;
            else if (avgResistance < 1)
                label33.ForeColor = Color.Blue;
            else
                label33.ForeColor = Color.Black;

            for (int i = 0; i < 18; i++)
            {
                if (resistance[i] == 0)
                {
                    listView7.Items[i].SubItems[2].Text = "×";
                    listView7.Items[i].SubItems[2].ForeColor = Color.Blue;
                }
                else if (resistance[i] < 1)
                {
                    listView7.Items[i].SubItems[2].Text = resistance[i].ToString();
                    listView7.Items[i].SubItems[2].ForeColor = Color.Blue;
                }
                else if (resistance[i] > 1)
                {
                    listView7.Items[i].SubItems[2].Text = resistance[i].ToString();
                    listView7.Items[i].SubItems[2].ForeColor = Color.Red;
                }
                else
                    listView7.Items[i].SubItems[2].Text = "";
            }
        }

        //选择新精灵
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;
            string pkId;
            if (compareMode)
                pkId = listView1.SelectedItems[0].SubItems[4].Text;
            else
                pkId = listView1.SelectedItems[0].SubItems[3].Text;

            currentFocusId = pkId;
            if (listView1.SelectedItems[0].SubItems[0].Text[0] != '-')
            {
                detailData1 = ah.SelectToDataTable("select * from " + rankTables[battleIndex] + " where poke_id=" + pkId);
                if (compareMode && !listView1.SelectedItems[0].SubItems[1].Text.Contains("新"))
                {
                    oldDetails = new Hashtable();
                    DataTable detailDataOld = ahOld.SelectToDataTable("select * from " + rankTables[battleIndex] + " where poke_id=" + pkId);
                    for (int i = 2; i < 20; i += 2)
                    {
                        if (detailDataOld.Rows[0][i].ToString() != "-")
                            oldDetails.Add(detailDataOld.Rows[0][i].ToString(), Math.Round((double)(detailDataOld.Rows[0][i + 1]), 1));
                    }
                    for (int i = 20; i < 80; i += 3)
                    {
                        if (detailDataOld.Rows[0][i].ToString() != "-")
                            oldDetails.Add(detailDataOld.Rows[0][i].ToString(), Math.Round((double)(detailDataOld.Rows[0][i + 2]), 1));
                    }
                }
            }
            detailData2 = ahDetail.SelectToDataTable("select * from pokemon where poke_id=" + pkId);


            if (compareMode)
            {
                if (listView1.SelectedItems[0].SubItems[1].Text.Contains('↑') || listView1.SelectedItems[0].SubItems[0].Text.Contains('N'))
                    label17.ForeColor = Color.Green;
                else if (listView1.SelectedItems[0].SubItems[1].Text.Contains('↓') || listView1.SelectedItems[0].SubItems[0].Text.Contains('X'))
                    label17.ForeColor = Color.Red;
                else
                    label17.ForeColor = Color.Blue;
            }
            else
                label17.ForeColor = Color.Blue;


            if (!detailData2.Rows[0][16].ToString().Equals("无"))
            {
                radioButton1.Text = detailData2.Rows[0][16].ToString();
                if (pkId.Equals("555"))
                    radioButton2.Text = "不倒翁";
                else if (pkId.Equals("681"))
                    radioButton2.Text = "剑";
                else if (pkId.Equals("648"))
                    radioButton2.Text = "舞步";
                else if (pkId.Equals("382"))
                    radioButton2.Text = "原始回归";
                else if (pkId.Equals("383"))
                    radioButton2.Text = "原始回归";
                else
                    radioButton2.Text = "Mega";


                if (pkId.Equals("6") || pkId.Equals("150"))
                {
                    radioButton3.Visible = true;
                    radioButton2.Text = "MegaX";
                }
                else
                    radioButton3.Visible = false;
                radioButton1.Visible = true;
                radioButton2.Visible = true;

                radioButton2.Checked = false;
                radioButton3.Checked = false;

            }
            else
            {
                radioButton1.Visible = false;
                radioButton2.Visible = false;
                radioButton3.Visible = false;
            }



            listView2.Items.Clear();
            listView3.Items.Clear();
            listView4.Items.Clear();
            listView5.Items.Clear();

            radioButton1.Checked = false;
            radioButton1.Checked = true;

            if (listView1.SelectedItems[0].SubItems[0].Text[0] == '-')
                return;

            int cur = 8;

            //更新性格列表
            for (int i = 0; i < 3; i++)
            {
                if (detailData1.Rows[0][cur].ToString() == "-")
                {
                    cur += 2;
                    continue;
                }
                ListViewItem lvi = new ListViewItem();
                lvi.Text = detailData1.Rows[0][cur++].ToString();
                double newRate = (double)(detailData1.Rows[0][cur++]);
                lvi.SubItems.Add(Math.Round(newRate, 1).ToString() + "%");
                if (compareMode && !listView1.SelectedItems[0].SubItems[1].Text.Contains("新"))
                {
                    if (oldDetails.Contains(lvi.Text))
                    {
                        double diff = newRate - (Double)(oldDetails[lvi.Text]);
                        if (diff <= -GlobalConstants.APPARENTUSAGERATECHANGE)
                        {
                            lvi.BackColor = Color.Red;
                            lvi.ForeColor = Color.White;
                            lvi.SubItems.Add(Math.Round(diff, 1).ToString() + "%");
                        }
                        else if (diff < 0)
                        {
                            lvi.BackColor = Color.LightPink;
                            lvi.SubItems.Add(Math.Round(diff, 1).ToString() + "%");
                        }
                        else if (diff >= GlobalConstants.APPARENTUSAGERATECHANGE)
                        {
                            lvi.ForeColor = Color.White;
                            lvi.BackColor = Color.Green;
                            lvi.SubItems.Add("+" + Math.Round(diff, 1).ToString() + "%");
                        }
                        else if (diff > 0)
                        {
                            lvi.BackColor = Color.PaleGreen;
                            lvi.SubItems.Add("+" + Math.Round(diff, 1).ToString() + "%");
                        }
                    }
                }
                listView3.Items.Add(lvi);
            }

            //更新道具列表
            for (int i = 0; i < 3; i++)
            {
                if (detailData1.Rows[0][cur].ToString() == "-")
                {
                    cur += 2;
                    continue;
                }
                ListViewItem lvi = new ListViewItem();
                lvi.Text = detailData1.Rows[0][cur++].ToString();
                double newRate = (double)(detailData1.Rows[0][cur++]);
                lvi.SubItems.Add(Math.Round(newRate, 1).ToString() + "%");
                if (compareMode && !listView1.SelectedItems[0].SubItems[1].Text.Contains("新"))
                {
                    if (oldDetails.Contains(lvi.Text))
                    {
                        double diff = newRate - (Double)(oldDetails[lvi.Text]);
                        if (diff <= -GlobalConstants.APPARENTUSAGERATECHANGE)
                        {
                            lvi.BackColor = Color.Red;
                            lvi.ForeColor = Color.White;
                            lvi.SubItems.Add(Math.Round(diff, 1).ToString() + "%");
                        }
                        else if (diff < 0)
                        {
                            lvi.BackColor = Color.LightPink;
                            lvi.SubItems.Add(Math.Round(diff, 1).ToString() + "%");
                        }
                        else if (diff >= GlobalConstants.APPARENTUSAGERATECHANGE)
                        {
                            lvi.ForeColor = Color.White;
                            lvi.BackColor = Color.Green;
                            lvi.SubItems.Add("+" + Math.Round(diff, 1).ToString() + "%");
                        }
                        else if (diff > 0)
                        {
                            lvi.BackColor = Color.PaleGreen;
                            lvi.SubItems.Add("+" + Math.Round(diff, 1).ToString() + "%");
                        }
                    }
                }
                listView4.Items.Add(lvi);
            }

            //更新招式列表
            for (int i = 0; i < 20; i++)
            {
                if (detailData1.Rows[0][cur].ToString() == "-")
                {
                    cur += 3;
                    continue;
                }
                ListViewItem lvi = new ListViewItem();
                lvi.Text = detailData1.Rows[0][cur++].ToString();

                if (!compareMode)
                {
                    lvi.ForeColor = Color.White;
                    lvi.BackColor = GlobalConstants.TYPECOLORS[(int)(detailData1.Rows[0][cur]) - 1];
                }

                //lvi.Font = new Font(lvi.Font,FontStyle.Bold);
                lvi.SubItems.Add(GlobalConstants.TYPENAME[(int)(detailData1.Rows[0][cur++]) - 1]);
                double newRate = (double)(detailData1.Rows[0][cur++]);
                lvi.SubItems.Add(Math.Round(newRate, 1).ToString() + "%");

                if (compareMode && !listView1.SelectedItems[0].SubItems[1].Text.Contains("新"))
                {
                    if (oldDetails.Contains(lvi.Text))
                    {
                        double diff = newRate - (Double)(oldDetails[lvi.Text]);
                        if (diff <= -GlobalConstants.APPARENTUSAGERATECHANGE)
                        {
                            lvi.BackColor = Color.Red;
                            lvi.ForeColor = Color.White;
                            lvi.SubItems.Add(Math.Round(diff, 1).ToString() + "%");
                        }
                        else if (diff < 0)
                        {
                            lvi.BackColor = Color.LightPink;
                            lvi.SubItems.Add(Math.Round(diff, 1).ToString() + "%");
                        }
                        else if (diff >= GlobalConstants.APPARENTUSAGERATECHANGE)
                        {
                            lvi.ForeColor = Color.White;
                            lvi.BackColor = Color.Green;
                            lvi.SubItems.Add("+" + Math.Round(diff, 1).ToString() + "%");
                        }
                        else if (diff > 0)
                        {
                            lvi.BackColor = Color.PaleGreen;
                            lvi.SubItems.Add("+" + Math.Round(diff, 1).ToString() + "%");
                        }
                    }
                }
                listView5.Items.Add(lvi);
            }
        }

        //选择基本形态
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == false)
                return;

            dt = detailData2;

            //检查有无特殊特性
            comboBox4.Items.Clear();
            bool hasSpecial = false;
            string ability = detailData2.Rows[0][6].ToString();
            comboBox4.Items.Add(ability);
            if (specialAbilities.Contains(ability))
                hasSpecial = true;
            //有第二特性
            if ((!detailData2.Rows[0][7].ToString().Equals("-")) && (!detailData2.Rows[0][7].ToString().Equals(detailData2.Rows[0][6].ToString())))
            {
                ability = detailData2.Rows[0][7].ToString();
                comboBox4.Items.Add(ability);
                if (specialAbilities.Contains(ability))
                    hasSpecial = true;
            }
            //有第三特性
            if ((!detailData2.Rows[0][8].ToString().Equals("-")) && (!detailData2.Rows[0][8].ToString().Equals(detailData2.Rows[0][7].ToString())))
            {
                ability = detailData2.Rows[0][8].ToString();
                comboBox4.Items.Add(ability);
                if (specialAbilities.Contains(ability))
                    hasSpecial = true;
            }

            if (hasSpecial)
            {
                if (comboBox4.Items.Count > 1)
                    comboBox4.Visible = true;
                else
                    comboBox4.Visible = false;
                comboBox4.SelectedIndex = 0;
            }
            else
            {
                comboBox4.Items.Clear();
                comboBox4.Visible = false;
            }

            basicInfo();

            listView2.Items.Clear();

            //无使用率
            if (listView1.SelectedItems[0].SubItems[0].Text[0] == '-')
            {
                ListViewItem lvi1 = new ListViewItem();
                lvi1.Text = detailData2.Rows[0][6].ToString();
                lvi1.SubItems.Add("-");
                listView2.Items.Add(lvi1);
                //有第二特性
                if ((!detailData2.Rows[0][7].ToString().Equals("-")) && (!detailData2.Rows[0][7].ToString().Equals(detailData2.Rows[0][6].ToString())))
                {
                    ListViewItem lvi2 = new ListViewItem();
                    lvi2.Text = detailData2.Rows[0][7].ToString();
                    lvi2.SubItems.Add("-");
                    listView2.Items.Add(lvi2);
                }
                //有第三特性
                if ((!detailData2.Rows[0][8].ToString().Equals("-")) && (!detailData2.Rows[0][8].ToString().Equals(detailData2.Rows[0][7].ToString())))
                {
                    ListViewItem lvi3 = new ListViewItem();
                    lvi3.Text = detailData2.Rows[0][8].ToString();
                    lvi3.SubItems.Add("-");
                    listView2.Items.Add(lvi3);
                }

                return;
            }

            int cur = 2;

            //更新特性列表
            for (int i = 0; i < 3; i++)
            {
                if (detailData1.Rows[0][cur].ToString() == "-")
                {
                    cur += 2;
                    continue;
                }
                ListViewItem lvi = new ListViewItem();
                lvi.Text = detailData1.Rows[0][cur++].ToString();
                double newRate = (double)(detailData1.Rows[0][cur++]);
                lvi.SubItems.Add(Math.Round(newRate, 1).ToString() + "%");
                if (compareMode && !listView1.SelectedItems[0].SubItems[1].Text.Contains("新"))
                {
                    if (oldDetails.Contains(lvi.Text))
                    {
                        double diff = newRate - (Double)(oldDetails[lvi.Text]);
                        if (diff <= -GlobalConstants.APPARENTUSAGERATECHANGE)
                        {
                            lvi.BackColor = Color.Red;
                            lvi.ForeColor = Color.White;
                            lvi.SubItems.Add(Math.Round(diff, 1).ToString() + "%");
                        }
                        else if (diff < 0)
                        {
                            lvi.BackColor = Color.LightPink;
                            lvi.SubItems.Add(Math.Round(diff, 1).ToString() + "%");
                        }
                        else if (diff >= GlobalConstants.APPARENTUSAGERATECHANGE)
                        {
                            lvi.ForeColor = Color.White;
                            lvi.BackColor = Color.Green;
                            lvi.SubItems.Add("+" + Math.Round(diff, 1).ToString() + "%");
                        }
                        else if (diff > 0)
                        {
                            lvi.BackColor = Color.PaleGreen;
                            lvi.SubItems.Add("+" + Math.Round(diff, 1).ToString() + "%");
                        }
                    }
                }
                listView2.Items.Add(lvi);
            }
        }
        
        //选择第二形态
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == false)
                return;
            dt = ahDetail.SelectToDataTable("select * from pokemon where poke_id=" + detailData2.Rows[0][0].ToString() + ".1");

            comboBox4.Items.Clear();
            if (specialAbilities.Contains(dt.Rows[0][6].ToString()))
            {
                comboBox4.Items.Add(dt.Rows[0][6].ToString());
                comboBox4.Visible = false;
                comboBox4.SelectedIndex = 0;
            }

            basicInfo();
            listView2.Items.Clear();
            ListViewItem lvi = new ListViewItem();
            lvi.Text = dt.Rows[0][6].ToString();
            lvi.SubItems.Add("-");
            listView2.Items.Add(lvi);
        }

        //选择第三形态
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked == false)
                return;
            dt = ahDetail.SelectToDataTable("select * from pokemon where poke_id=" + detailData2.Rows[0][0].ToString() + ".2");

            comboBox4.Items.Clear();
            if (specialAbilities.Contains(dt.Rows[0][6].ToString()))
            {
                comboBox4.Items.Add(dt.Rows[0][6].ToString());
                comboBox4.Visible = false;
                comboBox4.SelectedIndex = 0;
            }

            basicInfo();
            listView2.Items.Clear();
            ListViewItem lvi = new ListViewItem();
            lvi.Text = dt.Rows[0][6].ToString();
            lvi.SubItems.Add("-");
            listView2.Items.Add(lvi);
        }

        //更新族值条
        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            if (dt == null)
                return;
            // 添加一个画布

            g = e.Graphics;
            //g.Clear(Color.White);


            //画边框
            Color bzlink = Color.FromArgb(185, 185, 185);//标准线颜色
            Pen p = new Pen(new SolidBrush(bzlink), 2);

            int pY = 5;
            int index = 9;
            for (int i = 0; i < 6; i++)
            {
                int rectLength = ((int)dt.Rows[0][index]) * 234 / 255;
                g.DrawRectangle(p, 0, pY, rectLength, 13);
                SolidBrush bBrush = new SolidBrush(racialColors[i]);
                g.FillRectangle(bBrush, 0, pY, rectLength, 13);
                pY += 25;
                index++;
            }
        }

        //更新招式条目
        private void listView5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView5.SelectedItems.Count == 0)
                return;

            string eng = listView5.SelectedItems[0].SubItems[0].Text;
            DataTable detailData4 = ahDetail.SelectToDataTable("select * from move where eng='" + eng.Replace("'","''") + "'");
            if (detailData4.Rows.Count == 0)
                detailData4 = ahDetail.SelectToDataTable("select * from move where eng='" + eng.Replace("'", "''").Replace(" ", "") + "'");
            label20.Text = detailData4.Rows[0][0].ToString();
            label21.Text = detailData4.Rows[0][2].ToString();
            label22.Text = detailData4.Rows[0][1].ToString();
            label26.Text = detailData4.Rows[0][5].ToString();
            if (detailData4.Rows[0][6].ToString().Equals("—")|| detailData4.Rows[0][6].ToString().Equals("变化"))
                label27.Text = detailData4.Rows[0][6].ToString();
            else
                label27.Text = (int)(double.Parse(detailData4.Rows[0][6].ToString()) * 100) + "%";
            label28.Text = detailData4.Rows[0][7].ToString();
            textBox1.Text = detailData4.Rows[0][8].ToString();
            if (detailData4.Rows[0][4].ToString().Equals("物理"))
                pictureBox3.Image = Properties.Resources.physical;
            else if (detailData4.Rows[0][4].ToString().Equals("特殊"))
                pictureBox3.Image = Properties.Resources.special;
            else
                pictureBox3.Image = Properties.Resources.other;
             
            pictureBox6.Image = typePics[(int)(GlobalConstants.TYPEPAIR[detailData4.Rows[0][3].ToString()])];

            label20.Visible = true;
            label21.Visible = true;
            label22.Visible = true;
            label23.Visible = true;
            label24.Visible = true;
            label25.Visible = true;
            label26.Visible = true;
            label27.Visible = true;
            label28.Visible = true;
            pictureBox3.Visible = true;
            pictureBox6.Visible = true;
        }

        //更新特性条目
        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0)
                return;

            string eng = listView2.SelectedItems[0].SubItems[0].Text;
            DataTable detailData4 = ahDetail.SelectToDataTable("select * from ability where eng='" + eng.Replace("'", "''") + "'");
            if (detailData4.Rows.Count == 0)
                detailData4 = ahDetail.SelectToDataTable("select * from ability where eng='" + eng.Replace("'", "''").Replace(" ", "") + "'");
            label20.Text = detailData4.Rows[0][0].ToString();
            label21.Text = detailData4.Rows[0][2].ToString();
            label22.Text = detailData4.Rows[0][1].ToString();
            textBox1.Text = detailData4.Rows[0][3].ToString();
            label20.Visible = true;
            label21.Visible = true;
            label22.Visible = true;
            label23.Visible = false;
            label24.Visible = false;
            label25.Visible = false;
            label26.Visible = false;
            label27.Visible = false;
            label28.Visible = false;
            pictureBox3.Visible = false;
            pictureBox6.Visible = false;

        }

        //更新性格条目
        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView3.SelectedItems.Count == 0)
                return;

            string eng = listView3.SelectedItems[0].SubItems[0].Text;
            DataTable detailData4 = ahDetail.SelectToDataTable("select * from nature where eng='" + eng.Replace("'", "''") + "'");
            label20.Text = detailData4.Rows[0][0].ToString();
            label21.Text = detailData4.Rows[0][2].ToString();
            label22.Text = detailData4.Rows[0][1].ToString();
            if (detailData4.Rows[0][3].ToString().Equals("—"))
                textBox1.Text = "无能力值修正";
            else
                textBox1.Text = detailData4.Rows[0][3].ToString() + " +\r\n" + detailData4.Rows[0][4].ToString() + " -";
            label20.Visible = true;
            label21.Visible = true;
            label22.Visible = true;
            label23.Visible = false;
            label24.Visible = false;
            label25.Visible = false;
            label26.Visible = false;
            label27.Visible = false;
            label28.Visible = false;
            pictureBox3.Visible = false;
            pictureBox6.Visible = false;
        }

        //更新道具条目
        private void listView4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView4.SelectedItems.Count == 0)
                return;

            string eng = listView4.SelectedItems[0].SubItems[0].Text;
            DataTable detailData4 = ahDetail.SelectToDataTable("select * from item where eng='" + eng.Replace("'", "''") + "'");
            if(detailData4.Rows.Count == 0)
                detailData4 = ahDetail.SelectToDataTable("select * from item where eng='" + eng.Replace("'", "''").Replace(" ","") + "'");
            label20.Text = detailData4.Rows[0][0].ToString();
            label21.Text = detailData4.Rows[0][2].ToString();
            label22.Text = detailData4.Rows[0][1].ToString();
            textBox1.Text = detailData4.Rows[0][3].ToString();
            label20.Visible = true;
            label21.Visible = true;
            label22.Visible = true;
            label23.Visible = false;
            label24.Visible = false;
            label25.Visible = false;
            label26.Visible = false;
            label27.Visible = false;
            label28.Visible = false;
            pictureBox3.Visible = false;
            pictureBox6.Visible = false;
        }

        //搜索栏
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            listView1.Visible = false;
            listView6.Visible = true;
            //listView6.Items.Clear();

            if (textBox2.Text == "")
            {
                listView6.Visible = false;
                listView1.Visible = true;
                listView6.Items.Clear();
            }


            else if (proTarget == "" || !textBox2.Text.Contains(proTarget))
            {
                listView6.Items.Clear();
                foreach (DataRow row in raceData.Rows)
                {
                    if (row[2].ToString().Equals(""))
                        continue;
                    if (row[4].ToString().Equals("mega") || row[4].ToString().Equals("舞步") || row[4].ToString().Equals("不倒翁") || row[4].ToString().Equals("剑"))
                        continue;
                    if (row[0].ToString().ToLower().Contains(textBox2.Text.ToLower()) || row[1].ToString().ToLower().Contains(textBox2.Text.ToLower()) || row[3].ToString().Contains(textBox2.Text))
                    {
                        ListViewItem lvi = new ListViewItem();
                        lvi.Text = row[2].ToString();
                        lvi.SubItems.Add((string)row[0]);
                        lvi.SubItems.Add((string)row[1]);
                        lvi.SubItems.Add(row[3].ToString());
                        listView6.Items.Add(lvi);
                        if (row[0].ToString().ToLower().Equals(textBox2.Text.ToLower()) || row[1].ToString().ToLower().Equals(textBox2.Text.ToLower()) || row[3].ToString().Equals(textBox2.Text))
                            lvi.Selected = true;
                    }
                }
                foreach (DataRow row in raceData.Rows)
                {
                    if (!row[2].ToString().Equals(""))
                        break;
                    if (row[4].ToString().Equals("mega") || row[4].ToString().Equals("舞步") || row[4].ToString().Equals("不倒翁") || row[4].ToString().Equals("剑"))
                        continue;
                    if (row[0].ToString().ToLower().Contains(textBox2.Text.ToLower()) || row[1].ToString().ToLower().Contains(textBox2.Text.ToLower()) || row[3].ToString().Contains(textBox2.Text))
                    {
                        ListViewItem lvi = new ListViewItem();
                        lvi.Text = "-";
                        lvi.SubItems.Add((string)row[0]);
                        lvi.SubItems.Add((string)row[1]);
                        lvi.SubItems.Add(row[3].ToString());
                        listView6.Items.Add(lvi);
                        if (row[0].ToString().ToLower().Equals(textBox2.Text.ToLower()) || row[1].ToString().ToLower().Equals(textBox2.Text.ToLower()) || row[3].ToString().Equals(textBox2.Text))
                            lvi.Selected = true;
                    }
                }
                if (listView6.Items.Count == 1)
                    listView6.Items[0].Selected = true;
            }
            else
            {
                foreach (ListViewItem lvi in listView6.Items)
                {
                    if (!lvi.SubItems[1].Text.ToLower().Contains(textBox2.Text.ToLower()) && !lvi.SubItems[2].Text.ToLower().Contains(textBox2.Text.ToLower()) && !lvi.SubItems[3].Text.Contains(textBox2.Text))
                        listView6.Items.Remove(lvi);
                    else if (lvi.SubItems[1].Text.ToLower().Equals(textBox2.Text.ToLower()) || lvi.SubItems[2].Text.ToLower().Equals(textBox2.Text.ToLower()) || lvi.SubItems[3].Text.Equals(textBox2.Text))
                        lvi.Selected = true;
                }
                if (listView6.Items.Count == 1)
                    listView6.Items[0].Selected = true;
            }

            proTarget = textBox2.Text;
        }

        //选择搜索结果
        private void listView6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView6.SelectedItems.Count == 0)
                return;
            string pkId = listView6.SelectedItems[0].SubItems[3].ToString();
            foreach (ListViewItem lvi in listView1.Items)
            {
                if (compareMode&&lvi.SubItems[4].ToString().Equals(pkId))
                {
                    lvi.Selected = true;
                    return;
                }
                if ((!compareMode) && lvi.SubItems[3].ToString().Equals(pkId))
                {
                    lvi.Selected = true;
                    return;
                }
            }
        }

        //榜单排序调整
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (listView1.Columns[e.Column].Tag == null)
            {
                listView1.Columns[e.Column].Tag = true;
            }
            bool tabK = (bool)listView1.Columns[e.Column].Tag;
            if (tabK)
            {
                listView1.Columns[e.Column].Tag = false;
            }
            else
            {
                listView1.Columns[e.Column].Tag = true;
            }
            listView1.ListViewItemSorter = new ListViewSort(e.Column, listView1.Columns[e.Column].Tag,compareMode);
            listView1.Sort();
        }

        //更新界面
        private void updateDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateData ud = new UpdateData(false);
            if (ud.valid)
            {
                SelfDesignedMsg sdm = new SelfDesignedMsg("PGL发布新的数据了~ 是否查看更新信息?", true);
                DialogResult dr = sdm.ShowDialog();
                if (dr == DialogResult.Yes)
                    ud.Show();
            }
        }

        //数据分析界面
        private void analysisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (compareMode)
                {
                    SelfDesignedMsg sdm = new SelfDesignedMsg("该模块仅限于单期模式哦~", false);
                    sdm.ShowDialog();
                    return;
                }
                ShowAnalysis sa = new ShowAnalysis(ahDetail, curSingleFile, rankTables);
                sa.Show();
            }
            catch
            {
                SelfDesignedMsg sdm = new SelfDesignedMsg("Sorry~~ 无法对该榜单进行分析,可能榜单存在部分数据缺失", false);
                sdm.ShowDialog();
            }
        }

        //选择旧榜单
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex < 0)
                return;
            oldFile = files[comboBox2.SelectedIndex].Name;
        }

        //选择新榜单
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex < 0)
                return;
            newFile = files[comboBox3.SelectedIndex].Name;
        }

        //比较按钮响应
        private void button1_Click_1(object sender, EventArgs e)    
        {
            compareTables();
        }

        //进行比较
        private void compareTables()
        {
            if (oldFile == null || newFile == null)
                return;

            int oldSeason = int.Parse(oldFile.Split('-')[1]);
            int newSeason = int.Parse(newFile.Split('-')[1]);
            int oldDate = int.Parse(oldFile.Split('-')[2]);
            int newDate = int.Parse(newFile.Split('-')[2]);
            if (oldSeason > newSeason || (oldSeason == newSeason && oldDate >= newDate))
            {
                SelfDesignedMsg sdm = new SelfDesignedMsg("新旧榜单在时序上存在问题,请再确认下~", false);
                sdm.ShowDialog();
                return;
            }

            ah = new AccessHandler("Data/"+ generationSelected+ "/" + newFile);
            ahOld= new AccessHandler("Data/" + generationSelected + "/" + oldFile);
            rankTables = ah.showAllTables();
            string[] rankTablesOld = ahOld.showAllTables();
            HashSet<string> oldTables = new HashSet<string>(rankTablesOld);

            shownTables.Clear();
            comboBox1.Items.Clear();
            int defaultTable = -1;
            for (int i = 0; i < rankTables.Length; i++)
            {
                if (!oldTables.Contains(rankTables[i]))
                    continue;
                comboBox1.Items.Add(generationSelected + "-" + GlobalConstants.TABLENAMES[rankTables[i][10] - '0']);
                shownTables.Add(i);
                if (GlobalConstants.DEFAULTBATTLETYPE[0] == rankTables[i][10])
                    defaultTable = shownTables.Count-1;
            }
            if (defaultTable >= 0)
            {
                comboBox1.SelectedIndex = -1;
                comboBox1.SelectedIndex = defaultTable;
            }
            else
            {
                SelfDesignedMsg sdm = new SelfDesignedMsg("Sorry~~ 【对战类型】下拉菜单中没找到您的默认榜单, 请手动选择其它榜单~", false);
                sdm.ShowDialog();
            }
        }


        //换特性以更新抗性
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox4.SelectedIndex>=0)
                setResistance(comboBox4.SelectedIndex);
        }

        
        private void listView7_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listView7_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        //切换到单期模式
        private void singleModeToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (singleModeToolStripMenuItem.Text.Last() == '√')
                return;
            Config.setConfig("ViewOptions", "single");
            GlobalConstants.VIEWOPTIONS = "single";
            singleModeView();
        }

        //切换到比较模式
        private void compareModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (compareModeToolStripMenuItem.Text.Last() == '√')
                return;
            Config.setConfig("ViewOptions", "compare");
            GlobalConstants.VIEWOPTIONS = "compare";
            compareModeView();
        }

        //单期模式显示
        private void singleModeView()
        {
            compareMode = false;
            listView1.ListViewItemSorter = null;
            compareModeToolStripMenuItem.Text = "比较";
            compareModeToolStripMenuItem.ForeColor = SystemColors.ControlText;
            compareModeToolStripMenuItem.Font = defaultMenuFont;
            singleModeToolStripMenuItem.Text = "单期 √";
            singleModeToolStripMenuItem.ForeColor = SystemColors.MenuHighlight;
            singleModeToolStripMenuItem.Font = selectedMenuFont;

            

            textBox2.Text = "";
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            comboBox3.Items.Clear();
            comboBox5.Items.Clear();
            comboBox1.Text = "";
            comboBox2.Text = "";
            comboBox3.Text = "";
            comboBox5.Text = "";

            button1.Visible = false;
            label31.Visible = false;
            label32.Visible = false;
            comboBox2.Visible = false;
            comboBox3.Visible = false;
            
            button1.Enabled = false;
            label31.Enabled = false;
            label32.Enabled = false;
            comboBox2.Enabled = false;
            comboBox3.Enabled = false;

            Update();

            label34.Visible = true;
            comboBox5.Visible = true;

            label34.Enabled = true;
            comboBox5.Enabled = true;

            listView1.Columns.Clear();
            listView1.Items.Clear();
            listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                    columnHeader1,
                    columnHeader2,
                    columnHeader3,
                    columnHeader4});
            columnHeader1.Text = "排名";
            columnHeader1.Width = 64;
            columnHeader2.Text = "中文名";
            columnHeader2.Width = 121;
            columnHeader3.Text = "英文名";
            columnHeader3.Width = 100;
            columnHeader4.Text = "图鉴编号";
            columnHeader4.Width = 60;

            listView2.Columns.Clear();
            listView2.Items.Clear();
            listView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                    columnHeader5,
                    columnHeader6});
            listView2.Location = new System.Drawing.Point(158, 206);
            listView2.Size = new System.Drawing.Size(140, 79);

            listView3.Columns.Clear();
            listView3.Items.Clear();
            listView3.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                    columnHeader7,
                    columnHeader8});
            listView3.Location = new System.Drawing.Point(158, 316);
            listView3.Size = new System.Drawing.Size(140, 79);

            listView4.Columns.Clear();
            listView4.Items.Clear();
            listView4.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                    columnHeader9,
                    columnHeader10});
            listView4.Location = new System.Drawing.Point(158, 426);
            listView4.Size = new System.Drawing.Size(140, 79);


            listView5.Columns.Clear();
            listView5.Items.Clear();
            listView5.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                    columnHeader11,
                    columnHeader12,
                    columnHeader13});
            listView5.Location = new System.Drawing.Point(347, 206);
            listView5.Size = new System.Drawing.Size(208, 300);
            if(GlobalConstants.dpiX == 120)
                listView5.Size = new System.Drawing.Size(listView5.Size.Width+5, 300);



            DirectoryInfo TheFolder = new DirectoryInfo("Data/" + generationSelected);
            files = TheFolder.GetFiles("*.mdb");

            myCompare mc = new myCompare();
            var sortedFiles = files.OrderBy(a => a.Name, mc);
            files = sortedFiles.ToArray();

            int historySelected = -1; ;
            string curFileName = curSingleFile.Split('/')[2];
            foreach (FileInfo NextFile in files)
            {
                string[] tempParts = NextFile.Name.Split('-');
                string fileDate = tempParts[1]+ "赛季-" + tempParts[2];
                comboBox5.Items.Add(fileDate);
                if (NextFile.Name.Equals(curFileName))
                    historySelected = comboBox5.Items.Count - 1;
            }
            comboBox5.SelectedIndex = historySelected;


        }

        //根据选中历史榜单更新对战类型下拉菜单
        private void showCertainHistory()
        {
            shownTables.Clear();
            ah = new AccessHandler(curSingleFile);
            rankTables = ah.showAllTables();
            int defaultTable = -1;
            comboBox1.Items.Clear();
            for (int i = 0; i < rankTables.Length; i++)
            {
                comboBox1.Items.Add(generationSelected + "-" + GlobalConstants.TABLENAMES[rankTables[i][10] - '0']);
                shownTables.Add(i);
                if (GlobalConstants.DEFAULTBATTLETYPE[0] == rankTables[i][10])
                    defaultTable = shownTables.Count - 1;
            }
            if (defaultTable >= 0)
            {
                comboBox1.SelectedIndex = -1;
                comboBox1.SelectedIndex = defaultTable;
            }
            else
            {
                SelfDesignedMsg sdm = new SelfDesignedMsg("Sorry~~ 【对战类型】下拉菜单中没有找到您的默认榜单, 请手动选择其它榜单~", false);
                sdm.ShowDialog();
            }
        }

        //比较模式显示
        private void compareModeView()
        {
            compareMode = true;
            listView1.ListViewItemSorter = null;
            singleModeToolStripMenuItem.Text = "单期";
            singleModeToolStripMenuItem.ForeColor = SystemColors.ControlText;
            singleModeToolStripMenuItem.Font = defaultMenuFont;
            compareModeToolStripMenuItem.Text = "比较 √";
            compareModeToolStripMenuItem.ForeColor = SystemColors.MenuHighlight;
            compareModeToolStripMenuItem.Font = selectedMenuFont;

            

            textBox2.Text = "";
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            comboBox3.Items.Clear();
            comboBox5.Items.Clear();
            comboBox1.Text = "";
            comboBox2.Text = "";
            comboBox3.Text = "";
            comboBox5.Text = "";
         
            label34.Visible = false;
            comboBox5.Visible = false;
            
            label34.Enabled = false;
            comboBox5.Enabled = false;

            Update();

            button1.Visible = true;
            label31.Visible = true;
            label32.Visible = true;
            comboBox2.Visible = true;
            comboBox3.Visible = true;

            button1.Enabled = true;
            label31.Enabled = true;
            label32.Enabled = true;
            comboBox2.Enabled = true;
            comboBox3.Enabled = true;

            listView1.Columns.Clear();
            listView1.Items.Clear();
            listView1.Columns.AddRange(new ColumnHeader[] {
                    columnHeader1,
                    columnHeader18,
                    columnHeader2,
                    columnHeader3,
                    columnHeader4});
            columnHeader1.Text = "排名";
            columnHeader1.Width = 40;
            columnHeader18.Text = "升降";
            columnHeader18.Width = 44;
            columnHeader2.Text = "中文名";
            columnHeader2.Width = 121;
            columnHeader3.Text = "英文名";
            columnHeader3.Width = 100;
            columnHeader4.Text = "图鉴编号";
            columnHeader4.Width = 60;

            listView2.Columns.Clear();
            listView2.Items.Clear();
            listView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                    columnHeader5,
                    columnHeader6,
                    columnHeader21});
            listView2.Location = new System.Drawing.Point(121, 206);
            listView2.Size = new System.Drawing.Size(190, 79);

            listView3.Columns.Clear();
            listView3.Items.Clear();
            listView3.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                    columnHeader7,
                    columnHeader8,
                    columnHeader22});
            listView3.Location = new System.Drawing.Point(121, 316);
            listView3.Size = new System.Drawing.Size(190, 79);

            listView4.Columns.Clear();
            listView4.Items.Clear();
            listView4.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                    columnHeader9,
                    columnHeader10,
                     columnHeader23});
            listView4.Location = new System.Drawing.Point(121, 426);
            listView4.Size = new System.Drawing.Size(190, 79);

            listView5.Columns.Clear();
            listView5.Items.Clear();
            listView5.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                    columnHeader11,
                    columnHeader12,
                    columnHeader13,
                    columnHeader24});
            listView5.Location = new System.Drawing.Point(323, 206);
            listView5.Size = new System.Drawing.Size(258, 300);
            if (GlobalConstants.dpiX == 120)
                listView5.Size = new System.Drawing.Size(listView5.Size.Width + 5, 300);



            DirectoryInfo TheFolder = new DirectoryInfo("Data/" + generationSelected);
            files = TheFolder.GetFiles("*.mdb");
            if (files.Count() < 2)
            {
                SelfDesignedMsg sdm = new SelfDesignedMsg("您的历史数据暂时不足以进行比较哈~", false);
                sdm.ShowDialog();
                return;
            }
            myCompare mc = new myCompare();
            var sortedFiles = files.OrderBy(a => a.Name,mc);
            files = sortedFiles.ToArray();

            foreach (FileInfo NextFile in files)
            {
                string[] tempParts = NextFile.Name.Split('-');
                string fileDate = tempParts[1] +"赛季-" + tempParts[2];
                comboBox2.Items.Add(fileDate);
                comboBox3.Items.Add(fileDate);
            }

            comboBox2.SelectedIndex = 1;
            comboBox3.SelectedIndex = 0;
            compareTables();
            
        }

        //设置自动检测更新
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            if (toolStripMenuItem7.Text.Contains("√"))
                return;
            Config.setConfig("CheckUpdate", "1");
            GlobalConstants.CHECKUPDATE = "1";
            toolStripMenuItem7.Text += " √";
            toolStripMenuItem7.ForeColor = SystemColors.MenuHighlight;
            toolStripMenuItem7.Font = selectedMenuFont;

            toolStripMenuItem8.Text = toolStripMenuItem8.Text.Replace(" √", "");
            toolStripMenuItem8.ForeColor = SystemColors.ControlText;
            toolStripMenuItem8.Font = defaultMenuFont;

        }

        //取消自动更新
        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            if (toolStripMenuItem8.Text.Contains("√"))
                return;
            Config.setConfig("CheckUpdate", "0");
            GlobalConstants.CHECKUPDATE = "0";
            toolStripMenuItem8.Text += " √";
            toolStripMenuItem8.ForeColor = SystemColors.MenuHighlight;
            toolStripMenuItem8.Font = selectedMenuFont;

            toolStripMenuItem7.Text = toolStripMenuItem7.Text.Replace(" √", "");
            toolStripMenuItem7.ForeColor = SystemColors.ControlText;
            toolStripMenuItem7.Font = defaultMenuFont;
        }

        //设置时区
        private void timezoneStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 3; i++)
            {
                if (timezoneToolStripMenuItem.DropDownItems[i] == sender)
                {
                    if (timezoneToolStripMenuItem.DropDownItems[i].Text.Contains("√"))
                        return;
                    Config.setConfig("Timezone", timezoneToolStripMenuItem.DropDownItems[i].Text);
                    GlobalConstants.TIMEZONE = timezoneToolStripMenuItem.DropDownItems[i].Text;
                    timezoneToolStripMenuItem.DropDownItems[i].Text += " √";
                    timezoneToolStripMenuItem.DropDownItems[i].ForeColor = SystemColors.MenuHighlight;
                    timezoneToolStripMenuItem.DropDownItems[i].Font = selectedMenuFont;

                }
                else
                {
                    if (timezoneToolStripMenuItem.DropDownItems[i].Text.Contains("√"))
                        timezoneToolStripMenuItem.DropDownItems[i].Text = timezoneToolStripMenuItem.DropDownItems[i].Text.Replace(" √", "");
                    timezoneToolStripMenuItem.DropDownItems[i].ForeColor = SystemColors.ControlText;
                    timezoneToolStripMenuItem.DropDownItems[i].Font = defaultMenuFont;
                }
            }

        }

        //设置游戏版本
        private void generationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool isModified=false;
            for (int i = 0; i < generationToolStripMenuItem.DropDownItems.Count; i++)
            {
                if (generationToolStripMenuItem.DropDownItems[i] == sender)
                {
                    if (generationToolStripMenuItem.DropDownItems[i].Text.Contains("√"))
                        return;
                    isModified = true;
                    Config.setConfig("DefaultGeneration", i.ToString());
                    GlobalConstants.DEFAULTGENERATION = i.ToString();
                    generationSelected=GlobalConstants.GENERATION[i].ToString();
                    generationToolStripMenuItem.DropDownItems[i].Text += " √";
                    generationToolStripMenuItem.DropDownItems[i].ForeColor = SystemColors.MenuHighlight;
                    generationToolStripMenuItem.DropDownItems[i].Font = selectedMenuFont;

                }
                else
                {
                    if (generationToolStripMenuItem.DropDownItems[i].Text.Contains("√"))
                        generationToolStripMenuItem.DropDownItems[i].Text = generationToolStripMenuItem.DropDownItems[i].Text.Replace(" √", "");
                    generationToolStripMenuItem.DropDownItems[i].ForeColor = SystemColors.ControlText;
                    generationToolStripMenuItem.DropDownItems[i].Font = defaultMenuFont;
                }
            }
            if(isModified)
                reloadByViewMode();
        }

        //设置默认榜单
        private void defautBattleTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < defaultTableToolStripMenuItem.DropDownItems.Count; i++)
            {
                if (defaultTableToolStripMenuItem.DropDownItems[i] == sender)
                {
                    if (defaultTableToolStripMenuItem.DropDownItems[i].Text.Contains("√"))
                        return;
                    Config.setConfig("DefaultBattleType", i.ToString());
                    GlobalConstants.DEFAULTBATTLETYPE = i.ToString();
                    defaultTableToolStripMenuItem.DropDownItems[i].Text += " √";
                    defaultTableToolStripMenuItem.DropDownItems[i].ForeColor = SystemColors.MenuHighlight;
                    defaultTableToolStripMenuItem.DropDownItems[i].Font = selectedMenuFont;

                }
                else
                {
                    if (defaultTableToolStripMenuItem.DropDownItems[i].Text.Contains("√"))
                        defaultTableToolStripMenuItem.DropDownItems[i].Text = defaultTableToolStripMenuItem.DropDownItems[i].Text.Replace(" √", "");
                    defaultTableToolStripMenuItem.DropDownItems[i].ForeColor = SystemColors.ControlText;
                    defaultTableToolStripMenuItem.DropDownItems[i].Font = defaultMenuFont;
                }
            }

        }

        //更换历史榜单
        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox5.SelectedIndex < 0)
                return;
            curSingleFile = "Data/" + generationSelected + "/" +files[comboBox5.SelectedIndex].Name;
            showCertainHistory();
        }

        //更新软件
        private void updateAppToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateApp ua = new UpdateApp(false);
                if (ua.hasNew)
                {
                    SelfDesignedMsg sdm = new SelfDesignedMsg("主人发布新版本软件啦~ 是否查看更新信息?", true);
                    DialogResult dr = sdm.ShowDialog();
                    if (dr == DialogResult.Yes)
                        ua.Show();
                }
            }
            catch
            {
                SelfDesignedMsg sdm = new SelfDesignedMsg("Sorry~~ 暂时无法连接更新源,可能Github暂时无法连接或者主人在对更新源进行维护", false);
                sdm.ShowDialog();
            }
        }

        //更新配置文件
        private void configureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigModification cf = new ConfigModification();
            cf.Show();
        }


        //关于PGLData
        private void aboutPGLDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutPGLData apd = new AboutPGLData();
            apd.Show();
        }

        //使用说明
        private void instructionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Environment.CurrentDirectory + "/Update/使用说明.pdf");
            }
            catch
            {

            }
        }
        
        //下载旧赛季
        private void downloadOldSeasonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DownloadOldSeasons dos = new DownloadOldSeasons();
                if (dos.hasNew)
                    dos.Show();
                else
                {
                    SelfDesignedMsg sdm = new SelfDesignedMsg("您已拥有全部历史数据咯~", false);
                    sdm.ShowDialog();
                }
            }
            catch
            {
                SelfDesignedMsg sdm = new SelfDesignedMsg("Sorry~~ 小女子暂时无法链接github", false);
                sdm.ShowDialog();
            }
        }
    }
}
