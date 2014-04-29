//Steven Kawafuchi
//Tamara Sahagun
//ICS 168 Multi-Player Game Project

using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	int xdir, ydir;
	float size;
	GameProcess process;
	public static float slideX, slideY;
	public static Vector3 slideCenter;
	
	void Start () {
		process = GameObject.Find("GameProcess").GetComponent<GameProcess>();
		size = 0.2f;	
		transform.position = new Vector3(Random.Range (2,25),Random.Range (-18,18),0);
		
		slideCenter = GameObject.Find("MicroscopeSlide").transform.renderer.bounds.center;
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
		
		if(collision.gameObject.name == "Cylinder")
		{
			print ("Touched edge!");
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
				
	public static bool IsThisPointWithinBounds(Vector3 vector)
	{
		if((Mathf.Pow((vector.x - slideX),2f) + Mathf.Pow((vector.y - slideY),2f)) < 400f)
			return true;
		return false;
	}
}
