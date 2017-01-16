﻿using UnityEngine;
using Object = UnityEngine.Object;
using System;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

public class TileGenerator : MonoBehaviour
{
	private const int headMax = 8;
	private const float fallDownDelay = 0.5f;
	public Object[] myPrefabs;
	private bool nameMatch;
	private int colNum;
	private int fallCounter;
    public float keyDelay = 1f;  //used for continuous key press in a single direction
    private float timePassed = 0f;  //used for continuous key press in a single direction
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
		fallCounter = 0;
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
		InvokeRepeating ("Falling", fallDownDelay, fallDownDelay);
	}
	void Falling()
	{
		if (fallCounter < 9)
		{	
			if(transform.parent.GetComponent<GameBoundary> ().gameGrid[fallCounter+1,colNum] != null) // gameGrid location below exists
			{
				if (transform.parent.GetComponent<GameBoundary> ().gameGrid[fallCounter+1,colNum].GetComponent<AnimuHead>() != null) // AnimuHead below exists?
				{
					transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter,colNum] = Instantiate(go,new Vector2(colNum*2-9,fallCounter*-2+9), Quaternion.identity) as GameObject;
					transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter,colNum].name = go.name; //display proper AnimuHead name
					Debug.Log("GameGrid[" + fallCounter + "," + colNum + "] = " + transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter,colNum].name);
					transform.parent.GetComponent<GameBoundary>().idUpdate(fallCounter,colNum,goHeadNum);  
					goGridCnt++; // AnimuHead stamped on game grid, this line registers the AnimuHead count
					if(goGridCnt >=3)
					{
						fpRow = fallCounter;
						fpCol = colNum;
						//Debug.Log("Focal Point R,C is " + fpRow + "," + fpCol);
					}
					CancelInvoke ("Falling");
					if (fallCounter == 0)
					{
						Destroy(rowZeroClone);
						Debug.Log("Game should be over");
	                    //reload game over scene right here, once we have created the scene itself
	                    SceneManager.LoadScene("GameOverScene");
					}
					else
					{
						//Destroy(goCurrent);
						Destroy(goBelow);
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
						transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter+1,colNum] = Instantiate(go,new Vector2(colNum*2-9,(fallCounter+1)*-2+9), Quaternion.identity) as GameObject;  
						transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter+1,colNum].name = go.name;
						Debug.Log("GameGrid[" + (fallCounter+1) + "," + colNum + "] = " + transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter+1,colNum].name);
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
					// set current tile equal to default (transparent) NOTE: DO NOT INSTANTIATE
					goCurrent = Resources.Load("Default/DefaultTile") as GameObject;
					//--------------------------------------------------------------------------------------------------------------------------
					fallCounter++;
				}
			}
			else
			{
				goCurrent = goBelow; //goBelow was previous below, now's current
				if (rowZeroClone != null)
				{
					Destroy (rowZeroClone); //specific for only the first generated of each random AnimuHead
				}
				if (fallCounter == 8) // final iteration of THIS else loop
				{
					transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter+1,colNum] = Instantiate(go,new Vector2(colNum*2-9,(fallCounter+1)*-2+9), Quaternion.identity) as GameObject;  
					transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter+1,colNum].name = go.name;
					Debug.Log("GameGrid[" + (fallCounter+1) + "," + colNum + "] = " + transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter+1,colNum].name);
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
				}
				else
				{
					goBelow = Instantiate (go,new Vector2(colNum*2-9,(fallCounter+1)*-2+9), Quaternion.identity) as GameObject;
				}
				Destroy(goCurrent);
				goCurrent = Resources.Load("Default/DefaultTile") as GameObject;
				fallCounter++;
			}
		} 
		else //fallCounter >= 9
		{
			// destroys all illusion tiles to prepare for new instantiation
			//Destroy(goCurrent);
			Destroy(goBelow);
			CancelInvoke ("Falling");
			CreatePrefab();
		}

	}

	/*void CheckBox(int row, int col)
	{
		//this method should be in GameBoundary
	}*/

	int RandomNumber()
	{
		System.Random rand = new System.Random();
		return rand.Next(0,myPrefabs.Length);
	}

	void Update()
	{
        timePassed += Time.deltaTime;

		if (Input.GetKey(KeyCode.LeftArrow) && colNum > 0 && fallCounter < 9 && timePassed >= keyDelay)
		{
            //prevents overlapping
            //if (Input.GetKey(KeyCode.LeftArrow))
            //{
                if (transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter, colNum - 1].GetComponent<AnimuHead>() == null)
                {
                    if (rowZeroClone != null)
                    {
                        Destroy(rowZeroClone); //specific for only the first generated of each random AnimuHead
                    }
                    Destroy(goBelow);
                    goBelow = Resources.Load("Default/DefaultTile") as GameObject;
                    colNum -= 1;
                    goHorz = Instantiate(go, new Vector2(colNum * 2 - 9, fallCounter * -2 + 9), Quaternion.identity) as GameObject; // make left/right GameObject
                    goBelow = goHorz; // make current Gameobject now left/right GameObject
                }
            //}
            else
            {
                Debug.Log("You hit an animu head on the left D:");  //debugging purposes
            }

            timePassed = 0f;
        }

		if (Input.GetKey (KeyCode.RightArrow) && colNum < 9 && fallCounter < 9 && timePassed >= keyDelay)
		{
            //prevents overlapping
			if (transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter,colNum + 1].GetComponent<AnimuHead>() == null)
			{
				if (rowZeroClone != null)
				{
					Destroy (rowZeroClone); //specific for only the first generated of each random AnimuHead
				}
				Destroy(goBelow);
				goBelow = Resources.Load ("Default/DefaultTile") as GameObject;
				colNum += 1;
				goHorz = Instantiate(go,new Vector2(colNum*2-9,fallCounter*-2+9),Quaternion.identity) as GameObject;
				goBelow = goHorz;
			}
            else
			{
                Debug.Log("You hit an animu head on the right!!!");  //debugging purposes
			}

            timePassed = 0f;
        }
	}		
}