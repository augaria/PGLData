using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace PGLData
{
    //form for the visualization of analyis result
    public partial class ShowAnalysis : Form
    {

        public class myCompare : IComparer
        {

            int IComparer.Compare(Object x, Object y)
            {
                double a = ((KeyValuePair<string, double>)x).Value;
                double b = ((KeyValuePair<string, double>)y).Value;
                if (a < b)
                    return 1;
                else if (a > b)
                    return -1;
                else
                    return 0;
            }
        }

        Graphics g;
        Analyze az;
        string curSingleFile;
        AccessHandler ah;
        string[] rankTables;
        int battleType;
        double[] defaultpara;
        bool selfDefined;



        ArrayList racialSpeedPoints;
        ArrayList racialSpeedWeightedPoints;
        ArrayList racialSpeedWeightedAccumulatedPoints;
        ArrayList extSpeedPoints;


        public ShowAnalysis(AccessHandler ah,string curSingleFile,string[]  rankTables)
        {
            InitializeComponent();

            if (GlobalConstants.dpiX == 120)
            {
                foreach (Control ct in this.Controls)
                    ct.Font = new System.Drawing.Font(ct.Font.FontFamily, (float)(ct.Font.Size / 1.25), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            }
            label15.Text = "Top" + GlobalConstants.POPULARTHRESHOLD + label15.Text;

            battleType = -1;
            this.curSingleFile = curSingleFile;
            this.ah = ah;
            this.rankTables = rankTables;
            defaultpara = new double[6];
            defaultpara[0] = 0;
            for (int i = 1; i < 6; i++)
                defaultpara[i] = double.Parse(GlobalConstants.ANALYSISPARA[i - 1]);

            radioButton1.Checked = true;

            string fileName = curSingleFile.Split('/')[2].Replace(".mdb", "");
            string[] nameParts = fileName.Split('-');
            label6.Text = nameParts[0] + "-" + nameParts[1] + "赛季-" + nameParts[2];

            comboBox1.Items.Clear();
            string generationSelected = curSingleFile.Split('/')[1];
            int defaultTable = -1;
            for (int i = 0; i < rankTables.Length; i++)
            {
                comboBox1.Items.Add(generationSelected + "-" + GlobalConstants.TABLENAMES[rankTables[i][10] - '0']);
                if (GlobalConstants.DEFAULTBATTLETYPE[0]-'0' == rankTables[i][10] - '0')
                    defaultTable = i;
            }
            if (defaultTable >= 0)
            {
                comboBox1.SelectedIndex = -1;
                comboBox1.SelectedIndex = defaultTable;
            }
            else
            {
                SelfDesignedMsg sdm = new SelfDesignedMsg("Sorry~~【对战类型】下拉菜单中没有找到您所设的默认榜单, 请手动选择其它榜单~", false);
                sdm.ShowDialog();
            }
        }

        private void presentResult(int battleType)
        {
            //使用率分布
            KeyValuePair<string, double>[] ur = az.useageRateTop(200);

            //列表
            listView4.Items.Clear();
            for(int i=0;i<ur.Length;i++)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = (i + 1).ToString();
                lvi.SubItems.Add(ur[i].Key);

                double y;
                if (battleType == 1)
                    y = ur[i].Value * 300;
                else if (battleType == 2 || battleType == 4)
                    y = ur[i].Value * 400;
                else if (battleType == 3)
                    y = ur[i].Value * 600;
                else
                    y = ur[i].Value * 100 * ((int)GlobalConstants.GENESPECIALPARA[int.Parse(GlobalConstants.DEFAULTGENERATION)]);

                lvi.SubItems.Add(Math.Round(y, 2) + "%");
                listView4.Items.Add(lvi);
            }

            //折线图
            chart2.Series.Clear();
            chart2.ChartAreas[0].AxisX.Minimum = 1;
            chart2.ChartAreas[0].AxisY.Minimum = 0;
            chart2.ChartAreas[0].AxisY.Maximum = 80;
            Series series = new Series("StackedArea");
            series.ChartType = SeriesChartType.Line;
            series.BorderWidth = 2; series.ShadowOffset = 0;
            int limitation = Math.Min(50, ur.Length);
            for(int i= 0; i < limitation; i++)
            {
                double y;
                if (battleType == 1)
                    y=ur[i].Value * 300;
                else if (battleType == 2 || battleType == 4)
                    y = ur[i].Value * 400;
                else if (battleType == 3)
                    y = ur[i].Value * 600;
                else
                    y = ur[i].Value * 100 * ((int)GlobalConstants.GENESPECIALPARA[int.Parse(GlobalConstants.DEFAULTGENERATION)]);

                series.Points.AddXY(i+1, y);
            }
            chart2.Series.Add(series);

            //速度修正分布
            extSpeedPoints = az.speedDistribution();

            listView1.Items.Clear();
            foreach (KeyValuePair<int, double> point in extSpeedPoints)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.SubItems.Add(point.Key.ToString());
                lvi.SubItems.Add(Math.Round(point.Value, 1) + "%");
                if (point.Value >= GlobalConstants.EXTREMESPEEDRATEVERYHIGH)
                {
                    lvi.BackColor = Color.DeepSkyBlue;
                }
                else if (point.Value >= GlobalConstants.EXTREMESPEEDRATEHIGH)
                {
                    lvi.BackColor = Color.LightSkyBlue;
                }
                else if (point.Value >= GlobalConstants.EXTREMESPEEDRATELOW)
                {
                    lvi.BackColor = Color.LightPink;
                }
                else
                {
                    lvi.BackColor = Color.Red;
                }
                listView1.Items.Add(lvi);
            }

            //速度种族分布
            racialSpeedPoints = az.speedRacialDistribution();

            listView5.Items.Clear();
            foreach (KeyValuePair<int, int> point in racialSpeedPoints)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.SubItems.Add(point.Key.ToString());
                lvi.SubItems.Add(point.Value.ToString());
                listView5.Items.Add(lvi);
            }

            //速度种族权重分布
            racialSpeedWeightedPoints = az.speedRacialWeightedDistribution();

            listView6.Items.Clear();
            foreach (KeyValuePair<int, double> point in racialSpeedWeightedPoints)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.SubItems.Add(point.Key.ToString());
                lvi.SubItems.Add(Math.Round(point.Value, 1).ToString());
                listView6.Items.Add(lvi);
            }

            //速度种族权重累加分布
            racialSpeedWeightedAccumulatedPoints = az.speedRacialWeightedAccumulatedDistribution(racialSpeedWeightedPoints);

            listView7.Items.Clear();
            foreach (KeyValuePair<int, double> point in racialSpeedWeightedAccumulatedPoints)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.SubItems.Add(point.Key.ToString());
                lvi.SubItems.Add(Math.Round(point.Value, 2).ToString() + "%");
                listView7.Items.Add(lvi);
            }



            //技能列表
            listView2.Items.Clear();
            ArrayList moves = az.moveDistribution();
            int rank = 1;
            foreach (KeyValuePair<string, double> move in moves)
            {
                ListViewItem lvi = new ListViewItem();
                string[] moveDetail = move.Key.Split(',');
                lvi.Text = rank.ToString();
                lvi.SubItems.Add(moveDetail[0]);
                lvi.SubItems.Add(Math.Round(move.Value, 1) + "%");
                lvi.BackColor = GlobalConstants.TYPECOLORS[int.Parse(moveDetail[1]) - 1];
                lvi.ForeColor = Color.White;
                listView2.Items.Add(lvi);
                rank++;
            }

            //mega配比列表
            rank = 1;
            ArrayList megaDetails = az.megaUsageRate(ah);
            myCompare mc = new myCompare();
            megaDetails.Sort(mc);

            listView3.Items.Clear();
            double total = ((KeyValuePair<string, double>)(megaDetails[0])).Value;

            double single;
            if (battleType == 1)
                single = total * 3 / 100;
            else if (battleType == 2 || battleType == 4)
                single = total * 4 / 100;
            else if(battleType ==3)
                single = total * 6 / 100;
            else
                single = total * ((int)GlobalConstants.GENESPECIALPARA[int.Parse(GlobalConstants.DEFAULTGENERATION)]) / 100;

            label9.Text= Math.Round(single, 2) + "只";

            for (int i = 1; i < megaDetails.Count; i++)
            {
                KeyValuePair<string, double> megaPoke = (KeyValuePair<string, double>)megaDetails[i];
                ListViewItem lvi = new ListViewItem();
                lvi.Text = rank.ToString();
                lvi.SubItems.Add(megaPoke.Key.Replace("Mega", ""));
                lvi.SubItems.Add(Math.Round(megaPoke.Value * 100 / total, 2) + "%");
                listView3.Items.Add(lvi);
                rank++;
            }

            //神兽统计
            label11.Text = Math.Round(az.ancientUsageRate() * ((int)GlobalConstants.GENESPECIALPARA[int.Parse(GlobalConstants.DEFAULTGENERATION)]), 2) + "只";
        }


        //画物理特殊对比图
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (comboBox1.SelectedIndex < 0)
                return;

            g = e.Graphics;
            //g.Clear(Color.White);

            double[] defense = az.DefenseDistribution();

            double[] attack = az.AttackDistribution();

            //画边框
            Color bzlink = Color.FromArgb(185, 185, 185);//标准线颜色
            Pen p = new Pen(new SolidBrush(bzlink), 2);

            int pY = 5;

            int left = (int)(attack[0] * pictureBox1.Width / (attack[0]+ attack[1]));
            int right = (int)(attack[1] * pictureBox1.Width / (attack[0] + attack[1]));

            g.DrawRectangle(p, 0, pY, left, 25);
            SolidBrush bBrush = new SolidBrush(Color.FromArgb(247, 82, 49));
            g.FillRectangle(bBrush, 0, pY, left, 25);
            g.DrawRectangle(p, left, pY, right, 25);
            bBrush = new SolidBrush(Color.FromArgb(82, 115, 173));
            g.FillRectangle(bBrush, left, pY, right, 25);

            double barWidth;
            int offset;
            barWidth= Math.Round(attack[0], 1);
            if (barWidth < 10)
                offset = 14;
            else
                offset = 16;
            g.DrawString(barWidth + "%", new Font("Microsoft Sans Serif", Font.Size), Brushes.Black, new Point(left/ 2 - offset, pY + 6));
            barWidth = Math.Round(attack[1], 1);
            if (barWidth < 10)
                offset = 14;
            else
                offset = 16;
            g.DrawString(barWidth + "%", new Font("Microsoft Sans Serif", Font.Size), Brushes.Black, new Point(left + right / 2 - offset, pY + 6));

            label1.Location = new Point(label1.Location.X, pictureBox1.Location.Y + pY +6);
            label2.Location = new Point(label2.Location.X, pictureBox1.Location.Y + pY + 6);

            pY += 60;
            left = (int)(defense[0] * pictureBox1.Width / (defense[0]+ defense[1]));
            right = (int)(defense[1] * pictureBox1.Width / (defense[0] + defense[1]));
            g.DrawRectangle(p, 0, pY, left, 25);
            bBrush = new SolidBrush(Color.FromArgb(247, 82, 49));
            g.FillRectangle(bBrush, 0, pY, left, 25);
            g.DrawRectangle(p, left, pY, right, 25);
            bBrush = new SolidBrush(Color.FromArgb(82, 115, 173));
            g.FillRectangle(bBrush, left, pY, right, 25);

            barWidth = Math.Round(defense[0], 1);
            if (barWidth < 10)
                offset = 14;
            else
                offset = 16;
            g.DrawString(barWidth + "%", new Font("Microsoft Sans Serif", Font.Size), Brushes.Black, new Point(left / 2 - offset, pY + 6));
            barWidth = Math.Round(defense[1], 1);
            if (barWidth < 10)
                offset = 14;
            else
                offset = 16;
            g.DrawString(barWidth + "%", new Font("Microsoft Sans Serif", Font.Size), Brushes.Black, new Point(left + right / 2 - offset, pY + 6));
            label3.Location = new Point(label3.Location.X, pictureBox1.Location.Y+ pY + 6);
            label4.Location = new Point(label4.Location.X, pictureBox1.Location.Y + pY + 6);

        }

        //选择对战类型
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex < 0)
            {
                battleType = -1;
                return;
            }
            battleType = rankTables[comboBox1.SelectedIndex][10]-'0';
            if (battleType == 0)
            {
                SelfDesignedMsg sdm = new SelfDesignedMsg("该对战类型无法进行分析哦~", false);
                sdm.ShowDialog();
                return;
            }
            textBox1.Text = defaultpara[battleType].ToString();
            trackBar1.Value = (int)(defaultpara[battleType] / 0.05);
            updateAnalysis();
        }

        //使用长尾模型
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton1.Checked)
                return;
            selfDefined = false;
            radioButton2.Checked = false;
        }

        //使用自定义模型
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton2.Checked)
                return;
            selfDefined = true;
            radioButton1.Checked = false;
        }

        //点击统计按钮
        private void button1_Click(object sender, EventArgs e)
        {
            updateAnalysis();
        }

        //进行分析统计
        private void updateAnalysis()
        {
            if (battleType == -1)
                return;

            double para = 1;
            if (textBox1.Text != "")
            {
                try
                {
                    para = -double.Parse(textBox1.Text);
                }
                catch
                {
                    SelfDesignedMsg sdm = new SelfDesignedMsg("Sorry~ 小女子无法解析您所输入的参数a的值,请重新输入或直接拖动滑动条~", false);
                    sdm.ShowDialog();
                    return;
                }
            }
            az = new Analyze(curSingleFile, battleType, para, selfDefined);

            if (az.initalFailed)
                return;
            presentResult(battleType);
            pictureBox1.Refresh();
            
        }

        //调整A值的拖动条
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            textBox1.Text = (trackBar1.Value*0.05).ToString();
        }
 

        //速度种族分布图
        private void button2_Click(object sender, EventArgs e)
        {
            ChartPanel cp = new ChartPanel(racialSpeedPoints, label15.Text, "精灵数", false, true);
            cp.Show();
        }

        //速度种族权重分布图
        private void button3_Click(object sender, EventArgs e)
        {
            ChartPanel cp = new ChartPanel(racialSpeedWeightedPoints, "速度种族统计(W)", "权重", false, false);
            cp.Show();
        }

        //速度种族权重累加分布图
        private void button4_Click(object sender, EventArgs e)
        {
            ChartPanel cp = new ChartPanel(racialSpeedWeightedAccumulatedPoints, "<=某速度种族的百分比(W)", "百分比", true, false);
            cp.Show();
        }

        //速度修正分布图
        private void button5_Click(object sender, EventArgs e)
        {
            ChartPanel cp = new ChartPanel(extSpeedPoints, "性格加速度的百分比(W)", "百分比", false, false);
            cp.Show();
        }
    }
}
