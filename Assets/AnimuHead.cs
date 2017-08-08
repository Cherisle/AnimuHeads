using UnityEngine;
using System.Collections;
using System;

public class AnimuHead : MonoBehaviour, SoundClip
{
	private const float fallingSpeed = 2f;
	private const int HEAD_MAX = 8;
	public GameObject boundary;
	public GameBoundary gb;
	public bool isFalling;
	public int shiftRow;
	public int shiftColumn;
	private Vector3 newPos;
	public new AudioSource audio;  // stores audio clip of gameobject that has this script attached to it
    public Vector3 audioPos;  // used for playing clip at wherever this location is


	void Awake()
	{
		audio = GetComponent<AudioSource>();  // gets the audio source from the gameobject that has this script
	}

	void Start()
	{
		isFalling = false;
		shiftRow = 100;
		shiftColumn = 100;
		boundary = GameObject.Find("Boundary");
		gb = (GameBoundary)boundary.GetComponent<GameBoundary>();
		newPos = new Vector3(0,0,0);
	}

	public void PlaySound()
	{
		AudioSource.PlayClipAtPoint(audio.clip, audioPos);  // plays audio clip
	}

	public IEnumerator ComboFall(float time)
	{
		yield return new WaitForSeconds(time);
		if(isFalling == true)
		{
			Debug.Log("Position Update: (" + gb.gameGrid[shiftRow,shiftColumn].transform.position.x + "," 
				+ gb.gameGrid[shiftRow,shiftColumn].transform.position.y + ")");
			Debug.Log("Goal: (" + (shiftColumn*2 - 10) + "," + ((shiftRow+1)*-2 +10) + ")");
			newPos = Vector3.MoveTowards(gb.gameGrid[shiftRow,shiftColumn].transform.position,new Vector3(shiftColumn*2 - 10,(shiftRow+1)*-2 +10,0), Time.deltaTime*fallingSpeed);
			if(gb.gameGrid[shiftRow,shiftColumn].transform.position != newPos)
			{
				gb.gameGrid[shiftRow,shiftColumn].transform.position = newPos;
			}
			else
			{
				Debug.Log("entered here");
				isFalling = false;
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