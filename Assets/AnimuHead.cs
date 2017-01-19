using UnityEngine;
using System.Collections;
using System;

public class AnimuHead : MonoBehaviour, SoundClip
{

    public new AudioSource audio;  // stores audio clip of gameobject that has this script attached to it
    public Vector3 audioPos;

	// Use this for initialization
	void Start ()
	{

	}

    void Awake()
    {
        audio = GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update ()
    {

	}
    
    public void PlaySound()
    {
        AudioSource.PlayClipAtPoint(audio.clip, audioPos);
    }
}