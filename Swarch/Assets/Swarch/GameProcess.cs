//Steven Kawafuchi
//Tamara Sahagun
//ICS 168 Multi-Player Game Project

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class GameProcess : MonoBehaviour {	
	public GameObject prefab;
	
	public GameObject []pellets = new GameObject[5];
	public GameObject []pelletShadows = new GameObject[5];
	public int score;
	myNetwork net;
	Player myP;

	public Dictionary<int,GameObject> opponents;
	
	public int pNum;
	string myName = LoginScript.userName;
	GUIText pName,title,scoreText,scoreBoardTitle,scoreBoard;
	byte[]currentData;
	
	void Start () {
		opponents = new Dictionary<int, GameObject>();
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
			//pelletShadows[i]
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
		
		//Sets up score title on sideline
		scoreBoardTitle =  GameObject.Find("ScoreBoardTitle").GetComponent<GUIText>();
		scoreBoardTitle.fontSize = 25;
		scoreBoardTitle.text = "Score Board";
		
		scoreBoard =  GameObject.Find("ScoreBoard").GetComponent<GUIText>();
	}
	
	void OnApplicationQuit(){
		net.Disconnect();
	}
	
	void OnGUI(){
		//updates text when player scores
		scoreText.text = "Score: " + score;
		
		string temp = "";
		temp += "Your Score: " + score + "\n";
		foreach (int key in opponents.Keys){
			temp += opponents[key].GetComponent<Opponent>().pName + ": " + opponents[key].GetComponent<Opponent>().score + "\n";
		}
		scoreBoard.text = temp;
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
				try{
					if (currentData[0] == 0){
						//Denied
						
						//Was accepted
						if (currentData[1] == 1){
							pNum = currentData[2];
	
							//gets pellet positions from server
							int counter = 0;
							for (int i = 3; i <= 11; i+=2){
								pellets[i-(3+counter)].transform.position = new Vector3((float)(currentData[i]-2),(float)(currentData[i+1]-10),0);
								pelletShadows[i-(3+counter)].transform.position = pellets[i-(3+counter)].transform.position;
								counter++;
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
						if (currentData[1] == pNum){
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
							
						}else{
							if (currentData[2] == 0){
								opponents[currentData[1]].GetComponent<Opponent>().xdir = 0;
								opponents[currentData[1]].GetComponent<Opponent>().ydir = 1;
							}else if (currentData[2] == 1){
								opponents[currentData[1]].GetComponent<Opponent>().xdir = 0;
								opponents[currentData[1]].GetComponent<Opponent>().ydir = -1;
							}else if (currentData[2] == 2){
								opponents[currentData[1]].GetComponent<Opponent>().xdir = -1;
								opponents[currentData[1]].GetComponent<Opponent>().ydir = 0;
							}else if (currentData[2] == 3){
								opponents[currentData[1]].GetComponent<Opponent>().xdir = 1;
								opponents[currentData[1]].GetComponent<Opponent>().ydir = 0;
							}
							byte[] posCoord = new byte[4];
							System.Buffer.BlockCopy(currentData,3,posCoord,0,4);
		
							float x = toFloat (posCoord);			
							System.Buffer.BlockCopy(currentData,7,posCoord,0,4);
							float y = toFloat (posCoord);
							opponents[currentData[1]].GetComponent<Opponent>().transform.position = new Vector3(x,y,0);
						}
						
					//Player Died Command
					}else if (currentData[0] == 2){
						
						byte[] posCoord = new byte[4];
						System.Buffer.BlockCopy(currentData,2,posCoord,0,4);
						float x = toFloat (posCoord);			
						System.Buffer.BlockCopy(currentData,6,posCoord,0,4);
						float y = toFloat (posCoord);
						if (currentData[1] == pNum){
							myP.transform.position = new Vector3(x,y,0);
							myP.transform.localScale = new Vector3(1,1,1);
							myP.radius = 0;
							score = 0;
						}else{
							opponents[currentData[1]].GetComponent<Opponent>().transform.position=new Vector3(x,y,0);
							opponents[currentData[1]].GetComponent<Opponent>().transform.localScale = new Vector3(1,1,1);
							opponents[currentData[1]].GetComponent<Opponent>().radius = 0;
							opponents[currentData[1]].GetComponent<Opponent>().score = 0;
							
						}
					//Player ate pellet command
					}else if (currentData[0] == 3){
						
						if (currentData[1] == pNum){
							score = currentData[2];
							myP.radius = (int) currentData[3];
							myP.transform.localScale = new Vector3((Mathf.Pow(1.2f,myP.radius)),(Mathf.Pow(1.2f,myP.radius)),0);
						} else{
							opponents[currentData[1]].GetComponent<Opponent>().score = currentData[2];
							opponents[currentData[1]].GetComponent<Opponent>().radius = (int) currentData[3];
							opponents[currentData[1]].GetComponent<Opponent>().transform.localScale = new Vector3((Mathf.Pow(1.2f,opponents[currentData[1]].GetComponent<Opponent>().radius)),(Mathf.Pow(1.2f,opponents[currentData[1]].GetComponent<Opponent>().radius)),0);
						
						}
						
						//gets pellet positions from server
						int counter = 0;
						for (int i = 4; i <= 12; i+=2){
							
							pellets[i-(4+counter)].transform.position = new Vector3((float)(currentData[i]-2),(float)(currentData[i+1]-10),0);
							pelletShadows[i-(4+counter)].transform.position = pellets[i-(4+counter)].transform.position;
							counter++;
	
						}
					//Player connected
					}else if (currentData[0] == 4){
						//Ignore if data is about self
						try{
							if ((int)currentData[1] != pNum && pNum != -1){
								opponents.Add(currentData[1], Instantiate(prefab) as GameObject);
								opponents[currentData[1]].GetComponent<Opponent>().transform.localScale = new Vector3(1,1,1);
									
								byte[] posCoord = new byte[4];
								System.Buffer.BlockCopy(currentData,2,posCoord,0,4);
			
								float x = toFloat (posCoord);			
								System.Buffer.BlockCopy(currentData,6,posCoord,0,4);
								float y = toFloat (posCoord);
								opponents[currentData[1]].GetComponent<Opponent>().transform.position= new Vector3(x,y,0);
								
								byte[] name = new byte[currentData[10]];
								System.Buffer.BlockCopy(currentData, 11, name, 0,name.Length);
								opponents[currentData[1]].GetComponent<Opponent>().pName = Encoding.ASCII.GetString(name);
								
							}
						}catch{
								
						}
					//Player disconnected	
					}else if (currentData[0] == 5){
						Destroy(opponents[currentData[1]]);
						opponents.Remove(currentData[1]);
							
					//Player ate player
					}else if (currentData[0] == 6){
						if (currentData[1] == pNum){
							score = currentData[2];
							myP.radius = currentData[3];	
							myP.transform.localScale = new Vector3((Mathf.Pow(1.2f,myP.radius)),(Mathf.Pow(1.2f,myP.radius)),0);
						}else{
							opponents[currentData[1]].GetComponent<Opponent>().score = currentData[2];
							opponents[currentData[1]].GetComponent<Opponent>().radius = currentData[3];
							opponents[currentData[1]].GetComponent<Opponent>().transform.localScale = new Vector3((Mathf.Pow(1.2f,opponents[currentData[1]].GetComponent<Opponent>().radius)),(Mathf.Pow(1.2f,opponents[currentData[1]].GetComponent<Opponent>().radius)),0);
						
							
						}
					//Game state
					}else if (currentData[0] == 7){
						
						for (int i = 2; i <= currentData[1]*12; i+= 12){
							
							//ignore information about self
							if (currentData[i] != pNum){
								opponents.Add(currentData[i], Instantiate(prefab) as GameObject);
																
								byte[] posCoord = new byte[4];
								System.Buffer.BlockCopy(currentData,i+4,posCoord,0,4);
								float x = toFloat (posCoord);			
								System.Buffer.BlockCopy(currentData,i+8,posCoord,0,4);
								float y = toFloat (posCoord);
								opponents[currentData[i]].GetComponent<Opponent>().transform.position = new Vector3(x,y,0);
								opponents[currentData[i]].GetComponent<Opponent>().pNum = currentData[i];
									
								opponents[currentData[i]].GetComponent<Opponent>().score = currentData[i+1];
	
								opponents[currentData[i]].GetComponent<Opponent>().radius = currentData[i+2];
								opponents[currentData[i]].GetComponent<Opponent>().transform.localScale = new Vector3((Mathf.Pow(1.2f,opponents[currentData[i]].GetComponent<Opponent>().radius)),(Mathf.Pow(1.2f,opponents[currentData[i]].GetComponent<Opponent>().radius)),0);
															
								if (currentData[i+3] == 0){
									opponents[currentData[i]].GetComponent<Opponent>().xdir = 0;
									opponents[currentData[i]].GetComponent<Opponent>().ydir = 1;
								}else if (currentData[i+3] == 1){
									opponents[currentData[i]].GetComponent<Opponent>().xdir = 0;
									opponents[currentData[i]].GetComponent<Opponent>().ydir = -1;
								}else if (currentData[i+3] == 2){
									opponents[currentData[i]].GetComponent<Opponent>().xdir = -1;
									opponents[currentData[i]].GetComponent<Opponent>().ydir = 0;
								}else if (currentData[i+3] == 3){
									opponents[currentData[i]].GetComponent<Opponent>().xdir = 1;
									opponents[currentData[i]].GetComponent<Opponent>().ydir = 0;
								}
							}
						}
										
					}else if (currentData[0] == 8){
							
						//ignore self information
						if (currentData[1] != pNum){
							byte[] userName = new byte[currentData[2]];
							System.Buffer.BlockCopy(currentData, 3, userName, 0, currentData[2]);
							string strUN = Encoding.ASCII.GetString(userName);
							opponents[currentData[1]].GetComponent<Opponent>().pName = strUN;
							print ("OPPONENTS NAME IS " + strUN);
								
						}
							
					}
				}catch{}
			}
		}
	}
}