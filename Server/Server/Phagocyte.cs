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
        public bool inGame;
        //Game variables
        public string clientName;
        public float radius, xpos, ypos;
        public int xDir, yDir, score, myPNum;

        //Network information
        TcpClient myPlayer;
        NetworkStream netStream;
        Thread readWrite;
        byte[] read;
        int msgSize;

        public Phagocyte(object player, int pNum, int startX, int startY)
        {
            inGame = false;
            myPNum = pNum;
            myPlayer = (TcpClient)player;
            netStream = myPlayer.GetStream();

            readWrite = new Thread(new ThreadStart(readWriteLoop));
            readWrite.Start();

            read = new byte[50];
            xDir = yDir = 0;

            xpos = startX;
            ypos = startY;
            radius = 0;


        }


        //Helper class that moves the player based on how big he/she is
        public void move()
        {
            xpos += xDir * 0.2f * (float)Math.Pow(0.75, radius);
            ypos += yDir * 0.2f * (float)Math.Pow(0.75, radius);
        }

        //helper class that converts a floating point decimal into a byte array
        public static byte[] toByteArray(float value)
        {
            byte[] bytes = System.BitConverter.GetBytes(value);
            return bytes;
        }

        //Helper class that converts a byte array (length 4) to a floating point decimal
        public static float toFloat(byte[] bytes)
        {
            return System.BitConverter.ToSingle(bytes, 0);
        }

        //Sends specified message to this client
        public void sendMsg(byte[] msg)
        {
            try
            {
                netStream.Write(msg, 0, 50);
            }
            catch(Exception e)
            {
                Server.removeClient(myPNum);
                Console.WriteLine(e.Message + ": Unable to write message to client: " + myPNum);
            }
        }
        
        //Resets player position, their score, and broadcasts their death to the players
        public void resetPlayer()
        {
            xpos = Server.randGen.Next(-2, 18);
            ypos = Server.randGen.Next(-10, 10);
            radius = 0;
            score = 0;

            byte[] toSend = new Byte[50];
            toSend[0] = 2;
            toSend[1] = (byte)myPNum;
            System.Buffer.BlockCopy(toByteArray(xpos), 0, toSend, 2, 4);
            System.Buffer.BlockCopy(toByteArray(ypos), 0, toSend, 6, 4);
            Server.broadcast(toSend);
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
                            int indexOfParse = read[1];
                            //converts password and username from byte array to string
                            byte[] userName = new byte[indexOfParse];
                            byte[] password = new byte[msgSize - indexOfParse];
                            System.Buffer.BlockCopy(read, 2, userName, 0, indexOfParse);
                            System.Buffer.BlockCopy(read, indexOfParse + 2, password, 0, msgSize - indexOfParse - 2);
                            string strUN = Encoding.ASCII.GetString(userName);
                            string strPW = Encoding.ASCII.GetString(password);
                            //Need to trim off whitespace
                            strUN = strUN.TrimEnd('\0');
                            strPW = strPW.TrimEnd('\0');
                            clientName = strUN;

                            byte[] toSend = new byte[50];
                            //toSend[1] is 0 if they are not allowed access, 1 if allowed
                            //currently always allowing players to access

                            //Check to see if player is in database
                            int count = Server.oursqlite.numberOfUsernameMatches(strUN);
                            if (count > 0) //If in database,
                            {
                                //Check if passwords match
                                string compare = Server.oursqlite.getPassword(strUN);
                                if (strPW.Equals(compare))
                                {
                                    toSend[1] = 1;
                                }
                                else
                                {
                                    Console.WriteLine("Wrong password was entered.");
                                    toSend[1] = 0;
                                }
                            }
                            else //Add player to table
                            {
                                Server.oursqlite.addToTable(strUN, strPW);

                                //Print out table for testing
                                Server.oursqlite.printTable();

                                toSend[1] = 1;
                           }
                            //if they connected
                            //sends player pellet info
                            if (toSend[1] == 1)
                            {
                                int counter = 0;
                                toSend[2] = (byte)myPNum;
                                for (int i = 3; i <= 11; i += 2)
                                {
                                    toSend[i] = (byte)(Server.pellets[i - (3 + counter)].x + 2);
                                    toSend[i + 1] = (byte)(Server.pellets[i - (3 + counter)].y + 10);
                                    counter++;
                                }

                                //sends player position with hello command
                                System.Buffer.BlockCopy(toByteArray(xpos), 0, toSend, 13, 4);
                                System.Buffer.BlockCopy(toByteArray(ypos), 0, toSend, 17, 4);

                            }
                            netStream.Write(toSend, 0, 50);


                            if (toSend[1] == 1){
                                //Sends the player connect msg to everyone in the game
                                inGame = true;
                                sendMsg(Server.gameState());
                                Server.sendNames(myPNum);


                                byte[] helloMsg = new byte[50];
                                helloMsg[0] = 4;
                                helloMsg[1] = (byte)myPNum;
                                System.Buffer.BlockCopy(toByteArray(xpos), 0, helloMsg, 2, 4);
                                System.Buffer.BlockCopy(toByteArray(ypos), 0, helloMsg, 6, 4);
                                helloMsg[10] = (byte)Encoding.ASCII.GetBytes(clientName).Length;
                                System.Buffer.BlockCopy(Encoding.ASCII.GetBytes(clientName), 0, helloMsg, 11, Encoding.ASCII.GetBytes(clientName).Length);

                                //Send the player the gamestate after connecting
                                Server.broadcast(helloMsg);

                            }


                        }
                        //first byte is a 1 then it's a move command
                        else if (read[0] == 1)
                        {
                            byte[] toSend = new byte[50];
                            if (read[2] == 0)
                            {
                                xDir = 0;
                                yDir = 1;
                            }
                            else if (read[2] == 1)
                            {
                                xDir = 0;
                                yDir = -1;
                            }
                            else if (read[2] == 2)
                            {
                                xDir = -1;
                                yDir = 0;
                            }
                            else if (read[2] == 3)
                            {
                                xDir = 1;
                                yDir = 0;
                            }

                            toSend[0] = 1;
                            toSend[1] = (byte)myPNum;
                            toSend[2] = read[2];
                            byte[] byteCoord = (toByteArray(xpos));

                            //encode x position
                            toSend[3] = byteCoord[0];
                            toSend[4] = byteCoord[1];
                            toSend[5] = byteCoord[2];
                            toSend[6] = byteCoord[3];

                            //encode y position
                            byteCoord = toByteArray(ypos);
                            toSend[7] = byteCoord[0];
                            toSend[8] = byteCoord[1];
                            toSend[9] = byteCoord[2];
                            toSend[10] = byteCoord[3];
                            //send the player move to all clients!
                            Server.broadcast(toSend);
                        }else if (read[0] == 2)
                        {
                            Server.removeClient(myPNum);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message + ": during phagocyte loop");
                    }

                }
            }
        }
    }
}
