using System;
using System.Linq;
using Newtonsoft.Json.Linq;


namespace PGLData
{

    //parse json strings
    class JsonParser
    {
        public static string[] seasonInfo(string jsonText)
        {
            JObject seasonInfo = JObject.Parse(jsonText);
             string[] res = new string[2];
            res[0] = seasonInfo["seasonInfo"][0]["seasonId"].ToString();
            res[1] = seasonInfo["seasonInfo"][0]["seasonName"].ToString().Split(' ')[1];
            return res;
        }


        public static string updateTime(string jsonText)
        {
            try
            {
                JObject jb = JObject.Parse(jsonText);
                return jb["updateDate"].ToString().Replace(' ', '-').Replace("/", "").Replace(":","");
            }
            catch
            {
                return "fail";
            }
        }

        public static bool seasonPokemonDetail(AccessHandler db, int battleType,string pkIdStr,string jsonText)
        {
            try
            {
                JObject seasonPokemonDetail = JObject.Parse(jsonText);

                //check return code
                float pkId = float.Parse(pkIdStr.Replace("-", "."));
                int ranking = Convert.ToInt32(seasonPokemonDetail["rankingPokemonInfo"]["ranking"]);

                if (ranking == 0)
                    return true;
                string[] moveName = new string[20];
                int[] moveType = new int[20];
                float[] moveUsage = new float[20];
                string[] itemName = new string[3];
                float[] itemUsage = new float[3];
                string[] abilityName = new string[3];
                float[] abilityUsage = new float[3];
                string[] natureName = new string[3];
                float[] natureUsage = new float[3];

                int listSize = seasonPokemonDetail["rankingPokemonTrend"]["wazaInfo"].Count();
                if (listSize > 20)
                    listSize = 20;
                for (int i = 0; i < listSize; i++)
                {
                    moveName[i] = seasonPokemonDetail["rankingPokemonTrend"]["wazaInfo"][i]["name"].ToString();
                    moveType[i] = Convert.ToInt32(seasonPokemonDetail["rankingPokemonTrend"]["wazaInfo"][i]["typeId"]);
                    moveUsage[i] = (float)(Convert.ToDouble(seasonPokemonDetail["rankingPokemonTrend"]["wazaInfo"][i]["usageRate"]));
                }
                for (int i = listSize; i < 20; i++)
                {
                    moveName[i] = "-";
                    moveType[i] = -1;
                    moveUsage[i] = 0;
                }
                listSize = seasonPokemonDetail["rankingPokemonTrend"]["itemInfo"].Count();
                if (listSize > 3)
                    listSize = 3;
                for (int i = 0; i < listSize; i++)
                {
                    itemName[i] = seasonPokemonDetail["rankingPokemonTrend"]["itemInfo"][i]["name"].ToString();
                    itemUsage[i] = (float)(Convert.ToDouble(seasonPokemonDetail["rankingPokemonTrend"]["itemInfo"][i]["usageRate"]));
                }
                for (int i = listSize; i < 3; i++)
                {
                    itemName[i] = "-";
                    itemUsage[i] = 0;
                }
                listSize = seasonPokemonDetail["rankingPokemonTrend"]["tokuseiInfo"].Count();
                for (int i = 0; i < listSize; i++)
                {
                    abilityName[i] = seasonPokemonDetail["rankingPokemonTrend"]["tokuseiInfo"][i]["name"].ToString();
                    abilityUsage[i] = (float)(Convert.ToDouble(seasonPokemonDetail["rankingPokemonTrend"]["tokuseiInfo"][i]["usageRate"]));
                }
                for (int i = listSize; i < 3; i++)
                {
                    abilityName[i] = "-";
                    abilityUsage[i] = 0;
                }
                listSize = seasonPokemonDetail["rankingPokemonTrend"]["seikakuInfo"].Count();
                if (listSize > 3)
                    listSize = 3;
                for (int i = 0; i < listSize; i++)
                {
                    natureName[i] = seasonPokemonDetail["rankingPokemonTrend"]["seikakuInfo"][i]["name"].ToString();
                    natureUsage[i] = (float)(Convert.ToDouble(seasonPokemonDetail["rankingPokemonTrend"]["seikakuInfo"][i]["usageRate"]));
                }
                for (int i = listSize; i < 3; i++)
                {
                    natureName[i] = "-";
                    natureUsage[i] = 0;
                }


                string sql= "insert into battleType" + battleType+" values(";
                sql += pkId + ",";
                sql += ranking + ",";
                for (int i = 0; i < 3; i++)
                {
                    sql += "'"+abilityName[i]+"',";
                    sql += abilityUsage[i] + ",";
                }
                for (int i = 0; i < 3; i++)
                {
                    sql += "'" + natureName[i] + "',";
                    sql += natureUsage[i] + ",";
                }
                for (int i = 0; i < 3; i++)
                {
                    sql += "'" + itemName[i].Replace("'","''") + "',";
                    sql += itemUsage[i] + ",";
                }
                for (int i = 0; i < 19; i++)
                {
                    sql += "'" + moveName[i].Replace("'", "''") + "',";
                    sql += moveType[i] + ",";
                    sql += moveUsage[i] + ",";
                }
                sql += "'" + moveName[19].Replace("'", "''") + "',";
                sql += moveType[19] + ",";
                sql += moveUsage[19] + ")";
                db.ExecuteSQLNonquery(sql);
                
                return true;
            }
            catch
            {
                return false;
            }
        }


    }
}
