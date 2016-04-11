using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;

namespace PGLData
{

    //crawl data from PGL directly
    class Fetch
    {
        public int generationId;
        public string generation;
        private string referer;
        private string curSeasonId;
        public string curSeasonName;
        private ArrayList pkIdList;

        private int finished;
        private int total;
        private BackgroundWorker bkWorker;

        private System.Diagnostics.Stopwatch stopwatch;

        public int serverStatus;

        public string mostUpdated;
        public string updateTime;

        public Fetch(int generationIndex)
        {
            generationId = generationIndex+1;
            generation = (string)GlobalConstants.GENERATION[generationIndex];
            referer = GlobalConstants.REFERER + generation + "/";
            serverStatus = getSeason();
            if (serverStatus>0)
            {
                getUpdateTime();
                if (updateTime.Equals("fail"))
                    serverStatus = 0;
            }

        }

        //initialize a new thread
        public void updateInitial(int finished, int total, BackgroundWorker bkWorker, ArrayList pkIdList)
        {
            this.bkWorker = bkWorker;
            this.pkIdList = pkIdList;
            this.finished = finished;
            this.total = total;
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
        }


        public void getUpdateTime()
        {
            string url = "http://3ds.pokemon-gl.com/frontendApi/gbu/getSeasonPokemon";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = GlobalConstants.CONTENTTYPE;
            request.Referer = GlobalConstants.REFERER;
            string data = "languageId=2&seasonId=" + curSeasonId + "&battleType=0&timezone=GMT";
            request.ContentLength = Encoding.ASCII.GetByteCount(data);
            StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
            writer.Write(data);
            writer.Flush();
            writer.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

            updateTime = JsonParser.updateTime(reader.ReadToEnd());
            mostUpdated = "Data/"+generation + "/" + generation + "-"+curSeasonName+"-" + updateTime + ".mdb";
            reader.Close();
        }

        public string getSeasonPokemonDetail(int battleType)
        {
            string url = "http://3ds.pokemon-gl.com/frontendApi/gbu/getSeasonPokemonDetail";

            AccessHandler db = new AccessHandler(mostUpdated);
            db.createTablePkDetail(battleType);
            
            for (int i=0;i<pkIdList.Count;i++)
            {
                string pkId = pkIdList[i].ToString();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = GlobalConstants.CONTENTTYPE;
                request.Referer = GlobalConstants.REFERER;
                string data = "languageId=2&seasonId=" + curSeasonId + "&battleType=" + battleType + "&timezone=EST&pokemonId=" + pkId + "&displayNumberWaza=20&displayNumberTokusei=3&displayNumberSeikaku=3&displayNumberItem=3&displayNumberLevel=10&displayNumberPokemonIn=1&displayNumberPokemonDown=1&displayNumberPokemonDownWaza=1";
                request.ContentLength = Encoding.ASCII.GetByteCount(data);
                StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
                writer.Write(data);
                writer.Flush();
                writer.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                
                bool status = JsonParser.seasonPokemonDetail(db, battleType, pkId, reader.ReadToEnd());

                if (!status)
                {
                    bkWorker.ReportProgress(1, "请求精灵编号" + pkId + "数据失败\r\n");   
                }
                finished++;
                if (finished % 10 == 0)
                {
                    stopwatch.Stop();
                    TimeSpan timeSpan = stopwatch.Elapsed;
                    int secondsRemain=(int)(timeSpan.TotalSeconds * (total - finished) / finished);
                    bkWorker.ReportProgress(0,new int[] { finished*100/total, secondsRemain });
                    
                    stopwatch.Start();
                }

                
                
                reader.Close();
                
            }
            db.closeDatabase();
            return generation+" "+GlobalConstants.BATTLENAME[battleType]+"数据更新完成\r\n";
        }



        public int getSeason()
        {
            try
            {
                //first request to get cookies
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GlobalConstants.GETLOGINSTATUS);
                request.Method = "POST";
                request.ContentType = GlobalConstants.CONTENTTYPE;
                request.ContentLength = Encoding.ASCII.GetByteCount(GlobalConstants.POSTDATALOGIN);
                request.Referer = GlobalConstants.REFERER;
                StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
                writer.Write(GlobalConstants.POSTDATALOGIN);
                writer.Flush();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //second request to get season
                CookieContainer cc = new CookieContainer();
                cc.SetCookies(new Uri(referer), response.Headers["Set-Cookie"]);
                response.Close();
                writer.Close();
                HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(GlobalConstants.GETSEASON);
                request1.Method = "POST";
                request1.ContentType = GlobalConstants.CONTENTTYPE;
                request1.ContentLength = Encoding.ASCII.GetByteCount(GlobalConstants.POSTDATAGETSEASON+generationId);
                request1.Referer = referer;
                request1.CookieContainer = cc;
                StreamWriter writer1 = new StreamWriter(request1.GetRequestStream(), Encoding.ASCII);
                writer1.Write(GlobalConstants.POSTDATAGETSEASON + generationId);
                writer1.Flush();
                HttpWebResponse response1 = (HttpWebResponse)request1.GetResponse();
                StreamReader reader1 = new StreamReader(response1.GetResponseStream(), Encoding.UTF8);
                
                writer1.Close();
                string[] res = JsonParser.seasonInfo(reader1.ReadToEnd());

                curSeasonId = res[0];
                curSeasonName = res[1];

                response1.Close();
                reader1.Close();
                return 1;

            }
            catch
            {
                return -1;
            }
        }

    }
}
 