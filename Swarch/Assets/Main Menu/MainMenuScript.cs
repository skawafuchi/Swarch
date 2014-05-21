//Steven Kawafuchi
//Tamara Sahagun
//ICS 168 Multi-Player Game Project

using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;

public class MainMenuScript: MonoBehaviour {
	GUIText title;
	bool showInstructions = false;
	public Texture card;
	public GUIStyle style = null;
	
	void Start () {
		//UI MAY LOOK WEIRD IN UNITY PREVIEW WINDOW
		//SET UP TO LOOK NICE IN ACTUAL GAME WINDOW
		//Set up title
		title =  GameObject.Find("Title").GetComponent<GUIText>();
		title.transform.position = new Vector3(0.5f,0.9f,0f);
		title.fontSize = 50;
		card = Resources.Load("Instructions") as Texture;
	}
	
	void OnGUI () 
	{	
		if (GUI.Button (new Rect(Screen.width/2 - 50,Screen.height/2 +40,100,20),"Play!", style))
		{
			Application.LoadLevel("Login Menu");
		}
		else if (GUI.Button(new Rect(Screen.width/2 - 50,Screen.height/2 +80,100,20),"Instructions", style))
		{
			showInstructions = !showInstructions;
		}
		
		if(showInstructions)
		{
			GUI.Label(new Rect(Screen.width/2 - card.width/2,Screen.height/2 - card.height, 
				card.width, card.height), card);
		}
	}
}
