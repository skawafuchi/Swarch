using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    class Server
    {
        private TcpListener listen;
        Thread listenLoop;
        List<Phagocyte> clients;
        //Point[] pellets = new Point[5];


        public Server(int port) {
            listen = new TcpListener(IPAddress.Any, port);
            listenLoop = new Thread(new ThreadStart(addClient));
            listenLoop.Start();

            clients = new List<Phagocyte>();
        }


        private void addClient()
        {
            listen.Start();
            while (true)
            {

                TcpClient newClient = listen.AcceptTcpClient();
                clients.Add(new Phagocyte(newClient));
                Console.Write("Client Connected!\n");
             }
        }

        private void gameLoop() {
        
        }
    }


}
