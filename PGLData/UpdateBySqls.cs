using System.IO;


namespace PGLData
{

    //update data by sql insertion sentences
    class UpdateBySqls
    {
        AccessHandler db;

        public UpdateBySqls(string newFile)
        {
            db = new AccessHandler(newFile);            
        }

        public void setTable(int battleType,string sqlFile)
        {
            db.createTablePkDetail(battleType);
            FileStream aFile = new FileStream(sqlFile, FileMode.Open);
            StreamReader sr = new StreamReader(aFile);

            string line = sr.ReadLine();
            while (line != null)
            {
                db.ExecuteSQLNonquery(line);
                line = sr.ReadLine();
            }
            sr.Close();
            aFile.Close();
        }
    }
}
