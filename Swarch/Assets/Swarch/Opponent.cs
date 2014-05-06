//Steven Kawafuchi
//Tamara Sahagun
//ICS 168 Multi-Player Game Project

using UnityEngine;
using System.Collections;

public class Opponent : MonoBehaviour {
	public int xdir, ydir, radius, score;
	public string pName;
	public int pNum;
	Player mainP;
	
	void Start(){
		mainP = GameObject.Find("Player").GetComponent<Player>();
		renderer.material.shader = Shader.Find("Transparent/Diffuse");
		transform.GetChild(0).renderer.material.shader = Shader.Find("Transparent/Diffuse");
		
		renderer.material.color = new Color(255f,0f,0f,0.5f);
		transform.GetChild(0).renderer.material.color = new Color(255f,0f,0f,0.5f);
	}
		
	void FixedUpdate () {
		if (mainP.radius > radius){
			renderer.material.color = new Color(0f,255f,0f,0.5f);
			transform.GetChild(0).renderer.material.color = new Color(0f,255f,0f,0.5f);
		}else{
			renderer.material.color = new Color(255f,0f,0f,0.5f);
			transform.GetChild(0).renderer.material.color = new Color(255f,0f,0f,0.5f);
			
		}
		//movement (speed determined by size)
		transform.Translate (new Vector3(xdir*0.2f * Mathf.Pow(0.75f,(float)radius),ydir*0.2f* Mathf.Pow(0.75f,(float)radius),0));
	}				
}