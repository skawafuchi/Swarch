using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Server
{
    class myMain
    {

        static void Main(string[] args)
        {
            int port = 18503;
            Server x = new Server(port);
            Console.WriteLine("Opened a server on port: " + port);
            //WebServer y = new WebServer();
        }
    }

}

