//Steven Kawafuchi
//Tamara Sahagun
//ICS 168 Multi-Player Game Project

using UnityEngine;
using System.Collections;

public class GameProcess : MonoBehaviour {
	public GameObject []pellets = new GameObject[5];
	public GameObject []pelletShadows = new GameObject[5];
	public int score;
	myNetwork net;
	
	string myName = LoginScript.userName;
	GUIText pName,title,scoreText;
	byte[]currentData;
	
	void Start () {
		
		net = LoginScript.net;
		
		//create pellets
		for(int i = 0; i < 5; i++){
			pellets[i] = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			pellets[i].renderer.material.color = new Color(55f,0f,255f,0.5f);
			pellets[i].renderer.material.shader = Shader.Find("Transparent/Diffuse");
			pellets[i].transform.localScale = new Vector3(0.2f,0.2f,0.2f);
			pellets[i].transform.RotateAround(Vector3.zero, Vector3.forward, 20 * Random.Range(1,100));
			
			//sets the name of the pellet to the number of the pellet for easier collision detection (SEE PLAYER.CS)
			pellets[i].name = i.ToString();
			
			//Make outer membrane of pellets
			pelletShadows[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			pelletShadows[i].renderer.material.color = new Color(105f,50f,255f,0.5f);
			pelletShadows[i].renderer.material.shader = Shader.Find("Transparent/Diffuse");
			pelletShadows[i].transform.localScale = new Vector3(1f,1f,1f);
		}
		
		//sets up game variables
		score = 0;
		
		//Sets player name to login name
		pName =  GameObject.Find("PlayerName").GetComponent<GUIText>();
		pName.text = myName;
		
		//Sets title text on screen
		title =  GameObject.Find("Title").GetComponent<GUIText>();	
		title.fontSize = 50;
		
		//Sets up score on the screen
		scoreText =  GameObject.Find("Score").GetComponent<GUIText>();
		scoreText.text = "Score: " + score;
	}
	
	void OnApplicationQuit(){
		net.Disconnect();
	}
	
	void OnGUI(){
		//updates text when player scores
		scoreText.text = "Score: " + score;
	}
	
	void Update(){
		lock (net.data){
			if (net.data.Count > 0){
				currentData = (byte[])net.data.Dequeue();
				//response from server of client UN/PW sent
				if (currentData[0] == 0){
					//Was accepted
					if (currentData[1] == 1){
						int counter = 0;
						//gets pellet positions from server
						for (int i = 2; i <= 10; i+=2){
							pellets[i-(2+counter)].transform.position = new Vector3((float)(currentData[i]-2),(float)(currentData[i+1]-10),0);
							pelletShadows[i-(2+counter)].transform.position = pellets[i-(2+counter)].transform.position;
							counter++;
						}
					}
					
				}
			}
		}
		
	}
}
