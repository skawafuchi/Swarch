//Steven Kawafuchi
//Tamara Sahagun
//ICS 168 Multi-Player Game Project

using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	public int xdir, ydir, radius;
	GameProcess process;
	myNetwork net;
	public static float slideX, slideY;
	public static Vector3 slideCenter;
	byte[] toSend = new byte[50];
	void Start () {
		process = GameObject.Find("GameProcess").GetComponent<GameProcess>();
		net = LoginScript.net;
		
		transform.position = new Vector3(Random.Range (-2,18),Random.Range (-10,10),0);
		
		slideCenter = GameObject.Find("MicroscopeSlide").transform.renderer.bounds.center;
		slideX = GameObject.Find("MicroscopeSlide").transform.renderer.bounds.center.x;

		slideY = GameObject.Find("MicroscopeSlide").transform.renderer.bounds.center.y;
	} 
	
	void FixedUpdate () {
		//print ("");
		//movement (speed determined by size)
		//transform.Translate(new Vector3(xdir*size,ydir*size,0));
		transform.Translate (new Vector3(xdir*0.2f * Mathf.Pow(0.75f,(float)radius),ydir*0.2f* Mathf.Pow(0.75f,(float)radius),0));
		//changes direction of player based on button they pressed
		if (process.pNum != -1){
			toSend = new byte[50];
			if (Input.GetButton("UP") && !(xdir == 0 && ydir == 1)){	
				//xdir = 0;
				//ydir = 1;
						
				toSend[0] = (byte)1;
				toSend[1] = (byte)process.pNum;
				toSend[2] = (byte) 0;			
				net.SendTCPPacket(toSend,0,50);
			}else if (Input.GetButton("DOWN")){
				//xdir = 0;
				//ydir = -1;
				
				toSend[0] = (byte)1;
				toSend[1] = (byte)process.pNum;
				toSend[2] = (byte) 1;		
				net.SendTCPPacket(toSend,0,50);
			}else if (Input.GetButton("LEFT")){
				//xdir = -1;
				//ydir = 0;
				
				toSend[0] = (byte)1;
				toSend[1] = (byte)process.pNum;
				toSend[2] = (byte) 2;				
				net.SendTCPPacket(toSend,0,50);
			}else if (Input.GetButton("RIGHT")){
				//xdir = 1;
				//ydir = 0;
				
				toSend[0] = (byte)1;
				toSend[1] = (byte)process.pNum;
				toSend[2] = (byte) 3;	
				net.SendTCPPacket(toSend,0,50);
			}
		}
	}
	
//	void OnCollisionEnter(Collision collision) {
//		if(collision.gameObject.name == process.pellets[0].name || collision.gameObject.name == process.pellets[1].name ||
//			collision.gameObject.name == process.pellets[2].name || collision.gameObject.name == process.pellets[3].name ||
//			collision.gameObject.name == process.pellets[4].name)
//		{
//		//collision with the pellets
//			//update score
//			process.score++;
//			
//			//change size (which in turn changes speed)
//			size *= 0.75f;
//			float width = transform.localScale.x;
//			transform.localScale = new Vector3(width*1.2f,width*1.2f,0);
//			
//			//Moves pellet that was hit to a new position
//			process.pellets[int.Parse(collision.gameObject.name)].transform.position = new Vector3(Random.Range (-2,18),Random.Range (-10,10),0);
//			while(!IsThisPointWithinBounds(process.pellets[int.Parse(collision.gameObject.name)].transform.position))
//			{
//				print ("Generating new position");
//				process.pellets[int.Parse(collision.gameObject.name)].transform.position = new Vector3(Random.Range (-2,18),Random.Range (-10,10),0);
//			}
//			process.pelletShadows[int.Parse(collision.gameObject.name)].transform.position = process.pellets[int.Parse(collision.gameObject.name)].transform.position;
//		}
//		
//		if(collision.gameObject.name == "Cylinder")
//		{
//			print ("Touched edge!");
//			process.score = 0;
//			transform.position = new Vector3(Random.Range (-2,18),Random.Range (-10,10),0);
//			while(!IsThisPointWithinBounds(transform.position))
//			{
//				transform.position = new Vector3(Random.Range (-2,18),Random.Range (-10,10),0);
//			}
//			size = 0.20f;
//			transform.localScale = new Vector3(1,1,1);
//		}
//	}
				
	public static bool IsThisPointWithinBounds(Vector3 vector)
	{
		if((Mathf.Pow((vector.x - slideX),2f) + Mathf.Pow((vector.y - slideY),2f)) < 400f)
			return true;
		return false;
	}
}
