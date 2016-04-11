using System.Data;
using System.Data.OleDb;
using ADOX;
using System.IO;
using System.Collections;

namespace PGLData
{
    //handle the local access database
    public class AccessHandler
    {

        public OleDbConnection Conn;

        //sqls used to create tables 
        //readonly static string sqlCreateTablePokemon = "create table pokemon(poke_id float,chi varchar(20),jap varchar(20),eng varchar(20),type1 varchar(6),type2 varchar(6),ability1 varchar(10),ability2 varchar(10),ability3 varchar(10),hp int,attack int,defense int,specialAttack int,specialDefense int,speed int,total int,primary key(poke_id))";
        //readonly static string sqlCreateTableMove = "create table move(chi varchar(20),jap varchar(20),eng varchar(20),type varchar(6),category varchar(4),power char(4),accuracy varchar(6),pp int,description text,primary key(eng))";
        //readonly static string sqlCreateTableAbility = "create table ability(chi varchar(20),jap varchar(20),eng varchar(20),description text,normal int,special int,primary key(eng))";
        //readonly static string sqlCreateTableItem = "create table item(chi varchar(20),jap varchar(20),eng varchar(20),description text,primary key(eng))";
        //readonly static string sqlCreateTableNature = "create table nature(chi varchar(20),jap varchar(20),eng varchar(20),augment varchar(4),reduce varchar(4),primary key(eng))";
        readonly static string sqlCreateTablePkDetail = "(poke_id float,ranking int,ability1 varchar(20),au1 float, ability2 varchar(20),au2 float,ability3 varchar(20),au3 float,nature1 varchar(20),nu1 float,nature2 varchar(20),nu2 float,nature3 varchar(20),nu3 float,item1 varchar(20),iu1 float,item2 varchar(20),iu2 float,item3 varchar(20),iu3 float,move1 varchar(20),mt1 int,mu1 float,move2 varchar(20),mt2 int,mu2 float,move3 varchar(20),mt3 int,mu3 float,move4 varchar(20),mt4 int,mu4 float,move5 varchar(20),mt5 int,mu5 float,move6 varchar(20),mt6 int,mu6 float,move7 varchar(20),mt7 int,mu7 float,move8 varchar(20),mt8 int,mu8 float,move9 varchar(20),mt9 int,mu9 float,move10 varchar(20),mt10 int,mu10 float,move11 varchar(20),mt11 int,mu11 float,move12 varchar(20),mt12 int,mu12 float,move13 varchar(20),mt13 int,mu13 float,move14 varchar(20),mt14 int,mu14 float,move15 varchar(20),mt15 int,mu15 float,move16 varchar(20),mt16 int,mu16 float,move17 varchar(20),mt17 int,mu17 float,move18 varchar(20),mt18 int,mu18 float,move19 varchar(20),mt19 int,mu19 float,move20 varchar(20),mt20 int,mu20 float,primary key(poke_id))";

        //support the new access driver
        public AccessHandler(string name)
        {
            
            try
            {
                string ConnString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + name + ";Persist Security Info = False";

                if (!File.Exists(name))
                {
                    string[] parsedPath = name.Split('/');
                    if (!Directory.Exists("Data/" + parsedPath[1]))
                        Directory.CreateDirectory("Data/" + parsedPath[1]);
                    Catalog catalog = new Catalog();
                    catalog.Create(ConnString);
                }
                Conn = new OleDbConnection(ConnString);
                Conn.Open();
            }
            catch
            {
                backupConstr(name);
            }
        }

        //support the old access driver
        private void backupConstr(string name)
        {
            string ConnString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + name;
            if (!File.Exists(name))
            {
                string[] parsedPath = name.Split('/');
                if (!Directory.Exists("Data/" + parsedPath[1]))
                    Directory.CreateDirectory("Data/" + parsedPath[1]);
                Catalog catalog = new Catalog();
                catalog.Create(ConnString);
            }
            Conn = new OleDbConnection(ConnString);
            Conn.Open();

        }

        //fetch all the table names
        public string[] showAllTables()
        {
            ArrayList tableList=new ArrayList();
            DataTable dt = Conn.GetSchema("tables");
            for (int i = 0; i < dt.Rows.Count; i++)
                if(dt.Rows[i][3].ToString().Equals("TABLE"))
                    tableList.Add(dt.Rows[i][2]);
            string[] res = new string[tableList.Count];
            for (int i = 0; i < tableList.Count; i++)
                res[i] = tableList[i].ToString();
            return res;
        }
        
        public void closeDatabase()
        {
            Conn.Close();
        }

        //create a battleType table
        public void createTablePkDetail(int battleType)
        {
            ExecuteSQLNonquery("create table battleType"+battleType+sqlCreateTablePkDetail);
        }
 
        //selection query
        public DataTable SelectToDataTable(string SQL)
        {
            OleDbDataAdapter adapter = new OleDbDataAdapter();
            OleDbCommand command = new OleDbCommand(SQL, Conn);
            adapter.SelectCommand = command;
            DataTable Dt = new DataTable();
            adapter.Fill(Dt);
            return Dt;
        }

        //none selection query
        public bool ExecuteSQLNonquery(string SQL)
        {
            OleDbCommand cmd = new OleDbCommand(SQL, Conn);
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
