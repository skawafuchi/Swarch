//Steven Kawafuchi
//Tamara Sahagun
//ICS 168 Multi-Player Game Project

using UnityEngine;
using System.Collections;

public class LoginScript: MonoBehaviour {
	public static string userName,password;
	GUIText title, UNLabel,PWLabel;
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
	}
	
	void OnGUI () {
		
		//login button, changes scenes to Swarch Game
		if (GUI.Button(new Rect(Screen.width/2 - 50,Screen.height/2 +10,100,20),"Login!")){
			Application.LoadLevel("Swarch");
		}
		
		//User name and PW fields 
		userName = GUI.TextField (new Rect(Screen.width/2 - 50,Screen.height/2 + -90,100,20),userName,25);
		password = GUI.PasswordField (new Rect(Screen.width/2 - 50,Screen.height/2 + - 45,100,20),password,"*"[0],25);
	}
}
