using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;

namespace PGLData
{
    //check and load the user-defined model
    class SelfDefinedModelParser
    {
        FileStream aFile;
        StreamReader sr;

        public SelfDefinedModelParser(string fileName)
        {
            aFile = new FileStream(fileName, FileMode.Open);
            sr = new StreamReader(aFile);
        }

        public ArrayList getModelLists()
        {
            ArrayList res = new ArrayList();

            try
            {
                string strLine = sr.ReadLine();

                while (strLine != null)
                {
                    res.Add(double.Parse(strLine));
                    strLine = sr.ReadLine();
                }
            }
            catch
            {
                sr.Close();
                SelfDesignedMsg sdm = new SelfDesignedMsg("Sorry~ 您自定义模型中存在我无法解析的数据, 小女子只能识别int和double型数据...", false);
                sdm.ShowDialog();
                return null;
            }
            sr.Close();
            aFile.Close();
            return res;
        }
    }
}
