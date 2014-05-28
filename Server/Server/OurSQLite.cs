using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;

namespace Server
{
    class OurSQLite
    {
        public SQLiteConnection p_dbConnection;

        public OurSQLite()
        {

        }

        public void setUpDB(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("Making new db file!");
                makeDBFile(filename);
            }
            openDBConnection(filename);
            makeTable();
        }

        private void makeDBFile(string filename)
        {
            SQLiteConnection.CreateFile(filename);
        }

        private void openDBConnection(string filename)
        {
            p_dbConnection = new SQLiteConnection("Data Source=" + filename + ";Version=3;");
            p_dbConnection.Open();
        }

        private void makeTable()
        {
            string sql = "CREATE TABLE IF NOT EXISTS playerData (username VARCHAR(20), password CHAR(32), score INT)";
            SQLiteCommand command = new SQLiteCommand(sql, this.p_dbConnection);
            command.ExecuteNonQuery();
        }

        public int numberOfUsernameMatches(string username)
        {
            string sql = "SELECT COUNT(*) FROM playerData WHERE username=@username";
            SQLiteCommand command = new SQLiteCommand(sql, this.p_dbConnection);
            command.Parameters.AddWithValue("@username", username);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public string getPassword(string username)
        {
            string password = "";
            string sql = "SELECT password FROM playerData WHERE username=@username";
            SQLiteCommand command = new SQLiteCommand(sql, this.p_dbConnection);
            command.Parameters.AddWithValue("@username", username);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                password = (string)reader["password"];
            }
            return password;
        }

        public void addToTable(string username, string password)
        {
            string sql = "INSERT INTO playerData (username, password, score) VALUES (@username, @password, 0)";
            SQLiteCommand command = new SQLiteCommand(sql, this.p_dbConnection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);
            command.ExecuteNonQuery();
        }

        public void updateScore(string username, int score)
        {
            string sql = "UPDATE playerData SET score=@score WHERE username=@username";
            SQLiteCommand command = new SQLiteCommand(sql, this.p_dbConnection);
            command.Parameters.AddWithValue("@score", score);
            command.Parameters.AddWithValue("@username", username);
            command.ExecuteNonQuery();
        }

        public void printTable()
        {
            Console.WriteLine("------------------------------------------");
            string sql = "SELECT * FROM playerData";
            SQLiteCommand command = new SQLiteCommand(sql, this.p_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine("Username: " + reader["username"] + " | Password: "
                    + reader["password"] + " | Score: " + reader["score"]);
            }
        }
    }
}
