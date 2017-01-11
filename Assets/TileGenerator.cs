﻿using UnityEngine;
using System.Collections;
using System.Linq;

public class TileGenerator : MonoBehaviour
{
	public Object[] myPrefabs;
	public GameObject[,] array;
	private GameObject genLocation;
	private float xLoc;
	private int colControl;
	private GameObject go;
	private GameObject rowZeroClone;
	private GameObject goBelow;
	private GameObject goCurrent;
	private int fallCounter;
	// Use this for initialization
	void Start ()
	{
		fallCounter = 0; // default fall counter, 0 means at the very top row, e.g. row 0
		colControl = 5; // default row value for generator
		xLoc = colControl*2f-9; // tile x location
		myPrefabs = Resources.LoadAll ("Characters", typeof(GameObject)).Cast<GameObject> ().ToArray ();
		CreatePrefab();
	}
	void CreatePrefab()
	{
		colControl = 5; // resetting purposes
		go = (GameObject) myPrefabs[RandomNumber()]; //randomly generated GameObject "go"
		rowZeroClone = Instantiate(go,new Vector2(xLoc,fallCounter*-2+9),Quaternion.identity) as GameObject;
		InvokeRepeating ("Falling", 0.6f, 0.6f);
	}
	void Falling()
	{
		if (fallCounter < 9)
		{	
			if (transform.parent.GetComponent<GameBoundary> ().array2D [colControl, fallCounter + 1].GetComponent<AnimuHead>() != null) // AnimuHead below exists
			{
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
				goCurrent = goBelow; //goBelow was previous below, now's current
				if (fallCounter == 0)
				{
					Destroy (rowZeroClone); //specific for only the first generated of each random AnimuHead
				}
				if (fallCounter == 8) // final iteration of THIS else loop
				{
					transform.parent.GetComponent<GameBoundary>().array2D[colControl,fallCounter+1] = Instantiate(go,new Vector2(xLoc,(fallCounter+1)*-2+9), Quaternion.identity) as GameObject;
				}
				else
				{
					// instantiate GameObject tile below current tile
					goBelow = Instantiate (go, new Vector2 (xLoc, (fallCounter + 1) * -2 + 9), Quaternion.identity) as GameObject;
				}
				// destroys current tile to prepare for new instantiation
				Destroy(goCurrent);
				//--------------------------------------------------------------------------------------------------------------------------
				// instantiate current tile to default (transparent)
				goCurrent = Instantiate (Resources.Load ("Default/DefaultTile"), new Vector2 (xLoc, fallCounter*-2+9), Quaternion.identity) as GameObject;
				//--------------------------------------------------------------------------------------------------------------------------

				fallCounter++;
			}
		} 
		else
		{
			goCurrent = goBelow; //goBelow was previous below, now's current
			// destroys current tile to prepare for new instantiation
			Destroy(goCurrent);
			//--------------------------------------------------------------------------------------------------------------------------
			// instantiate current tile to default (transparent)
			goCurrent = Instantiate (Resources.Load ("Default/DefaultTile"), new Vector2 (xLoc, fallCounter*-2+9), Quaternion.identity) as GameObject;
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
		/*if (Input.GetKeyDown (KeyCode.LeftArrow) && colControl > 0)
		{
			colControl = colControl - 1;
		}
		if (Input.GetKeyDown (KeyCode.RightArrow) && colControl < 9)
		{
			colControl = colControl + 1;
		}*/
	}		
}