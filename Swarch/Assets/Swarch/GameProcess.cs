using UnityEngine;
using System.Collections;

public class GameProcess : MonoBehaviour {
	public GameObject []pellets = new GameObject[5];
	public int score;
	
	string myName = LoginScript.userName;
	GUIText pName,title,scoreText;
	
	void Start () {
		
		
		//create pellets
		for(int i = 0; i < 5; i++){
			pellets[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
			pellets[i].renderer.material.color = new Color(255f,255f,255f,1);
			pellets[i].transform.localScale = new Vector3(0.5f,0.5f,0.5f);
			
			//sets the name of the pellet to the number of the pellet for easier collision detection (SEE PLAYER.CS)
			pellets[i].name = i.ToString();
			pellets[i].transform.position = new Vector3(Random.Range (2,25),Random.Range (-18,18),0);
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
	
	void OnGUI(){
		//updates text when player scores
		scoreText.text = "Score: " + score;
	}
}
