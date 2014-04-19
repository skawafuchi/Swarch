using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	int xdir, ydir;
	float size;
	GameProcess process;
	
	void Start () {
		process = GameObject.Find("GameProcess").GetComponent<GameProcess>();
		size = 0.2f;	
		transform.position = new Vector3(Random.Range (2,25),Random.Range (-18,18),0);
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
		
		//collision with walls reset score, player size, and position
		if (collision.gameObject.name=="LeftBound" ||
			collision.gameObject.name=="RightBound" ||
			collision.gameObject.name=="LowerBound" ||
			collision.gameObject.name=="UpperBound"){
			
			process.score = 0;
			transform.position = new Vector3(Random.Range (2,25),Random.Range (-18,18),0);
			size = 0.20f;
			transform.localScale = new Vector3(1,1,1);
			
		//collision with the pellets
		}else{
			//update score
			process.score++;
			
			//change size (which in turn changes speed)
			//NEEDS TO BE CHANGED
			size *= 0.75f;
			float width = transform.localScale.x;
			transform.localScale = new Vector3(width*1.2f,width*1.2f,0);
			
			//Moves pellet that was hit to a new position
			process.pellets[int.Parse(collision.gameObject.name)].transform.position = new Vector3(Random.Range (2,25),Random.Range (-18,18),0);
			
		}
    }
}
