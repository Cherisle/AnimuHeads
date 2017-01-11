﻿using UnityEngine;
using System.Collections;
using System.Linq;

public class TileGenerator : MonoBehaviour
{
	public Object[] myPrefabs;
	private GameObject genLocation;
	private int colControl;
	private GameObject go;
	private GameObject rowZeroClone;
	private GameObject goBelow;
	private GameObject goCurrent;
	private GameObject goHorz;
	private int fallCounter;
	// Use this for initialization
	void Start ()
	{
		fallCounter = 0; // default fall counter, 0 means at the very top row, e.g. row 0
		colControl = 5; // default row value for generator
		myPrefabs = Resources.LoadAll ("Characters", typeof(GameObject)).Cast<GameObject> ().ToArray ();
		CreatePrefab();
	}
	void CreatePrefab()
	{
		colControl = 5; // resetting purposes
		go = (GameObject) myPrefabs[RandomNumber()]; //randomly generated GameObject "go"
		rowZeroClone = Instantiate(go,new Vector2(colControl*2f-9,fallCounter*-2+9),Quaternion.identity) as GameObject;
		InvokeRepeating ("Falling", 0.6f, 0.6f);
	}
	void Falling()
	{
		if (fallCounter < 9)
		{	
			if (transform.parent.GetComponent<GameBoundary> ().array2D [colControl, fallCounter + 1].GetComponent<AnimuHead>() != null) // AnimuHead below exists
			{
				Debug.Log("Detected AnimuHead at fall counter " + fallCounter);
				transform.parent.GetComponent<GameBoundary>().array2D[colControl,fallCounter] = Instantiate(go,new Vector2(colControl*2-9,fallCounter*-2+9), Quaternion.identity) as GameObject;
				goCurrent = goBelow; //goBelow was previous below, now's current
				Destroy(goCurrent);
				goCurrent = Instantiate (Resources.Load ("Default/DefaultTile"), new Vector2 (colControl*2-9, fallCounter*-2+9), Quaternion.identity) as GameObject;
				CancelInvoke ("Falling");
				if (fallCounter == 0)
				{
					//don't create new prefab
				}
				else
				{
					fallCounter = 0;
					CreatePrefab ();
				}
			}
			else // tile below is NOT an AnimuHead, then is DEFAULT
			{
				Debug.Log(fallCounter);
				goCurrent = goBelow; //goBelow was previous below, now's current
				if (fallCounter == 0 && rowZeroClone != null)
				{
					Destroy (rowZeroClone); //specific for only the first generated of each random AnimuHead
				}
				if (fallCounter == 8) // final iteration of THIS else loop
				{
					transform.parent.GetComponent<GameBoundary>().array2D[colControl,fallCounter+1] = Instantiate(go,new Vector2(colControl*2f-9,(fallCounter+1)*-2+9), Quaternion.identity) as GameObject;
				}
				else
				{
					// instantiate GameObject tile below current tile
					goBelow = Instantiate (go, new Vector2 (colControl*2f-9, (fallCounter + 1) * -2 + 9), Quaternion.identity) as GameObject;
				}
				// destroys current tile to prepare for new instantiation
				Destroy(goCurrent);
				//--------------------------------------------------------------------------------------------------------------------------
				// instantiate current tile to default (transparent)
				goCurrent = Instantiate (Resources.Load ("Default/DefaultTile"), new Vector2 (colControl*2f-9, fallCounter*-2+9), Quaternion.identity) as GameObject;
				//--------------------------------------------------------------------------------------------------------------------------

				fallCounter++;
			}
		} 
		else
		{
			Debug.Log("what happened here");
			goCurrent = goBelow; //goBelow was previous below, now's current
			// destroys current tile to prepare for new instantiation
			Destroy(goCurrent);
			//--------------------------------------------------------------------------------------------------------------------------
			// instantiate current tile to default (transparent)
			goCurrent = Instantiate (Resources.Load ("Default/DefaultTile"), new Vector2 (colControl*2f-9, fallCounter*-2+9), Quaternion.identity) as GameObject;
			//--------------------------------------------------------------------------------------------------------------------------
			CancelInvoke ("Falling");
			fallCounter = 0;
			CreatePrefab();
		}

	}
	int RandomNumber()
	{
		System.Random rand = new System.Random();
		return rand.Next(0,myPrefabs.Length);
	}
	void Update()
	{
		if (Input.GetKeyDown (KeyCode.LeftArrow) && colControl > 0)
		{
            //prevents overlapping
            if (transform.parent.GetComponent<GameBoundary>().array2D[colControl - 1, fallCounter].GetComponent<AnimuHead>() == null)
			{
				if (fallCounter == 0 && rowZeroClone != null)
				{
					Destroy (rowZeroClone); //specific for only the first generated of each random AnimuHead
				}
				Destroy(goBelow);
				goBelow = Instantiate (Resources.Load ("Default/DefaultTile"), new Vector2 (colControl*2f-9, fallCounter*-2+9), Quaternion.identity) as GameObject;
				colControl -= 1;
				goHorz = Instantiate(go,new Vector2(colControl*2f-9,fallCounter*-2+9),Quaternion.identity) as GameObject; // make left/right GameObject
				goBelow = goHorz; // make current Gameobject now left/right GameObject
			}
            else
			{
                Debug.Log("You hit an animu head on the left D:");  //debugging purposes, delete later
			}
        }
		if (Input.GetKeyDown (KeyCode.RightArrow) && colControl < 9)
		{
            //prevents overlapping
            if (transform.parent.GetComponent<GameBoundary>().array2D[colControl + 1, fallCounter].GetComponent<AnimuHead>() == null)
			{
				if (fallCounter == 0 && rowZeroClone != null)
				{
					Destroy (rowZeroClone); //specific for only the first generated of each random AnimuHead
				}
				Destroy(goBelow);
				goBelow = Instantiate (Resources.Load ("Default/DefaultTile"), new Vector2 (colControl*2f-9, fallCounter*-2+9), Quaternion.identity) as GameObject;
				colControl += 1;
				goHorz = Instantiate(go,new Vector2(colControl*2f-9,fallCounter*-2+9),Quaternion.identity) as GameObject;
				goBelow = goHorz;
			}
            else
			{
                Debug.Log("You hit an animu head on the right!!!");  //debugging purposes, delete later
			}
        }
	}		
}