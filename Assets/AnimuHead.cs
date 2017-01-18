using UnityEngine;
using System.Collections;

public abstract class AnimuHead : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
    {
	    
	}

    public abstract void PlaySound();  // abstract method to be overridden within characters' respective scripts
}