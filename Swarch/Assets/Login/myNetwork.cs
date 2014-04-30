
using UnityEngine;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net;
using System;
using System.Diagnostics;
using System.Threading;

public class myNetwork : MonoBehaviour {
	
	const string SERVER_ADDR = "192.168.1.5"; 
	const int SERVER_PORT = 18503;
	
	public TcpClient client;
	public NetworkStream nws;
	public bool serverAppr = false;
	Thread t = null;
	byte[] readData;
	
	public Queue data;
	
	public myNetwork(){
		data = new Queue();
		readData = new byte[50];
	}
	
	public void Connect ()
	{
		try
		{
			client = new TcpClient(SERVER_ADDR,SERVER_PORT);
			if (client.Connected){
				nws = client.GetStream();
				t = new Thread(new ThreadStart(myReadData));
				t.IsBackground = true;
				t.Start();
			}
			
		}
		catch ( Exception ex )
		{
			print ( ex.Message + " : OnConnect");
		}
	}
	
	void myReadData(){
		//constantly check stream for data to read from the server and do something with it.
		try{
			while (true){
				if (nws.DataAvailable){
					nws.Read(readData,0,50);
					data.Enqueue(readData);
					//flag to switch scenes
					if (readData[0] == 0 && readData[1] == 1){
						serverAppr = true;
					}
				}
				
			}
		}catch(Exception e){
			print(e.Message);
		}
	}
	
	public void Disconnect ()
	{	
		t.Abort();
		nws.Close();
		client.Close();
	}
	
	public void SendTCPPacket ( byte[] toSend,int offset,int size)
	{
		try
		{	
			nws.Write(toSend,offset,size);
		}
		catch ( Exception ex )
		{
			print ( ex.Message + ": OnTCPPacket" );
		}	
	}
	
}
