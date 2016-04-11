using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;

namespace PGLData
{
    //form for app update
    public partial class UpdateApp : Form
    {

        public bool hasNew;
        string newVersion;

        public UpdateApp(bool autoCheck)
        {
            InitializeComponent();

            if (GlobalConstants.dpiX == 120)
            {
                foreach (Control ct in this.Controls)
                    ct.Font = new System.Drawing.Font(ct.Font.FontFamily, (float)(ct.Font.Size / 1.25), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            }


            Register();

            hasNew = false;
            if (!checkUpdate())
            {
                if (!autoCheck)
                {
                    SelfDesignedMsg sdm = new SelfDesignedMsg("您的软件已经是最新版本啦~", false);
                    sdm.ShowDialog();
                }
                return;
            }
            hasNew = true;
            string url = "https://raw.githubusercontent.com/DearUnknown/PGLToolUpdate/master/PGLToolUpdate/VersionV1.0/info.txt";
            WebClient  myWebClient = new WebClient();
            myWebClient.DownloadFile(url, "Update/info.txt");
            FileStream aFile = new FileStream("Update/info.txt", FileMode.Open);
            StreamReader sr = new StreamReader(aFile,Encoding.GetEncoding("UTF-8"));
            string info= sr.ReadToEnd();
            textBox1.Text = info.Replace("\n","\r\n");
            sr.Close();
            aFile.Close();
        }

        //register the current user on the server
        private void Register()
        {
            try
            {
                SocketHandler sh = new SocketHandler();
                OperatingSystem osInfo = Environment.OSVersion;
                if (!GlobalConstants.REGISTEREDVERSION.Equals(GlobalConstants.APPVERSION) || !osInfo.ToString().Equals(GlobalConstants.REGISTEREDOS))
                {
                    string mac = GetPhyAddr();
                    if (mac.Equals(""))
                    {
                        if (GlobalConstants.PSEUDOMAC.Equals("xxx"))
                        {
                            mac = "R-" + Guid.NewGuid().ToString().Replace("-", "").ToUpper();
                            Config.setConfig("PseudoMac", mac);
                        }
                        else
                            mac = GlobalConstants.PSEUDOMAC;
                    }
                    string registerStatus = sh.sendMessageR("1", mac, GlobalConstants.APPVERSION, osInfo.ToString().Replace("Microsoft Windows ", ""));
                    if (registerStatus.Equals("done"))
                    {
                        Config.setConfig("RegisteredVersion", GlobalConstants.APPVERSION);
                        Config.setConfig("RegisteredOS", osInfo.ToString());
                    }
                }
                sh.stop();
            }
            catch
            { }
        }


        //getMac
        private string GetPhyAddr()
        {
            try
            {
                string mac = "";

                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface ni in interfaces)
                {
                    if (ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                    {
                        mac = ni.GetPhysicalAddress().ToString().ToUpper();
                        break;
                    }
                }
                return mac;
            }
            catch
            {
                return "";
            }
        }

        private bool checkUpdate()
        {
            HtmlParser hp = new HtmlParser();
            newVersion = hp.checkAppUpdate();
            if (newVersion.Equals(""))
                return false;
            return true;
        }

        //start app updating
        private void button1_Click(object sender, EventArgs e)
        {
            SelfDesignedMsg sdm = new SelfDesignedMsg("更新就交给我啦~ 我会暂时关闭当前程序,在下载和替换完成后会启动新版本程序,那么,现在开始吗?", true);
            DialogResult dr = sdm.ShowDialog();
            if (dr == DialogResult.Yes)
            {
                //call the PGLToolUpdate app
                Process proc =Process.Start(Environment.CurrentDirectory+ "/Update/PGLToolUpdateV1.0.exe", newVersion);
                Environment.Exit(0);
            }
        }
    }
}
