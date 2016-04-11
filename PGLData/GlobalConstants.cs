using System.Collections;
using System.Drawing;

namespace PGLData
{
    class GlobalConstants
    {
        public static string GETLOGINSTATUS;
        public static string GETSEASON;
        public static string GETSEASONPOKEMON;
        public static string GETSEASONPOKEMONDETAIL;
        public static string REFERER;
        public static string CONTENTTYPE;
        public static string POSTDATAGETSEASON;
        public static string POSTDATALOGIN;
        public static ArrayList GENERATION;
        public static ArrayList MOSTRECENTFILES;
        public static string DEFAULTGENERATION;
        public static string DEFAULTBATTLETYPE;
        public static string CHECKUPDATE;
        public static string TIMEZONE;
        public static string VIEWOPTIONS;
        public static string APPVERSION;
        public static int MAXSPEED;
        public static int DEFINEATTACK;
        public static int DEFINESPECIALATTACK;      
        public static int DEFINEPHYSICALSHIELD;
        public static int DEFINESPECIALSHIELD;

        public static int EXTREMESPEEDRATELOW;
        public static int EXTREMESPEEDRATEHIGH;
        public static int EXTREMESPEEDRATEVERYHIGH;
        public static int APPARENTRANKINGCHANGE;
        public static int APPARENTUSAGERATECHANGE;

        public static int POPULARTHRESHOLD;

        
        public static string REGISTEREDVERSION;
        public static string REGISTEREDOS;
        public static string PSEUDOMAC;


        public static ArrayList GENESPECIALPARA;

        public static string[] ANCIENTPOKES;
        public static string[] MEGASTONES;
        public static string[] ANALYSISPARA;

        public static float dpiX;

        public static Color[] TYPECOLORS = { Color.FromArgb(173, 165, 148), Color.FromArgb(165, 82, 57), Color.FromArgb(247, 82, 49), Color.FromArgb(90, 206, 231), Color.FromArgb(255, 198, 49), Color.FromArgb(156, 173, 247), Color.FromArgb(123, 206, 82), Color.FromArgb(214, 181, 90), Color.FromArgb(181, 90, 165), Color.FromArgb(173, 189, 33), Color.FromArgb(115, 90, 74), Color.FromArgb(57, 156, 255), Color.FromArgb(255, 115, 165), Color.FromArgb(123, 99, 231), Color.FromArgb(189, 165, 90), Color.FromArgb(99, 99, 181), Color.FromArgb(173, 173, 198), Color.FromArgb(247, 181, 247) };

        public static string[]  TABLENAMES = { "All Matches", "Single", "Double", "Triple", "Rotation", "Special" };


        public static string[] BATTLENAME = { "All Mathes", "Single", "Double", "Triple", "Rotation", "Special" };
        public static string[] TYPENAME = { "普", "斗", "火", "冰", "电", "飞", "草", "地", "毒", "虫", "恶", "水", "超", "龙", "岩", "鬼", "钢", "妖" };
        public static Hashtable TYPEPAIR = new Hashtable();
        public static double[,] TYPERELATIONS = {{1, 1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  0.5,    0,  0.5,    1},
                                                 {2, 1,  1,  2,  1,  0.5,    1,  1,  0.5,    0.5,    2,  1,  0.5,    1,  2,  0,  2,  0.5},
                                                 {1, 1,  0.5,    2,  1,  1,  2,  1,  1,  2,  1,  0.5,    1,  0.5,    0.5,    1,  2,  1},
                                                 {1, 1,  0.5,    0.5,    1,  2,  2,  2,  1,  1,  1,  0.5,    1,  2,  1,  1,  0.5,    1},
                                                 {1, 1,  1,  1,  0.5,    2,  0.5,    0,  1,  1,  1,  2,  1,  0.5,    1,  1,  1,  1},
                                                 {1, 2,  1,  1,  0.5,    1,  2,  1,  1,  2,  1,  1,  1,  1,  0.5,    1,  0.5,    1},
                                                 {1, 1,  0.5,    1,  1,  0.5,    0.5,    2,  0.5,    0.5,    1,  2,  1,  0.5,    2,  1,  0.5,    1},
                                                 {1, 1,  2,  1,  2,  0,  0.5,    1,  2,  0.5,    1,  1,  1,  1,  2,  1,  2,  1},
                                                 {1, 1,  1,  1,  1,  1,  2,  0.5,    0.5,    1,  1,  1,  1,  1,  0.5,    0.5,    0,  2},
                                                 {1, 0.5,    0.5,    1,  1,  0.5,    2,  1,  0.5,    1,  2,  1,  2,  1,  1,  0.5,    0.5,    0.5},
                                                 {1, 0.5,    1,  1,  1,  1,  1,  1,  1,  1,  0.5,    1,  2,  1,  1,  2,  1,  0.5},
                                                 {1, 1,  2,  1,  1,  1,  0.5,    2,  1,  1,  1,  0.5,    1,  0.5,    2,  1,  1,  1},
                                                 {1, 2,  1,  1,  1,  1,  1,  1,  2,  1,  0,  1,  0.5,    1,  1,  1,  0.5,    1},
                                                 {1, 1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  2,  1,  1,  0.5,    0},
                                                 {1, 0.5,    2,  2,  1,  2,  1,  0.5,    1,  2,  1,  1,  1,  1,  1,  1,  0.5,    1},
                                                 {0, 1,  1,  1,  1,  1,  1,  1,  1,  1,  0.5,    1,  2,  1,  1,  2,  1,  1},
                                                 {1, 1,  0.5,    2,  0.5,    1,  1,  1,  1,  1,  1,  0.5,    1,  1,  2,  1,  0.5,    2},
                                                 {1, 2,  0.5,    1,  1,  1,  1,  1,  0.5,    1,  2,  1,  1,  2,  1,  1,  0.5,    1}};
        
    }
}
