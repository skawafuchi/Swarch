using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace WebServer
{
    class WebServer
    {
        private static SQLiteConnection web_dbConnection;

        public WebServer()
        {
            web_dbConnection = new SQLiteConnection("Data Source=PhagocyteDatabase.sqlite;Version=3;");
            web_dbConnection.Open();
            WebServerThread();
        }

        public static string updateHighscorePage()
        {
            string response = "<table border=\"1\" style=\"width:300px\"><tr><th>User</th><th>Highscore</th></tr>";

            string sql = "select * from playerData order by score desc";
            SQLiteCommand command = new SQLiteCommand(sql, web_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                response += ("<tr><td>" + reader["username"] + "</td><td>" + reader["score"] + "</td></tr>");
            response += "</table>";


            return response;
        }

        public void WebServerThread()
        {
            while (true)
                SimpleListenerExample(new string[] { "http://localhost:8080/test/" }, updateHighscorePage());
        }

        //Taken from HttpListener Class documentation example
        // This example requires the System and System.Net namespaces. 
        public static void SimpleListenerExample(string[] prefixes, string responseString)
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            // URI prefixes are required, 
            // for example "http://contoso.com:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // Create a listener.
            HttpListener listener = new HttpListener();
            // Add the prefixes. 
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }
            listener.Start();
            Console.WriteLine("Listening...");
            // Note: The GetContext method blocks while waiting for a request. 
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            // Obtain a response object.
            HttpListenerResponse response = context.Response;
            // Construct a response. 
            //string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();
            listener.Stop();
        }
    }
}
