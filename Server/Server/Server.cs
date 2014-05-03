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
    //Helper struct that is used for the pellets
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
        //Variables for game loop
        const double HERTZ = 50.0, UPDATE_TIME = 1000000000/ HERTZ ;
	    const int MAX_RENDER = 5;
        double lastUpdate, now;
        Thread tGameLoop;
        
        //Variables for network
        private TcpListener listen;
        Thread listenLoop;
        List<int> removedPlayers;

        public static Random randGen = new Random();
        //List<Phagocyte> clients; //Old way
        public static Dictionary<int, Phagocyte> clients;
        public static Dictionary<int, Point> pellets;
        public static SQLiteConnection p_dbConnection;

        public Server(int port)
        {
            clients = new Dictionary<int, Phagocyte>();
            pellets = new Dictionary<int, Point>();

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


            //Game loop
            lastUpdate = DateTime.Now.TimeOfDay.TotalMilliseconds * 1000000; 
            tGameLoop = new Thread(new ThreadStart(gameLoop));
            tGameLoop.Start();

            pellets = new Dictionary<int, Point>();


            //PELLETS ARE KEPT IN +/- VALUES, BUT SENT IN AS POSITIVE VALUES SHIFTED BY 2 AND 10 RESPECTIVELY
            //CLIENT HAS TO SHIFT VALUES BACK
            for (int i = 0; i < 5; i++)
            {
                pellets.Add(i, new Point(randGen.Next(-2, 18), randGen.Next(-10, 10)));
            }
        }


        public void removeClient()
        {

        }

        //Constantly listen for and add new clients while the server is running
        private void addClient()
        {
            listen.Start();
            while (true)
            {
                TcpClient newClient = listen.AcceptTcpClient();
                //For now, use old way
                //clients.Add(clients.Count,new Phagocyte(newClient));
                clients.Add(clients.Count, new Phagocyte(newClient, clients.Count, randGen.Next(-2, 18), randGen.Next(-10, 10)));
                //clients[clients.Count - 1].sendMsg(gameState());
                Console.Write("Client Connected!\n");
            }
        }

        public byte[] gameState()
        {

            //Needs to send the player the positions and directions of all players
            byte[] toSend = new byte[50];
            //3 indicates player positions
            toSend[0] = 4;
            toSend[1] = (byte)Server.clients.Count;

            int counter = 0;
            for (int i = 2; i <= Server.clients.Count * 6; i += 6)
            {
                toSend[i] = (byte)(Server.clients[i - (2 + counter)].myPNum);
                toSend[i + 1] = (byte)(Server.clients[i - (2 + counter)].xDir);
                toSend[i + 2] = (byte)(Server.clients[i - (2 + counter)].yDir);
                toSend[i + 3] = (byte)(Server.clients[i - (2 + counter)].xpos);
                toSend[i + 4] = (byte)(Server.clients[i - (2 + counter)].ypos);
                toSend[i + 5] = (byte)(Server.clients[i - (2 + counter)].radius);
            }
            return toSend;
        }

        //Method to check if a player hits a pellet
        //Increments player score, their radius, and broadcasts the info to all players
        private void checkPellet(Phagocyte client) {
            for (int i = 0; i < 5; i++) { 
                if (((client.xpos - pellets[i].x)*(client.xpos - pellets[i].x)) + ((client.ypos - pellets[i].y)*(client.ypos - pellets[i].y)) <= (Math.Pow((Math.Pow(1.2,client.radius)/2)+0.5,2))){
                    client.score++;
                    client.radius++;

                    pellets[i] = new Point(randGen.Next(-2, 18), randGen.Next(-10, 10));
                    byte[] toSend = new byte[50];
                    toSend[0] = 3;
                    toSend[1] = (byte)client.myPNum;
                    toSend[2] = (byte)client.score;
                    toSend[3] = (byte)client.radius;

                    int counter = 0;
                    for (int k = 4; k <= 12; k += 2)
                    {
                        toSend[k] = (byte)(Server.pellets[k - (4 + counter)].x + 2);
                        toSend[k + 1] = (byte)(Server.pellets[k - (4 + counter)].y + 10);
                        counter++;
                    }
                    broadcast(toSend);
                }
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

        //helper function that sends a specified message to all clients
        public static void broadcast(byte[] msg)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].sendMsg(msg);
            }
        }

        //Reliable game loop that runs at 50FPS
        private void gameLoop()
        {

            while (true)

            {

                now = DateTime.Now.TimeOfDay.TotalMilliseconds * 1000000;
                int updateCount = 0;

                while (now - lastUpdate > UPDATE_TIME && updateCount < MAX_RENDER)
                {
                    //Update
                    foreach (Phagocyte client in clients.Values)
                    {
                        client.move();
                        //if player collides with the wall
                        if (((client.xpos - 10.5) * (client.xpos - 10.5)) + ((client.ypos - 1) * (client.ypos - 1)) > (Math.Pow((Math.Pow(1.2, client.radius) / 2) - 20, 2)))
                        {
                            client.resetPlayer();
                        }

                        //checks if player eats any of the pellets and sends out data
                        checkPellet(client);
                    }

                    lastUpdate += UPDATE_TIME;
                    updateCount++;
                

                }
                if (now - lastUpdate > UPDATE_TIME)
                {
                    lastUpdate = now - UPDATE_TIME;
                }


                while (now - lastUpdate < UPDATE_TIME)
                {
                    Thread.Yield();
                    now = DateTime.Now.TimeOfDay.TotalMilliseconds * 1000000;
                }

            }
        }
    }


}
