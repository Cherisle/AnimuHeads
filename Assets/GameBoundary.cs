﻿using UnityEngine;
using System.Collections;
using System.Linq;

public class GameBoundary : MonoBehaviour
{
	private const int rows = 11;
	private const int columns = 11;
	private const int headMax = 8;
	private const float fallDownDelay = 0.4f;
	public GameObject[,] gameGrid;
	public GameObject myObject;
	//--------------------------------------------------------
	private float maxRayDistX, maxRayDistY;
	private float ctrXLoc, ctrYLoc;
	private Vector2 rectNWCorner,rectNECorner,rectSWCorner;
	//--------------------------------------------------------
	private directions dir;
	private bool checkWest, checkNorthWest, checkNorthEast, checkEast, checkSouthEast, checkSouth, checkSouthWest; 
	// ^(above) detected initial AH match in this direction
	private bool checkContW, checkContNW, checkContNE, checkContE, checkContSE, checkContS, checkContSW; 
	// ^(above) detected continuous AH match in this direction (more than 1)
	private int comboCnt;
    public int[,] identifier;

    public enum directions { UNSET, SOUTH, EAST, WEST, NORTHEAST, NORTHWEST, SOUTHEAST, SOUTHWEST };

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
		checkWest = checkNorthWest  = checkNorthEast = checkEast = checkSouthEast = checkSouth = checkSouthWest = false; // initialize
		checkContW = checkContNW = checkContNE = checkContE = checkContSE = checkContS = checkContSW = false; // initialize
		//--------------------------------------------------------------------------------
		maxRayDistX = GetComponent<RectTransform>().sizeDelta.x; // stretches the width
		maxRayDistY = GetComponent<RectTransform>().sizeDelta.y; // stretches the height
		ctrXLoc = transform.position.x;
		ctrYLoc = transform.position.y;
		rectNWCorner = new Vector2 (ctrXLoc - maxRayDistX / 2, ctrYLoc + maxRayDistY / 2);
		rectNECorner = new Vector2 (ctrXLoc + maxRayDistX / 2, ctrYLoc + maxRayDistY / 2);
		rectSWCorner = new Vector2 (ctrXLoc - maxRayDistX / 2, ctrYLoc - maxRayDistY / 2);
		//--------------------------------------------------------------------------------
	}

    void Awake()
    {
        myObject = Resources.Load("Default/DefaultTile") as GameObject;
        gameGrid = new GameObject[rows, columns];
        identifier = new int[rows, columns];
        for (int ii = 0; ii < rows; ii++)
        {
            for (int jj = 0; jj < columns; jj++)
            {
                gameGrid[ii, jj] = myObject;
                identifier[ii, jj] = 8; //default identifier, we use 0-7 as index
            }
        }
    }

	public void CheckLBCorner(int row, int col) // checks LEFT BOTTOM corner
	{
		//should be used exclusively to check surroundings of R9C0 (row 9, column 0)
		//only need Northeast and East directions checked
		comboCnt = 0; // reset
		checkNorthEast = checkEast = false; // reset
		checkContNE = checkContE = false; // reset
		int rightOfCol = col+1; // ease of reference
		int rowAbove = row-1; // ease of reference
		int fpIdentifier = identifier[row,col]; // identifier
		int storeContNE, storeContE; // declare
		storeContNE = storeContE = 0; // initialize
		if (fpIdentifier == identifier [rowAbove, rightOfCol]) // matched w/ NorthEast AnimuHead
		{
			comboCnt++;
			dir = directions.NORTHEAST;
			checkNorthEast = true;
			storeContNE = ContDirCheck (dir, rowAbove, rightOfCol);
			comboCnt += storeContNE;
		}
		if (fpIdentifier == identifier [row, rightOfCol]) // matched w/ East AnimuHead
		{
			comboCnt++;
			dir = directions.EAST;
			checkEast = true;
			storeContE = ContDirCheck (dir, row, rightOfCol);
			comboCnt += storeContE; //comboCnt adds however many more combos in direction (EAST)
		}
		comboCnt++; //always need to include focal point in the combo count
		Debug.Log ("Total Combo Count is " + comboCnt);
		if(checkEast == true && checkNorthEast == false) // Possibility 1: east matches, northeast fails
		{
			if(storeContE >=1) // 1 continuous means at least 3 combo (FP AnimuHead, Matching East AnimuHead, 1st ContinuousE AnimuHead...)
			{
				//-----Destroy Proper Tiles via Combo------------------
				Destroy(gameGrid[row,col],fallDownDelay);
				Destroy(gameGrid[row,rightOfCol],fallDownDelay);
				for(int ii=1;ii<=storeContE;ii++) // continuous East
				{
					Destroy(gameGrid[row,rightOfCol+ii],fallDownDelay);
				}
				//-----------------------------------------------------
				//-----Reset Proper Tiles after Combo------------------
				gameGrid[row,col] = gameGrid[row,rightOfCol] = myObject;
				identifier[row,col] = identifier[row,rightOfCol] = headMax;
				for(int ii=1;ii<=storeContE;ii++)
				{
					gameGrid[row,rightOfCol+ii] = myObject;
					identifier[row,rightOfCol+ii] = headMax;
				}
				//-----------------------------------------------------
				transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt); // subtracts proper # of destroyed objects from goGridCnt
			}
		} // END Possibility 1 ---------------------------------------------------------------------------
		if(checkEast == false && checkNorthEast == true) // Possibility 2: east fails, northeast matches
		{
			if(storeContNE >=1)
			{
				//-----Destroy Proper Tiles via Combo--------------------------
				Destroy(gameGrid[row,col],fallDownDelay);
				Destroy(gameGrid[rowAbove,rightOfCol],fallDownDelay);
				for(int ii=1;ii<=storeContNE;ii++) // continuous Northeast
				{
					Destroy(gameGrid[rowAbove-ii,rightOfCol+ii],fallDownDelay);
				}
				//-------------------------------------------------------------
				//-----Reset Proper Tiles after Combo--------------------------
				gameGrid[row,col] = gameGrid[rowAbove,rightOfCol] = myObject;
				identifier[row,col] = identifier[rowAbove,rightOfCol] = headMax;
				for(int ii=1;ii<=storeContNE;ii++) // reset continuous Northeast
				{
					gameGrid[rowAbove-ii,rightOfCol+ii] = myObject;
					identifier[rowAbove-ii,rightOfCol+ii]= headMax;
				}
				//-------------------------------------------------------------
				transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt); // refer to above comment for function, will no longer be commented below
			}
		} // END Possibility 2 ---------------------------------------------------------------------------
		if(checkEast == true && checkNorthEast == true) // Possibility 3: both east and northeast match
		{
			if(storeContE >=1 || storeContNE >=1)
			{
				Destroy(gameGrid[row,col],fallDownDelay);
				if(storeContE >=1)
				{	
					Destroy(gameGrid[row,rightOfCol],fallDownDelay);
					for(int ii=1;ii<=storeContE;ii++) // continuous East
					{
						Destroy(gameGrid[row,rightOfCol+ii],fallDownDelay);
					}
					//-----------------------------------------------------
					//-----Reset Proper Tiles after Combo------------------
					gameGrid[row,col] = gameGrid[row,rightOfCol] = myObject;
					identifier[row,col] = identifier[row,rightOfCol] = headMax;
					for(int ii=1;ii<=storeContE;ii++)
					{
						gameGrid[row,rightOfCol+ii] = myObject;
						identifier[row,rightOfCol+ii] = headMax;
					}
				}
				if(storeContNE >=1)
				{
					Destroy(gameGrid[rowAbove,rightOfCol],fallDownDelay);
					for(int ii=1;ii<=storeContNE;ii++) // continuous NorthEast
					{
						Destroy(gameGrid[rowAbove-ii,rightOfCol+ii],fallDownDelay);
					}
					//-------------------------------------------------------------
					//-----Reset Proper Tiles after Combo--------------------------
					gameGrid[row,col] = gameGrid[rowAbove,rightOfCol] = myObject;
					identifier[row,col] = identifier[rowAbove,rightOfCol] = headMax;
					for(int ii=1;ii<=storeContNE;ii++) // reset continuous NorthEast
					{
						gameGrid[rowAbove-ii,rightOfCol+ii] = myObject;
						identifier[rowAbove-ii,rightOfCol+ii]= headMax;
					}
				}
				transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
			}
		}
	} // end CheckLBCorner Method ---------------------------------------------------------

	public void CheckRBCorner(int row, int col)
	{
		//should be used exclusively to check surroundings of R9C9 (row 9, column 9) **POSSIBLY WILL HAVE A COLUMN 10 WHEN WE MAKE IT 11x11**
		//only need Northwest and west directions checked
		comboCnt = 0; // reset
		checkNorthWest = checkWest = false; // reset
		checkContNW = checkContW = false; // reset
		int leftOfCol = col-1; // ease of reference
		int rowAbove = row-1; // ease of reference
		int fpIdentifier = identifier[row,col]; // identifier
		int storeContNW, storeContW; // declare
		storeContNW = storeContW = 0; // initialize
		if (fpIdentifier == identifier [rowAbove, leftOfCol]) // matched w/ NorthWest AnimuHead
		{
			comboCnt++;
			dir = directions.NORTHWEST;
			checkNorthWest = true;
			storeContNW = ContDirCheck (dir, rowAbove, leftOfCol);
			comboCnt += storeContNW;
		}
		if (fpIdentifier == identifier [row, leftOfCol]) // matched w/ West AnimuHead
		{
			comboCnt++;
			dir = directions.WEST;
			checkWest = true;
			storeContW = ContDirCheck (dir, row, leftOfCol);
			comboCnt += storeContW;
		}
		comboCnt++; //always need to include focal point in the combo count
		Debug.Log ("Total Combo Count is " + comboCnt);
		if(checkWest == true && checkNorthWest == false) // Possibility 1: west matches, northwest fails
		{
			if(storeContW >=1) // 1 continuous means at least 3 combo obtained
			{
				//-----Destroy Proper Tiles via Combo------------------
				Destroy(gameGrid[row,col],fallDownDelay);
				Destroy(gameGrid[row,leftOfCol],fallDownDelay);
				for(int ii=1;ii<=storeContW;ii++)
				{
					Destroy(gameGrid[row,leftOfCol-ii],fallDownDelay);
				}
				//-----------------------------------------------------
				//-----Reset Proper Tiles after Combo------------------
				gameGrid[row,col] = gameGrid[row,leftOfCol] = myObject;
				identifier[row,col] = identifier[row,leftOfCol] = headMax;
				for(int ii=1;ii<=storeContW;ii++)
				{
					gameGrid[row,leftOfCol-ii] = myObject;
					identifier[row,leftOfCol-ii] = headMax;
				}
				//-----------------------------------------------------
				transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
			}
		} // END Possibility 1 ---------------------------------------------------------------------------
		if(checkWest == false && checkNorthWest == true) // Possibility 2: west fails, northwest matches
		{
			if(storeContNW >=1)
			{
				//-----Destroy Proper Tiles via Combo--------------------------
				Destroy(gameGrid[row,col],fallDownDelay);
				Destroy(gameGrid[rowAbove,leftOfCol],fallDownDelay);
				for(int ii=1;ii<=storeContNW;ii++)
				{
					Destroy(gameGrid[rowAbove-ii,leftOfCol-ii],fallDownDelay);
				}
				//-------------------------------------------------------------
				//-----Reset Proper Tiles after Combo--------------------------
				gameGrid[row,col] = gameGrid[rowAbove,leftOfCol] = myObject;
				identifier[row,col] = identifier[rowAbove,leftOfCol] = headMax;
				for(int ii=1;ii<=storeContNW;ii++)
				{
					gameGrid[rowAbove-ii,leftOfCol-ii] = myObject;
					identifier[rowAbove-ii,leftOfCol-ii]= headMax;
				}
				//-------------------------------------------------------------
				transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
			}
		} // END Possibility 2 ---------------------------------------------------------------------------
		if(checkWest == true && checkNorthWest == true) // Possibility 3: both west and northwest match
		{
			if(storeContW >=1 || storeContNW >=1)
			{
				Destroy(gameGrid[row,col],fallDownDelay);
				if(storeContW >=1)
				{
					Destroy(gameGrid[row,leftOfCol],fallDownDelay);
					for(int ii=1;ii<=storeContW;ii++)
					{
						Destroy(gameGrid[row,leftOfCol-ii],fallDownDelay);
					}
					gameGrid[row,col] = gameGrid[row,leftOfCol] = myObject;
					identifier[row,col] = identifier[row,leftOfCol] = headMax;
					for(int ii=1;ii<=storeContW;ii++)
					{
						gameGrid[row,leftOfCol-ii] = myObject;
						identifier[row,leftOfCol-ii] = headMax;
					}
				}
				if(storeContNW >=1)
				{
					Destroy(gameGrid[rowAbove,leftOfCol],fallDownDelay);
					for(int ii=1;ii<=storeContNW;ii++)
					{
						Destroy(gameGrid[rowAbove-ii,leftOfCol-ii],fallDownDelay);
					}
					gameGrid[row,col] = gameGrid[rowAbove,leftOfCol] = myObject;
					identifier[row,col] = identifier[rowAbove,leftOfCol] = headMax;
					for(int ii=1;ii<=storeContNW;ii++)
					{
						gameGrid[rowAbove-ii,leftOfCol-ii] = myObject;
						identifier[rowAbove-ii,leftOfCol-ii]= headMax;
					}
				}
				transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
			}
		}
	} // end CheckRBCorner Method ---------------------------------------------------------

	public void CheckPillar(int row, int col)
	{
		comboCnt = 0; //reset
		checkWest = checkNorthWest  = checkNorthEast = checkEast = false; // reset
		checkContW = checkContNW  = checkContNE = checkContE = false; // reset
		//------------------------------------------------------------------------------------
		int leftOfCol = col-1; // these lines between the comments are used to simplify meaning
		int rightOfCol = col+1; // this one too
		int rowAbove = row-1; // this one too
		int fpIdentifier = identifier[row,col]; // this one too
		int storeContW, storeContNW, storeContNE, storeContE; // store continuous combo amts of a direction
		storeContW = storeContNW = storeContNE = storeContE = 0; // initialize from within
		//------------------------------------------------------------------------------------
		for (int ii = leftOfCol; ii <= rightOfCol; ii++) // checks NW, NE directions
		{
			if (fpIdentifier == identifier [rowAbove, ii])
			{
				comboCnt++;
				if (ii == leftOfCol){ // matched w/ northwest AnimuHead 
					dir = directions.NORTHWEST;
					checkNorthWest = true;
					storeContNW = ContDirCheck (dir, rowAbove, leftOfCol);
					comboCnt += storeContNW;
				}
				else if (ii == rightOfCol){ // matched w/ northeast AnimuHead	
					dir = directions.NORTHEAST;
					checkNorthEast = true;
					storeContNE = ContDirCheck (dir, rowAbove, rightOfCol);
					comboCnt += storeContNE;
				}
			}
		}
		if (fpIdentifier == identifier [row, leftOfCol]) // checks WEST
		{
			comboCnt++; // indentifiers match, comboCnt increments by 1
			dir = directions.WEST;
			checkWest = true;
			storeContW = ContDirCheck (dir, row, leftOfCol);
			comboCnt += storeContW; //comboCnt adds however many more combos in direction (WEST)
		}
		if (fpIdentifier == identifier [row, rightOfCol]) // checks EAST
		{
			comboCnt++;
			dir = directions.EAST;
			checkEast = true;
			storeContE = ContDirCheck (dir, row, rightOfCol);
			comboCnt += storeContE; //comboCnt adds however many more combos in direction (EAST)
		}
		comboCnt++; //always need to include focal point in the combo count
		Debug.Log ("INITIAL Total Combo Count is " + comboCnt);
		if(checkWest == true && checkEast == true) // [I-1] instant combo INITIAL(ENTRY) condition, matches both west & east 
		{
			if(checkNorthWest == false && checkNorthEast == false) //northern neighbors don't match, then can break 3-5 in a row HORZ
			{
				if(comboCnt>3) // must be >=4 combo as a result of the above
				{
					if(checkContE == false && checkContW == true) // X-number combo west with no CONTINUOUS combo on the east e.g. Match Match FP Match Fail
					{
						Debug.Log("Focal Point combo'd with " + storeContW + " AnimuHeads in the west direction");
						for(int ii=1; ii<=storeContW; ii++)
						{
							Destroy(gameGrid[row,col-1-ii],fallDownDelay);
						}
						for(int ii=1; ii<=storeContW; ii++)
						{
							gameGrid[row,leftOfCol-ii] = myObject;
							identifier[row,leftOfCol-ii] = headMax;
						}
					}
					else if(checkContW == false && checkContE == true) // X-number combo east with no CONTINUOUS combo on the west e.g. Fail Match FP Match Match
					{
						Debug.Log("Focal Point combo'd with " + storeContE + " AnimuHeads in the east direction");
						for(int ii=1;ii<=storeContE;ii++) // continuous East
						{
							Destroy(gameGrid[row,rightOfCol+ii],fallDownDelay);
						}
						for(int ii=1; ii<=storeContE; ii++)
						{
							gameGrid[row,rightOfCol+ii] = myObject;
							identifier[row,rightOfCol+ii] = headMax;
						}
					}
					else if(checkContW == true && checkContE == true) // X-number combo with CONTINUOUS combo on BOTH WEST AND EAST e.g. Match Match FP Match Match
					{
						for(int ii=1; ii<=storeContW; ii++)
						{
							Destroy(gameGrid[row,col-1-ii],fallDownDelay);
						}
						for(int ii=1; ii<=storeContE; ii++)
						{
							Destroy(gameGrid[row,col+1+ii],fallDownDelay);
						}
						for(int ii=1; ii<=storeContW; ii++)
						{
							gameGrid[row,leftOfCol-ii] = myObject;
							identifier[row,leftOfCol-ii] = headMax;
						}
						for(int ii=1; ii<=storeContE; ii++)
						{
							gameGrid[row,rightOfCol+ii] = myObject;
							identifier[row,rightOfCol+ii] = headMax;
						}
						// maximum continuous on each side: (M FP M) does not count as CONTINUOUS direction check
						// maxContW == 4  <--> maxContE == 3 e.g.  M M M M (M FP M) M M M
						// maxContW == 3  <--> maxContE == 4 e.g.  M M M (M FP M) M M M M
					}
					else // both continuous west and east are false, would never occur if comboCnt > 3
					{
						// code never reaches here, would run the else below (comboCnt == 3) e.g. Fail Match FP Match Fail
					}
					Destroy(gameGrid[row,leftOfCol],fallDownDelay); // FROM HERE DOWN, done for all cases
					Destroy(gameGrid[row,col],fallDownDelay);
					Destroy(gameGrid[row,rightOfCol],fallDownDelay);
					gameGrid[row,leftOfCol] = gameGrid[row,col] = gameGrid[row,rightOfCol] = myObject;
					identifier[row,leftOfCol] = identifier[row,col] = identifier[row,rightOfCol] = headMax;
					transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt); // subtracts proper # of destroyed objects from goGridCnt
				}
				else // MUST be 3 combo w/ focal point in middle (comboCnt == 3) e.g. M FP M , THIS IS A FINAL STEP before GRIDALLCHECK 
				{
                    //plays sound from the position of the middle head in the 3 combo before all 3 get destroyed
                    gameGrid[row, col].GetComponent<AnimuHead>().audioPos = gameGrid[row, col].transform.position;
                    gameGrid[row, col].GetComponent<AnimuHead>().PlaySound();

                    Destroy(gameGrid[row,leftOfCol], fallDownDelay);
					Destroy(gameGrid[row,col], fallDownDelay);
					Destroy(gameGrid[row,rightOfCol], fallDownDelay);
					gameGrid[row,leftOfCol] = gameGrid[row,col] = gameGrid[row,rightOfCol] = myObject;
					identifier[row,leftOfCol] = identifier[row,col] = identifier[row,rightOfCol] = headMax;
					transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt); // subtracts the 3 destroyed objects from goGridCnt
				}
			}
			if(checkNorthWest == true && checkNorthEast == true) //northern neighbors both match
			{
				for(int ii=1;ii<=storeContNW;ii++) // continuous Northwest
				{
					Destroy(gameGrid[rowAbove-ii,leftOfCol-ii],fallDownDelay);
				}
				for(int ii=1; ii<=storeContW; ii++) // continuous West
				{
					Destroy(gameGrid[row,col-1-ii],fallDownDelay);
				}
				for(int ii=1;ii<=storeContNE;ii++) // continuous Northeast
				{
					Destroy(gameGrid[rowAbove-ii,rightOfCol+ii],fallDownDelay);
				}
				for(int ii=1; ii<=storeContE; ii++) // continuous East
				{
					Destroy(gameGrid[row,col+1+ii],fallDownDelay);
				}
				for(int ii=1;ii<=storeContNW;ii++) // reset continuous Northwest
				{
					gameGrid[rowAbove-ii,leftOfCol-ii] = myObject;
					identifier[rowAbove-ii,leftOfCol-ii]= headMax;
				}
				for(int ii=1; ii<=storeContW; ii++) // reset continuous West
				{
					gameGrid[row,leftOfCol-ii] = myObject;
					identifier[row,leftOfCol-ii] = headMax;
				}
				for(int ii=1;ii<=storeContNE;ii++) // reset continuous Northeast
				{
					gameGrid[rowAbove-ii,rightOfCol+ii] = myObject;
					identifier[rowAbove-ii,rightOfCol+ii]= headMax;
				}
				for(int ii=1; ii<=storeContE; ii++) // reset continuous East
				{
					gameGrid[row,rightOfCol+ii] = myObject;
					identifier[row,rightOfCol+ii] = headMax;
				}
				Destroy(gameGrid[rowAbove,leftOfCol],fallDownDelay);
				Destroy(gameGrid[rowAbove,rightOfCol],fallDownDelay);
				Destroy(gameGrid[row,leftOfCol],fallDownDelay);
				Destroy(gameGrid[row,col],fallDownDelay);
				Destroy(gameGrid[row,rightOfCol],fallDownDelay);
				gameGrid[row,leftOfCol] = gameGrid[row,col] = gameGrid[row,rightOfCol] = gameGrid[rowAbove,leftOfCol] = gameGrid[rowAbove,rightOfCol] = myObject;
				identifier[row,leftOfCol] = identifier[row,col] = identifier[row,rightOfCol] = identifier[rowAbove,leftOfCol] = identifier[rowAbove,rightOfCol] = headMax;
				transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
			}
			if(checkNorthWest == true && checkNorthEast == false) // NW initial matches, NE initial fails
			{
				for(int ii=1;ii<=storeContNW;ii++) // continuous Northwest
				{
					Destroy(gameGrid[rowAbove-ii,leftOfCol-ii],fallDownDelay);
				}
				for(int ii=1; ii<=storeContW; ii++) // continuous West
				{
					Destroy(gameGrid[row,col-1-ii],fallDownDelay);
				}
				for(int ii=1; ii<=storeContE; ii++) // continuous East
				{
					Destroy(gameGrid[row,col+1+ii],fallDownDelay);
				}
				for(int ii=1;ii<=storeContNW;ii++) // reset continuous Northwest
				{
					gameGrid[rowAbove-ii,leftOfCol-ii] = myObject;
					identifier[rowAbove-ii,leftOfCol-ii]= headMax;
				}
				for(int ii=1; ii<=storeContW; ii++) // reset continuous West
				{
					gameGrid[row,leftOfCol-ii] = myObject;
					identifier[row,leftOfCol-ii] = headMax;
				}
				for(int ii=1; ii<=storeContE; ii++) // reset continuous East
				{
					gameGrid[row,rightOfCol+ii] = myObject;
					identifier[row,rightOfCol+ii] = headMax;
				}
				Destroy(gameGrid[rowAbove,leftOfCol],fallDownDelay);
				Destroy(gameGrid[row,leftOfCol],fallDownDelay);
				Destroy(gameGrid[row,col],fallDownDelay);
				Destroy(gameGrid[row,rightOfCol],fallDownDelay);
				gameGrid[row,leftOfCol] = gameGrid[row,col] = gameGrid[row,rightOfCol] = gameGrid[rowAbove,leftOfCol] = myObject;
				identifier[row,leftOfCol] = identifier[row,col] = identifier[row,rightOfCol] = identifier[rowAbove,leftOfCol] = headMax;
				transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
			}
			if(checkNorthWest == false && checkNorthEast == true) // NW initial fails, NE initial matches
			{
				for(int ii=1; ii<=storeContW; ii++) // continuous West
				{
					Destroy(gameGrid[row,col-1-ii],fallDownDelay);
				}
				for(int ii=1;ii<=storeContNE;ii++) // continuous Northeast
				{
					Destroy(gameGrid[rowAbove-ii,rightOfCol+ii],fallDownDelay);
				}
				for(int ii=1; ii<=storeContE; ii++) // continuous East
				{
					Destroy(gameGrid[row,col+1+ii],fallDownDelay);
				}
				for(int ii=1; ii<=storeContW; ii++) // reset continuous West
				{
					gameGrid[row,leftOfCol-ii] = myObject;
					identifier[row,leftOfCol-ii] = headMax;
				}
				for(int ii=1;ii<=storeContNE;ii++) // reset continuous Northeast
				{
					gameGrid[rowAbove-ii,rightOfCol+ii] = myObject;
					identifier[rowAbove-ii,rightOfCol+ii]= headMax;
				}
				for(int ii=1; ii<=storeContE; ii++) // reset continuous East
				{
					gameGrid[row,rightOfCol+ii] = myObject;
					identifier[row,rightOfCol+ii] = headMax;
				}
				Destroy(gameGrid[rowAbove,rightOfCol],fallDownDelay);
				Destroy(gameGrid[row,leftOfCol],fallDownDelay);
				Destroy(gameGrid[row,col],fallDownDelay);
				Destroy(gameGrid[row,rightOfCol],fallDownDelay);
				gameGrid[row,leftOfCol] = gameGrid[row,col] = gameGrid[row,rightOfCol] = gameGrid[rowAbove,rightOfCol] = myObject;
				identifier[row,leftOfCol] = identifier[row,col] = identifier[row,rightOfCol] = identifier[rowAbove,rightOfCol] = headMax;
				transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt); // subtracts proper # of destroyed objects from goGridCnt
			}
		} // end checkWest == TRUE && checkEast == TRUE INITIAL(ENTRY) condition
		//-------------------------------------------------------------------------------------------------------------------------------------------
		else if(checkWest == true && checkEast == false) // [I-2] INITIAL(ENTRY) condition: matches west, fails east
		{
			if(checkNorthWest == false && checkNorthEast == false) // Possibility 1: both northern checks (NW,NE) fail
			{
				if(storeContW >= 1) // at least one continuous on the west matching required for combo
				{
					Destroy(gameGrid[row,leftOfCol],fallDownDelay);
					Destroy(gameGrid[row,col],fallDownDelay);
					for(int ii=1;ii<=storeContW;ii++)
					{
						Destroy(gameGrid[row,leftOfCol-ii],fallDownDelay);
					}
					for(int ii=1;ii<=storeContW;ii++)
					{
						gameGrid[row,leftOfCol-ii] = myObject;
						identifier[row,leftOfCol-ii] = headMax;
					}
					gameGrid[row,leftOfCol] = gameGrid[row,col] = myObject;
					identifier[row,leftOfCol] = identifier[row,col] = headMax;
					transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
				}
			}
			if(checkNorthWest == true && checkNorthEast == true) // Possibility 2: both northern checks (NW,NE) pass
			{
				if(storeContW >=1 || storeContNW >=1 || storeContNE >=1)
				{
					if(storeContW >=1) // if continuous match is on east dir
					{
						Destroy(gameGrid[row,leftOfCol],fallDownDelay);
						for(int ii=1;ii<=storeContW;ii++)
						{
							Destroy(gameGrid[row,leftOfCol-ii],fallDownDelay);
						}
						for(int ii=1;ii<=storeContW;ii++)
						{
							gameGrid[row,leftOfCol-ii] = myObject;
							identifier[row,leftOfCol-ii]= headMax;
						}
						resetGridInfo(row,leftOfCol);
					}
					else {comboCnt--;}
					if(storeContNW >=1) // if continuous match is on NW dir
					{
						Destroy(gameGrid[rowAbove,leftOfCol],fallDownDelay);
						for(int ii=1;ii<=storeContNW;ii++)
						{
							Destroy(gameGrid[rowAbove-ii,leftOfCol-ii],fallDownDelay);
						}
						for(int ii=1;ii<=storeContNW;ii++)
						{
							gameGrid[rowAbove-ii,leftOfCol-ii] = myObject;
							identifier[rowAbove-ii,leftOfCol-ii]= headMax;
						}
						resetGridInfo(rowAbove,leftOfCol);
					}
					else {comboCnt--;}
					if(storeContNE >=1) // if continuous match is on NE dir
					{
						Destroy(gameGrid[rowAbove,rightOfCol],fallDownDelay);
						for(int ii=1;ii<=storeContNE;ii++)
						{
							Destroy(gameGrid[rowAbove-ii,rightOfCol+ii],fallDownDelay);
						}
						for(int ii=1;ii<=storeContNE;ii++)
						{
							gameGrid[rowAbove-ii,rightOfCol+ii] = myObject;
							identifier[rowAbove-ii,rightOfCol+ii]= headMax;
						}
						resetGridInfo(rowAbove,rightOfCol);
					}
					else {comboCnt--;}
					Debug.Log ("UPDATED Total Combo Count is " + comboCnt);
					resetGridFP(row,col);
					transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
				}
			}
			if(checkNorthWest == true && checkNorthEast == false) // Possibility 3: matches Northwest, fails Northeast
			{
				if(storeContW >=1 || storeContNW >=1) // if either contain a continuous match ; necessary because two chains in separate directions aren't combo, even if comboCount reads 3
				{
					if(storeContW >=1) // if continuous match is on east dir
					{
						Destroy(gameGrid[row,leftOfCol],fallDownDelay);
						for(int ii=1;ii<=storeContW;ii++)
						{
							Destroy(gameGrid[row,leftOfCol-ii],fallDownDelay);
						}
						for(int ii=1;ii<=storeContW;ii++)
						{
							gameGrid[row,leftOfCol-ii] = myObject;
							identifier[row,leftOfCol-ii]= headMax;
						}
						resetGridInfo(row,leftOfCol);
					}
					else {comboCnt--;}
					if(storeContNW >=1) // if continuous match is on NW dir
					{
						Destroy(gameGrid[rowAbove,leftOfCol],fallDownDelay);
						for(int ii=1;ii<=storeContNW;ii++)
						{
							Destroy(gameGrid[rowAbove-ii,leftOfCol-ii],fallDownDelay);
						}
						for(int ii=1;ii<=storeContNW;ii++)
						{
							gameGrid[rowAbove-ii,leftOfCol-ii] = myObject;
							identifier[rowAbove-ii,leftOfCol-ii]= headMax;
						}
						resetGridInfo(rowAbove,leftOfCol);
					}
					else {comboCnt--;}
					Debug.Log ("UPDATED Total Combo Count is " + comboCnt);
					resetGridFP(row,col);
					transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
				}
			}
			if(checkNorthWest == false && checkNorthEast == true) // Possibility 4: fails Northwest, matches Northeast
			{
				if(storeContW >=1 || storeContNE >=1) // if either contain a continuous match ; necessary because two chains in separate directions aren't combo, even if comboCount reads 3
				{
					if(storeContW >=1) // if continuous match is on east dir
					{
						Destroy(gameGrid[row,leftOfCol],fallDownDelay);
						for(int ii=1;ii<=storeContW;ii++)
						{
							Destroy(gameGrid[row,leftOfCol-ii],fallDownDelay);
						}
						for(int ii=1;ii<=storeContW;ii++)
						{
							gameGrid[row,leftOfCol-ii] = myObject;
							identifier[row,leftOfCol-ii]= headMax;
						}
						resetGridInfo(row,leftOfCol);
					}
					else {comboCnt--;}
					if(storeContNE >=1) // if continuous match is on NE dir
					{
						Destroy(gameGrid[rowAbove,rightOfCol],fallDownDelay);
						for(int ii=1;ii<=storeContNE;ii++)
						{
							Destroy(gameGrid[rowAbove-ii,rightOfCol+ii],fallDownDelay);
						}
						for(int ii=1;ii<=storeContNE;ii++)
						{
							gameGrid[rowAbove-ii,rightOfCol+ii] = myObject;
							identifier[rowAbove-ii,rightOfCol+ii]= headMax;
						}
						resetGridInfo(rowAbove,rightOfCol);
					}
					else {comboCnt--;}
					Debug.Log ("UPDATED Total Combo Count is " + comboCnt);
					resetGridFP(row,col);
					transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
				}
			}
		} // end checkWest == TRUE && checkEast == FALSE INITIAL(ENTRY) condition
		//-------------------------------------------------------------------------------------------------------------------------------------------
		else if(checkWest == false && checkEast == true) // [I-3] INITIAL(ENTRY) condition: fails west, matches east
		{
			if(checkNorthWest == false && checkNorthEast == false) // Possibility 1: both northern checks (NW,NE) fail
			{
				if(storeContE >= 1) // at least one continuous on the east matching required for combo
				{
					Destroy(gameGrid[row,rightOfCol],fallDownDelay);
					Destroy(gameGrid[row,col],fallDownDelay);
					for(int ii=1;ii<=storeContE;ii++)
					{
						Destroy(gameGrid[row,rightOfCol+ii],fallDownDelay);
					}
					for(int ii=1;ii<=storeContE;ii++)
					{
						gameGrid[row,rightOfCol+ii] = myObject;
						identifier[row,rightOfCol+ii] = headMax;
					}
					gameGrid[row,rightOfCol] = gameGrid[row,col] = myObject;
					identifier[row,rightOfCol] = identifier[row,col] = headMax;
					transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
				}
			}
			if(checkNorthWest == true && checkNorthEast == true) // Possibility 2: both northern checks (NW,NE) pass
			{
				if(storeContE >=1 || storeContNW >=1 || storeContNE >=1) // if either dir contains a continuous match
				{
					if(storeContE >=1) // if continuous match is on east dir
					{
						Destroy(gameGrid[row,rightOfCol],fallDownDelay);
						for(int ii=1;ii<=storeContE;ii++)
						{
							Destroy(gameGrid[row,rightOfCol+ii],fallDownDelay);
						}
						for(int ii=1;ii<=storeContE;ii++)
						{
							gameGrid[row,rightOfCol+ii] = myObject;
							identifier[row,rightOfCol+ii]= headMax;
						}
						resetGridInfo(row,rightOfCol);
					}
					else {comboCnt--;}
					if(storeContNW >=1) // if continuous match is on NW dir
					{
						Destroy(gameGrid[rowAbove,leftOfCol],fallDownDelay);
						for(int ii=1;ii<=storeContNW;ii++)
						{
							Destroy(gameGrid[rowAbove-ii,leftOfCol-ii],fallDownDelay);
						}
						for(int ii=1;ii<=storeContNW;ii++)
						{
							gameGrid[rowAbove-ii,leftOfCol-ii] = myObject;
							identifier[rowAbove-ii,leftOfCol-ii]= headMax;
						}
						resetGridInfo(rowAbove,leftOfCol);
					}
					else {comboCnt--;}
					if(storeContNE >=1) // if continuous match is on NE dir
					{
						Destroy(gameGrid[rowAbove,rightOfCol],fallDownDelay);
						for(int ii=1;ii<=storeContNE;ii++)
						{
							Destroy(gameGrid[rowAbove-ii,rightOfCol+ii],fallDownDelay);
						}
						for(int ii=1;ii<=storeContNE;ii++)
						{
							gameGrid[rowAbove-ii,rightOfCol+ii] = myObject;
							identifier[rowAbove-ii,rightOfCol+ii]= headMax;
						}
						resetGridInfo(rowAbove,rightOfCol);
					}
					else {comboCnt--;}
					Debug.Log ("UPDATED Total Combo Count is " + comboCnt);
					resetGridFP(row,col);
					transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
				}
			}
			if(checkNorthWest == true && checkNorthEast == false) // Possibility 3: matches Northwest, fails Northeast
			{
				if(storeContE >=1 || storeContNW >=1)
				{
					if(storeContE >=1) // if continuous match is on east dir
					{
						Destroy(gameGrid[row,rightOfCol],fallDownDelay);
						for(int ii=1;ii<=storeContE;ii++)
						{
							Destroy(gameGrid[row,rightOfCol+ii],fallDownDelay);
						}
						for(int ii=1;ii<=storeContE;ii++)
						{
							gameGrid[row,rightOfCol+ii] = myObject;
							identifier[row,rightOfCol+ii]= headMax;
						}
						resetGridInfo(row,rightOfCol);
					}
					else {comboCnt--;}
					if(storeContNW >=1) // if continuous match is on NW dir
					{
						Destroy(gameGrid[rowAbove,leftOfCol],fallDownDelay);
						for(int ii=1;ii<=storeContNW;ii++)
						{
							Destroy(gameGrid[rowAbove-ii,leftOfCol-ii],fallDownDelay);
						}
						for(int ii=1;ii<=storeContNW;ii++)
						{
							gameGrid[rowAbove-ii,leftOfCol-ii] = myObject;
							identifier[rowAbove-ii,leftOfCol-ii]= headMax;
						}
						resetGridInfo(rowAbove,leftOfCol);
					}
					else {comboCnt--;}
					Debug.Log ("UPDATED Total Combo Count is " + comboCnt);
					resetGridFP(row,col);
					transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
				}
			}
			if(checkNorthWest == false && checkNorthEast == true) // Possibility 4: fails Northwest, matches Northeast
			{
				if(storeContE >=1 || storeContNE >=1)
				{
					if(storeContE >=1) // if continuous match is on east dir
					{
						Destroy(gameGrid[row,rightOfCol],fallDownDelay);
						for(int ii=1;ii<=storeContE;ii++)
						{
							Destroy(gameGrid[row,rightOfCol+ii],fallDownDelay);
						}
						for(int ii=1;ii<=storeContE;ii++)
						{
							gameGrid[row,rightOfCol+ii] = myObject;
							identifier[row,rightOfCol+ii]= headMax;
						}
						resetGridInfo(row,rightOfCol);
					}
					else {comboCnt--;}
					if(storeContNE >=1) // if continuous match is on NE dir
					{
						Destroy(gameGrid[rowAbove,rightOfCol],fallDownDelay);
						for(int ii=1;ii<=storeContNE;ii++)
						{
							Destroy(gameGrid[rowAbove-ii,rightOfCol+ii],fallDownDelay);
						}
						for(int ii=1;ii<=storeContNE;ii++)
						{
							gameGrid[rowAbove-ii,rightOfCol+ii] = myObject;
							identifier[rowAbove-ii,rightOfCol+ii]= headMax;
						}
						resetGridInfo(rowAbove,rightOfCol);
					}
					else {comboCnt--;}
					Debug.Log ("UPDATED Total Combo Count is " + comboCnt);
					resetGridFP(row,col);
					transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
				}
			} 
		} // end checkWest == FALSE && checkEast == TRUE INITIAL(ENTRY) condition
		//-------------------------------------------------------------------------------------------------------------------------------------------
		else if(checkWest == false && checkEast == false) // [I-4] INITIAL(ENTRY) condition: both horz west & east checks fail ; NOTE: skip ALL FALSE CONDITION (meaning NW & NE also fail)
		{
			if(checkNorthWest == true && checkNorthEast == false) // Possibility 1: NW only match
			{
				if(storeContNW >=1) // at least one continuous on NW dir matching
				{
					Destroy(gameGrid[rowAbove,leftOfCol],fallDownDelay);
					Destroy(gameGrid[row,col],fallDownDelay);
					for(int ii=1;ii<=storeContNW;ii++)
					{
						Destroy(gameGrid[rowAbove-ii,leftOfCol-ii],fallDownDelay);
					}
					for(int ii=1;ii<=storeContNW;ii++)
					{
						gameGrid[rowAbove-ii,leftOfCol-ii] = myObject;
						identifier[rowAbove-ii,leftOfCol-ii]= headMax;
					}
					gameGrid[rowAbove,leftOfCol] = gameGrid[row,col] = myObject;
					identifier[rowAbove,leftOfCol] = identifier[row,col] = headMax;
					transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
				}
			}
			if(checkNorthWest == false && checkNorthEast == true) // Possibility 2: NE only match
			{
				if(storeContNE >=1) // at least one continuous on NE dir matching
				{
					Destroy(gameGrid[rowAbove,rightOfCol],fallDownDelay);
					Destroy(gameGrid[row,col],fallDownDelay);
					for(int ii=1;ii<=storeContNE;ii++)
					{
						Destroy(gameGrid[rowAbove-ii,rightOfCol+ii],fallDownDelay);
					}
					for(int ii=1;ii<=storeContNE;ii++)
					{
						gameGrid[rowAbove-ii,rightOfCol+ii] = myObject;
						identifier[rowAbove-ii,rightOfCol+ii]= headMax;
					}
					gameGrid[rowAbove,rightOfCol] = gameGrid[row,col] = myObject;
					identifier[rowAbove,rightOfCol] = identifier[row,col] = headMax;
					transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
				}
			}
			if(checkNorthWest == true && checkNorthEast == true) // Possibility 3: both NW & NE match
			{
				if(storeContNW >=1 || storeContNE >=1) //if either one contains a continuous match
				{
					if(storeContNW >=1) // if the continuous match is on NW dir
					{
						Destroy(gameGrid[rowAbove,leftOfCol],fallDownDelay);
						for(int ii=1;ii<=storeContNW;ii++)
						{
							Destroy(gameGrid[rowAbove-ii,leftOfCol-ii],fallDownDelay);
						}
						for(int ii=1;ii<=storeContNW;ii++)
						{
							gameGrid[rowAbove-ii,leftOfCol-ii] = myObject;
							identifier[rowAbove-ii,leftOfCol-ii]= headMax;
						}
						resetGridInfo(rowAbove,leftOfCol);
					}
					else {comboCnt--;}
					if(storeContNE >=1) // if the continuous match is on NE dir
					{
						Destroy(gameGrid[rowAbove,rightOfCol],fallDownDelay);
						for(int ii=1;ii<=storeContNE;ii++)
						{
							Destroy(gameGrid[rowAbove-ii,rightOfCol+ii],fallDownDelay);
						}
						for(int ii=1;ii<=storeContNE;ii++)
						{
							gameGrid[rowAbove-ii,rightOfCol+ii] = myObject;
							identifier[rowAbove-ii,rightOfCol+ii]= headMax;
						}
						resetGridInfo(rowAbove,rightOfCol);
					}
					else {comboCnt--;}
					Debug.Log ("UPDATED Total Combo Count is " + comboCnt);
					resetGridFP(row,col);
					transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
				}
			}
			// SKIPPED: checkNorthWest == false && checkNorthEast == false; REASON: unnecessary
		}
	}

	private int ContDirCheck(directions d, int fpRow, int fpCol) //continue direction check w/ UPDATED focal point
	{
		switch(d)
		{
			case directions.SOUTH: return ContSouthCheck(fpRow,fpCol);
			case directions.WEST: return ContWestCheck(fpRow,fpCol);
			case directions.EAST: return ContEastCheck(fpRow,fpCol);
			case directions.NORTHWEST: return ContNWCheck(fpRow,fpCol);
			case directions.NORTHEAST: return ContNECheck(fpRow,fpCol);
			case directions.SOUTHWEST: return ContSWCheck(fpRow,fpCol);
			case directions.SOUTHEAST: return ContSECheck(fpRow,fpCol);
			default: return 0; //does nothing, direction unaltered
		}
	}

	private int ContSouthCheck(int fpRow, int fpCol)
	{
		if(fpRow!=rows-1) // as long as fpRow is not the furthest southern row...
		{
			if(identifier[fpRow,fpCol] == identifier[fpRow+1,fpCol])
			{
				dir = directions.SOUTH;
				checkContS = true;
				Debug.Log("Found continuous match with south neighbor AnimuHead");
				return 1 + ContDirCheck(dir,fpRow+1,fpCol);
			} else {return 0;}
		} else {return 0;}
	}

	private int ContWestCheck(int fpRow, int fpCol)
	{
		if(fpCol!=0) // as long as fpCol is not the furthest west column...
		{
			if(identifier[fpRow,fpCol] == identifier[fpRow,fpCol-1])
			{
				dir = directions.WEST;
				checkContW = true;
				Debug.Log("Found continuous match with west neighbor AnimuHead");
				return 1 + ContDirCheck(dir,fpRow,fpCol-1);
			} else {return 0;}
		} else {return 0;}
	}

	private int ContEastCheck(int fpRow, int fpCol)
	{
		if(fpCol!=columns-1) // as long as fpCol is not the furthest east column...
		{
			if(identifier[fpRow,fpCol] == identifier[fpRow,fpCol+1])
			{
				dir = directions.EAST;
				checkContE = true;
				Debug.Log("Found continuous match with east neighbor AnimuHead");
				return 1 + ContDirCheck(dir,fpRow,fpCol+1);
			} else {return 0;}
		} else {return 0;}
	}

	private int ContNWCheck(int fpRow, int fpCol)
	{
		if(fpRow!=0 && fpCol!=0) // as long as fpRow not furthest N row and fpCol not furthest W col
		{
			if(identifier[fpRow,fpCol] == identifier[fpRow-1,fpCol-1])
			{
				dir = directions.NORTHWEST;
				checkContNW = true;
				Debug.Log("Found continuous match with northwest neighbor AnimuHead");
				return 1 + ContDirCheck(dir,fpRow-1,fpCol-1);
			} else {return 0;}
		} else {return 0;}
	}

	private int ContNECheck(int fpRow, int fpCol)
	{
		if(fpRow!=0 && fpCol!=columns-1) // as long as fpRow not furthest N row and fpCol not furthest E col
		{
			if(identifier[fpRow,fpCol] == identifier[fpRow-1,fpCol+1])
			{
				dir = directions.NORTHEAST;
				checkContNE = true;
				Debug.Log("Found continuous match with northeast neighbor AnimuHead");
				return 1 + ContDirCheck(dir,fpRow-1,fpCol+1);
			} else {return 0;}
		} else {return 0;}
	}

	private int ContSWCheck(int fpRow, int fpCol)
	{
		if(fpRow!=rows-1 && fpCol!=0) // as long as fpRow not furthest S row and fpCol not furthest W col
		{
			if(identifier[fpRow,fpCol] == identifier[fpRow+1,fpCol-1])
			{
				dir = directions.SOUTHWEST;
				checkContSW = true;
				Debug.Log("Found continuous match with southwest neighbor AnimuHead");
				return 1 + ContDirCheck(dir,fpRow+1,fpCol-1);
			} else {return 0;}
		} else {return 0;}
	}

	private int ContSECheck(int fpRow, int fpCol)
	{
		if(fpRow!=rows-1 && fpCol!=columns-1) // as long as fpRow not furthest S row and fpCol not furthest E col
		{
			if(identifier[fpRow,fpCol] == identifier[fpRow+1,fpCol+1])
			{
				dir = directions.SOUTHEAST;
				checkContSE = true;
				Debug.Log("Found continuous match with southeast neighbor AnimuHead");
				return 1 + ContDirCheck(dir,fpRow+1,fpCol+1);
			} else {return 0;}
		} else {return 0;}
	}

	private void resetGridInfo(int r, int c)
	{
		gameGrid[r,c] = myObject;
		identifier[r,c] = headMax;
	}

	private void resetGridFP(int r, int c)
	{
		Destroy(gameGrid[r,c],fallDownDelay);
		gameGrid[r,c] = myObject;
		identifier[r,c] = headMax;
	}

	public void setID(int r, int c, int hNum)
	{
		identifier[r,c] = hNum;
		Debug.Log("Identifier [" + r + "," + c + "] = " + hNum);
	}
		
	void Update ()
	{
		
	}
}