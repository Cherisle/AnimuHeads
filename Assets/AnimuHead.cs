using UnityEngine;
using System.Collections;
using System;

public class AnimuHead : MonoBehaviour, SoundClip
{
    public new AudioSource audio;  // stores audio clip of gameobject that has this script attached to it
    public Vector3 audioPos;  // used for playing clip at wherever this location is
	void Awake()
	{
		audio = GetComponent<AudioSource>();  // gets the audio source from the gameobject that has this script
	}
	public void PlaySound()
	{
		AudioSource.PlayClipAtPoint(audio.clip, audioPos);  // plays audio clip
	}
}