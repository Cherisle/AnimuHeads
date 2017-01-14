﻿using UnityEngine;
using Object = UnityEngine.Object;
using System;
using System.Collections;
using System.Linq;

public class TileGenerator : MonoBehaviour
{
	private const int headMax = 8;
	public Object[] myPrefabs;
	private bool nameMatch;
	private int colNum;
	private int fallCounter;
	//-----------------------------------------------
	private GameObject genLocation;
	private GameObject go;
	private GameObject goBelow;
	private GameObject goCurrent;
	private GameObject goHorz;
	private GameObject rowZeroClone;
	//---Vars Below Here Used For Combo Algorithm----
	private string[] createdHeads;
	private int fpRow; //focal point row value
	private int fpCol; //focal point col value
	private int goGridCnt; // gameObject grid count
	private int goHeadNum; // gameObject head number -- used to represent character name
	private int numMatches; // number of matches detected for AnimuHead

	// Use this for initialization
	void Start ()
	{
		createdHeads = new string[headMax];
		fpRow = 0;
		fpCol = 0;
		goGridCnt = 0;
		goHeadNum = 8; // cannot be 0-7
		fallCounter = 0; // default fall counter, 0 means at the very top row, e.g. row 0
		nameMatch = false; // default value for boolean to check for matching gameObject names
		colNum = 5; // default row value for generator
		myPrefabs = Resources.LoadAll ("Characters", typeof(GameObject)).Cast<GameObject> ().ToArray ();
		CreatePrefab();
	}
	void CreatePrefab()
	{
		goHeadNum = 8;
		colNum = 5; // resetting purposes
		nameMatch = false; //resetting purposes
		go = (GameObject) myPrefabs[RandomNumber()]; //randomly generated GameObject "go"
		if(createdHeads.Count(s => s != null) == 0)
		{
			createdHeads[0] = go.name;
			goHeadNum = 0;
		}
		else // array contains previously existing charHead names
		{
			for(int ii=0;ii<createdHeads.Length;ii++) // loop to check through createdHeads name array, and set boolean value accordingly
			{
				if(go.name == createdHeads[ii])
				{
					nameMatch = true;
					goHeadNum = ii; 
					break;
				}
			}
			if(nameMatch == false)
			{
				createdHeads[createdHeads.Count(s => s != null)] = go.name; // adds new AnimuHead name into string array, previous DNE
				goHeadNum = createdHeads.Count(s => s != null) - 1;
			}
		}
		//Debug.Log("["+(colNum*2-9)+","+(fallCounter*-2+9)+"]");
		rowZeroClone = Instantiate(go,new Vector2(colNum*2-9,fallCounter*-2+9),Quaternion.identity) as GameObject;
		rowZeroClone.name = go.name;
		InvokeRepeating ("Falling", 0.6f, 0.6f);
	}
	void Falling()
	{
		if (fallCounter < 9)
		{	
			if (transform.parent.GetComponent<GameBoundary> ().gameGrid[fallCounter+1,colNum].GetComponent<AnimuHead>() != null) // AnimuHead below exists
			{
				Destroy(transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter,colNum]); // destroys stamped DefaultTile from initialization
				transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter,colNum] = Instantiate(go,new Vector2(colNum*2-9,fallCounter*-2+9), Quaternion.identity) as GameObject;
				transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter,colNum].name = go.name; //display proper AnimuHead name
				Debug.Log("GameGrid[" + fallCounter + "," + colNum + "] = " + transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter,colNum].name);
				transform.parent.GetComponent<GameBoundary>().identifier[fallCounter,colNum] = goHeadNum;
				transform.parent.GetComponent<GameBoundary>().idUpdate(fallCounter,colNum,goHeadNum);  
				goGridCnt++; // AnimuHead stamped on game grid, this line registers the AnimuHead count
				if(goGridCnt >=3)
				{
					fpRow = fallCounter;
					fpCol = colNum;
					//Debug.Log("Focal Point R,C is " + fpRow + "," + fpCol);
				}
				//Debug.Log ("AnimuHead Grid Count: " + goGridCnt);
				CancelInvoke ("Falling");
				if (fallCounter == 0)
				{
					if(rowZeroClone != null)
					{
						Destroy(rowZeroClone);
					}
					Debug.Log("Game should be over");
				}
				else
				{
					Destroy(goCurrent);
					Destroy(goBelow);
					fallCounter = 0;
					CreatePrefab ();
				}
			}
			else // tile below is NOT an AnimuHead, then is DEFAULT
			{
				goCurrent = goBelow; //goBelow was previous below, now's current
				if (rowZeroClone != null)
				{
					Destroy (rowZeroClone); //specific for only the first generated of each random AnimuHead
				}
				if (fallCounter == 8) // final iteration of THIS else loop
				{
					Destroy(transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter+1,colNum]); // destroys stamped DefaultTile from initialization
					transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter+1,colNum] = Instantiate(go,new Vector2(colNum*2-9,(fallCounter+1)*-2+9), Quaternion.identity) as GameObject;  
					transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter+1,colNum].name = go.name;
					Debug.Log("GameGrid[" + (fallCounter+1) + "," + colNum + "] = " + goHeadNum);
					transform.parent.GetComponent<GameBoundary>().identifier[fallCounter+1,colNum] = goHeadNum;
					transform.parent.GetComponent<GameBoundary>().idUpdate(fallCounter+1,colNum,goHeadNum); 
					goGridCnt++;
					if(goGridCnt >=3)
					{
						fpRow = fallCounter+1; // because we are at fallCounter == 8, but we stamped at fallcounter == 9 [above as fallCounter+1]
						fpCol = colNum;
						//Debug.Log("Focal Point R,C is " + fpRow + "," + fpCol);
						//row here is guaranteed to be 9 so...
						numMatches = transform.parent.GetComponent<GameBoundary>().CheckPillar(fpRow,fpCol);
					}
					//Debug.Log ("AnimuHead Grid Count: " + goGridCnt);
				}
				else
				{
					// instantiate GameObject tile below current tile
					goBelow = Instantiate (go,new Vector2(colNum*2-9,(fallCounter+1)*-2+9), Quaternion.identity) as GameObject;
				}
				// destroys current tile to prepare for new instantiation
				Destroy(goCurrent);
				//--------------------------------------------------------------------------------------------------------------------------
				// instantiate current tile to default (transparent)
				goCurrent = Instantiate (Resources.Load("Default/DefaultTile"),new Vector2(colNum*2-9,fallCounter*-2+9), Quaternion.identity) as GameObject;
				//--------------------------------------------------------------------------------------------------------------------------
				fallCounter++;
			}
		} 
		else //fallCounter >= 9
		{
			// destroys all illusion tiles to prepare for new instantiation
			Destroy(goCurrent);
			Destroy(goBelow);
			if (rowZeroClone != null)
			{
				Destroy (rowZeroClone); //specific for only the first generated of each random AnimuHead
			}
			CancelInvoke ("Falling");
			fallCounter = 0;
			CreatePrefab();
		}

	}

	/*void CheckBox(int row, int col)
	{
		//stuff
	}*/

	int RandomNumber()
	{
		System.Random rand = new System.Random();
		return rand.Next(0,myPrefabs.Length);
	}

	void Update()
	{
		if (Input.GetKeyDown (KeyCode.LeftArrow) && colNum > 0 && fallCounter < 9)
		{
            //prevents overlapping
			if (transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter,colNum - 1].GetComponent<AnimuHead>() == null)
			{
				if (rowZeroClone != null)
				{
					Destroy (rowZeroClone); //specific for only the first generated of each random AnimuHead
				}
				Destroy(goBelow);
				goBelow = Instantiate (Resources.Load ("Default/DefaultTile"),new Vector2(colNum*2-9,fallCounter*-2+9), Quaternion.identity) as GameObject;
				colNum -= 1;
				goHorz = Instantiate(go,new Vector2(colNum*2-9,fallCounter*-2+9),Quaternion.identity) as GameObject; // make left/right GameObject
				goBelow = goHorz; // make current Gameobject now left/right GameObject
			}
            else
			{
                Debug.Log("You hit an animu head on the left D:");  //debugging purposes, delete later
			}
        }
		if (Input.GetKeyDown (KeyCode.RightArrow) && colNum < 9 && fallCounter < 9)
		{
            //prevents overlapping
			if (transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter,colNum + 1].GetComponent<AnimuHead>() == null)
			{
				if (rowZeroClone != null)
				{
					Destroy (rowZeroClone); //specific for only the first generated of each random AnimuHead
				}
				Destroy(goBelow);
				goBelow = Instantiate (Resources.Load ("Default/DefaultTile"),new Vector2(colNum*2-9,fallCounter*-2+9), Quaternion.identity) as GameObject;
				colNum += 1;
				goHorz = Instantiate(go,new Vector2(colNum*2-9,fallCounter*-2+9),Quaternion.identity) as GameObject;
				goBelow = goHorz;
			}
            else
			{
                Debug.Log("You hit an animu head on the right!!!");  //debugging purposes, delete later
			}
        }
	}		
}