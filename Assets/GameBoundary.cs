using UnityEngine;
using System.Collections;
using System.Linq;

public class GameBoundary : MonoBehaviour
{
	public const int rows = 10;
	public const int columns = 10;
	private const float fallDownDelay = 0.5f;
	public GameObject[,] gameGrid;
	public GameObject myObject;
	//--------------------------------------------------------
	private float maxRayDistX, maxRayDistY;
	private float ctrXLoc, ctrYLoc;
	private Vector2 rectNWCorner,rectNECorner,rectSWCorner;
	//--------------------------------------------------------
	private directions dir;
	private bool checkWest, checkNorthWest, checkNorth, checkNorthEast, checkEast; //detected initial AH match in this direction
	private bool checkContW, checkContNW, checkContN, checkContNE, checkContE; //detected continuous AH match in this direction (more than 1)
	private int comboCnt;
    public int[,] identifier;

    //use this to do anything with directions
    public enum directions { UNSET, NORTH, SOUTH, EAST, WEST, NORTHEAST, NORTHWEST, SOUTHEAST, SOUTHWEST };

    void FixedUpdate()
	{
		Debug.DrawLine (rectNWCorner, rectNWCorner + Vector2.right * maxRayDistX);
		Debug.DrawLine (rectNECorner, rectNECorner + Vector2.down * maxRayDistY);
		Debug.DrawLine (rectSWCorner, rectSWCorner + Vector2.right * maxRayDistX);
		Debug.DrawLine (rectNWCorner, rectNWCorner + Vector2.down * maxRayDistY);
	}

	void Start ()
	{
		comboCnt = 0; // initialize
		dir = directions.UNSET; // initialize
		checkWest = checkNorthWest = checkNorth = checkNorthEast = checkEast = false;
		checkContW = checkContNW = checkContN = checkContNE = checkContE = false; // initialize
		//--------------------------------------------------------------------------------
		maxRayDistX = GetComponent<RectTransform>().sizeDelta.x; // stretches the width
		maxRayDistY = GetComponent<RectTransform>().sizeDelta.y; // stretches the height
		ctrXLoc = transform.position.x;
		ctrYLoc = transform.position.y;
		rectNWCorner = new Vector2 (ctrXLoc - maxRayDistX / 2, ctrYLoc + maxRayDistY / 2);
		rectNECorner = new Vector2 (ctrXLoc + maxRayDistX / 2, ctrYLoc + maxRayDistY / 2);
		rectSWCorner = new Vector2 (ctrXLoc - maxRayDistX / 2, ctrYLoc - maxRayDistY / 2);
		//--------------------------------------------------------------------------------
		myObject = Resources.Load("Default/DefaultTile") as GameObject;
		gameGrid = new GameObject[rows,columns];
        identifier = new int[rows,columns];
		for(int ii=0;ii<rows;ii++)
		{
			for(int jj=0;jj<columns;jj++)
			{
				gameGrid[ii, jj] = Instantiate (myObject,new Vector2(jj*2-9,ii*-2+9), Quaternion.identity) as GameObject;
				identifier[ii,jj] = 8; //default identifier, we use 0-7 as index
			}
		}
	}

	public int CheckPillar(int row, int col)
	{
		comboCnt = 0; //reset
		checkWest = checkNorthWest = checkNorth = checkNorthEast = checkEast = false; // reset
		checkContW = checkContNW = checkContN = checkContNE = checkContE = false; // reset
		//------------------------------------------------------------------------------------
		int leftOfCol = col-1; // these lines between the comments are used to simplify meaning
		int rightOfCol = col+1; // this one too
		int rowAbove = row-1; // this one too
		int fpIdentifier = identifier[row,col]; // this one too
		//------------------------------------------------------------------------------------
		if(col == 0 || col == 9)
		{
			return 100; //should check something though
		}
		else
		{
			for(int ii = leftOfCol; ii<=rightOfCol; ii++)
			{
				if(fpIdentifier == identifier[rowAbove,ii])
				{
					comboCnt++;
					if(ii == leftOfCol){ // matched with northwest AnimuHead 
						dir = directions.NORTHWEST;
						checkNorthWest = true;
						ContDirCheck(dir,rowAbove,leftOfCol);
						//Debug.Log("Found a match with northwest neighbor AnimuHead");
					}
					if(ii == col){ // matched with north AnimuHead
						dir = directions.NORTH;
						checkNorth = true;
						ContDirCheck(dir,rowAbove,col);
						//Debug.Log("Found a match with north neighbor AnimuHead");
					}
					if(ii == rightOfCol){ // matched with northeast AnimuHead
						dir = directions.NORTHEAST;
						checkNorthEast = true;
						ContDirCheck(dir,rowAbove,rightOfCol);
						//Debug.Log("Found a match with northeast neighbor AnimuHead");
					}
				}
			}
			if(fpIdentifier == identifier[row,leftOfCol])
			{
				comboCnt++; // indentifiers match, comboCnt increments by 1
				dir = directions.WEST;
				checkWest = true;
				comboCnt += ContDirCheck(dir,row,leftOfCol); //comboCnt adds however many more combos in the direction
				Debug.Log("Combo Count after checking continuous west: " + comboCnt);
			}
			if(fpIdentifier == identifier[row,rightOfCol])
			{
				comboCnt++;
				dir = directions.EAST;
				checkEast = true;
				ContDirCheck(dir,row,rightOfCol);
				//Debug.Log("Found a match with east neighbor AnimuHead");
			}
			comboCnt++; //always need to include focal point in the combo count
			Debug.Log("Total Combo Count is " + comboCnt);
		}
		if(checkWest == true && checkEast == true) //instant combo 1st condition, w/o continual dirCheck
		{
			if(checkNorthWest != true && checkNorth != true && checkNorthEast != true) //northern neighbors don't match, then can break 3-5 in a row HORZ
			{
				if(comboCnt>3) // must be 4 or 5 combo as a result of the above
				{
					if(checkContE == false) // X combo west with no CONTINUOUS combo on the east e.g. Match Match FP Match Fail
					{
						int contMatchWest = comboCnt - 3; // excludes initial west-matched AnimuHead, focalpoint AnimuHead, and initial east-matched AnimuHead
						Debug.Log("Focal Point combo'd with " + contMatchWest + " AnimuHeads in the west direction");
						switch(contMatchWest)  
						{
							// we have to minus one extra column over << because we work with row 0 and col 0, (thus the +1 after contMatchWest as initial)
							case 1:
								Destroy(gameGrid[row,col-(contMatchWest+1)],fallDownDelay);
								break;
							case 2:
								Destroy(gameGrid[row,col-(contMatchWest+1)],fallDownDelay);
								Destroy(gameGrid[row,col-contMatchWest],fallDownDelay);
								break;
							case 3:
								Destroy(gameGrid[row,col-(contMatchWest+1)],fallDownDelay);
								Destroy(gameGrid[row,col-contMatchWest],fallDownDelay);
								Destroy(gameGrid[row,col-(contMatchWest-1)],fallDownDelay);
								break;
							case 4:
								Destroy(gameGrid[row,col-(contMatchWest+1)],fallDownDelay);
								Destroy(gameGrid[row,col-contMatchWest],fallDownDelay);
								Destroy(gameGrid[row,col-(contMatchWest-1)],fallDownDelay);
								Destroy(gameGrid[row,col-(contMatchWest-2)],fallDownDelay);
								break;
							case 5:
								Destroy(gameGrid[row,col-(contMatchWest+1)],fallDownDelay);
								Destroy(gameGrid[row,col-contMatchWest],fallDownDelay);
								Destroy(gameGrid[row,col-(contMatchWest-1)],fallDownDelay);
								Destroy(gameGrid[row,col-(contMatchWest-2)],fallDownDelay);
								Destroy(gameGrid[row,col-(contMatchWest-3)],fallDownDelay);
								break;
						}
						Destroy(gameGrid[row,leftOfCol],fallDownDelay); // done for all cases
						Destroy(gameGrid[row,col],fallDownDelay); // done for all cases
						Destroy(gameGrid[row,rightOfCol],fallDownDelay); // done for all cases
					}
					else if(checkContW == false)
					{
						//west direction has no CONTINUOUS combo
					}
				}
				else // must be a 3 combo with focal point directly in the middle
				{
					Destroy(gameGrid[row,leftOfCol],fallDownDelay);
					Destroy(gameGrid[row,col],fallDownDelay);
					Destroy(gameGrid[row,rightOfCol],fallDownDelay);
					gameGrid[row,leftOfCol] = Instantiate(myObject,new Vector2(leftOfCol*2-9,row*-2+9), Quaternion.identity) as GameObject;
					gameGrid[row,col] = Instantiate(myObject,new Vector2(col*2-9,row*-2+9),Quaternion.identity) as GameObject;
					gameGrid[row,rightOfCol] = Instantiate(myObject,new Vector2(rightOfCol*2-9,row*-2+9),Quaternion.identity) as GameObject;
				}
			}
		}
		return comboCnt;
	}

	private int ContDirCheck(directions d, int fpRow, int fpCol) //continue direction check w/ UPDATED focal point
	{
		switch(d)
		{
			case directions.NORTH: return ContNorthCheck(fpRow,fpCol);
			case directions.SOUTH: return ContSouthCheck(fpRow,fpCol);
			case directions.WEST: return ContWestCheck(fpRow,fpCol);
			case directions.EAST: return ContEastCheck(fpRow,fpCol);
			case directions.NORTHWEST: return ContNWCheck(fpRow,fpCol);
			case directions.NORTHEAST: return ContNECheck(fpRow,fpCol);
			case directions.SOUTHWEST: return ContSWCheck(fpRow,fpCol);
			case directions.SOUTHEAST: return ContSECheck(fpRow,fpCol);
			case directions.UNSET: return 0; //does nothing, direction unaltered
			default: return 0; //just incase for function to work
		}
	}

	private int ContNorthCheck(int fpRow, int fpCol)
	{
		return 0; //continue your northern combo check, placeholder return
	}

	private int ContSouthCheck(int fpRow, int fpCol)
	{
		return 0; //continue your southern combo check, placeholder return
	}

	private int ContWestCheck(int fpRow, int fpCol)
	{
		if(fpCol!=0) // as long as fpCol is not zero...
		{
			if(identifier[fpRow,fpCol] == identifier[fpRow,fpCol-1] && fpCol!=0)
			{
				dir = directions.WEST;
				checkContW = true;
				Debug.Log("Found continuous match with west neighbor AnimuHead");
				return 1 + ContDirCheck(dir,fpRow,fpCol-1); // send in an updated focal point
			}
			else
			{
				return 0;
			}
		}
		else
		{
			return 0;
		}
	}

	private int ContEastCheck(int fpRow, int fpCol)
	{
		return 0; //continue your eastern combo check, placeholder return
	}

	private int ContNWCheck(int fpRow, int fpCol)
	{
		return 0; //continue your northwest combo check, placeholder return
	}

	private int ContNECheck(int fpRow, int fpCol)
	{
		return 0; //continue your northeast combo check, placeholder return
	}

	private int ContSWCheck(int fpRow, int fpCol)
	{
		return 0; //continue your southwest combo check, placeholder return
	}

	private int ContSECheck(int fpRow, int fpCol)
	{
		return 0; //continue your southeast combo check, placeholder return
	}

	public void idUpdate(int r, int c, int hNum)
	{
		identifier[r,c] = hNum;
		Debug.Log("Identifier [" + r + "," + c + "] = " + hNum);
	}
		
	void Update ()
	{
		
	}
}