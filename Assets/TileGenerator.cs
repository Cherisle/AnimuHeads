using UnityEngine;
using Object = UnityEngine.Object;
using System;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;


public class TileGenerator : MonoBehaviour
{
	private const int ROWS = 11;
	private const int COLUMNS = 11;
	private const int HEAD_MAX = 8;
	private const float FALL_DOWN_DELAY = 0.4f;
	public Object[] myPrefabs;
	private bool nameMatch;
	private int colNum;
	private int fallCounter;
    public float keyDelay = 0.08f;  //used for continuous key press in a single direction
    private float timePassed = 0f;  //used for continuous key press in a single direction
    //-----------------------------------------------
    private GameObject dropSpot;
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
	private int goHeadNum; // gameObject head number -- used to represent character name
    //---Pause Menu ---
    private GameObject[] pauseObjects;
    private PauseMenu m;
    // Use this for initialization
    void Start ()
	{
        Time.timeScale = 1;
		pauseObjects = GameObject.Find("Canvas").GetComponent<PauseMenu>().pauseObjects;
        
        createdHeads = new string[HEAD_MAX];
		fpRow = 0;
		fpCol = 0;
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
        bool properSpot = false;
        for (int dropRow = ROWS - 1; dropRow >= 0; dropRow--)
        {
            if (transform.parent.gameObject.GetComponent<GameBoundary>().identifier[dropRow, colNum] == 8)
            {
                for (int headsAbove = dropRow - 1; headsAbove >= 0; headsAbove--)
                {
                    if (transform.parent.gameObject.GetComponent<GameBoundary>().identifier[headsAbove, colNum] != 8)
                    {
                        if (transform.parent.gameObject.GetComponent<GameBoundary>().identifier[headsAbove - 1, colNum] == 8)
                        {
                            dropSpot = Instantiate(Resources.Load("DropSpot/GreenCrossHair"), new Vector2(colNum * 2 - 10, (headsAbove - 1) * -2 + 10), Quaternion.identity) as GameObject;
                            dropSpot.name = "Landing Spot";
                            properSpot = true;
                            break;
                        }
                    }
                }
                if (properSpot == false)
                {
                    dropSpot = Instantiate(Resources.Load("DropSpot/GreenCrossHair"), new Vector2(colNum * 2 - 10, dropRow * -2 + 10), Quaternion.identity) as GameObject;
                    dropSpot.name = "Landing Spot";
                    break;
                }
            }
        }
        rowZeroClone = Instantiate(go,new Vector2(colNum*2-10,fallCounter*-2+10),Quaternion.identity) as GameObject;
		rowZeroClone.name = go.name;
		InvokeRepeating ("Falling", FALL_DOWN_DELAY, FALL_DOWN_DELAY);
	}
	void Falling()
	{
		if (fallCounter < ROWS - 1) //checks for inbounds
		{	
			if(transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter+1,colNum] != null) // gameGrid location below exists, also checks for inbounds
			{
                if (fallCounter<9 && transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter+2, colNum].GetComponent<AnimuHead>() != null) // AnimuHead below exists 2 spots below?
                {
                    Destroy(dropSpot);
                }
                if (transform.parent.GetComponent<GameBoundary> ().gameGrid[fallCounter+1,colNum].GetComponent<AnimuHead>() != null) // AnimuHead below exists?
				{
                    transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter,colNum] = Instantiate(go,new Vector2(colNum*2-10,fallCounter*-2+10), Quaternion.identity) as GameObject;
					transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter,colNum].name = go.name; //display proper AnimuHead name
					Debug.Log("GameGrid[" + fallCounter + "," + colNum + "] = " + transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter,colNum].name);
					transform.parent.GetComponent<GameBoundary>().setID(fallCounter,colNum,goHeadNum);
					transform.parent.GetComponent<GameBoundary>().addHeadToGrid(); // AnimuHead stamped on game grid, this line registers the AnimuHead count
					if(transform.parent.GetComponent<GameBoundary>().goGridCnt >=3)
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
					if (fallCounter == ROWS-2) // final iteration of THIS else loop, fallCounter set == second to last row (because stamping occurs one down) 
					{
						transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter+1,colNum] = Instantiate(go,new Vector2(colNum*2-10,(fallCounter+1)*-2+10), Quaternion.identity) as GameObject;  
						transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter+1,colNum].name = go.name;
						Debug.Log("GameGrid[" + (fallCounter+1) + "," + colNum + "] = " + transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter+1,colNum].name);
						transform.parent.GetComponent<GameBoundary>().setID(fallCounter+1,colNum,goHeadNum);
                        Destroy(dropSpot); 
						transform.parent.GetComponent<GameBoundary>().addHeadToGrid();
						if(transform.parent.GetComponent<GameBoundary>().goGridCnt >=3)
						{
							fpRow = fallCounter+1; // because we are at fallCounter == 8, but we stamped at fallcounter == 9 [above as fallCounter+1]
							fpCol = colNum;
							if(fpCol == 0 || fpCol == COLUMNS - 1)
							{
								//should check bottom corners, not pillar
								if(fpCol == 0) // bottom left corner
								{
									transform.parent.GetComponent<GameBoundary>().CheckLBCorner(fpRow,fpCol);
									Debug.Log("successfully entered here message");
								}
							}
							else
							{
								transform.parent.GetComponent<GameBoundary>().CheckPillar(fpRow,fpCol);
							}
						}
					}
					else
					{
						// instantiate GameObject tile below current tile
						goBelow = Instantiate (go,new Vector2(colNum*2-10,(fallCounter+1)*-2+10), Quaternion.identity) as GameObject;
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
		} 
		else // fallCounter >= 10, signals to create new AnimuHead character tile
		{
			// destroys all illusion tiles to prepare for new instantiation
			//Destroy(goCurrent);
			Destroy(goBelow);
			CancelInvoke ("Falling");
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
        timePassed += Time.deltaTime;
        //start of pause block
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("I paused");
            //Game is playing, but we want to pause it now.
            if (Time.timeScale == 1)
            {
                Time.timeScale = 0;
                foreach (GameObject g in pauseObjects)
                {
					Debug.Log("how many"); // returned 7 objects: PauseText,PlayButton,Resume,RestartButton,Restart,MainMenuButton,MainMenu
					g.SetActive(true); //Set every tag where  pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause"); to show.
                }
            }
            //Game is paused, but we want to resume it now.
            else if (Time.timeScale == 0)
            {
                Debug.Log("high");
                Time.timeScale = 1;
                
                foreach (GameObject g in pauseObjects)
                {
                    g.SetActive(false);// Hide every tag where  pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
                }
            }
        }
        //end pause block

        if (Input.GetKey(KeyCode.LeftArrow) && colNum > 0 && fallCounter < (ROWS-1) && timePassed >= keyDelay &&  Time.timeScale != 0)
		    {
            //prevents overlapping
            if (transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter, colNum - 1].GetComponent<AnimuHead>() == null)
            {
                if (rowZeroClone != null)
                {
                    Destroy(rowZeroClone); //specific for only the first generated of each random AnimuHead
                }
                Destroy(goBelow);
                goBelow = Resources.Load("Default/DefaultTile") as GameObject;
                colNum -= 1;
                goHorz = Instantiate(go, new Vector2(colNum * 2 - 10, fallCounter * -2 + 10), Quaternion.identity) as GameObject; // make left/right GameObject
                goBelow = goHorz; // make current Gameobject now left/right GameObject
                Destroy(dropSpot);
                bool properSpot = false;
                for (int dropRow = ROWS - 1; dropRow >= 0; dropRow--)
                {
                    if (transform.parent.gameObject.GetComponent<GameBoundary>().identifier[dropRow, colNum] == 8)
                    {
                        for (int headsAbove = dropRow - 1; headsAbove >= 0; headsAbove--)
                        {
                            if (transform.parent.gameObject.GetComponent<GameBoundary>().identifier[headsAbove, colNum] != 8)
                            {
                                if (transform.parent.gameObject.GetComponent<GameBoundary>().identifier[headsAbove - 1, colNum] == 8)
                                {
                                    dropSpot = Instantiate(Resources.Load("DropSpot/GreenCrossHair"), new Vector2(colNum * 2 - 10, (headsAbove - 1) * -2 + 10), Quaternion.identity) as GameObject;
                                    dropSpot.name = "Landing Spot";
                                    properSpot = true;
                                    break;
                                }
                            }
                        }
                        if (properSpot == false)
                        {
                            dropSpot = Instantiate(Resources.Load("DropSpot/GreenCrossHair"), new Vector2(colNum * 2 - 10, dropRow * -2 + 10), Quaternion.identity) as GameObject;
                            dropSpot.name = "Landing Spot";
                            break;
                        }
                    }
                }
            }
            //}
            else
            {
                Debug.Log("You hit an animu head on the left D:");  //debugging purposes
            }
            timePassed = 0f;
        }

		if (Input.GetKey (KeyCode.RightArrow) && colNum < (COLUMNS-1) && fallCounter < (ROWS-1) && timePassed >= keyDelay && Time.timeScale != 0)
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
				goHorz = Instantiate(go,new Vector2(colNum*2-10,fallCounter*-2+10),Quaternion.identity) as GameObject;
				goBelow = goHorz;
                Destroy(dropSpot);
                bool properSpot = false;
                for (int dropRow = ROWS - 1; dropRow >= 0; dropRow--)
                {
                    if (transform.parent.gameObject.GetComponent<GameBoundary>().identifier[dropRow, colNum] == 8)
                    {
                        for (int headsAbove = dropRow - 1; headsAbove >= 0; headsAbove--)
                        {
                            if (transform.parent.gameObject.GetComponent<GameBoundary>().identifier[headsAbove, colNum] != 8)
                            {
                                if (transform.parent.gameObject.GetComponent<GameBoundary>().identifier[headsAbove - 1, colNum] == 8)
                                {
                                    dropSpot = Instantiate(Resources.Load("DropSpot/GreenCrossHair"), new Vector2(colNum * 2 - 10, (headsAbove - 1) * -2 + 10), Quaternion.identity) as GameObject;
                                    dropSpot.name = "Landing Spot";
                                    properSpot = true;
                                    break;
                                }
                            }
                        }
                        if (properSpot == false)
                        {
                            dropSpot = Instantiate(Resources.Load("DropSpot/GreenCrossHair"), new Vector2(colNum * 2 - 10, dropRow * -2 + 10), Quaternion.identity) as GameObject;
                            dropSpot.name = "Landing Spot";
                            break;
                        }
                    }
                }
            }
            else
			{
                Debug.Log("You hit an animu head on the right!!!");  //debugging purposes
			}
            timePassed = 0f;
        }

		if (Input.GetKeyDown (KeyCode.DownArrow) && Time.timeScale != 0)
		{
			Destroy(goBelow);
			Destroy(rowZeroClone);
			for (int dropRow = fallCounter; dropRow <= 9; dropRow++)
			{
				if(transform.parent.GetComponent<GameBoundary> ().gameGrid[dropRow+1,colNum].GetComponent<AnimuHead>() != null) // AnimuHead below exists? (meaning, AH below is TRUE)
				{
					transform.parent.GetComponent<GameBoundary>().gameGrid[dropRow,colNum] = Instantiate(go,new Vector2(colNum*2-10,dropRow*-2+10), Quaternion.identity) as GameObject;
					transform.parent.GetComponent<GameBoundary>().gameGrid[dropRow,colNum].name = go.name; //display proper AnimuHead name
					//Debug.Log("GameGrid[" + fallCounter + "," + colNum + "] = " + transform.parent.GetComponent<GameBoundary>().gameGrid[fallCounter,colNum].name);
					transform.parent.GetComponent<GameBoundary>().setID(dropRow,colNum,goHeadNum);
					transform.parent.GetComponent<GameBoundary>().addHeadToGrid();
					if(transform.parent.GetComponent<GameBoundary>().goGridCnt >=3)
					{
						fpRow = dropRow;
						fpCol = colNum;
						//Debug.Log("Focal Point R,C is " + fpRow + "," + fpCol);
					}
					if (dropRow == 0)
					{
						Destroy(rowZeroClone);
						Debug.Log("Game should be over");
						//reload game over scene right here, once we have created the scene itself
						SceneManager.LoadScene("GameOverScene");
					}
					dropRow = 100;
					CancelInvoke("Falling");
					CreatePrefab();
				}
				else // tile below is NOT an AnimuHead, then is DEFAULT (BUT STILL INSIDE THE GRID, not on the boundaries)
				{	
					if (dropRow == 9) // final iteration of THIS else loop ; stamps always occur +1 down, that's why this is 9
					{
						transform.parent.GetComponent<GameBoundary>().gameGrid[dropRow+1,colNum] = Instantiate(go,new Vector2(colNum*2-10,(dropRow+1)*-2+10), Quaternion.identity) as GameObject;  
						transform.parent.GetComponent<GameBoundary>().gameGrid[dropRow+1,colNum].name = go.name;
						transform.parent.GetComponent<GameBoundary>().setID(dropRow+1,colNum,goHeadNum);
						Destroy(dropSpot); 
						transform.parent.GetComponent<GameBoundary>().addHeadToGrid();
						if(transform.parent.GetComponent<GameBoundary>().goGridCnt >=3)
						{
							fpRow = dropRow+1; // because we are at fallCounter == 8, but we stamped at fallcounter == 9 [above as fallCounter+1]
							fpCol = colNum;
							if(fpCol == 0 || fpCol == COLUMNS - 1)
							{
								if(fpCol == 0) // bottom left corner
								{
									transform.parent.GetComponent<GameBoundary>().CheckLBCorner(fpRow,fpCol);
								}
								if(fpCol == COLUMNS-1) // bottom right corner
								{
									transform.parent.GetComponent<GameBoundary>().CheckRBCorner(fpRow,fpCol);
								}
							}
							else
							{
								transform.parent.GetComponent<GameBoundary>().CheckPillar(fpRow,fpCol);

							}
						}
						dropRow = 100;
						CancelInvoke("Falling");
						CreatePrefab();
					}						
				}
				Destroy(dropSpot);
			}
		}
    }		
}