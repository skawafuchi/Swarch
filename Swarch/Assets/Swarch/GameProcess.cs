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
	Player myP;
	
	public int pNum;
	string myName = LoginScript.userName;
	GUIText pName,title,scoreText;
	byte[]currentData;
	
	void Start () {
		pNum = -1;
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
		
		myP = GameObject.Find("Player").GetComponent<Player>();
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
	
    public static byte[] toByteArray(float value){
        byte[] bytes =System.BitConverter.GetBytes(value);
        return bytes;
    }

    public static float toFloat(byte[] bytes){
        return System.BitConverter.ToSingle(bytes, 0);
    }
	
	void Update(){
		lock (net.data){
			if (net.data.Count > 0){
				currentData = (byte[])net.data.Dequeue();
				//response from server of client UN/PW sent
				if (currentData[0] == 0){
					//Was accepted
					if (currentData[1] == 1){
						pNum = currentData[2];			

						//gets pellet positions from server
						int counter = 0;
						for (int i = 3; i <= 11; i+=2){
							pellets[i-(3+counter)].transform.position = new Vector3((float)(currentData[i]-2),(float)(currentData[i+1]-10),0);
							pelletShadows[i-(3+counter)].transform.position = pellets[i-(3+counter)].transform.position;
							counter++;
							print ("NEW POINT: ");
						}
						byte[] posCoord = new byte[4];
						System.Buffer.BlockCopy(currentData,13,posCoord,0,4);

						float x = toFloat (posCoord);			
						System.Buffer.BlockCopy(currentData,17,posCoord,0,4);
						float y = toFloat (posCoord);
						myP.transform.position = new Vector3(x,y,0);
					}
					
				//Move Command from the player
				}else if (currentData[0] == 1){
					if (currentData[2] == 0){
						myP.xdir = 0;
						myP.ydir = 1;
					}else if (currentData[2] == 1){
						myP.xdir = 0;
						myP.ydir = -1;
					}else if (currentData[2] == 2){
						myP.xdir = -1;
						myP.ydir = 0;
					}else if (currentData[2] == 3){
						myP.xdir = 1;
						myP.ydir = 0;
					}
					byte[] posCoord = new byte[4];
					System.Buffer.BlockCopy(currentData,3,posCoord,0,4);

					float x = toFloat (posCoord);			
					System.Buffer.BlockCopy(currentData,7,posCoord,0,4);
					float y = toFloat (posCoord);
					myP.transform.position = new Vector3(x,y,0);
					
				//Player Died Command
				}else if (currentData[0] == 2){
					
					byte[] posCoord = new byte[4];
					System.Buffer.BlockCopy(currentData,2,posCoord,0,4);
					float x = toFloat (posCoord);			
					System.Buffer.BlockCopy(currentData,6,posCoord,0,4);
					float y = toFloat (posCoord);
					myP.transform.position = new Vector3(x,y,0);
				}
			}
		}
		
	}
}
