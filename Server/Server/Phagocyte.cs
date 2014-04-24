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
        TcpClient myPlayer;
        NetworkStream netStream;
        Thread readWrite;
        int xDir, yDir;
        double radius, xpos, ypos;
        byte[] read;
        int msgSize;


        public Phagocyte(object player) {
            myPlayer = (TcpClient)player;
            netStream = myPlayer.GetStream();

            readWrite = new Thread(new ThreadStart(readWriteLoop));
            readWrite.Start();

            read = new byte[50];
            xDir = yDir = 0;

        }

        private void readWriteLoop() { 
            while (true){
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
                            System.Buffer.BlockCopy(read, indexOfParse+1, password, 0, msgSize - indexOfParse-1);
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

                            netStream.Write(toSend,0,50);
                        }
                    }
                    catch
                    {
                        Console.Write("Error Trying to Read\n");
                    }

                }
            }
        }

       
    }
}
