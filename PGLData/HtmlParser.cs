using System;
using System.Collections;
using System.IO;
using System.Net;

namespace PGLData
{
    //check github webpage for app or data updates
    class HtmlParser
    {

        public string checkAppUpdate()
        { 

            try
            {
                string url = "https://raw.githubusercontent.com/DearUnknown/PGLToolUpdate/master/PGLToolUpdate/VersionV1.0/checkUpdate.txt";
                WebClient webClient = new WebClient();
                webClient.DownloadFile(url, "Update/versionCheckUpdate.txt");

                FileStream aFile = new FileStream("Update/versionCheckUpdate.txt", FileMode.Open);
                StreamReader sr = new StreamReader(aFile);

                string line = sr.ReadLine();
                sr.Close();
                aFile.Close();
                File.Delete("Update/versionCheckUpdate.txt");
                if (line.Equals(GlobalConstants.APPVERSION))
                    return "";
                else
                    return line;
            }
            catch (Exception e)
            {
                BugBox bb = new BugBox(e.ToString());
                bb.ShowDialog();
                return "";
            }
        }

        public ArrayList checkDataUpdate(string newVersion)
        {
            ArrayList gitVersions = new ArrayList();
            try
            {
                string url = "https://raw.githubusercontent.com/DearUnknown/PGLToolUpdate/master/PGLToolUpdate/Data/checkUpdate.txt";
                WebClient webClient = new WebClient();
                webClient.DownloadFile(url, "Update/dataCheckUpdate.txt");

                FileStream aFile = new FileStream("Update/dataCheckUpdate.txt", FileMode.Open);
                StreamReader sr = new StreamReader(aFile);

                for (int i = 0; i < GlobalConstants.GENERATION.Count; i++)
                    gitVersions.Add(sr.ReadLine());

                sr.Close();
                aFile.Close();
                File.Delete("Update/dataCheckUpdate.txt");

                if (!newVersion.Equals(""))
                {
                    int i;
                    for (i = 0; i < GlobalConstants.GENERATION.Count; i++)
                    {
                        if (gitVersions[i].ToString().Equals(newVersion))
                            break;
                    }
                    if (i == GlobalConstants.GENERATION.Count)
                    {
                        gitVersions.Clear();
                        return gitVersions;
                    }
                }

                for (int i = 0; i < GlobalConstants.GENERATION.Count; i++)
                {
                    string localName = GlobalConstants.MOSTRECENTFILES[i].ToString().Split('/')[2];
                    long localDate = long.Parse(localName.Split('-')[1] + localName.Split('-')[2]);
                    string gitName = gitVersions[i].ToString().Split('/')[2];
                    long gitDate = long.Parse(gitName.Split('-')[1] + gitName.Split('-')[2]);
                    if (localDate > gitDate)
                    {
                        gitVersions.Clear();
                        return gitVersions;
                    }
                    else if (localDate == gitDate)
                        gitVersions[i] = "";
                }
              
                return gitVersions;
            }
            catch (Exception e)
            {
                BugBox bb = new BugBox(e.ToString());
                bb.ShowDialog();
                gitVersions.Clear();
                return gitVersions;
            }
        }
    }
}
