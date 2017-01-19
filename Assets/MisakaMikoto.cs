using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MisakaMikoto : AnimuHead
{
    new AudioSource audio;  // initialize variable to hold audio clip

    void Start ()
	{
        audio = GetComponent<AudioSource>();  //gets the audio clip attached to the Prefab that has this script
	}

	void Update ()
	{
		
	}

    public void PlaySound()
    {
        audio.Play();  //plays whatever audio clip is attached to the Prefab that has this script
    }
}
