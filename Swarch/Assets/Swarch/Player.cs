using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	int xdir, ydir;
	float size;
	GameProcess process;
	Vector3[] colliderVertices = new Vector3[8]; 
	public static float slideX, slideY;
	
	void Start () {
		process = GameObject.Find("GameProcess").GetComponent<GameProcess>();
		size = 0.2f;	
		transform.position = new Vector3(Random.Range (2,25),Random.Range (-18,18),0);
		
		colliderVertices[0] = collider.bounds.min;
		colliderVertices[1] = collider.bounds.max;
		colliderVertices[2] = new Vector3(colliderVertices[0].x, colliderVertices[0].y, colliderVertices[1].z);
		colliderVertices[3] = new Vector3(colliderVertices[0].x, colliderVertices[1].y, colliderVertices[0].z);
		colliderVertices[4] = new Vector3(colliderVertices[1].x, colliderVertices[0].y, colliderVertices[0].z);
		colliderVertices[5] = new Vector3(colliderVertices[0].x, colliderVertices[1].y, colliderVertices[1].z);
		colliderVertices[6] = new Vector3(colliderVertices[1].x, colliderVertices[0].y, colliderVertices[1].z);
		colliderVertices[7] = new Vector3(colliderVertices[1].x, colliderVertices[1].y, colliderVertices[0].z);
		
		slideX = GameObject.Find("MicroscopeSlide").transform.renderer.bounds.center.x;
		slideY = GameObject.Find("MicroscopeSlide").transform.renderer.bounds.center.y;
	}
	
	// Update is called once per frame
	void Update () {
		//movement (speed determined by size)
		transform.Translate(new Vector3(xdir*size,ydir*size,0));
		
		//changes direction of player based on button they pressed
		if (Input.GetButton("UP")){	
			xdir = 0;
			ydir = 1;
		}else if (Input.GetButton("DOWN")){
			xdir = 0;
			ydir = -1;
		}else if (Input.GetButton("LEFT")){
			xdir = -1;
			ydir = 0;
		}else if (Input.GetButton("RIGHT")){
			xdir = 1;
			ydir = 0;
		}	
	}
	
	//Currently checks if player has completely exited the circular bounds of the slide; 
	//need to find a way to check if a point of the player's Box Collider is not contained
	//within the slide's Capsule Collider.
	void OnTriggerExit(Collider otherCollider)
	{
		if(otherCollider.gameObject.name == "MicroscopeSlide")
		{
			print("No longer in contact with " + otherCollider.transform.name);
			process.score = 0;
			transform.position = new Vector3(Random.Range (-2,18),Random.Range (-10,10),0);
			while(!IsThisPointWithinBounds(transform.position))
			{
				transform.position = new Vector3(Random.Range (-2,18),Random.Range (-10,10),0);
			}
			size = 0.20f;
			transform.localScale = new Vector3(1,1,1);
		}
	}
	
//	void OnTriggerStay(Collider otherCollider)
//	{
//		if(otherCollider.gameObject.name == "MicroscopeSlide")
//		{
//			
//			for(int i = 0; i < 8; i++)
//			{
//				if(!IsThisPointWithinBounds(colliderVertices[i]))
//				{
//					print("Math rules!");
//					process.score = 0;
//					transform.position = new Vector3(Random.Range (-2,18),Random.Range (-10,10),0);
//					size = 0.20f;
//					transform.localScale = new Vector3(1,1,1);
//				}
//			}
//		}
//	}
	
	void OnCollisionEnter(Collision collision) {
		if(collision.gameObject.name == process.pellets[0].name || collision.gameObject.name == process.pellets[1].name ||
			collision.gameObject.name == process.pellets[2].name || collision.gameObject.name == process.pellets[3].name ||
			collision.gameObject.name == process.pellets[4].name)
		{
		//collision with the pellets
			//update score
			process.score++;
			
			//change size (which in turn changes speed)
			//NEEDS TO BE CHANGED
			size *= 0.75f;
			float width = transform.localScale.x;
			transform.localScale = new Vector3(width*1.2f,width*1.2f,0);
			
			//Moves pellet that was hit to a new position
			process.pellets[int.Parse(collision.gameObject.name)].transform.position = new Vector3(Random.Range (-2,18),Random.Range (-10,10),0);
			while(!IsThisPointWithinBounds(process.pellets[int.Parse(collision.gameObject.name)].transform.position))
			{
				print ("Generating new position");
				process.pellets[int.Parse(collision.gameObject.name)].transform.position = new Vector3(Random.Range (-2,18),Random.Range (-10,10),0);
			}
			process.pelletShadows[int.Parse(collision.gameObject.name)].transform.position = process.pellets[int.Parse(collision.gameObject.name)].transform.position;
		}
	}
				
	public static bool IsThisPointWithinBounds(Vector3 vector)
	{
		if((Mathf.Pow((vector.x - slideX),2f) + Mathf.Pow((vector.y - slideY),2f)) < 400)
			return true;
		return false;
	}
}
