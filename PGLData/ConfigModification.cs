using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PGLData
{
    //form that used for modification for the configuration file
    public partial class ConfigModification : Form
    {
        ArrayList al;

        public ConfigModification()
        {

            InitializeComponent();

            if (GlobalConstants.dpiX == 120)
            {
                foreach (Control ct in this.Controls)
                    ct.Font = new System.Drawing.Font(ct.Font.FontFamily, (float)(ct.Font.Size / 1.25), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            }

            al = new ArrayList();
            al.Add("MaxSpeed");
            al.Add("DefineAttack");
            al.Add("DefineSpecialAttack");
            al.Add("DefinePhysicalShield");
            al.Add("DefineSpecialShield");
            al.Add("ExtremeSpeedRateLow");
            al.Add("ExtremeSpeedRateHigh");
            al.Add("ExtremeSpeedRateVeryHigh");
            al.Add("ApparentRankingChange");
            al.Add("ApparentUsageRateChange");

            al.Add("para-single");
            al.Add("para-double");
            al.Add("para-triple");
            al.Add("para-rotation");
            al.Add("para-special");
            al.Add("PopularThreshold");
            al.Add("AncientPokes");
            al.Add("MegaStones");

            for(int i= 0;i< GlobalConstants.GENESPECIALPARA.Count;i++)
            {
                al.Add("Gene" + (i + 1).ToString() + "SpecialPara");
            }

            int index;
            index=dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = "最大速度种族";
            dataGridView1.Rows[index].Cells[1].Value = GlobalConstants.MAXSPEED;
            index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = "高物攻种族";
            dataGridView1.Rows[index].Cells[1].Value = GlobalConstants.DEFINEATTACK;
            index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = "高特攻种族";
            dataGridView1.Rows[index].Cells[1].Value = GlobalConstants.DEFINESPECIALATTACK;
            index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = "高物理耐久";
            dataGridView1.Rows[index].Cells[1].Value = GlobalConstants.DEFINEPHYSICALSHIELD;
            index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = "高特殊耐久";
            dataGridView1.Rows[index].Cells[1].Value = GlobalConstants.DEFINESPECIALSHIELD;
            index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = "低极速比例";
            dataGridView1.Rows[index].Cells[1].Value = GlobalConstants.EXTREMESPEEDRATELOW;
            index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = "高极速比例";
            dataGridView1.Rows[index].Cells[1].Value = GlobalConstants.EXTREMESPEEDRATEHIGH;
            index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = "很高极速比例";
            dataGridView1.Rows[index].Cells[1].Value = GlobalConstants.EXTREMESPEEDRATEVERYHIGH;
            index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = "高排名变化";
            dataGridView1.Rows[index].Cells[1].Value = GlobalConstants.APPARENTRANKINGCHANGE;
            index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = "高使用率变化";
            dataGridView1.Rows[index].Cells[1].Value = GlobalConstants.APPARENTUSAGERATECHANGE;

            index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = "使用率模型single参数";
            dataGridView1.Rows[index].Cells[1].Value = GlobalConstants.ANALYSISPARA[0];
            index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = "使用率模型double参数";
            dataGridView1.Rows[index].Cells[1].Value = GlobalConstants.ANALYSISPARA[1];
            index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = "使用率模型triple参数";
            dataGridView1.Rows[index].Cells[1].Value = GlobalConstants.ANALYSISPARA[2];
            index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = "使用率模型rotation参数";
            dataGridView1.Rows[index].Cells[1].Value = GlobalConstants.ANALYSISPARA[3];
            index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = "使用率模型special参数";
            dataGridView1.Rows[index].Cells[1].Value = GlobalConstants.ANALYSISPARA[4];
            index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = "速度统计排名下限";
            dataGridView1.Rows[index].Cells[1].Value = GlobalConstants.POPULARTHRESHOLD;


            index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = "神兽";
            dataGridView1.Rows[index].Cells[1].Value = "";
            foreach (string ancientPoke in GlobalConstants.ANCIENTPOKES)
                dataGridView1.Rows[index].Cells[1].Value += ancientPoke+",";
            dataGridView1.Rows[index].Cells[1].Value = ((string)dataGridView1.Rows[index].Cells[1].Value).Substring(0, ((string)dataGridView1.Rows[index].Cells[1].Value).Length-1);
            index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = "Mega石";
            dataGridView1.Rows[index].Cells[1].Value = "";
            foreach (string megaStone in GlobalConstants.MEGASTONES)
                dataGridView1.Rows[index].Cells[1].Value += megaStone + ",";
            dataGridView1.Rows[index].Cells[1].Value = ((string)dataGridView1.Rows[index].Cells[1].Value).Substring(0, ((string)dataGridView1.Rows[index].Cells[1].Value).Length - 1);

            for(int i=0;i<GlobalConstants.GENESPECIALPARA.Count;i++)
            {
                index = dataGridView1.Rows.Add();
                dataGridView1.Rows[index].Cells[0].Value = GlobalConstants.GENERATION[i]+"-Special单方总人数";
                dataGridView1.Rows[index].Cells[1].Value = GlobalConstants.GENESPECIALPARA[i].ToString();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < al.Count; i++)
            {
                Config.setConfig(al[i].ToString(), dataGridView1.Rows[i].Cells[1].Value.ToString());
            }
            Config cf = new Config();
        }
    }
}
