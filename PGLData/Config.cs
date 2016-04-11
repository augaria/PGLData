using System;
using System.Collections.Generic;
using System.Configuration;
using System.Collections;

namespace PGLData
{

    //handle with the configuration file
    class Config
    {
        public Config()
        {
            GlobalConstants.GETSEASON = getConfig("GetSeason");
            GlobalConstants.GETSEASONPOKEMON = getConfig("GetSeasonPokemon");
            GlobalConstants.GETSEASONPOKEMONDETAIL = getConfig("GetSeasonPokemonDetail");
            GlobalConstants.REFERER = getConfig("Referer");
            GlobalConstants.CONTENTTYPE = getConfig("ContentType");
            GlobalConstants.GETLOGINSTATUS = getConfig("GetLoginStatus");
            GlobalConstants.POSTDATAGETSEASON = getConfig("PostDataGetSeason");
            GlobalConstants.POSTDATALOGIN = getConfig("PostDataLogin");
            GlobalConstants.DEFAULTGENERATION = getConfig("DefaultGeneration");
            GlobalConstants.DEFAULTBATTLETYPE = getConfig("DefaultBattleType");
            GlobalConstants.CHECKUPDATE = getConfig("CheckUpdate");
            GlobalConstants.TIMEZONE = getConfig("Timezone");
            GlobalConstants.VIEWOPTIONS = getConfig("ViewOptions");
            GlobalConstants.APPVERSION = getConfig("AppVersion");
            GlobalConstants.MAXSPEED = getConfigInt("MaxSpeed");
            GlobalConstants.DEFINEATTACK = getConfigInt("DefineAttack");
            GlobalConstants.DEFINESPECIALATTACK = getConfigInt("DefineSpecialAttack");
            GlobalConstants.DEFINEPHYSICALSHIELD = getConfigInt("DefinePhysicalShield");
            GlobalConstants.DEFINESPECIALSHIELD= getConfigInt("DefineSpecialShield");
            GlobalConstants.EXTREMESPEEDRATELOW = getConfigInt("ExtremeSpeedRateLow");
            GlobalConstants.EXTREMESPEEDRATEHIGH = getConfigInt("ExtremeSpeedRateHigh");
            GlobalConstants.EXTREMESPEEDRATEVERYHIGH = getConfigInt("ExtremeSpeedRateVeryHigh");
            GlobalConstants.APPARENTRANKINGCHANGE = getConfigInt("ApparentRankingChange");
            GlobalConstants.APPARENTUSAGERATECHANGE = getConfigInt("ApparentUsageRateChange");
            GlobalConstants.ANCIENTPOKES = getConfig("AncientPokes").Split(',');
            GlobalConstants.MEGASTONES = getConfig("MegaStones").Split(',');
            GlobalConstants.ANALYSISPARA = new string[5];
            GlobalConstants.ANALYSISPARA[0] = getConfig("para-single");
            GlobalConstants.ANALYSISPARA[1] = getConfig("para-double");
            GlobalConstants.ANALYSISPARA[2] = getConfig("para-triple");
            GlobalConstants.ANALYSISPARA[3] = getConfig("para-rotation");
            GlobalConstants.ANALYSISPARA[4] = getConfig("para-special");
            GlobalConstants.POPULARTHRESHOLD = getConfigInt("PopularThreshold");

            
            GlobalConstants.REGISTEREDVERSION = getConfig("RegisteredVersion");
            GlobalConstants.REGISTEREDOS = getConfig("RegisteredOS");
            GlobalConstants.PSEUDOMAC = getConfig("PseudoMac");

            int i = 1;
            string temp;
            GlobalConstants.GENERATION = new ArrayList();
            GlobalConstants.MOSTRECENTFILES = new ArrayList();
            GlobalConstants.GENESPECIALPARA = new ArrayList();
            while ((temp = getConfig("Generation" + i)) != null)
            {
                GlobalConstants.GENERATION.Add(temp);
                GlobalConstants.MOSTRECENTFILES.Add(getConfig("MostRecentFile" + i));
                GlobalConstants.GENESPECIALPARA.Add(getConfigInt("Gene" + i + "SpecialPara"));
                i++;
            }
        }

        private string getConfig(string key)
        {
            string connectionString = ConfigurationManager.AppSettings[key];
            return connectionString;
        }

        private int getConfigInt(string key)
        {
            string connectionString = ConfigurationManager.AppSettings[key];
            return int.Parse(connectionString);
        }

        public static void setConfig(string key,string value)
        {
            Configuration cf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cf.AppSettings.Settings[key].Value = value;
            cf.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

    }
}
