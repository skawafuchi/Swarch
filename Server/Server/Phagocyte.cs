using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    class Phagocyte
    {
        public double radius, xpos, ypos;
        TcpClient myPlayer;
        NetworkStream netStream;
        Thread readWrite;
        int xDir, yDir, score;

        byte[] read;
        int msgSize;


        public Phagocyte(object player)
        {
            myPlayer = (TcpClient)player;
            netStream = myPlayer.GetStream();

            readWrite = new Thread(new ThreadStart(readWriteLoop));
            readWrite.Start();

            read = new byte[50];
            xDir = yDir = 0;

        }

        public void resetPlayer()
        {

        }

        public void move()
        {
            xpos += xDir;
            ypos += yDir;
        }

        private void readWriteLoop()
        {
            while (true)
            {
                if (netStream.DataAvailable)
                {
                    try
                    {
                        msgSize = netStream.Read(read, 0, 50);
                        //first byte is a 0 then its a User name and password message
                        if (read[0] == 0)
                        {
                            //gets the index of the space inbetween username and password
                            int indexOfParse = 0;
                            for (int i = 1; i < msgSize; i++)
                            {
                                if (read[i] == 0)
                                {
                                    indexOfParse = i;
                                    break;
                                }
                            }
                            //converts password and username from byte array to string
                            byte[] userName = new byte[indexOfParse];
                            byte[] password = new byte[msgSize - indexOfParse];
                            System.Buffer.BlockCopy(read, 1, userName, 0, indexOfParse);
                            System.Buffer.BlockCopy(read, indexOfParse + 1, password, 0, msgSize - indexOfParse - 1);
                            string strUN = Encoding.ASCII.GetString(userName);
                            string strPW = Encoding.ASCII.GetString(password);
                            Console.WriteLine("username: " + strUN + "password: " + strPW);

                            //Check to see if database
                            //NEEDS IMPLEMENTATION
                            //if(IN DATABASE)
                            byte[] toSend = new byte[50];
                            //toSend[1] is 0 if they are not allowed access, 1 if allowed
                            //currently always allowing players to access
                            toSend[1] = 1;

                            //if they connected
                            //sends player pellet info
                            int counter = 0;
                            for (int i = 2; i <= Server.pellets.Count*2; i+=2) {
                                toSend[i] = (byte)(Server.pellets[i - (2 + counter)].x);
                                toSend[i + 1] = (byte)(Server.pellets[i-(2+counter)].y);
                                counter++;
                            }

                            netStream.Write(toSend, 0, 50);

                        }
                        //first byte is a 1 then it's a move command
                        else if (read[0] == 1)
                        {
                            xDir = (int)read[1];
                            yDir = (int)read[2];
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Error Trying to Read");
                    }

                }
            }
        }


    }
}
