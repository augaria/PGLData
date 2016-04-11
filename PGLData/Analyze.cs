using System;
using System.Collections.Generic;
using System.Data;
using System.Collections;
using System.Windows.Forms;

namespace PGLData
{
    class Analyze
    { 
        AccessHandler ah;
        DataTable raceData;
        DataTable rankingDetails;
        int battleType;
        string curDir;
        double total;
        double para;
        double[] model;
        public bool initalFailed; 

        public Analyze(string dbFile, int battleType, double userPara, bool selfDefined)
        {
            para = userPara;
            this.battleType = battleType;
            ah = new AccessHandler(dbFile);
            curDir = Environment.CurrentDirectory;

            initalFailed=true;

            raceData = ah.SelectToDataTable("select pokemon.hp,pokemon.attack,pokemon.defense,pokemon.specialAttack,pokemon.specialDefense,pokemon.speed,battleType" + battleType + ".ranking,battleType" + battleType + ".nature1,battleType" + battleType + ".nu1,battleType" + battleType + ".nature2,battleType" + battleType + ".nu2,battleType" + battleType + ".nature3,battleType" + battleType + ".nu3 from battleType" + battleType + " inner join [;database=" + curDir + "/Data/PokeData.mdb].pokemon on pokemon.poke_id = battleType" + battleType + ".poke_id");
            rankingDetails = ah.SelectToDataTable("select * from battleType" + battleType + "");
            total = 0;
            
            if (selfDefined)
            {
                SelfDefinedModelParser sdmp;
                try
                {
                     sdmp= new SelfDefinedModelParser("Data/SelfDefinedModel.txt");
                }
                catch
                {
                    SelfDesignedMsg sdm = new SelfDesignedMsg("Sorry~ 小女子找不到您定义模型的文件...\r\n请确认文件Data/SelfDefinedModel.txt的存在~", false);
                    sdm.ShowDialog();
                    return;
                }
                ArrayList selfModel = sdmp.getModelLists();
                if (selfModel == null)
                    return;
                if(selfModel.Count<rankingDetails.Rows.Count)
                {
                    SelfDesignedMsg sdm = new SelfDesignedMsg("您定义的模型数据不足哦~ 请至少定义" + rankingDetails.Rows.Count + "条", false);
                    sdm.ShowDialog();
                    return;
                }
                model = new double[rankingDetails.Rows.Count+1];
                model[0] = 0;
                for (int i = 1; i <= rankingDetails.Rows.Count; i++)
                {
                    model[i] = (double)selfModel[i - 1];
                    total += model[i];
                }
            }
            else
            {
                model = new double[rankingDetails.Rows.Count + 1];
                model[0] = 0;
                for (int i = 1; i <= rankingDetails.Rows.Count; i++)
                {
                    model[i] = Math.Pow(i, para);
                    total += model[i];
                }
            }
            initalFailed = false;

        }


        //使用率统计
        public KeyValuePair<string, double>[] useageRateTop(int limitation)
        {
            DataTable rankingNames = ah.SelectToDataTable("select pokemon.chi,battleType" + battleType + ".ranking from battleType" + battleType + " inner join [;database=" + curDir + "/Data/PokeData.mdb].pokemon on pokemon.poke_id = battleType" + battleType + ".poke_id order by battleType" + battleType + ".ranking");
            limitation = Math.Min(limitation,rankingNames.Rows.Count);
            KeyValuePair<string, double>[] ur = new KeyValuePair<string, double>[limitation];
            for (int i = 1; i <= limitation; i++)
                ur[i - 1] = new KeyValuePair<string, double>(rankingNames.Rows[i - 1][0].ToString(), model[i] / total);

            return ur;
        }  


        //速度修正统计
        public ArrayList speedDistribution()
        {
            ArrayList res=new ArrayList();
            
            double[] extSpeedDistribution = new double[GlobalConstants.MAXSPEED+1];
            extSpeedDistribution.Initialize();
            double[] speedDistribution = new double[GlobalConstants.MAXSPEED+1];
            speedDistribution.Initialize();

            foreach (DataRow row in raceData.Rows)
            {
                double weight = model[(int)row[6]];
                speedDistribution[(int)row[5]]+=weight;
                if ((string)row[7] == "Timid" || (string)row[7] == "Naive" || (string)row[7] == "Jolly" || (string)row[7] == "Hasty")
                    extSpeedDistribution[(int)row[5]] += weight*(double)row[8];
                if ((string)row[9] == "Timid" || (string)row[9] == "Naive" || (string)row[9] == "Jolly" || (string)row[9] == "Hasty")
                    extSpeedDistribution[(int)row[5]] += weight*(double)row[10];
                if ((string)row[11] == "Timid" || (string)row[11] == "Naive" || (string)row[11] == "Jolly" || (string)row[11] == "Hasty")
                    extSpeedDistribution[(int)row[5]] += weight*(double)row[12];
            }

            for (int i = 0; i < GlobalConstants.MAXSPEED+1; i++)
            {
                if (speedDistribution[i] > 0)
                    res.Add(new KeyValuePair<int, double>(i, extSpeedDistribution[i] / speedDistribution[i]));  
            }
            return res;
        }

        //速度种族统计
        public ArrayList speedRacialDistribution()
        {
            ArrayList res = new ArrayList();

            int[] recialDistribution = new int[GlobalConstants.MAXSPEED + 1];
            recialDistribution.Initialize();

            foreach (DataRow row in raceData.Rows)
            {
                if ((int)row[6] <= GlobalConstants.POPULARTHRESHOLD)
                    recialDistribution[(int)row[5]]++;
            }

            for (int i = 0; i < GlobalConstants.MAXSPEED + 1; i++)
            {
                if (recialDistribution[i] > 0)
                    res.Add(new KeyValuePair<int, int>(i, recialDistribution[i]));
            }
            return res;
        }

        //速度种族加权统计
        public ArrayList speedRacialWeightedDistribution()
        {
            ArrayList res = new ArrayList();

            double[] recialWeightedDistribution = new double[GlobalConstants.MAXSPEED + 1];
            recialWeightedDistribution.Initialize();

            foreach (DataRow row in raceData.Rows)
                recialWeightedDistribution[(int)row[5]] += model[(int)row[6]];

            for (int i = 0; i < GlobalConstants.MAXSPEED + 1; i++)
            {
                if (recialWeightedDistribution[i] > 0)
                    res.Add(new KeyValuePair<int, double>(i, 1000* recialWeightedDistribution[i]/total));
            }
            return res;
        }

        //速度种族加权累加统计
        public ArrayList speedRacialWeightedAccumulatedDistribution(ArrayList weightedPoints)
        {
            ArrayList res = new ArrayList();

            double accumulatedTotal = 0;

            foreach (KeyValuePair<int, double> point in weightedPoints)
            {
                accumulatedTotal += point.Value;
                res.Add(new KeyValuePair<int, double>(point.Key, accumulatedTotal/10));
            }
            return res;
        }


        //物攻特攻统计
        public double[] AttackDistribution()
        {
            double attack = 0;
            double specialAttack = 0;
            foreach (DataRow row in raceData.Rows)
            {
                double weight = model[(int)row[6]] / total;

                if ((string)row[7] == "Naughty" || (string)row[7] == "Lonely" || (string)row[7] == "Brave" || (string)row[7] == "Adamant" || ( ((string)row[7] == "Naive" || (string)row[7] == "Jolly" || (string)row[7] == "Hasty") && (int)row[1] >= GlobalConstants.DEFINEATTACK))
                    attack += weight * (double)row[8];
                if ((string)row[9] == "Naughty" || (string)row[9] == "Lonely" || (string)row[9] == "Brave" || (string)row[9] == "Adamant" || (((string)row[9] == "Naive" || (string)row[9] == "Jolly" || (string)row[9] == "Hasty") && (int)row[1] >= GlobalConstants.DEFINEATTACK))
                    attack += weight * (double)row[10];
                if ((string)row[11] == "Naughty" || (string)row[11] == "Lonely" || (string)row[11] == "Brave" || (string)row[11] == "Adamant" || (((string)row[11] == "Naive" || (string)row[11] == "Jolly" || (string)row[11] == "Hasty") && (int)row[1] >= GlobalConstants.DEFINEATTACK))
                    attack += weight * (double)row[12];



                if ((string)row[7] == "Modest" || (string)row[7] == "Mild" || (string)row[7] == "Mild" || (string)row[7] == "Rash" || (((string)row[7] == "Timid" || (string)row[7] == "Naive" || (string)row[7] == "Hasty") && (int)row[3] >= GlobalConstants.DEFINESPECIALATTACK))
                    specialAttack += weight * (double)row[8];
                if ((string)row[9] == "Modest" || (string)row[9] == "Mild" || (string)row[9] == "Mild" || (string)row[9] == "Rash" || (((string)row[9] == "Timid" || (string)row[9] == "Naive" || (string)row[9] == "Hasty") && (int)row[3] >= GlobalConstants.DEFINESPECIALATTACK))
                    specialAttack += weight * (double)row[10];
                if ((string)row[11] == "Modest" || (string)row[11] == "Mild" || (string)row[11] == "Mild" || (string)row[11] == "Rash" || (((string)row[11] == "Timid" || (string)row[11] == "Naive" || (string)row[11] == "Hasty") && (int)row[3] >= GlobalConstants.DEFINESPECIALATTACK))
                    specialAttack += weight * (double)row[12];

            }
            return new double[2] { attack, specialAttack };
        }

        //物盾特盾统计
        public double[] DefenseDistribution()
        {
            double defense = 0;
            double specialDefense = 0;
            foreach (DataRow row in raceData.Rows)
            {
                double weight = model[(int)row[6]] / total;

                if ((int)row[0] * (int)row[2] >= GlobalConstants.DEFINEPHYSICALSHIELD&& (!((string)row[7] == "Hasty"|| (string)row[7] == "Gentle"|| (string)row[7] == "Mild"|| (string)row[7] == "Lonely")))
                    defense += weight*100;
                else
                {
                    if ((string)row[7] == "Bold" || (string)row[7] == "Relaxed" || (string)row[7] == "Lax" || (string)row[7] == "Impish")
                        defense += weight * (double)row[8];
                    if ((string)row[9] == "Bold" || (string)row[9] == "Relaxed" || (string)row[9] == "Lax" || (string)row[9] == "Impish")
                        defense += weight * (double)row[10];
                    if ((string)row[11] == "Bold" || (string)row[11] == "Relaxed" || (string)row[11] == "Lax" || (string)row[11] == "Impish")
                        defense += weight * (double)row[12];
                }

                if ((int)row[0] * (int)row[4] >= GlobalConstants.DEFINESPECIALSHIELD && (!((string)row[7] == "Lax" || (string)row[7] == "Naive" || (string)row[7] == "Rash" || (string)row[7] == "Naughty")))
                    specialDefense += weight*100;
                else
                {
                    if ((string)row[7] == "Gentle" || (string)row[7] == "Careful" || (string)row[7] == "Calm" || (string)row[7] == "Sassy")
                        specialDefense += weight * (double)row[8];
                    if ((string)row[9] == "Gentle" || (string)row[9] == "Careful" || (string)row[9] == "Calm" || (string)row[9] == "Sassy")
                        specialDefense += weight * (double)row[10];
                    if ((string)row[11] == "Gentle" || (string)row[11] == "Careful" || (string)row[11] == "Calm" || (string)row[11] == "Sassy")
                        specialDefense += weight * (double)row[12];                   
                }
            }
            return new double[2] { defense, specialDefense };
        }

        //技能使用率统计
        public ArrayList moveDistribution()
        {
            ArrayList res = new ArrayList();

            Hashtable moves = new Hashtable();
            //double [] moveTypes=new double[19]; 


            foreach (DataRow row in rankingDetails.Rows)
            {
                double weight = model[(int)row[1]] / total;
                for (int i = 20; i < 80; i += 3)
                {
                    if ((string)row[i] == "-")
                        continue;
                    if (moves.Contains((string)row[i]+"," +(int)row[i+1]))
                        moves[(string)row[i] + "," + (int)row[i + 1]] = (double)(moves[(string)row[i] + "," + (int)row[i + 1]]) + weight * (double)row[i + 2];
                    else
                        moves.Add((string)row[i] + "," + (int)row[i + 1], weight * (double)row[i + 2]);
                }
            }

            string[] keyArray = new string[moves.Count];
            double[] valueArray = new double[moves.Count];
            moves.Keys.CopyTo(keyArray, 0);
            moves.Values.CopyTo(valueArray, 0);
            Array.Sort(valueArray, keyArray);
            Array.Reverse(keyArray);
            Array.Reverse(valueArray);
            for (int i = 0; i < keyArray.Length; i++)
            {
                if (valueArray[i] < 1)
                    break;
                res.Add(new KeyValuePair<string, double>(keyArray[i], valueArray[i]));
            }
            return res;
        }

        //神兽出现率统计
        public double ancientUsageRate()
        {
            double res=0;
            HashSet<int> ancientSet = new HashSet<int>();
            foreach (string ancientPoke in GlobalConstants.ANCIENTPOKES)
                ancientSet.Add(int.Parse(ancientPoke));
            foreach (DataRow row in rankingDetails.Rows)
            {
                double pokeId = (double)row[0];
                if (ancientSet.Contains((int)pokeId))
                    res += model[(int)row[1]] / total;
            }

            return res;
        }

        //mega配比统计
        public ArrayList megaUsageRate(AccessHandler ah)
        {
            ArrayList res=new ArrayList();
            double totalMegaRate=0;
            HashSet<string> megaSet= new HashSet<string>(GlobalConstants.MEGASTONES);

            foreach (DataRow row in rankingDetails.Rows)
            {
                double weight = model[(int)row[1]] / total;
                if (megaSet.Contains(row[14].ToString()))
                {
                    double megaRate = weight * (double)(row[15]);                  
                    string pkId = row[0].ToString() + ".1";
                    if (row[14].ToString().Equals("Charizardite Y")|| row[14].ToString().Equals("Mewtwonite Y"))
                        pkId = row[0].ToString() + ".2";
                    DataTable dt = ah.SelectToDataTable("select chi from pokemon where poke_id = " + pkId);
                    if (dt.Rows.Count > 0)
                    {
                        res.Add(new KeyValuePair<string, double>(dt.Rows[0][0].ToString(), megaRate));
                        totalMegaRate += megaRate;
                    }

                }
                if (megaSet.Contains(row[16].ToString()))
                {
                    double megaRate = weight * (double)(row[17]);
                    string pkId = row[0].ToString() + ".1";
                    if (row[16].ToString().Equals("Charizardite Y") || row[16].ToString().Equals("Mewtwonite Y"))
                        pkId = row[0].ToString() + ".2";
                    DataTable dt = ah.SelectToDataTable("select chi from pokemon where poke_id = " + pkId);
                    if (dt.Rows.Count > 0)
                    {
                        res.Add(new KeyValuePair<string, double>(dt.Rows[0][0].ToString(), megaRate));
                        totalMegaRate += megaRate;
                    }
                }
                if (megaSet.Contains(row[18].ToString()))
                {
                    double megaRate = weight * (double)(row[19]);
                    string pkId = row[0].ToString() + ".1";
                    if (row[18].ToString().Equals("Charizardite Y") || row[18].ToString().Equals("Mewtwonite Y"))
                        pkId = row[0].ToString() + ".2";
                    DataTable dt = ah.SelectToDataTable("select chi from pokemon where poke_id = " + pkId);
                    if (dt.Rows.Count > 0)
                    {
                        res.Add(new KeyValuePair<string, double>(dt.Rows[0][0].ToString(), megaRate));
                        totalMegaRate += megaRate;
                    }
                }
                if ((double)row[0] == 384)
                {
                    for (int i = 20; i < 80; i += 3)
                    {
                        if ((string)row[i] == "Dragon Ascent")
                        {
                            double megaRate = weight * (double)(row[i+2]);
                            res.Add(new KeyValuePair<string, double>("烈空坐", megaRate));
                            totalMegaRate += megaRate;
                            break;
                        }
                    }
                }
            }
            res.Add(new KeyValuePair<string, double>("total", totalMegaRate));
            return res;
        }
    }
}
