using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MisakaMikoto : AnimuHead {

    new AudioSource audio;

    // Use this for initialization
    void Start () {
        audio = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void PlaySound()
    {
        audio.Play();
    }
}
