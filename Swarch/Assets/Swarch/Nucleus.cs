//Steven Kawafuchi
//Tamara Sahagun
//ICS 168 Multi-Player Game Project

using UnityEngine;
using System.Collections;

public class Nucleus : MonoBehaviour
{
	void Start ()
	{
		transform.localPosition = new Vector3(-0.08f,0.05f,0f); //I like the nucleus off-center
	}
	
	void Update ()
	{
		transform.localPosition = new Vector3(-0.08f,0.05f,0f);
	}
}