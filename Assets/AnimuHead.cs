using UnityEngine;
using System.Collections;
using System;

public class AnimuHead : MonoBehaviour, SoundClip
{
	private const float fallingSpeed = 2f;
	private const int HEAD_MAX = 8;
	public GameObject boundary;
	private GameObject copyObject;
	private GameObject movingObject;
	private GameObject tgObject;
	private TileGenerator tg;
	public GameBoundary gb;
	public bool isFalling;
	public int shiftRow;
	public int shiftColumn;
	private Vector2 destination;
	private Vector2 downMovement;
	private Vector2 startPosition;
	public new AudioSource[] audio;  // stores audio clip of gameobject that has this script attached to it
    public Vector3 audioPos;  // used for playing clip at wherever this location is


	void Awake()
	{
		audio = GetComponents<AudioSource>();  // gets the audio source from the gameobject that has this script
	}

	void Start()
	{
		isFalling = false;
		shiftRow = 100;
		shiftColumn = 100;
		boundary = GameObject.Find("Boundary");
		tgObject = boundary.transform.GetChild(0).gameObject;
		gb = (GameBoundary)boundary.GetComponent<GameBoundary>();
		tg = (TileGenerator)tgObject.GetComponent<TileGenerator>();
		destination = new Vector2(0,0);
		downMovement = new Vector2(0,0.1f); // remember to subtract this
		startPosition = new Vector2(0,0);
		copyObject = null;
		movingObject = null;
	}

	public void PlaySound(int clipSelector)
	{
		AudioSource.PlayClipAtPoint(audio[clipSelector].clip, audioPos);  // plays audio clip
	}

	public IEnumerator ComboFall(float time)
	{
		yield return new WaitForSeconds(time);
		if(isFalling == true)
		{
			/*Debug.Log("ShiftRow: " + shiftRow); // working
			Debug.Log("ShiftColumn: " + shiftColumn); // working
			Debug.Log("Position Update: (" + this.transform.position.x + "," 
				+ this.transform.position.y + ") : " + this.name); // issue here, name is wrong bc of issue
			Debug.Log("Goal: (" + (shiftColumn*2 - 10) + "," + ((shiftRow+1)*-2 +10) + ")");*/
			startPosition.Set(shiftColumn*2 - 10,shiftRow*-2 + 10);
			destination.Set(shiftColumn*2 - 10,(shiftRow+1)*-2 +10);
			copyObject = this.gameObject; // gets copy of gameGrid[shiftRow,shiftColumn]
			movingObject = this.gameObject; // gets copy of gameGrid[shiftRow,shiftColumn]
			//Destroy(gb.gameGrid[shiftRow,shiftColumn]); // destroy the image that (WAS) above
			while(startPosition != destination)
			{
				startPosition.Set(startPosition.x - 0, startPosition.y - downMovement.y); //adjust startPosition
				copyObject = movingObject; //retain above object
				Destroy(copyObject); // now destroy it
				movingObject = Instantiate(movingObject,new Vector2(startPosition.x,startPosition.y), Quaternion.identity) as GameObject;
				// ^ above creates the new "stamp" of the movingObject
                //movingObject.name = movingObject.name.Trim
                movingObject.name = movingObject.name.Substring(0, movingObject.name.Length - 7);
			}
			Debug.Log("if successful see this");

			gb.gameGrid[shiftRow+1,shiftColumn] = Instantiate(movingObject,new Vector2(startPosition.x,startPosition.y), Quaternion.identity) as GameObject;
            Debug.Log(this.gameObject.name);
			gb.gameGrid[shiftRow+1,shiftColumn].name = movingObject.name;
           
            gb.setID(shiftRow+1,shiftColumn,tg.goHeadNum);
			Destroy(this); // deletes the movement copy
			isFalling = false;
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

    public void PlaySound()
    {
        throw new NotImplementedException();
    }
}