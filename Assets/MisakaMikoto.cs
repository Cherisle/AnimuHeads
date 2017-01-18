using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MisakaMikoto : AnimuHead {

    new AudioSource audio;  // initialize variable to hold audio clip

    // Use this for initialization
    void Start () {
        audio = GetComponent<AudioSource>();  //gets the audio clip attached to the Prefab that has this script
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void PlaySound()
    {
        audio.Play();  //plays whatever audio clip is attached to the Prefab that has this script
    }
}
