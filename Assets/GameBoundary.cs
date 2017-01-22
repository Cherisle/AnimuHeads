﻿using UnityEngine;
using System.Collections;
using System.Linq;

public class GameBoundary : MonoBehaviour
{
	private const int rows = 10;
	private const int columns = 10;
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
	private bool checkWest, checkNorthWest, checkNorth, checkNorthEast, checkEast, checkSouthEast, checkSouth, checkSouthWest; 
	// ^(above) detected initial AH match in this direction
	private bool checkContW, checkContNW, checkContN, checkContNE, checkContE, checkContSE, checkContS, checkContSW; 
	// ^(above) detected continuous AH match in this direction (more than 1)
	private int comboCnt;
    public int[,] identifier;

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
		checkWest = checkNorthWest = checkNorth = checkNorthEast = checkEast = checkSouthEast = checkSouth = checkSouthWest = false; // initialize
		checkContW = checkContNW = checkContN = checkContNE = checkContE = checkContSE = checkContS = checkContSW = false; // initialize
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
				gameGrid[ii,jj] = myObject;
				identifier[ii,jj] = 8; //default identifier, we use 0-7 as index
			}
		}
	}

	public void CheckPillar(int row, int col)
	{
		comboCnt = 0; //reset
		checkWest = checkNorthWest = checkNorth = checkNorthEast = checkEast = false; // reset
		checkContW = checkContNW = checkContN = checkContNE = checkContE = false; // reset
		//------------------------------------------------------------------------------------
		int leftOfCol = col-1; // these lines between the comments are used to simplify meaning
		int rightOfCol = col+1; // this one too
		int rowAbove = row-1; // this one too
		int fpIdentifier = identifier[row,col]; // this one too
		int storeContW, storeContNW, storeContN, storeContNE, storeContE; // store continuous combo amts of a direction
		storeContW = storeContNW = storeContN = storeContNE = storeContE = 0; // initialize from within
		//------------------------------------------------------------------------------------
		for (int ii = leftOfCol; ii <= rightOfCol; ii++)
		{
			if (fpIdentifier == identifier [rowAbove, ii])
			{
				comboCnt++;
				if (ii == leftOfCol) // matched w/ northwest AnimuHead
				{ 
					dir = directions.NORTHWEST;
					checkNorthWest = true;
					storeContNW = ContDirCheck (dir, rowAbove, leftOfCol);
					comboCnt += storeContNW;
					//Debug.Log("Found a match with northwest neighbor AnimuHead");
				}
				else if (ii == col) // matched w/ north AnimuHead
				{
					dir = directions.NORTH;
					checkNorth = true;
					storeContN = ContDirCheck (dir, rowAbove, col);
					comboCnt += storeContN;
					//Debug.Log("Found a match with north neighbor AnimuHead");
				}
				else if (ii == rightOfCol) // matched w/ northeast AnimuHead
				{	
					dir = directions.NORTHEAST;
					checkNorthEast = true;
					storeContNE = ContDirCheck (dir, rowAbove, rightOfCol);
					comboCnt += storeContNE;
					//Debug.Log("Found a match with northeast neighbor AnimuHead");
				}
			}
		}
		if (fpIdentifier == identifier [row, leftOfCol]) // WEST
		{
			comboCnt++; // indentifiers match, comboCnt increments by 1
			dir = directions.WEST;
			checkWest = true;
			storeContW = ContDirCheck (dir, row, leftOfCol);
			comboCnt += storeContW; //comboCnt adds however many more combos in direction (WEST)
		}
		if (fpIdentifier == identifier [row, rightOfCol]) // EAST
		{
			comboCnt++;
			dir = directions.EAST;
			checkEast = true;
			storeContE = ContDirCheck (dir, row, rightOfCol);
			comboCnt += storeContE; //comboCnt adds however many more combos in direction (EAST)
		}
		comboCnt++; //always need to include focal point in the combo count
		Debug.Log ("INITIAL Total Combo Count is " + comboCnt);
		if(checkWest == true && checkEast == true) //instant combo 1st condition, w/o continual dirCheck
		{
			if(checkNorthWest == false && checkNorth == false && checkNorthEast == false) //northern neighbors don't match, then can break 3-5 in a row HORZ
			{
				if(comboCnt>3) // must be >=4 combo as a result of the above
				{
					if(checkContE == false && checkContW == true) // X-number combo west with no CONTINUOUS combo on the east e.g. Match Match FP Match Fail
					{
						int contMatchWest = comboCnt - 3; // excludes initial west-matched AnimuHead, focalpoint AnimuHead, and initial east-matched AnimuHead
						Debug.Log("Focal Point combo'd with " + contMatchWest + " AnimuHeads in the west direction");
						switch(contMatchWest)  
						{
							// maximum possible combo (VERY RARELY) is 10 e.g. M M M M M M M M FP M where taking max, contMatchWest = (10) - 3 = 7, thus 7 cases
							case 1:
								Destroy(gameGrid[row,col-(contMatchWest+1)],fallDownDelay);
								break;
							case 2:
								Debug.Log("check");	
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
							case 6:
								Destroy(gameGrid[row,col-(contMatchWest+1)],fallDownDelay);
								Destroy(gameGrid[row,col-contMatchWest],fallDownDelay);
								Destroy(gameGrid[row,col-(contMatchWest-1)],fallDownDelay);
								Destroy(gameGrid[row,col-(contMatchWest-2)],fallDownDelay);
								Destroy(gameGrid[row,col-(contMatchWest-3)],fallDownDelay);
								Destroy(gameGrid[row,col-(contMatchWest-4)],fallDownDelay);
								break;
							case 7:
								Destroy(gameGrid[row,col-(contMatchWest+1)],fallDownDelay);
								Destroy(gameGrid[row,col-contMatchWest],fallDownDelay);
								Destroy(gameGrid[row,col-(contMatchWest-1)],fallDownDelay);
								Destroy(gameGrid[row,col-(contMatchWest-2)],fallDownDelay);
								Destroy(gameGrid[row,col-(contMatchWest-3)],fallDownDelay);
								Destroy(gameGrid[row,col-(contMatchWest-4)],fallDownDelay);
								Destroy(gameGrid[row,col-(contMatchWest-5)],fallDownDelay);
								break;
						}
						for(int ii=1; ii>(1-contMatchWest);ii--)
						{
							gameGrid[row,col-(contMatchWest+ii)] = myObject;
							identifier[row,col-(contMatchWest+ii)] = headMax;
						}
					}
					else if(checkContW == false && checkContE == true) // X-number combo east with no CONTINUOUS combo on the west e.g. Fail Match FP Match Match
					{
						int contMatchEast = comboCnt - 3; // excludes initial west-matched AnimuHead, focalpoint AnimuHead, and initial east-matched AnimuHead
						Debug.Log("Focal Point combo'd with " + contMatchEast + " AnimuHeads in the east direction");
						switch(contMatchEast)
						{
							case 1:
								Destroy(gameGrid[row,col+(contMatchEast+1)],fallDownDelay);
								break;
							case 2:
								Destroy(gameGrid[row,col+(contMatchEast+1)],fallDownDelay);
								Destroy(gameGrid[row,col+contMatchEast],fallDownDelay);
								break;
							case 3:
								Destroy(gameGrid[row,col+(contMatchEast+1)],fallDownDelay);
								Destroy(gameGrid[row,col+contMatchEast],fallDownDelay);
								Destroy(gameGrid[row,col+(contMatchEast-1)],fallDownDelay);
								break;
							case 4:
								Destroy(gameGrid[row,col+(contMatchEast+1)],fallDownDelay);
								Destroy(gameGrid[row,col+contMatchEast],fallDownDelay);
								Destroy(gameGrid[row,col+(contMatchEast-1)],fallDownDelay);
								Destroy(gameGrid[row,col+(contMatchEast-2)],fallDownDelay);
								break;
							case 5:
								Destroy(gameGrid[row,col+(contMatchEast+1)],fallDownDelay);
								Destroy(gameGrid[row,col+contMatchEast],fallDownDelay);
								Destroy(gameGrid[row,col+(contMatchEast-1)],fallDownDelay);
								Destroy(gameGrid[row,col+(contMatchEast-2)],fallDownDelay);
								Destroy(gameGrid[row,col+(contMatchEast-3)],fallDownDelay);
								break;
							case 6:
								Destroy(gameGrid[row,col+(contMatchEast+1)],fallDownDelay);
								Destroy(gameGrid[row,col+contMatchEast],fallDownDelay);
								Destroy(gameGrid[row,col+(contMatchEast-1)],fallDownDelay);
								Destroy(gameGrid[row,col+(contMatchEast-2)],fallDownDelay);
								Destroy(gameGrid[row,col+(contMatchEast-3)],fallDownDelay);
								Destroy(gameGrid[row,col+(contMatchEast-4)],fallDownDelay);
								break;
							case 7:
								Destroy(gameGrid[row,col+(contMatchEast+1)],fallDownDelay);
								Destroy(gameGrid[row,col+contMatchEast],fallDownDelay);
								Destroy(gameGrid[row,col+(contMatchEast-1)],fallDownDelay);
								Destroy(gameGrid[row,col+(contMatchEast-2)],fallDownDelay);
								Destroy(gameGrid[row,col+(contMatchEast-3)],fallDownDelay);
								Destroy(gameGrid[row,col+(contMatchEast-4)],fallDownDelay);
								Destroy(gameGrid[row,col+(contMatchEast-5)],fallDownDelay);
								break;
						}
						for(int ii=1; ii>(1-contMatchEast);ii--)
						{
							gameGrid[row,col+(contMatchEast+ii)] = myObject;
							identifier[row,col+(contMatchEast+ii)] = headMax;
						}
					}
					else if(checkContW == true && checkContE == true) // X-number combo with CONTINUOUS combo on BOTH WEST AND EAST e.g. Match Match FP Match Match
					{
						checkContW = checkContE = false;
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
		}
		else if(checkWest == true && checkEast == false) // WEST neighbor matches, EAST neighbor fails
		{
			if(checkNorthWest == false && checkNorth == false && checkNorthEast == false) //northern neighbors don't match
			{
				if(storeContW >= 1) // at least one continuous on the west matching
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
			else if(checkNorthWest == true && checkNorth == false && checkNorthEast == false)
			{
				if(storeContW >=1 || storeContNW >=1) // if either one contains a continuous match
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
			else if(checkNorthWest == false && checkNorth == false && checkNorthEast == true)
			{
				if(storeContW >=1 || storeContNE >=1) // if either one contains a continuous match
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
			else if(checkNorthWest == true && checkNorth == false && checkNorthEast == true)
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
		}
		else if(checkWest == false && checkEast == true) // EAST neighbor matches, WEST neighbor fails
		{
			if(checkNorthWest == false && checkNorth == false && checkNorthEast == false) //northern neighbors don't match
			{
				if(storeContE >= 1) // at least one continuous on the east matching
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
			} //--------------------------------------------------------------------------------------------------------------------
			else if(checkNorthWest == true && checkNorth == false && checkNorthEast == false) // EAST AND NORTHWEST MATCH, REST FAIL
			{
				if(storeContE >=1 || storeContNW >=1) // if either one contains a continuous match
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
						gameGrid[row,rightOfCol] = myObject;
						identifier[row,rightOfCol] = headMax;
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
						gameGrid[rowAbove,leftOfCol] = myObject;
						identifier[rowAbove,leftOfCol] = headMax;
					}
					else {comboCnt--;}
					Debug.Log ("UPDATED Total Combo Count is " + comboCnt);
					resetGridFP(row,col);
					transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
				}
			} //--------------------------------------------------------------------------------------------------------------------
			else if(checkNorthWest == false && checkNorth == false && checkNorthEast == true) // EAST AND NORTHEAST MATCH, REST FAIL
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
						gameGrid[row,rightOfCol] = myObject;
						identifier[row,rightOfCol] = headMax;
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
						gameGrid[rowAbove,rightOfCol] = myObject;
						identifier[rowAbove,rightOfCol] = headMax;
					}
					else {comboCnt--;}
					Debug.Log ("UPDATED Total Combo Count is " + comboCnt);
					resetGridFP(row,col);
					transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
				}
			} //----------------------------------------------------------------------------------------------------
			else if(checkNorthWest == true && checkNorth == false && checkNorthEast == true)
			{
				if(storeContE >=1 || storeContNW >=1 || storeContNE >=1) // if either one contains a continuous match
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
						gameGrid[row,rightOfCol] = myObject;
						identifier[row,rightOfCol] = headMax;
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
						gameGrid[rowAbove,leftOfCol] = myObject;
						identifier[rowAbove,leftOfCol] = headMax;
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
						gameGrid[rowAbove,rightOfCol] = myObject;
						identifier[rowAbove,rightOfCol] = headMax;
					}
					else {comboCnt--;}
					Debug.Log ("UPDATED Total Combo Count is " + comboCnt);
					resetGridFP(row,col);
					transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
				}
			}
		}
		else if(checkWest == false && checkEast == false) // both horz initial checks fail
		{
			if(checkNorthWest == false && checkNorth == false && checkNorthEast == false)
			{
				return; //exits CheckPillar
			}
			else if(checkNorthWest == true && checkNorth == false && checkNorthEast == false) // NW only match
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
			} //-----------------------------------------------------------------------------------------------
			else if(checkNorthWest == false && checkNorth == false && checkNorthEast == true) // NE only match
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
			} //-----------------------------------------------------------------------------------------------
			else if(checkNorthWest == true && checkNorth == false && checkNorthEast == true) // NW & NE match
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
			} //-----------------------------------------------------------------------------------------------------------
			else if(checkNorthWest == false && checkNorth == true && checkNorthEast == false) // N only match (gridAllCheck)
			{
				if(storeContN >=1) // at least one continuous on N dir matching
				{
					Destroy(gameGrid[rowAbove,col],fallDownDelay);
					Destroy(gameGrid[row,col],fallDownDelay);
					for(int ii=1;ii<=storeContN;ii++)
					{
						Destroy(gameGrid[rowAbove-ii,col],fallDownDelay);
					}
					for(int ii=1;ii<=storeContN;ii++)
					{
						gameGrid[rowAbove-ii,col] = myObject;
						identifier[rowAbove-ii,col]= headMax;
					}
					resetGridInfo(rowAbove,col);
					transform.GetChild(0).GetComponent<TileGenerator>().SubtractGrid(comboCnt);
				}
			} //-----------------------------------------------------------------------------------------------------------
			else if(checkNorthWest == true && checkNorth == true && checkNorthEast == false)
			{
				// NW and N true, NE false
			}
			else if(checkNorthWest == false && checkNorth == true && checkNorthEast == true)
			{
				//NE and N are true, NW false
			}
			else if(checkNorthWest == true && checkNorth == true && checkNorthEast == true)
			{
				// all true
			}
		}
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
			default: return 0; //does nothing, direction unaltered
		}
	}

	private int ContNorthCheck(int fpRow, int fpCol)
	{
		if(fpRow!=0) // as long as fpRow is not the furthest northern row...
		{
			if(identifier[fpRow,fpCol] == identifier[fpRow-1,fpCol])
			{
				dir = directions.NORTH;
				checkContN = true;
				Debug.Log("Found continuous match with north neighbor AnimuHead");
				return 1 + ContDirCheck(dir,fpRow-1,fpCol);
			} else {return 0;}
		} else {return 0;}
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