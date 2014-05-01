using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Data.SQLite;

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
                            //Need to trim off whitespace
                            strUN = strUN.TrimEnd('\0');
                            strPW = strPW.TrimEnd('\0');
                            Console.WriteLine("username: " + strUN + "password: " + strPW);

                            byte[] toSend = new byte[50];
                            //toSend[1] is 0 if they are not allowed access, 1 if allowed
                            //currently always allowing players to access

                            //Check to see if player is in database
                            Console.WriteLine("1");
                            string sql = "SELECT COUNT(*) FROM playerData WHERE username=@username";
                            Console.WriteLine("2");
                            SQLiteCommand command = new SQLiteCommand(sql, Server.p_dbConnection);
                            Console.WriteLine("3");
                            command.Parameters.AddWithValue("@username", strUN);
                            Console.WriteLine("4");
                            int count = Convert.ToInt32(command.ExecuteScalar());
                            Console.WriteLine("match = " + count);
                            if (count > 0)
                            {
                                Console.WriteLine("Checking...");
                                //Check if passwords match
                                string compare = "";
                                sql = "SELECT password FROM playerData WHERE username=@username";
                                command = new SQLiteCommand(sql, Server.p_dbConnection);
                                command.Parameters.AddWithValue("@username", strUN);
                                Console.WriteLine("5");
                                SQLiteDataReader reader2 = command.ExecuteReader();
                                Console.WriteLine("6");
                                while (reader2.Read())
                                {
                                    Console.WriteLine("7");
                                    compare = (string)reader2["password"];
                                    Console.WriteLine("8");
                                    
                                }
                                Console.WriteLine("strPw = " + strPW + "| compare = " + compare);
                                if (strPW.Equals(compare))
                                {
                                    toSend[1] = 1;
                                }
                                else
                                {
                                    Console.WriteLine("Please enter the correct password.");
                                }
                            }
                            else //Add player to table
                            {
                                //Password will not be encrypted yet
                                sql = "INSERT INTO playerData (username, password, score) VALUES (@username, @password, 0)";
                                command = new SQLiteCommand(sql, Server.p_dbConnection);
                                command.Parameters.AddWithValue("@username", strUN);
                                command.Parameters.AddWithValue("@password", strPW);
                                command.ExecuteNonQuery();

                                //Print out table for testing
                                sql = "SELECT * FROM playerData";
                                command = new SQLiteCommand(sql, Server.p_dbConnection);
                                SQLiteDataReader reader3 = command.ExecuteReader();
                                while (reader3.Read())
                                {
                                    Console.WriteLine("Username: " + reader3["username"] + " | Password: "
                                        + reader3["password"] + " | Score: " + reader3["score"]);
                                }

                                toSend[1] = 1;
                           }
                            //if they connected
                            //sends player pellet info
                            if (toSend[1] == 1)
                            {
                                int counter = 0;
                                for (int i = 2; i <= Server.pellets.Count * 2; i += 2)
                                {
                                    toSend[i] = (byte)(Server.pellets[i - (2 + counter)].x);
                                    toSend[i + 1] = (byte)(Server.pellets[i - (2 + counter)].y);
                                    counter++;
                                }

                                netStream.Write(toSend, 0, 50);
                            }
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
