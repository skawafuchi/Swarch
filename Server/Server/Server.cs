using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Data.SQLite;

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
        List<int> removedPlayers;
        List<Phagocyte> clients; //Old way
        //public static Dictionary<int, Phagocyte> clients;
        public static Random randGen = new Random();
        public static Dictionary<int, Point> pellets;
        public static SQLiteConnection p_dbConnection;

        public Server(int port)
        {
            //Create database file
            SQLiteConnection.CreateFile("PhagocyteDatabase.sqlite");

            //Open connection to database
            p_dbConnection = new SQLiteConnection("Data Source=PhagocyteDatabase.sqlite;Version=3;");
            p_dbConnection.Open();

            //Create table
            string sql = "CREATE TABLE playerData (username VARCHAR(20), password VARCHAR(20), score INT)";
            SQLiteCommand command = new SQLiteCommand(sql, p_dbConnection);
            command.ExecuteNonQuery();

            //for looking for new clients
            listen = new TcpListener(IPAddress.Any, port);
            listenLoop = new Thread(new ThreadStart(addClient));
            listenLoop.Start();

            System.Timers.Timer gameLoopRate = new System.Timers.Timer();
            gameLoopRate.Elapsed += new ElapsedEventHandler(gameLoop);
            gameLoopRate.Interval = 15;
            gameLoopRate.Enabled = true;

            clients = new List<Phagocyte>(); //new Dictionary<int, Phagocyte>();
            pellets = new Dictionary<int, Point>();


            //PELLETS ARE KEPT IN +/- VALUES, BUT SENT IN AS POSITIVE VALUES SHIFTED BY 2 AND 10 RESPECTIVELY
            //CLIENT HAS TO SHIFT VALUES BACK
            for (int i = 0; i < 5; i++)
            {
                pellets.Add(i, new Point(randGen.Next(-2, 18), randGen.Next(-10, 10)));
            }
            foreach (Point x in pellets.Values)
            {
                Console.WriteLine("Point: " + x.x + "," + x.y);
            }
        }


        public void removeClient()
        {

        }

        private void addClient()
        {
            listen.Start();
            while (true)
            {

                TcpClient newClient = listen.AcceptTcpClient();
                //For now, use old way
                clients.Add(new Phagocyte(newClient));
                //clients.Add(clients.Count, new Phagocyte(newClient, clients.Count, randGen.Next(-2, 18), randGen.Next(-10, 10)));
                //clients[clients.Count - 1].sendMsg(gameState());
                Console.Write("Client Connected!\n");
            }
        }

        //public byte[] gameState()
        //{

        //    //Needs to send the player the positions and directions of all players
        //    byte[] toSend = new byte[50];
        //    //3 indicates player positions
        //    toSend[0] = 2;
        //    toSend[1] = (byte)Server.clients.Count;

        //    int counter = 0;
        //    for (int i = 2; i <= Server.clients.Count * 6; i += 6)
        //    {
        //        toSend[i] = (byte)(Server.clients[i - (2 + counter)].myPNum);
        //        toSend[i + 1] = (byte)(Server.clients[i - (2 + counter)].xDir);
        //        toSend[i + 2] = (byte)(Server.clients[i - (2 + counter)].yDir);
        //        toSend[i + 3] = (byte)(Server.clients[i - (2 + counter)].xpos);
        //        toSend[i + 4] = (byte)(Server.clients[i - (2 + counter)].ypos);
        //        toSend[i + 5] = (byte)(Server.clients[i - (2 + counter)].radius);
        //    }
        //    return toSend;
        //}

        //public static void broadcast(byte[] msg)
        //{
        //    for (int i = 0; i < clients.Count; i++)
        //    {
        //        clients[i].sendMsg(msg);
        //    }
        //}

        private void gameLoop(object source, ElapsedEventArgs e)
        {
            foreach (Phagocyte client in clients)//.Values)
            {
                client.move();
                //if player collides with the wall
                if (((client.xpos - 10.5) * (client.xpos - 10.5)) + ((client.ypos - 1) * (client.ypos - 1)) > 400f)
                {
                    client.resetPlayer();
                }
                /* IF PLAYER COLLIDES WITH A PELLETS
                else if (){
                    client.eatPellet();
                }*/
            }

        }
    }


}
