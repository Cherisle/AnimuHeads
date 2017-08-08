using UnityEngine;
using System.Collections;
using System;

public class AnimuHead : MonoBehaviour, SoundClip
{
	private const float fallingSpeed = 2f;
	public GameObject boundary;
	public GameBoundary gb;
	public bool isFalling {get;set;}
	public int shiftRow {get;set;}
	public int shiftColumn {get;set;}
	public new AudioSource audio;  // stores audio clip of gameobject that has this script attached to it
    public Vector3 audioPos;  // used for playing clip at wherever this location is


	void Awake()
	{
		audio = GetComponent<AudioSource>();  // gets the audio source from the gameobject that has this script
	}

	void Start()
	{
		shiftRow = 100;
		shiftColumn = 100;
		boundary = GameObject.Find("Boundary");
		gb = (GameBoundary)boundary.GetComponent<GameBoundary>();
	}

	public void PlaySound()
	{
		AudioSource.PlayClipAtPoint(audio.clip, audioPos);  // plays audio clip
	}

	public IEnumerator ComboFall(float time)
	{
		yield return new WaitForSeconds(time);
		if(gb.gameGrid[shiftRow,shiftColumn].gameObject.GetComponent<AnimuHead>().isFalling == true)
		{
			Vector3 newPos = Vector3.MoveTowards(gb.gameGrid[shiftRow,shiftColumn].transform.position,new Vector3(shiftColumn*2 - 10,(shiftRow+1)*-2 +10,0), Time.deltaTime*fallingSpeed);
			if(gb.gameGrid[shiftRow,shiftColumn].transform.position != newPos)
			{
				gb.gameGrid[shiftRow,shiftColumn].transform.position = newPos;
			}
			else
			{
				gb.gameGrid[shiftRow,shiftColumn].gameObject.GetComponent<AnimuHead>().isFalling = false;
			}
		}
	}

	void Update()
	{
		if(shiftRow != 100 && shiftColumn != 100) // default is 100, prevents out of bound error
		{
			StopCoroutine(ComboFall(0.4f));
			StartCoroutine(ComboFall(0.4f));

		}
	}
}