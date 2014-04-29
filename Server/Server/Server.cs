﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;

namespace Server
{
    public struct Point
    {
        public int x, y;
        public Point(int Parx, int Pary)
        {
            x = Parx;
            y = Pary;
        }
    }

    class Server
    {
        private TcpListener listen;
        Thread listenLoop;
        List<Phagocyte> clients;
        Random randGen = new Random();
        public static List<Point> pellets;


        public Server(int port)
        {
            listen = new TcpListener(IPAddress.Any, port);
            listenLoop = new Thread(new ThreadStart(addClient));
            listenLoop.Start();

            System.Timers.Timer gameLoopRate = new System.Timers.Timer();
            gameLoopRate.Elapsed += new ElapsedEventHandler(gameLoop);
            gameLoopRate.Interval = 500;
            gameLoopRate.Enabled = true;

            clients = new List<Phagocyte>();
            pellets = new List<Point>();

            for (int i = 0; i < 5; i++)
            {
                pellets.Add(new Point(randGen.Next(0, 20), randGen.Next(0, 20)));
            }
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

        private void gameLoop(object source, ElapsedEventArgs e) {
            foreach (Phagocyte client in clients){
                client.move();
                
                //if (COLLIDES WITH PELLET OROTHERPLAYER){
                //    NEWPOINT
                //}
            }
        }
    }


}