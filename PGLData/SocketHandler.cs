using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;  


using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections;
using System.Windows.Forms;

namespace PGLData
{
    class SocketHandler
    {

        IPAddress server_ip;

        TcpClient socket5500; 

        StreamReader sr5500;
        StreamWriter sw5500;

        //constructor, set up connection to the server
        public SocketHandler() 
        {
            server_ip = IPAddress.Parse("***********");
            try
            {
                //setup client socket
                socket5500 = new TcpClient();
                socket5500.Connect(new IPEndPoint(server_ip, 5500)); 

                NetworkStream ns = socket5500.GetStream();
                sr5500 = new StreamReader(ns);
                sw5500 = new StreamWriter(ns);
            }
            catch
            {
                throw;
            }
        }
     
        //port 5500
        public string sendMessageR(string message1, string message2, string message3)
        {
            sw5500.WriteLine(message1);
            sw5500.WriteLine(message2);
            sw5500.WriteLine(message3);
            sw5500.Flush();
            string result = sr5500.ReadLine();
            return result;
        }

        public string sendMessageR(string message1, string message2, string message3, string message4)
        {
            sw5500.WriteLine(message1);
            sw5500.WriteLine(message2);
            sw5500.WriteLine(message3);
            sw5500.WriteLine(message4);
            sw5500.Flush();
            string result = sr5500.ReadLine();
            return result;
        }

        //stop the socket
        public void stop() 
        {
            sr5500.Close();
            sw5500.Close();
            socket5500.Close();
        }

    }
}

