using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.IO;

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
        static List<int> removedPlayers;

        public static Random randGen = new Random();
        public static Dictionary<int, Phagocyte> clients;
        public static Dictionary<int, Point> pellets;

        public static OurSQLite oursqlite = new OurSQLite();

        public Server(int port)
        {
            removedPlayers = new List<int>();
            clients = new Dictionary<int, Phagocyte>();
            pellets = new Dictionary<int, Point>();

            //SQLite Setup
            oursqlite.setUpDB("PhagocyteDatabase.sqlite");
            oursqlite.printTable();

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

        public static void removeClient(int clientNum)
        {
            removedPlayers.Add(clientNum);
            clients[clientNum].inGame = false;
            clients.Remove(clientNum);

            Console.WriteLine("Client Disconnected! Client size: " + clients.Count + " RemovedPlayer count: " + removedPlayers.Count);
            byte[] toSend = new byte[50];
            toSend[0] = 5;
            toSend[1] = (byte)clientNum;
            broadcast(toSend);

        }
        //Constantly listen for and add new clients while the server is running
        private void addClient()
        {
            listen.Start();
            while (true)
            {
                try
                {
                    TcpClient newClient = listen.AcceptTcpClient();
                    int addedPNum;
                    if (removedPlayers.Count == 0)
                    {
                        addedPNum = clients.Count;
                        Console.WriteLine("Client Connected! Player number given: " + clients.Count);
                        clients.Add(clients.Count, new Phagocyte(newClient, clients.Count, randGen.Next(-2, 18), randGen.Next(-10, 10)));
                    }
                    else
                    {
                        addedPNum = removedPlayers[0];
                        Console.WriteLine("Client Connected! Client size: " + clients.Count + " RemovedPlayer count: " + removedPlayers.Count + "Client number given: " + removedPlayers[0]);
                        clients.Add(removedPlayers[0], new Phagocyte(newClient, removedPlayers[0], randGen.Next(-2, 18), randGen.Next(-10, 10)));
                        removedPlayers.RemoveAt(0);
                        Console.WriteLine("State after connect Client size: " + clients.Count + " RemovedPlayer count: " + removedPlayers.Count);
                    }
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message + ": on client add");
                }

            }
        }

        //byte format of all current players, positions, scores, and radiuses
        public static byte[] gameState()
        {

            //Needs to send the player the positions and directions of all players
            byte[] toSend = new byte[50];
            //3 indicates player positions
            toSend[0] = 7;
            int clientsInGame = 0;

            foreach (int key in clients.Keys) {
                if (clients[key].inGame) {
                    clientsInGame++;
                }
            }

            toSend[1] = (byte) clientsInGame;


            int counter = 2;
            foreach (int index in clients.Keys) {
                if (clients[index].inGame)
                {
                    toSend[counter] = (byte)(clients[index].myPNum);
                    toSend[++counter] = (byte)(clients[index].score);
                    toSend[++counter] = (byte)(clients[index].radius);
                    if (clients[index].xDir == 0 && clients[index].yDir == 1)
                    {
                        toSend[++counter] = 0;
                    }
                    else if (clients[index].xDir == 0 && clients[index].yDir == -1)
                    {
                        toSend[++counter] = 1;
                    }
                    else if (clients[index].xDir == -1 && clients[index].yDir == 0)
                    {
                        toSend[++counter] = 2;
                    }
                    else if (clients[index].xDir == 1 && clients[index].yDir == 0)
                    {
                        toSend[++counter] = 3;
                    }
                        //player isnt moving, still need to move the counter though
                    else {
                        toSend[++counter] = 4;
                    }
                    counter++;
                    System.Buffer.BlockCopy(toByteArray(clients[index].xpos), 0, toSend, counter, 4);
                    counter += 4;
                    System.Buffer.BlockCopy(toByteArray(clients[index].ypos), 0, toSend, counter, 4);
                    counter += 4;
                }
            }
            return toSend;
        }

        //sends the player names of all currently connected players to the client number specified
        public static void sendNames(int clientNum){
            foreach (int key in clients.Keys) { 
                if (clients[key].inGame){
                    byte[] toSend = new byte[50];
                    toSend[0] = 8;
                    toSend[1] = (byte)clients[key].myPNum;
                    toSend[2] = (byte)clients[key].clientName.Length;
                    System.Buffer.BlockCopy(Encoding.ASCII.GetBytes(clients[key].clientName), 0, toSend, 3, Encoding.ASCII.GetBytes(clients[key].clientName).Length);

                    clients[clientNum].sendMsg(toSend);
                }
            }
        }

        //Method to check if a player hits a pellet
        //Increments player score, their radius, and broadcasts the info to all players
        private void checkPellet(Phagocyte client) {
            for (int i = 0; i < 5; i++) { 
                if (((client.xpos - pellets[i].x)*(client.xpos - pellets[i].x)) + ((client.ypos - pellets[i].y)*(client.ypos - pellets[i].y)) <= (Math.Pow((Math.Pow(1.2,client.radius)/2)+0.5,2))){
                    client.score++;
                    client.radius++;
                    //Update client's score in database
                    oursqlite.updateScore(client.clientName, client.score);
                    oursqlite.printTable();

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

        private void checkPlayers(Phagocyte client) {
            foreach (int i in clients.Keys) {
                if (clients[i].inGame && i != client.myPNum) {
                    //check to see if collision
                    if (((client.xpos - clients[i].xpos) * (client.xpos - clients[i].xpos)) + ((client.ypos - clients[i].ypos) * (client.ypos - clients[i].ypos)) <= (Math.Pow((Math.Pow(1.2, client.radius) / 2) + (Math.Pow(1.2, clients[i].radius) / 2), 2)))
                    {
                        //if the two players are the same size, reset them both
                        if (client.radius == clients[i].radius) {

                            client.resetPlayer();
                            clients[i].resetPlayer();
                            //Update scores in database.
                            //Must do this for all clients involved
                            oursqlite.updateScore(client.clientName, client.score);
                            oursqlite.updateScore(clients[i].clientName, clients[i].score);
                        }

                        else if (client.radius > clients[i].radius)
                        {
                            client.score += 10;
                            client.radius++;

                            byte[] toSend = new byte[50];
                            toSend[0] = 6;
                            toSend[1] = (byte)client.myPNum;
                            toSend[2] = (byte)client.score;
                            toSend[3] = (byte)client.radius;

                            broadcast(toSend);

                            clients[i].resetPlayer();
                            oursqlite.updateScore(client.clientName, client.score);
                            oursqlite.updateScore(clients[i].clientName, clients[i].score);
                        }
                        else {
                            byte[] toSend = new byte[50];
                            clients[i].score += 10;
                            clients[i].radius++;

                            toSend[0] = 6;
                            toSend[1] = (byte)clients[i].myPNum;
                            toSend[2] = (byte)clients[i].score;
                            toSend[3] = (byte)clients[i].radius;

                            broadcast(toSend);

                            client.resetPlayer();
                            oursqlite.updateScore(client.clientName, client.score);
                            oursqlite.updateScore(clients[i].clientName, clients[i].score);
                        }
                        oursqlite.printTable();
                    }
                }
            }
        
        }
        
        //helper function that sends a specified message to all clients
        public static void broadcast(byte[] msg)
        {
            try
            {
                foreach (int i in clients.Keys)
                {
                    clients[i].sendMsg(msg);
                }
            }catch(Exception e){
                Console.WriteLine(e.Message + ": on broadcast message");
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
                            oursqlite.updateScore(client.clientName, client.score);
                            oursqlite.printTable();
                        }

                        //checks if player eats any of the pellets and sends out data
                        if (client.inGame)
                        {
                            checkPellet(client);
                            checkPlayers(client);
                        }
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


        //helper class that converts a floating point decimal into a byte array
        private static byte[] toByteArray(float value)
        {
            byte[] bytes = System.BitConverter.GetBytes(value);
            return bytes;
        }

        //Helper class that converts a byte array (length 4) to a floating point decimal
        private static float toFloat(byte[] bytes)
        {
            return System.BitConverter.ToSingle(bytes, 0);
        }

    }


}
