//Steven Kawafuchi
//Tamara Sahagun
//ICS 168 Multi-Player Game Project

using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

public class LoginScript: MonoBehaviour {
	public static string userName,password;
	GUIText title, UNLabel,PWLabel;
	myNetwork net;
	
	string errMsg;
	const string SERVER_ADDR = "192.168.1.78";
	const int SERVER_PORT = 35081;
	
	void Start () {
		//UI MAY LOOK WEIRD IN UNITY PREVIEW WINDOW
		//SET UP TO LOOK NICE IN ACTUAL GAME WINDOW
		//Set up title
		title =  GameObject.Find("Title").GetComponent<GUIText>();
		title.transform.position = new Vector3(0.5f,0.9f,0f);
		title.fontSize = 50;
		
		//UserName Label on the screen
		UNLabel =  GameObject.Find("UNLabel").GetComponent<GUIText>();
		UNLabel.transform.position = new Vector3(0.5f,0.63f,0f);
		
		
		//Password Label on the screen 
		PWLabel =  GameObject.Find("PWLabel").GetComponent<GUIText>();
		PWLabel.transform.position = new Vector3(0.5f,0.570f,0f);
		
		
		//Initialize pw and user name to avoid null pointer
		password = userName = "Default";
		
		errMsg = "";
		
		//connect to the server when they open up a game client
		net = gameObject.AddComponent<myNetwork>();
		net.Connect();
	}
	
	
	void OnApplicationClose(){
		net.Disconnect();
	}
	
	void OnGUI () {
		if (net.serverAppr){
			Application.LoadLevel("Swarch");
		}
		
		
		//login button, changes scenes to Swarch Game
		if (GUI.Button(new Rect(Screen.width/2 - 50,Screen.height/2 +10,100,20),"Login!")){
			if (password.Length > 5 && userName.Length > 5){
				byte[]toSend = new byte[50];
				
				//Encode username and password into a byte array
				//username and PW separated by a 0 in the byte stream
				System.Buffer.BlockCopy(Encoding.ASCII.GetBytes (userName),0,toSend,1,Encoding.ASCII.GetBytes(userName).Length);
				System.Buffer.BlockCopy(Encoding.ASCII.GetBytes (password),0,toSend,(Encoding.ASCII.GetBytes(userName).Length)+ 2,Encoding.ASCII.GetBytes(password).Length);
				net.SendTCPPacket(toSend,0,toSend.Length);
			}else{
				errMsg = "Username/PW invalid";
				GUI.Label(new Rect(0,0,100,100),errMsg);
			}
		}
		
		//User name and PW fields 
		userName = GUI.TextField (new Rect(Screen.width/2 - 50,Screen.height/2 + -90,100,20),userName,24);
		password = GUI.PasswordField (new Rect(Screen.width/2 - 50,Screen.height/2 + - 45,100,20),password,"*"[0],24);
	}
}
