//Steven Kawafuchi
//Tamara Sahagun
//ICS 168 Multi-Player Game Project

using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	public int xdir, ydir, radius;
	GameProcess process;
	myNetwork net;
	byte[] toSend = new byte[50];
	
	void Start () {
		process = GameObject.Find("GameProcess").GetComponent<GameProcess>();
		net = LoginScript.net;
	} 
	
	void FixedUpdate () {
		//movement (speed determined by size)
		transform.Translate (new Vector3(xdir*0.2f * Mathf.Pow(0.75f,(float)radius),ydir*0.2f* Mathf.Pow(0.75f,(float)radius),0));
		//changes direction of player based on button they pressed
		if (process.pNum != -1){
			toSend = new byte[50];
			if (Input.GetButton("UP")){	
				toSend[0] = (byte)1;
				toSend[1] = (byte)process.pNum;
				toSend[2] = (byte) 0;			
				net.SendTCPPacket(toSend,0,50);
			}else if (Input.GetButton("DOWN")){
				toSend[0] = (byte)1;
				toSend[1] = (byte)process.pNum;
				toSend[2] = (byte) 1;		
				net.SendTCPPacket(toSend,0,50);
			}else if (Input.GetButton("LEFT")){
				toSend[0] = (byte)1;
				toSend[1] = (byte)process.pNum;
				toSend[2] = (byte) 2;				
				net.SendTCPPacket(toSend,0,50);
			}else if (Input.GetButton("RIGHT")){
				toSend[0] = (byte)1;
				toSend[1] = (byte)process.pNum;
				toSend[2] = (byte) 3;	
				net.SendTCPPacket(toSend,0,50);
			}
		}
	}				
}